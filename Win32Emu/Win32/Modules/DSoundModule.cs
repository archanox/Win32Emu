using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Win32Emu.Threading;
using Win32Emu.Win32.COM;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Win32Emu.Win32.NativeTypes;

namespace Win32Emu.Win32.Modules
{
	public class DSoundModule : IWin32ModuleAsync
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public DSoundModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "DSOUND.DLL";

		// DirectSound object handles
		private readonly Dictionary<uint, DirectSoundObject> _dsoundObjects = new();
		private readonly Dictionary<uint, DirectSoundBuffer> _buffers = new();
		private readonly Dictionary<uint, uint> _comObjectToBufferHandle = new(); // Maps COM object address to buffer handle
		private uint _nextDSoundHandle = 0x80000000;
		private uint _nextBufferHandle = 0x81000000;
		private ICpu? _cpu;
		private VirtualMemory? _memory;

		// Constants for async callback execution
		private const int INFINITE_LOOP_CHECK_INTERVAL = 100000; // Check for infinite loops every 100K steps
		private const int STUCK_COUNTER_THRESHOLD = 3; // Number of consecutive checks at same EIP to consider it stuck
		private const int CANCELLATION_CHECK_INTERVAL = 1000; // Check cancellation token every 1K steps
		private const uint MINIMUM_VALID_EIP = 0x00001000; // Minimum valid instruction pointer (4KB)

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			_cpu = cpu;
			_memory = memory;
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTSOUNDCREATE":
					returnValue = DirectSoundCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "DIRECTSOUNDENUMERATEA":
					returnValue = DirectSoundEnumerateA(a.UInt32(0), a.UInt32(1));
					return true;
				default:
					_logger.LogInformation("[DSound] Unimplemented export: {Export}", export);
					return false;
			}
		}

		/// <summary>
		/// Async implementation for Win32 APIs that may call back into emulated code.
		/// Routes APIs through async paths to avoid blocking calls that fail on WASM.
		/// </summary>
		public async Task<(bool success, uint returnValue)> TryInvokeAsync(
			string export,
			ICpu cpu,
			VirtualMemory memory,
			CancellationToken cancellationToken = default)
		{
			_cpu = cpu;
			_memory = memory;
			var a = new StackArgs(cpu, memory);

			// Route APIs through async paths to avoid .GetAwaiter().GetResult()
			// which throws PlatformNotSupportedException on WASM
			switch (export.ToUpperInvariant())
			{
				case "DIRECTSOUNDCREATE":
					return (true, await DirectSoundCreateAsync(a.UInt32(0), a.UInt32(1), a.UInt32(2)).ConfigureAwait(false));
				case "DIRECTSOUNDENUMERATEA":
					return (true, await DirectSoundEnumerateAAsync(a.UInt32(0), a.UInt32(1), cancellationToken).ConfigureAwait(false));
			}

			// For all other APIs, use synchronous implementation
			if (TryInvokeUnsafe(export, cpu, memory, out var syncReturnValue))
			{
				return (true, syncReturnValue);
			}

			// No async work performed; return failure immediately
			return (false, 0);
		}

		[DllModuleExport(1, entryPoint: 0x0002C7DF, Version = "4.90.0.3000")]
		[DllModuleExport(1, entryPoint: 0x0000473B, Version = "5.1.2600.6532")]
		private uint DirectSoundCreate(uint lpGuid, uint lplpDs, uint pUnkOuter)
		{
			// Sync wrapper for non-WASM runtimes that support .GetAwaiter().GetResult()
			// On WASM, TryInvokeAsync routes directly to DirectSoundCreateAsync, bypassing this method
			if (PlatformHelpers.IsWasm)
			{
				_logger.LogError("[DSound] DirectSoundCreate called on WASM - should use async path");
				return (uint)DSResult.DSERR_GENERIC;
			}
			
			return DirectSoundCreateAsync(lpGuid, lplpDs, pUnkOuter).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Async implementation of DirectSoundCreate.
		/// </summary>
		private async Task<uint> DirectSoundCreateAsync(uint lpGuid, uint lplpDs, uint pUnkOuter)
		{
			_logger.LogInformation("[DSound] DirectSoundCreate(lpGuid=0x{LpGuid:X8}, lplpDS=0x{LplpDs:X8}, pUnkOuter=0x{PUnkOuter:X8})", lpGuid, lplpDs, pUnkOuter);

// Create DirectSound object with COM vtable
			var dsHandle = _nextDSoundHandle++;
			var dsObj = new DirectSoundObject
			{
				Handle = dsHandle,
				Frequency = 44100,
				BitsPerSample = 16,
				Channels = 2
			};
			_dsoundObjects[dsHandle] = dsObj;

// Initialize audio backend if not already done
			if (_env.AudioBackend == null && _env.BackendFactory != null)
			{
				_env.AudioBackend = _env.BackendFactory.CreateAudioBackend(_logger);
				var success = await _env.AudioBackend.InitializeAsync();
				if (!success)
				{
					_logger.LogError("[DSound] Failed to initialize audio backend");
					return (uint)DSResult.DSERR_GENERIC;
				}
				_logger.LogInformation("[DSound] Audio backend initialized successfully");
			}

// Create COM vtable for IDirectSound interface
			var vtableMethods = new List<KeyValuePair<string, Win32.COM.ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectSound.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))), // this + riid + ppvObject
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectSound.AddRef>((cpu, mem) => ComAddRef(cpu, mem))), // this only
				new("Release", ComVtableDispatcher.FromDelegate<IDirectSound.Release>((cpu, mem) => ComRelease(cpu, mem))), // this only
				new("CreateSoundBuffer", ComVtableDispatcher.FromDelegate<IDirectSound.CreateSoundBuffer>((cpu, mem) => DSound_CreateSoundBuffer(cpu, mem, dsHandle))), // this + pcDSBufferDesc + ppDSBuffer + pUnkOuter
				new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectSound.GetCaps>((cpu, mem) => DSound_GetCaps(cpu, mem))), // this + pDSCaps
				new("DuplicateSoundBuffer", ComVtableDispatcher.FromDelegate<IDirectSound.DuplicateSoundBuffer>((cpu, mem) => DSound_DuplicateSoundBuffer(cpu, mem))), // this + pDSBufferOriginal + ppDSBufferDuplicate
				new("SetCooperativeLevel", ComVtableDispatcher.FromDelegate<IDirectSound.SetCooperativeLevel>((cpu, mem) => DSound_SetCooperativeLevel(cpu, mem, dsHandle))), // this + hwnd + dwLevel
				new("Compact", ComVtableDispatcher.FromDelegate<IDirectSound.Compact>((cpu, mem) => DSound_Compact(cpu, mem))), // this only
				new("GetSpeakerConfig", ComVtableDispatcher.FromDelegate<IDirectSound.GetSpeakerConfig>((cpu, mem) => DSound_GetSpeakerConfig(cpu, mem))), // this + pdwSpeakerConfig
				new("SetSpeakerConfig", ComVtableDispatcher.FromDelegate<IDirectSound.SetSpeakerConfig>((cpu, mem) => DSound_SetSpeakerConfig(cpu, mem))), // this + dwSpeakerConfig
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectSound.Initialize>((cpu, mem) => DSound_Initialize(cpu, mem))) // this + pcGuidDevice
			};

// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectSound", vtableMethods);

// Write COM object pointer to output parameter
			if (lplpDs != 0)
			{
				_env.MemWrite32(lplpDs, comObjectAddr);
			}

			_logger.LogInformation("[DSound] Created IDirectSound COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(2, entryPoint: 0x0002D554, Version = "4.90.0.3000")]
		[DllModuleExport(2, entryPoint: 0x0002708D, Version = "5.1.2600.6532")]
		private uint DirectSoundEnumerateA(uint lpDsEnumCallback, uint lpContext)
		{
			// Sync wrapper for non-WASM runtimes that support .GetAwaiter().GetResult()
			// On WASM, TryInvokeAsync routes directly to DirectSoundEnumerateAAsync, bypassing this method
			_logger.LogInformation("[DSound] DirectSoundEnumerateA(lpDSEnumCallback=0x{LpDsEnumCallback:X8}, lpContext=0x{LpContext:X8})", lpDsEnumCallback, lpContext);

			// If no callback is provided, just return success
			if (lpDsEnumCallback == 0)
			{
				_logger.LogInformation("[DSound] DirectSoundEnumerateA: No callback provided");
				return (uint)DSResult.DS_OK;
			}

			return DirectSoundEnumerateAAsync(lpDsEnumCallback, lpContext).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Async implementation of DirectSoundEnumerateA.
		/// </summary>
		private async Task<uint> DirectSoundEnumerateAAsync(uint lpDsEnumCallback, uint lpContext, CancellationToken cancellationToken = default)
		{
			// Enumerate audio devices and call the callback for each one
			// For now, we'll enumerate at least one default device
			// The callback signature is: BOOL Callback(LPGUID lpGuid, LPCSTR lpcstrDescription, LPCSTR lpcstrModule, LPVOID lpContext)

			// Allocate strings for the default device
			var descriptionStr = "Primary Sound Driver";
			var moduleStr = "Primary Sound Driver";

			uint descriptionPtr = _env.WriteAnsiString(descriptionStr);
			uint modulePtr = _env.WriteAnsiString(moduleStr);

			// Call the callback with NULL GUID for the default device using async version
			bool continueEnum = await CallEnumerationCallbackAsync(lpDsEnumCallback, 0, descriptionPtr, modulePtr, lpContext, cancellationToken).ConfigureAwait(false);

			if (!continueEnum)
			{
				_logger.LogInformation("[DSound] DirectSoundEnumerateAAsync: Callback returned FALSE, stopping enumeration");
			}

			return (uint)DSResult.DS_OK;
		}

		/// <summary>
		/// Calls the DirectSound enumeration callback function.
		/// </summary>
		/// <returns>True if enumeration should continue, false otherwise</returns>
		private bool CallEnumerationCallback(uint callbackAddress, uint lpGuid, uint lpcstrDescription, uint lpcstrModule, uint lpContext)
		{
			if (_cpu == null || _memory == null)
			{
				_logger.LogWarning("[DSound] CallEnumerationCallback: CPU or Memory not available");
				return false;
			}

			_logger.LogInformation("[DSound] CallEnumerationCallback: Calling 0x{CallbackAddress:X8}", callbackAddress);

			// Save current CPU state
			var savedEip = _cpu.GetEip();
			var savedEsp = _cpu.GetRegister("ESP");
			var savedEbp = _cpu.GetRegister("EBP");

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			var esp = savedEsp;

			// Push return address (we'll use a special marker address)
			// Use a unique marker address that must never be mapped in the emulated address space.
			const uint RETURN_ADDRESS = 0xFFFFFFFF;
			esp -= 4;
			_memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			esp -= 4;
			_memory.Write32(esp, lpContext);

			esp -= 4;
			_memory.Write32(esp, lpcstrModule);

			esp -= 4;
			_memory.Write32(esp, lpcstrDescription);

			esp -= 4;
			_memory.Write32(esp, lpGuid);

			// Update CPU registers
			_cpu.SetRegister("ESP", esp);
			_cpu.SetEip(callbackAddress);

			// Execute until we hit the return address
			const int MAX_STEPS = 100000;
			var steps = 0;
			var returnValue = 0u;

			try
			{
				while (steps < MAX_STEPS)
				{
					var currentEip = _cpu.GetEip();
					if (currentEip == RETURN_ADDRESS)
					{
						// Callback returned, get return value from EAX
						returnValue = _cpu.GetRegister("EAX");
						break;
					}

					_cpu.SingleStep(_memory);
					steps++;
				}

				if (steps >= MAX_STEPS)
				{
					_logger.LogWarning("[DSound] CallEnumerationCallback: Exceeded max steps ({MaxSteps}), aborting", MAX_STEPS);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "[DSound] CallEnumerationCallback: Exception during execution: {ExMessage}", ex.Message);
			}
			finally
			{
				// Restore CPU state
				_cpu.SetEip(savedEip);
				_cpu.SetRegister("ESP", savedEsp);
				_cpu.SetRegister("EBP", savedEbp);
			}

			_logger.LogInformation("[DSound] CallEnumerationCallback: Completed with return value {ReturnValue}", returnValue);

			// Return TRUE means continue enumeration, FALSE means stop
			return returnValue != 0;
		}

		/// <summary>
		/// Async version of CallEnumerationCallback that eliminates the need for STACK_SAFETY_MARGIN.
		/// Uses async/await pattern for clean separation of host (C#) and guest (x86) execution stacks.
		/// </summary>
		/// <returns>True if enumeration should continue, false otherwise</returns>
		private async Task<bool> CallEnumerationCallbackAsync(
			uint callbackAddress, 
			uint lpGuid, 
			uint lpcstrDescription, 
			uint lpcstrModule, 
			uint lpContext,
			CancellationToken cancellationToken = default)
		{
			if (_cpu == null || _memory == null)
			{
				_logger.LogWarning("[DSound] CallEnumerationCallbackAsync: CPU or Memory not available");
				return false;
			}

			_logger.LogInformation("[DSound] CallEnumerationCallbackAsync: Calling 0x{CallbackAddress:X8}", callbackAddress);

			// Validate callback address
			if (callbackAddress == 0)
			{
				_logger.LogWarning("[DSound] CallEnumerationCallbackAsync: Callback address is NULL (0x00000000), aborting");
				return false;
			}

			// Save current CPU state
			var savedEip = _cpu.GetEip();
			var savedEsp = _cpu.GetRegister("ESP");
			var savedEbp = _cpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			// NOTE: No STACK_SAFETY_MARGIN needed! The async architecture provides clean stack separation.
			var esp = savedEsp;

			// Push return address first
			esp -= 4;
			_memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			esp -= 4;
			_memory.Write32(esp, lpContext);

			esp -= 4;
			_memory.Write32(esp, lpcstrModule);

			esp -= 4;
			_memory.Write32(esp, lpcstrDescription);

			esp -= 4;
			_memory.Write32(esp, lpGuid);

			// Update CPU registers
			_cpu.SetRegister("ESP", esp);
			_cpu.SetEip(callbackAddress);

			// Execute until we hit the return address with cancellation support
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var executionSuccessful = true;
			var lastCheckEip = _cpu.GetEip();
			var stuckCounter = 0;

			try
			{
				// Execute in unbounded loop with safeguards:
				// 1. Return detection: Break when EIP hits RETURN_ADDRESS marker
				// 2. Cancellation: Regular checks for cancellation requests
				// 3. Progress tracking: Detect stuck execution by monitoring EIP changes
				// 4. Yielding: Periodic Task.Yield() allows other async operations to proceed
				while (true)
				{
					// Check for cancellation at regular intervals
					if (steps % CANCELLATION_CHECK_INTERVAL == 0)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							_logger.LogInformation("[DSound] CallEnumerationCallbackAsync: Cancellation requested at step {Steps}", steps);
							executionSuccessful = false;
							break;
						}

						// Suspend execution to preserve CPU state across async boundary
						var cpuState = CpuHelpers.SuspendExecution(_cpu);
						
						// Yield to allow other async operations to proceed
						await Task.Yield();
						
						// Resume execution with preserved state
						CpuHelpers.ResumeExecution(_cpu, cpuState);
					}

					var eip = _cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogWarning("[DSound] CallEnumerationCallbackAsync: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting");
						executionSuccessful = false;
						break;
					}

					// Check for other invalid low addresses
					if (eip < MINIMUM_VALID_EIP && eip != RETURN_ADDRESS)
					{
						_logger.LogError("[DSound] CallEnumerationCallbackAsync: Execution jumped to invalid low address 0x{Eip:X8}", eip);
						executionSuccessful = false;
						break;
					}

					// Detect potential infinite loops
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = _cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								_logger.LogWarning("[DSound] CallEnumerationCallbackAsync: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting", 
									currentEip, stuckCounter);
								executionSuccessful = false;
								break;
							}
						}
						else
						{
							stuckCounter = 0;
							lastCheckEip = currentEip;
						}
					}

					// Execute instruction(s) - uses ExecuteBlockAsync for JIT CPUs, SingleStepAsync for interpreters
					await CpuHelpers.ExecuteAsync(_cpu, _memory);
					steps++;

					// Periodically yield for cooperative multitasking
					if (steps % YIELD_INTERVAL == 0)
					{
						await Task.Yield();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DSound] CallEnumerationCallbackAsync: Exception during execution: {ExMessage}", ex.Message);
				executionSuccessful = false;
			}

			// Get return value from EAX, but only if execution was successful
			var returnValue = executionSuccessful ? _cpu.GetRegister("EAX") : 0u;

			// Restore CPU state
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[DSound] CallEnumerationCallbackAsync: Completed with return value 0x{ReturnValue:X8}", returnValue);

			// Return TRUE means continue enumeration, FALSE means stop
			return returnValue != 0;
		}

		private sealed class DirectSoundObject
		{
			public uint Handle { get; set; }
			public int Frequency { get; set; } = 44100;
			public int BitsPerSample { get; set; } = 16;
			public int Channels { get; set; } = 2;
			public uint CooperativeLevel { get; set; }
			public uint WindowHandle { get; set; }
		}

		private sealed class DirectSoundBuffer
		{
			public uint Handle { get; set; }
			public uint AudioStreamId { get; set; }
			public int Size { get; set; }
			public byte[]? Data { get; set; }
			public bool IsPrimary { get; set; }
			public int Frequency { get; set; } = 44100;
			public int Channels { get; set; } = 2;
			public int BitsPerSample { get; set; } = 16;
			public int Volume { get; set; } = 0; // 0 = 0 dB (full volume) in DirectSound; negative values reduce volume
			public int Pan { get; set; } = 0; // 0 = center
			public uint PlayCursor { get; set; } = 0;
			public uint WriteCursor { get; set; } = 0;
			public bool IsPlaying { get; set; } = false;
			public bool IsLooping { get; set; } = false;
		}

		// COM interface methods for IDirectSound
		private uint ComQueryInterface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var riid = args.UInt32(1);
			var ppvObject = args.UInt32(2);

			_logger.LogInformation("[DSound COM] IUnknown::QueryInterface(this=0x{ThisPtr:X8}, riid=0x{Riid:X8}, ppvObject=0x{PpvObject:X8})", thisPtr, riid, ppvObject);

			return (uint)DSResult.DSERR_NOINTERFACE;
		}

		private uint ComAddRef(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DSound COM] IUnknown::AddRef(this=0x{ThisPtr:X8})", thisPtr);
			return 1; // Reference count
		}

		private uint ComRelease(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DSound COM] IUnknown::Release(this=0x{ThisPtr:X8})", thisPtr);
			return 0; // Reference count after release
		}

		private uint DSound_CreateSoundBuffer(ICpu cpu, VirtualMemory memory, uint dsHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pcDSBufferDesc = args.UInt32(1);
			var lplpDirectSoundBuffer = args.UInt32(2);
			var pUnkOuter = args.UInt32(3);

			_logger.LogInformation("[DSound COM] IDirectSound::CreateSoundBuffer(this=0x{ThisPtr:X8}, pcDSBufferDesc=0x{PcDsBufferDesc:X8}, lplpDSBuffer=0x{LplpDirectSoundBuffer:X8}, pUnkOuter=0x{PUnkOuter:X8})", thisPtr, pcDSBufferDesc, lplpDirectSoundBuffer, pUnkOuter);

			// Parse DSBUFFERDESC structure if provided
			var bufferSize = 0;
			var frequency = 44100;
			var channels = 2;
			var bitsPerSample = 16;
			var isPrimary = false;

			if (pcDSBufferDesc != 0)
			{
				// DSBUFFERDESC structure:
				// DWORD dwSize
				// DWORD dwFlags
				// DWORD dwBufferBytes
				// DWORD dwReserved
				// LPWAVEFORMATEX lpwfxFormat
				var dwSize = memory.Read32(pcDSBufferDesc);
				var dwFlags = memory.Read32(pcDSBufferDesc + 4);
				var dwBufferBytes = memory.Read32(pcDSBufferDesc + 8);
				var lpwfxFormat = memory.Read32(pcDSBufferDesc + 16);

				bufferSize = (int)dwBufferBytes;
				
				isPrimary = (dwFlags & (uint)DSBCapsFlags.PRIMARYBUFFER) != 0;

				_logger.LogInformation("[DSound COM] DSBUFFERDESC: size={DwSize}, flags={DwFlags}, bufferBytes={DwBufferBytes}, format=0x{LpwfxFormat:X8}", dwSize, (DSBCapsFlags)dwFlags, dwBufferBytes, lpwfxFormat);

				// Parse WAVEFORMATEX if provided and not primary buffer
				if (lpwfxFormat != 0 && !isPrimary)
				{
					// WAVEFORMATEX structure:
					// WORD wFormatTag
					// WORD nChannels
					// DWORD nSamplesPerSec
					// DWORD nAvgBytesPerSec
					// WORD nBlockAlign
					// WORD wBitsPerSample
					// WORD cbSize
					var wFormatTag = memory.Read16(lpwfxFormat);
					var nChannels = memory.Read16(lpwfxFormat + 2);
					var nSamplesPerSec = memory.Read32(lpwfxFormat + 4);
					var wBitsPerSample = memory.Read16(lpwfxFormat + 14);

					frequency = (int)nSamplesPerSec;
					channels = nChannels;
					bitsPerSample = wBitsPerSample;

					_logger.LogInformation("[DSound COM] WAVEFORMATEX: formatTag={WFormatTag}, channels={NChannels}, samplesPerSec={NSamplesPerSec}, bitsPerSample={WBitsPerSample}", wFormatTag, nChannels, nSamplesPerSec, wBitsPerSample);
				}
			}

			// Create a sound buffer COM object with its own vtable
			var bufferHandle = _nextBufferHandle++;
			var bufferObj = new DirectSoundBuffer
			{
				Handle = bufferHandle,
				Size = bufferSize,
				IsPrimary = isPrimary,
				Frequency = frequency,
				Channels = channels,
				BitsPerSample = bitsPerSample,
				Data = bufferSize > 0 ? new byte[bufferSize] : null
			};
			_buffers[bufferHandle] = bufferObj;

			// Create COM vtable for IDirectSoundBuffer interface
			var bufferMethods = new List<KeyValuePair<string, Win32.COM.ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectSound.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))), // this + riid + ppvObject
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectSound.AddRef>((cpu, mem) => ComAddRef(cpu, mem))), // this only
				new("Release", ComVtableDispatcher.FromDelegate<IDirectSound.Release>((cpu, mem) => ComRelease(cpu, mem))), // this only
				new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.GetCaps>((cpu, mem) => DSoundBuffer_GetCaps(cpu, mem))), // this + pDSBufferCaps
				new("GetCurrentPosition", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.GetCurrentPosition>((cpu, mem) => DSoundBuffer_GetCurrentPosition(cpu, mem))), // this + pdwCurrentPlayCursor + pdwCurrentWriteCursor
				new("GetFormat", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.GetFormat>((cpu, mem) => DSoundBuffer_GetFormat(cpu, mem))), // this + pwfxFormat + dwSizeAllocated + pdwSizeWritten
				new("GetVolume", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.GetVolume>((cpu, mem) => DSoundBuffer_GetVolume(cpu, mem))), // this + plVolume
				new("GetPan", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.GetPan>((cpu, mem) => DSoundBuffer_GetPan(cpu, mem))), // this + plPan
				new("GetFrequency", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.GetFrequency>((cpu, mem) => DSoundBuffer_GetFrequency(cpu, mem))), // this + pdwFrequency
				new("GetStatus", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.GetStatus>((cpu, mem) => DSoundBuffer_GetStatus(cpu, mem))), // this + pdwStatus
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.Initialize>((cpu, mem) => DSoundBuffer_Initialize(cpu, mem))), // this + pDirectSound + pcDSBufferDesc
				new("Lock", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.Lock>((cpu, mem) => DSoundBuffer_Lock(cpu, mem))), // this + dwOffset + dwBytes + ppvAudioPtr1 + pdwAudioBytes1 + ppvAudioPtr2 + pdwAudioBytes2 + dwFlags
				new("Play", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.Play>((cpu, mem) => DSoundBuffer_Play(cpu, mem))), // this + dwReserved1 + dwPriority + dwFlags
				new("SetCurrentPosition", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.SetCurrentPosition>((cpu, mem) => DSoundBuffer_SetCurrentPosition(cpu, mem))), // this + dwNewPosition
				new("SetFormat", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.SetFormat>((cpu, mem) => DSoundBuffer_SetFormat(cpu, mem))), // this + pcfxFormat
				new("SetVolume", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.SetVolume>((cpu, mem) => DSoundBuffer_SetVolume(cpu, mem))), // this + lVolume
				new("SetPan", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.SetPan>((cpu, mem) => DSoundBuffer_SetPan(cpu, mem))), // this + lPan
				new("SetFrequency", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.SetFrequency>((cpu, mem) => DSoundBuffer_SetFrequency(cpu, mem))), // this + dwFrequency
				new("Stop", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.Stop>((cpu, mem) => DSoundBuffer_Stop(cpu, mem))), // this only
				new("Unlock", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.Unlock>((cpu, mem) => DSoundBuffer_Unlock(cpu, mem))), // this + pvAudioPtr1 + dwAudioBytes1 + pvAudioPtr2 + dwAudioBytes2
				new("Restore", ComVtableDispatcher.FromDelegate<IDirectSoundBuffer.Restore>((cpu, mem) => DSoundBuffer_Restore(cpu, mem))) // this only
			};

			var bufferComAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectSoundBuffer", bufferMethods);

			// Store the mapping from COM object address to buffer handle
			_comObjectToBufferHandle[bufferComAddr] = bufferHandle;

			if (lplpDirectSoundBuffer != 0)
			{
				_env.MemWrite32(lplpDirectSoundBuffer, bufferComAddr);
			}

			_logger.LogInformation("[DSound COM] Created IDirectSoundBuffer COM object at 0x{BufferComAddr:X8} (handle=0x{BufferHandle:X8})", bufferComAddr, bufferHandle);
			return (uint)DSResult.DS_OK;
		}

		private uint DSound_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pDSCaps = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSound::GetCaps(this=0x{ThisPtr:X8}, pDSCaps=0x{PDSCaps:X8})", thisPtr, pDSCaps);

			if (pDSCaps == 0)
			{
				_logger.LogError("[DSound COM] IDirectSound::GetCaps: pDSCaps is NULL");
				return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			// Use the generated DSCAPS ref struct
			var caps = new DSCAPSRef(memory, pDSCaps);

			// Validate structure size (must be exactly 96 bytes)
			if (caps.dwSize != 96)
			{
				_logger.LogError("[DSound COM] IDirectSound::GetCaps: Invalid structure size {DwSize}, expected 96", caps.dwSize);
				return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			// Fill in device capabilities
			// We report software mixing capabilities
			caps.dwFlags = 0; // No special hardware capabilities
			caps.dwMinSecondarySampleRate = 100;        // Minimum sample rate
			caps.dwMaxSecondarySampleRate = 200000;     // Maximum sample rate
			caps.dwPrimaryBuffers = 1;                  // Always 1 primary buffer
			caps.dwMaxHwMixingAllBuffers = 0;           // No hardware mixing
			caps.dwMaxHwMixingStaticBuffers = 0;
			caps.dwMaxHwMixingStreamingBuffers = 0;
			caps.dwFreeHwMixingAllBuffers = 0;
			caps.dwFreeHwMixingStaticBuffers = 0;
			caps.dwFreeHwMixingStreamingBuffers = 0;
			caps.dwMaxHw3DAllBuffers = 0;               // No hardware 3D
			caps.dwMaxHw3DStaticBuffers = 0;
			caps.dwMaxHw3DStreamingBuffers = 0;
			caps.dwFreeHw3DAllBuffers = 0;
			caps.dwFreeHw3DStaticBuffers = 0;
			caps.dwFreeHw3DStreamingBuffers = 0;
			caps.dwTotalHwMemBytes = 0;                 // No hardware memory
			caps.dwFreeHwMemBytes = 0;
			caps.dwMaxContigFreeHwMemBytes = 0;
			caps.dwUnlockTransferRateHwBuffers = 0;     // Obsolete
			caps.dwPlayCpuOverheadSwBuffers = 0;        // Obsolete
			caps.dwReserved1 = 0;
			caps.dwReserved2 = 0;

			_logger.LogInformation("[DSound COM] IDirectSound::GetCaps: Returned software mixing capabilities");

			return (uint)DSResult.DS_OK;
		}

		private uint DSound_DuplicateSoundBuffer(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::DuplicateSoundBuffer() - stub");
			return (uint)DSResult.DS_OK;
		}

		private uint DSound_SetCooperativeLevel(ICpu cpu, VirtualMemory memory, uint dsHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var hwnd = args.UInt32(1);
			var dwLevel = args.UInt32(2);

			_logger.LogInformation("[DSound COM] IDirectSound::SetCooperativeLevel(this=0x{ThisPtr:X8}, hwnd=0x{Hwnd:X8}, level={Level})", 
				thisPtr, hwnd, (DSSCL)dwLevel);
			
			// Get the DirectSound object
			if (!_dsoundObjects.TryGetValue(dsHandle, out var dsObj))
			{
			    _logger.LogError("[DSound] SetCooperativeLevel: Invalid DirectSound handle 0x{DsHandle:X8}", dsHandle);
			    return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			// Store the cooperative level and window handle
			dsObj.CooperativeLevel = dwLevel;
			dsObj.WindowHandle = hwnd;

			// Ensure audio backend is initialized
			if (_env.AudioBackend == null)
			{
				_logger.LogWarning("[DSound] SetCooperativeLevel: Audio backend not initialized, initializing now");
				_env.AudioBackend = _env.BackendFactory?.CreateAudioBackend(_logger);
				if (PlatformHelpers.IsWasm)
				{
					_logger.LogError("[DSound] SetCooperativeLevel called on WASM before backend initialized - should use async path");
					return (uint)DSResult.DSERR_GENERIC;
				}
				
				if (!_env.AudioBackend!.InitializeAsync().GetAwaiter().GetResult())
				{
					_logger.LogError("[DSound] SetCooperativeLevel: Failed to initialize audio backend");
					return (uint)DSResult.DSERR_GENERIC;
				}
			}
			
			_logger.LogInformation("[DSound] Cooperative level set to {Level} for window 0x{Hwnd:X8}", (DSSCL)dwLevel, hwnd);
			
			return (uint)DSResult.DS_OK;
		}

		private uint DSound_Compact(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::Compact() - stub");
			return (uint)DSResult.DS_OK;
		}

		private uint DSound_GetSpeakerConfig(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pdwSpeakerConfig = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSound::GetSpeakerConfig(this=0x{ThisPtr:X8}, pdwSpeakerConfig=0x{PdwSpeakerConfig:X8})", thisPtr, pdwSpeakerConfig);

			if (pdwSpeakerConfig == 0)
			{
				_logger.LogError("[DSound COM] IDirectSound::GetSpeakerConfig: pdwSpeakerConfig is NULL");
				return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			// Return stereo speaker configuration as default
			memory.Write32(pdwSpeakerConfig, (uint)DSSpeakerConfig.DSSPEAKER_STEREO);

			_logger.LogInformation("[DSound COM] IDirectSound::GetSpeakerConfig: Returned STEREO configuration");

			return (uint)DSResult.DS_OK;
		}

		private uint DSound_SetSpeakerConfig(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwSpeakerConfig = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSound::SetSpeakerConfig(this=0x{ThisPtr:X8}, speakerConfig={SpeakerConfig})", 
				thisPtr, (DSSpeakerConfig)dwSpeakerConfig);

			// Accept the speaker configuration but don't actually change anything
			// Our implementation always uses stereo output
			_logger.LogInformation("[DSound COM] IDirectSound::SetSpeakerConfig: Accepted configuration (no-op)");

			return (uint)DSResult.DS_OK;
		}

		private uint DSound_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::Initialize() - stub");
			return (uint)DSResult.DS_OK;
		}

		// Helper method to get buffer from COM object address
		private DirectSoundBuffer? GetBufferFromThisPtr(uint thisPtr)
		{
			if (_comObjectToBufferHandle.TryGetValue(thisPtr, out var bufferHandle))
			{
				if (_buffers.TryGetValue(bufferHandle, out var buffer))
				{
					return buffer;
				}
			}
			return null;
		}

		// IDirectSoundBuffer COM methods
		private uint DSoundBuffer_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pDSBufferCaps = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetCaps(this=0x{ThisPtr:X8}, pDSBufferCaps=0x{PDSBufferCaps:X8})", thisPtr, pDSBufferCaps);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::GetCaps: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			if (pDSBufferCaps == 0)
			{
				_logger.LogError("[DSound COM] IDirectSoundBuffer::GetCaps: pDSBufferCaps is NULL");
				return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			// Use the generated DSBCAPS ref struct
			var caps = new DSBCAPSRef(memory, pDSBufferCaps);
			
			// Validate structure size (must be exactly 20 bytes)
			if (caps.dwSize != 20)
			{
			    _logger.LogError("[DSound COM] IDirectSoundBuffer::GetCaps: Invalid structure size {DwSize}, expected 20", caps.dwSize);
			    return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			// Set flags based on buffer properties
			DSBCapsFlags dwFlags = 0;
			
			if (buffer.IsPrimary)
			{
			    dwFlags |= DSBCapsFlags.PRIMARYBUFFER;
			    dwFlags |= DSBCapsFlags.LOCSOFTWARE;
			}
			else
			{
			    // Set common flags for software buffers with full control
			    dwFlags |= DSBCapsFlags.LOCSOFTWARE;
			    dwFlags |= DSBCapsFlags.CTRLFREQUENCY;
			    dwFlags |= DSBCapsFlags.CTRLPAN;
			    dwFlags |= DSBCapsFlags.CTRLVOLUME;
			    dwFlags |= DSBCapsFlags.GETCURRENTPOSITION2;
			}

			// Write capabilities structure using ref struct properties
			caps.dwFlags = (uint)dwFlags;
			caps.dwBufferBytes = (uint)buffer.Size;
			caps.dwUnlockTransferRate = 0; // Obsolete, not used
			caps.dwPlayCpuOverhead = 0; // Obsolete, not used

			_logger.LogInformation("[DSound] Buffer caps: flags={Flags}, size={Size}, isPrimary={IsPrimary}", 
				dwFlags, buffer.Size, buffer.IsPrimary);

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_GetCurrentPosition(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pdwCurrentPlayCursor = args.UInt32(1);
			var pdwCurrentWriteCursor = args.UInt32(2);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetCurrentPosition(this=0x{ThisPtr:X8})", thisPtr);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::GetCurrentPosition: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// Return current positions
			if (pdwCurrentPlayCursor != 0)
			{
				memory.Write32(pdwCurrentPlayCursor, buffer.PlayCursor);
			}
			if (pdwCurrentWriteCursor != 0)
			{
				memory.Write32(pdwCurrentWriteCursor, buffer.WriteCursor);
			}

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_GetFormat(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pwfxFormat = args.UInt32(1);
			var dwSizeAllocated = args.UInt32(2);
			var pdwSizeWritten = args.UInt32(3);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetFormat(this=0x{ThisPtr:X8}, format=0x{PwfxFormat:X8}, size={DwSizeAllocated})", thisPtr, pwfxFormat, dwSizeAllocated);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::GetFormat: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// WAVEFORMATEX structure size
			const uint WAVEFORMATEX_SIZE = 18;

			// Write the size needed
			if (pdwSizeWritten != 0)
			{
				memory.Write32(pdwSizeWritten, WAVEFORMATEX_SIZE);
			}

			// If buffer is provided and large enough, write the format
			if (pwfxFormat != 0 && dwSizeAllocated >= WAVEFORMATEX_SIZE)
			{
				// WAVEFORMATEX structure:
				// WORD wFormatTag (1 = PCM)
				// WORD nChannels
				// DWORD nSamplesPerSec
				// DWORD nAvgBytesPerSec
				// WORD nBlockAlign
				// WORD wBitsPerSample
				// WORD cbSize
				memory.Write16(pwfxFormat, 1); // WAVE_FORMAT_PCM
				memory.Write16(pwfxFormat + 2, (ushort)buffer.Channels);
				memory.Write32(pwfxFormat + 4, (uint)buffer.Frequency);
				var bytesPerSec = buffer.Frequency * buffer.Channels * (buffer.BitsPerSample / 8);
				memory.Write32(pwfxFormat + 8, (uint)bytesPerSec);
				var blockAlign = buffer.Channels * (buffer.BitsPerSample / 8);
				memory.Write16(pwfxFormat + 12, (ushort)blockAlign);
				memory.Write16(pwfxFormat + 14, (ushort)buffer.BitsPerSample);
				memory.Write16(pwfxFormat + 16, 0); // cbSize = 0 for PCM

				_logger.LogInformation("[DSound COM] GetFormat: Returned format {Channels}ch, {Frequency}Hz, {BitsPerSample}bit", buffer.Channels, buffer.Frequency, buffer.BitsPerSample);
			}

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_GetVolume(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var plVolume = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetVolume(this=0x{ThisPtr:X8})", thisPtr);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::GetVolume: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			if (plVolume != 0)
			{
				memory.Write32(plVolume, (uint)buffer.Volume);
			}

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_GetPan(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var plPan = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetPan(this=0x{ThisPtr:X8})", thisPtr);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::GetPan: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			if (plPan != 0)
			{
				memory.Write32(plPan, (uint)buffer.Pan);
			}

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_GetFrequency(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pdwFrequency = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetFrequency(this=0x{ThisPtr:X8})", thisPtr);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::GetFrequency: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			if (pdwFrequency != 0)
			{
				memory.Write32(pdwFrequency, (uint)buffer.Frequency);
			}

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_GetStatus(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pdwStatus = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetStatus(this=0x{ThisPtr:X8})", thisPtr);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::GetStatus: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// DirectSound buffer status flags:
			// DSBSTATUS_PLAYING = 0x00000001
			// DSBSTATUS_BUFFERLOST = 0x00000002
			// DSBSTATUS_LOOPING = 0x00000004
			uint status = 0;
			if (buffer.IsPlaying)
			{
				status |= (uint)DSBStatus.PLAYING;
			}
			if (buffer.IsLooping)
			{
				status |= (uint)DSBStatus.LOOPING;
			}

			if (pdwStatus != 0)
			{
				memory.Write32(pdwStatus, status);
			}

			_logger.LogInformation("[DSound COM] GetStatus: status=0x{Status:X8} (playing={IsPlaying}, looping={IsLooping})", status, buffer.IsPlaying, buffer.IsLooping);
			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_Initialize(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pDirectSound = args.UInt32(1);
			var pcDSBufferDesc = args.UInt32(2);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Initialize(this=0x{ThisPtr:X8}, pDirectSound=0x{PDirectSound:X8}, pcDSBufferDesc=0x{PcDSBufferDesc:X8})", 
				thisPtr, pDirectSound, pcDSBufferDesc);

			// Validate parameters
			if (pDirectSound == 0 || pcDSBufferDesc == 0)
			{
				_logger.LogError("[DSound COM] IDirectSoundBuffer::Initialize: NULL parameter");
				return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			// This method is only for use with CoCreateInstance
			// Since we create buffers through CreateSoundBuffer, they are always pre-initialized
			// Returning DSERR_ALREADYINITIALIZED is the correct and expected behavior per DirectSound spec
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Initialize: Buffer was created via CreateSoundBuffer (already initialized)");

			return (uint)DSResult.DSERR_ALREADYINITIALIZED;
		}

		private uint DSoundBuffer_Lock(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwOffset = args.UInt32(1);
			var dwBytes = args.UInt32(2);
			var ppvAudioPtr1 = args.UInt32(3);
			var pdwAudioBytes1 = args.UInt32(4);
			var ppvAudioPtr2 = args.UInt32(5);
			var pdwAudioBytes2 = args.UInt32(6);
			var dwFlags = args.UInt32(7);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Lock(this=0x{ThisPtr:X8}, offset={DwOffset}, bytes={DwBytes}, flags=0x{DwFlags:X8})", thisPtr, dwOffset, dwBytes, dwFlags);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null || buffer.Data == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::Lock: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// Check if entire buffer should be locked
			if ((dwFlags & (uint)DSBLock.ENTIREBUFFER) != 0)
			{
				dwOffset = 0;
				dwBytes = (uint)buffer.Size;
			}

			// Ensure we don't go beyond buffer size
			if (dwOffset + dwBytes > buffer.Size)
			{
				dwBytes = (uint)(buffer.Size - dwOffset);
			}

			// Allocate memory for the audio buffer pointer
			var audioPtr = _env.SimpleAlloc(dwBytes);
			
			// Copy current buffer data to the allocated memory
			if (buffer.Data != null && dwBytes > 0)
			{
				// Use a bulk memory copy for efficiency
				var span = new ReadOnlySpan<byte>(buffer.Data, (int)dwOffset, (int)dwBytes);
				memory.WriteBytes(audioPtr, span);
			}

			// Write the audio pointer and size to output parameters
			if (ppvAudioPtr1 != 0)
			{
				memory.Write32(ppvAudioPtr1, audioPtr);
			}
			if (pdwAudioBytes1 != 0)
			{
				memory.Write32(pdwAudioBytes1, dwBytes);
			}

			// Handle wraparound (for circular buffers) - write null for now
			if (ppvAudioPtr2 != 0)
			{
				memory.Write32(ppvAudioPtr2, 0);
			}
			if (pdwAudioBytes2 != 0)
			{
				memory.Write32(pdwAudioBytes2, 0);
			}

			// Store lock info for later Unlock call
			buffer.PlayCursor = dwOffset;
			buffer.WriteCursor = dwOffset;

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Lock: Locked buffer at 0x{AudioPtr:X8}, size={DwBytes}", audioPtr, dwBytes);
			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_Play(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwReserved1 = args.UInt32(1);
			var dwPriority = args.UInt32(2);
			var dwFlags = args.UInt32(3);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Play(this=0x{ThisPtr:X8}, priority={DwPriority}, flags=0x{DwFlags:X8})", thisPtr, dwPriority, dwFlags);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::Play: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// Don't play primary buffers
			if (buffer.IsPrimary)
			{
				_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Play: Primary buffer, nothing to do");
				return (uint)DSResult.DS_OK;
			}

			// Check if buffer should loop
			buffer.IsLooping = (dwFlags & (uint)DSBPlay.LOOPING) != 0;
			buffer.IsPlaying = true;

			// Create audio stream if not already created
			if (buffer.AudioStreamId == 0 && _env.AudioBackend != null)
			{
				buffer.AudioStreamId = _env.AudioBackend.CreateAudioStream(
					buffer.Frequency,
					buffer.Channels,
					buffer.Size
				);
				_logger.LogInformation("[DSound COM] Created audio stream {StreamId} for buffer", buffer.AudioStreamId);
			}

			// Write audio data to the backend if we have data
			if (buffer.AudioStreamId != 0 && buffer.Data != null && buffer.Data.Length > 0 && _env.AudioBackend != null)
			{
				_env.AudioBackend.WriteAudioData(buffer.AudioStreamId, buffer.Data, 0, buffer.Data.Length);
				_logger.LogInformation("[DSound COM] Wrote {Length} bytes of audio data to stream {StreamId}", buffer.Data.Length, buffer.AudioStreamId);
			}

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Play: Started playback (looping={IsLooping})", buffer.IsLooping);
			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_SetCurrentPosition(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwNewPosition = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetCurrentPosition(this=0x{ThisPtr:X8}, position={DwNewPosition})", thisPtr, dwNewPosition);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::SetCurrentPosition: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// Don't allow setting position on playing buffers
			if (buffer.IsPlaying)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::SetCurrentPosition: Cannot set position while playing");
				return (uint)DSResult.DSERR_INVALIDCALL;
			}

			// Ensure position is within buffer bounds
			if (dwNewPosition >= buffer.Size)
			{
				_logger.LogError("[DSound COM] IDirectSoundBuffer::SetCurrentPosition: Position {DwNewPosition} exceeds buffer size {Size}", dwNewPosition, buffer.Size);
				return (uint)DSResult.DSERR_INVALIDPARAM;
			}

			buffer.PlayCursor = dwNewPosition;
			buffer.WriteCursor = dwNewPosition;

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetCurrentPosition: Set position to {DwNewPosition}", dwNewPosition);

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_SetFormat(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pcfxFormat = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetFormat(this=0x{ThisPtr:X8}, format=0x{PcfxFormat:X8})", thisPtr, pcfxFormat);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::SetFormat: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// Only primary buffers can have their format set
			if (!buffer.IsPrimary)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::SetFormat: Can only set format on primary buffer");
				return (uint)DSResult.DSERR_BADFORMAT;
			}

			// Parse WAVEFORMATEX if provided
			if (pcfxFormat != 0)
			{
				var wFormatTag = memory.Read16(pcfxFormat);
				var nChannels = memory.Read16(pcfxFormat + 2);
				var nSamplesPerSec = memory.Read32(pcfxFormat + 4);
				var wBitsPerSample = memory.Read16(pcfxFormat + 14);

				buffer.Frequency = (int)nSamplesPerSec;
				buffer.Channels = nChannels;
				buffer.BitsPerSample = wBitsPerSample;

				_logger.LogInformation("[DSound COM] SetFormat: Set format to {Channels}ch, {Frequency}Hz, {BitsPerSample}bit", buffer.Channels, buffer.Frequency, buffer.BitsPerSample);
			}

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_SetVolume(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lVolume = args.Int32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetVolume(this=0x{ThisPtr:X8}, volume={LVolume})", thisPtr, lVolume);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::SetVolume: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			buffer.Volume = lVolume;

			// Convert DirectSound volume (in hundredths of decibels, typically -10000 to 0) to 0.0-1.0 range using exponential scaling
			// DirectSound: 0 = full volume, -10000 = silence
			var normalizedVolume = lVolume <= -10000 ? 0.0f : (float)Math.Pow(10.0, lVolume / 2000.0);

			if (buffer.AudioStreamId != 0 && _env.AudioBackend != null)
			{
				_env.AudioBackend.SetStreamVolume(buffer.AudioStreamId, normalizedVolume);
			}

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_SetPan(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lPan = args.Int32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetPan(this=0x{ThisPtr:X8}, pan={LPan})", thisPtr, lPan);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::SetPan: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			buffer.Pan = lPan;
			// Pan is typically -10000 (full left) to +10000 (full right), with 0 being center
			// For now we just store it, full implementation would require stereo positioning in OpenAL

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_SetFrequency(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFrequency = args.UInt32(1);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetFrequency(this=0x{ThisPtr:X8}, frequency={DwFrequency})", thisPtr, dwFrequency);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::SetFrequency: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			buffer.Frequency = (int)dwFrequency;
			// Changing frequency would require recreating the audio stream in OpenAL
			// For now we just store it

			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_Stop(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Stop(this=0x{ThisPtr:X8})", thisPtr);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::Stop: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			buffer.IsPlaying = false;

			// Pause the audio stream if it exists
			if (buffer.AudioStreamId != 0 && _env.AudioBackend != null)
			{
				_env.AudioBackend.SetStreamPaused(buffer.AudioStreamId, true);
				_logger.LogInformation("[DSound COM] Paused audio stream {StreamId}", buffer.AudioStreamId);
			}

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Stop: Stopped playback");
			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_Unlock(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pvAudioPtr1 = args.UInt32(1);
			var dwAudioBytes1 = args.UInt32(2);
			var pvAudioPtr2 = args.UInt32(3);
			var dwAudioBytes2 = args.UInt32(4);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Unlock(this=0x{ThisPtr:X8}, ptr1=0x{PvAudioPtr1:X8}, bytes1={DwAudioBytes1})", thisPtr, pvAudioPtr1, dwAudioBytes1);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null || buffer.Data == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::Unlock: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// Copy data from the locked memory region back to the buffer
			if (pvAudioPtr1 != 0 && dwAudioBytes1 > 0)
			{
				var offset = buffer.WriteCursor;
				var bytesToCopy1 = (int)Math.Min(dwAudioBytes1, buffer.Size - offset);
				if (bytesToCopy1 > 0)
				{
					var temp = memory.GetSpan(pvAudioPtr1, bytesToCopy1);
					Buffer.BlockCopy(temp, 0, buffer.Data, (int)offset, bytesToCopy1);
				}
			}

			// Handle second buffer region if present (wraparound)
			if (pvAudioPtr2 != 0 && dwAudioBytes2 > 0)
			{
				var bytesToCopy2 = (int)Math.Min(dwAudioBytes2, buffer.Size);
				if (bytesToCopy2 > 0)
				{
					var temp = memory.GetSpan(pvAudioPtr2, bytesToCopy2);
					temp.CopyTo(new Span<byte>(buffer.Data, 0, bytesToCopy2));
				}
			}

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Unlock: Unlocked buffer");
			return (uint)DSResult.DS_OK;
		}

		private uint DSoundBuffer_Restore(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Restore(this=0x{ThisPtr:X8})", thisPtr);

			var buffer = GetBufferFromThisPtr(thisPtr);
			if (buffer == null)
			{
				_logger.LogWarning("[DSound COM] IDirectSoundBuffer::Restore: Invalid buffer");
				return (uint)DSResult.DSERR_GENERIC;
			}

			// In a real implementation, this would restore a lost hardware buffer
			// Since we use software buffers, they never get lost
			// Just return success
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Restore: Software buffer, nothing to restore");

			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(3, entryPoint: 0x0002D571, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(3, entryPoint: 0x000270AA, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectSoundEnumerateW(uint lpDSEnumCallback, uint lpContext)
		{
			_logger.LogWarning("[dsound] DirectSoundEnumerateW: lpDSEnumCallback={lpDSEnumCallback}, lpContext={lpContext}", lpDSEnumCallback, lpContext);
			// TODO: Implement DirectSoundEnumerateW
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(4, entryPoint: 0x00035E9D, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(4, entryPoint: 0x0002BE61, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllCanUnloadNow()
		{
			_logger.LogWarning("[dsound] DllCanUnloadNow called");
			
			// Check if there are any active DirectSound objects or buffers
			if (_dsoundObjects.Count > 0 || _buffers.Count > 0)
			{
				_logger.LogInformation("[dsound] DllCanUnloadNow: Cannot unload - {DsCount} DirectSound objects and {BufferCount} buffers active", 
					_dsoundObjects.Count, _buffers.Count);
				return 1; // S_FALSE - cannot unload
			}
			
			_logger.LogInformation("[dsound] DllCanUnloadNow: Can unload - no active objects");
			return 0; // S_OK - can unload
		}

		[DllModuleExport(5, entryPoint: 0x00036A41, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(5, entryPoint: 0x000109C5, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllGetClassObject()
		{
			_logger.LogWarning("[dsound] DllGetClassObject called (not implemented)");
			// Class factory not implemented - DirectSound objects are created directly via DirectSoundCreate
			return (uint)DDResult.CLASS_E_NOAGGREGATION; // 0x80040110
		}

		[DllModuleExport(6, entryPoint: 0x0002C95C, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(6, entryPoint: 0x000268BB, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectSoundCaptureCreate(uint pcGuidDevice, uint ppDSC, uint pUnkOuter)
		{
			_logger.LogWarning("[dsound] DirectSoundCaptureCreate: pcGuidDevice={pcGuidDevice}, ppDSC=0x{ppDSC:X8}, pUnkOuter={pUnkOuter}", pcGuidDevice, ppDSC, pUnkOuter);
			// TODO: Implement DirectSoundCaptureCreate
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(7, entryPoint: 0x0002D58E, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(7, entryPoint: 0x000270C7, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectSoundCaptureEnumerateA(uint lpDSEnumCallback, uint lpContext)
		{
			_logger.LogWarning("[dsound] DirectSoundCaptureEnumerateA: lpDSEnumCallback={lpDSEnumCallback}, lpContext={lpContext}", lpDSEnumCallback, lpContext);
			// TODO: Implement DirectSoundCaptureEnumerateA
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(8, entryPoint: 0x0002D5AB, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(8, entryPoint: 0x000270E4, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectSoundCaptureEnumerateW(uint lpDSEnumCallback, uint lpContext)
		{
			_logger.LogWarning("[dsound] DirectSoundCaptureEnumerateW: lpDSEnumCallback={lpDSEnumCallback}, lpContext={lpContext}", lpDSEnumCallback, lpContext);
			// TODO: Implement DirectSoundCaptureEnumerateW
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(9, entryPoint: 0x0002CDE2, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(9, entryPoint: 0x00026D42, Version = "5.1.2600.6532", IsStub = true)]
		public uint GetDeviceID(uint pGuidSrc, uint pGuidDest)
		{
			_logger.LogWarning("[dsound] GetDeviceID: pGuidSrc={pGuidSrc}, pGuidDest={pGuidDest}", pGuidSrc, pGuidDest);
			// TODO: Implement GetDeviceID
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(10, entryPoint: 0x0002CAD3, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(10, entryPoint: 0x00026A32, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectSoundFullDuplexCreate(uint pcGuidCaptureDevice, uint pcGuidRenderDevice, uint pcDSCBufferDesc, uint pcDSBufferDesc, uint hWnd, uint dwLevel, uint ppDSFD, uint ppDSCBuffer8, uint ppDSBuffer8, uint pUnkOuter)
		{
			_logger.LogWarning("[dsound] DirectSoundFullDuplexCreate: pcGuidCaptureDevice={pcGuidCaptureDevice}, pcGuidRenderDevice={pcGuidRenderDevice}, pcDSCBufferDesc={pcDSCBufferDesc}, pcDSBufferDesc={pcDSBufferDesc}, hWnd=0x{hWnd:X8}, dwLevel=0x{dwLevel:X8}, ppDSFD=0x{ppDSFD:X8}, ppDSCBuffer8=0x{ppDSCBuffer8:X8}, ppDSBuffer8=0x{ppDSBuffer8:X8}, pUnkOuter={pUnkOuter}", pcGuidCaptureDevice, pcGuidRenderDevice, pcDSCBufferDesc, pcDSBufferDesc, hWnd, dwLevel, ppDSFD, ppDSCBuffer8, ppDSBuffer8, pUnkOuter);
			// TODO: Implement DirectSoundFullDuplexCreate
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(11, entryPoint: 0x0002C896, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(11, entryPoint: 0x000267F5, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectSoundCreate8(uint lpcGuidDevice, uint ppDS8, uint pUnkOuter)
		{
			_logger.LogWarning("[dsound] DirectSoundCreate8: lpcGuidDevice={lpcGuidDevice}, ppDS8=0x{ppDS8:X8}, pUnkOuter={pUnkOuter}", lpcGuidDevice, ppDS8, pUnkOuter);
			// TODO: Implement DirectSoundCreate8
			return (uint)DSResult.DS_OK;
		}

		[DllModuleExport(12, entryPoint: 0x0002CA10, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(12, entryPoint: 0x0002696F, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectSoundCaptureCreate8(uint lpcGUID, uint lplpDSC, uint pUnkOuter)
		{
			_logger.LogWarning("[dsound] DirectSoundCaptureCreate8: lpcGUID={lpcGUID}, lplpDSC=0x{lplpDSC:X8}, pUnkOuter={pUnkOuter}", lpcGUID, lplpDSC, pUnkOuter);
			// TODO: Implement DirectSoundCaptureCreate8
			return (uint)DSResult.DS_OK;
		}
	}
}