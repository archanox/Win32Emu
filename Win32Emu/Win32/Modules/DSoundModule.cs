using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules
{
	public class DSoundModule : IWin32ModuleUnsafe
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
		private uint _nextDSoundHandle = 0x80000000;
		private uint _nextBufferHandle = 0x81000000;
		private ICpu? _cpu;
		private VirtualMemory? _memory;

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
		
		[DllModuleExport(1)]
		private uint DirectSoundCreate(uint lpGuid, uint lplpDs, uint pUnkOuter)
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
			if (_env.AudioBackend == null)
			{
				_env.AudioBackend = Rendering.BackendFactory.CreateAudioBackend(_logger);
				_env.AudioBackend.Initialize();
			}

// Create COM vtable for IDirectSound interface
			var vtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "CreateSoundBuffer", (cpu, mem) => DSound_CreateSoundBuffer(cpu, mem, dsHandle) },
				{ "GetCaps", (cpu, mem) => DSound_GetCaps(cpu, mem) },
				{ "DuplicateSoundBuffer", (cpu, mem) => DSound_DuplicateSoundBuffer(cpu, mem) },
				{ "SetCooperativeLevel", (cpu, mem) => DSound_SetCooperativeLevel(cpu, mem) },
				{ "Compact", (cpu, mem) => DSound_Compact(cpu, mem) },
				{ "GetSpeakerConfig", (cpu, mem) => DSound_GetSpeakerConfig(cpu, mem) },
				{ "SetSpeakerConfig", (cpu, mem) => DSound_SetSpeakerConfig(cpu, mem) },
				{ "Initialize", (cpu, mem) => DSound_Initialize(cpu, mem) }
			};

// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectSound", vtableMethods);

// Write COM object pointer to output parameter
			if (lplpDs != 0)
			{
				_env.MemWrite32(lplpDs, comObjectAddr);
			}

			_logger.LogInformation("[DSound] Created IDirectSound COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
			return 0; // DS_OK
		}
		
		[DllModuleExport(1)]
		private uint DirectSoundEnumerateA(uint lpDsEnumCallback, uint lpContext)
		{
			_logger.LogInformation("[DSound] DirectSoundEnumerateA(lpDSEnumCallback=0x{LpDsEnumCallback:X8}, lpContext=0x{LpContext:X8})", lpDsEnumCallback, lpContext);

			// If no callback is provided, just return success
			if (lpDsEnumCallback == 0)
			{
				_logger.LogInformation("[DSound] DirectSoundEnumerateA: No callback provided");
				return 0; // DS_OK
			}

			// Enumerate audio devices and call the callback for each one
			// For now, we'll enumerate at least one default device
			// The callback signature is: BOOL Callback(LPGUID lpGuid, LPCSTR lpcstrDescription, LPCSTR lpcstrModule, LPVOID lpContext)
			
			// Allocate strings for the default device
			var descriptionStr = "Primary Sound Driver";
			var moduleStr = "Primary Sound Driver";
			
			uint descriptionPtr = _env.WriteAnsiString(descriptionStr);
			uint modulePtr = _env.WriteAnsiString(moduleStr);
			
			// Call the callback with NULL GUID for the default device
			bool continueEnum = CallEnumerationCallback(lpDsEnumCallback, 0, descriptionPtr, modulePtr, lpContext);
			
			if (!continueEnum)
			{
				_logger.LogInformation("[DSound] DirectSoundEnumerateA: Callback returned FALSE, stopping enumeration");
			}

			return 0; // DS_OK
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
			const uint RETURN_ADDRESS = 0xDEADBEEF;
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

		private sealed class DirectSoundObject
		{
			public uint Handle { get; set; }
			public int Frequency { get; set; } = 44100;
			public int BitsPerSample { get; set; } = 16;
			public int Channels { get; set; } = 2;
		}

		private sealed class DirectSoundBuffer
		{
			public uint Handle { get; set; }
			public uint AudioStreamId { get; set; }
			public int Size { get; set; }
			public byte[]? Data { get; set; }
			public bool IsPrimary { get; set; }
		}

		// COM interface methods for IDirectSound
		private uint ComQueryInterface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var riid = args.UInt32(1);
			var ppvObject = args.UInt32(2);

			_logger.LogInformation("[DSound COM] IUnknown::QueryInterface(this=0x{ThisPtr:X8}, riid=0x{Riid:X8}, ppvObject=0x{PpvObject:X8})", thisPtr, riid, ppvObject);

			// E_NOINTERFACE = 0x80004002
			return 0x80004002;
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

			// Create a sound buffer COM object with its own vtable
			var bufferHandle = _nextBufferHandle++;
			var bufferObj = new DirectSoundBuffer
			{
				Handle = bufferHandle,
				Size = 0,
				IsPrimary = false
			};
			_buffers[bufferHandle] = bufferObj;

			// Create COM vtable for IDirectSoundBuffer interface
			var bufferMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "GetCaps", (cpu, mem) => DSoundBuffer_GetCaps(cpu, mem) },
				{ "GetCurrentPosition", (cpu, mem) => DSoundBuffer_GetCurrentPosition(cpu, mem) },
				{ "GetFormat", (cpu, mem) => DSoundBuffer_GetFormat(cpu, mem) },
				{ "GetVolume", (cpu, mem) => DSoundBuffer_GetVolume(cpu, mem) },
				{ "GetPan", (cpu, mem) => DSoundBuffer_GetPan(cpu, mem) },
				{ "GetFrequency", (cpu, mem) => DSoundBuffer_GetFrequency(cpu, mem) },
				{ "GetStatus", (cpu, mem) => DSoundBuffer_GetStatus(cpu, mem) },
				{ "Initialize", (cpu, mem) => DSoundBuffer_Initialize(cpu, mem) },
				{ "Lock", (cpu, mem) => DSoundBuffer_Lock(cpu, mem) },
				{ "Play", (cpu, mem) => DSoundBuffer_Play(cpu, mem) },
				{ "SetCurrentPosition", (cpu, mem) => DSoundBuffer_SetCurrentPosition(cpu, mem) },
				{ "SetFormat", (cpu, mem) => DSoundBuffer_SetFormat(cpu, mem) },
				{ "SetVolume", (cpu, mem) => DSoundBuffer_SetVolume(cpu, mem) },
				{ "SetPan", (cpu, mem) => DSoundBuffer_SetPan(cpu, mem) },
				{ "SetFrequency", (cpu, mem) => DSoundBuffer_SetFrequency(cpu, mem) },
				{ "Stop", (cpu, mem) => DSoundBuffer_Stop(cpu, mem) },
				{ "Unlock", (cpu, mem) => DSoundBuffer_Unlock(cpu, mem) },
				{ "Restore", (cpu, mem) => DSoundBuffer_Restore(cpu, mem) }
			};

			var bufferComAddr = _env.ComDispatcher.CreateComObject("IDirectSoundBuffer", bufferMethods);

			if (lplpDirectSoundBuffer != 0)
			{
				_env.MemWrite32(lplpDirectSoundBuffer, bufferComAddr);
			}

			_logger.LogInformation("[DSound COM] Created IDirectSoundBuffer COM object at 0x{BufferComAddr:X8}", bufferComAddr);
			return 0; // DS_OK
		}

		private uint DSound_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::GetCaps() - stub");
			return 0; // DS_OK
		}

		private uint DSound_DuplicateSoundBuffer(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::DuplicateSoundBuffer() - stub");
			return 0; // DS_OK
		}

		private uint DSound_SetCooperativeLevel(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var hwnd = args.UInt32(1);
			var dwLevel = args.UInt32(2);

			_logger.LogInformation("[DSound COM] IDirectSound::SetCooperativeLevel(this=0x{ThisPtr:X8}, hwnd=0x{Hwnd:X8}, level=0x{DwLevel:X8}) - stub", thisPtr, hwnd, dwLevel);
			return 0; // DS_OK
		}

		private uint DSound_Compact(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::Compact() - stub");
			return 0; // DS_OK
		}

		private uint DSound_GetSpeakerConfig(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::GetSpeakerConfig() - stub");
			return 0; // DS_OK
		}

		private uint DSound_SetSpeakerConfig(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::SetSpeakerConfig() - stub");
			return 0; // DS_OK
		}

		private uint DSound_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::Initialize() - stub");
			return 0; // DS_OK
		}

		// IDirectSoundBuffer COM methods
		private uint DSoundBuffer_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetCaps() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetCurrentPosition(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetCurrentPosition() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetFormat(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetFormat() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetVolume(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetVolume() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetPan(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetPan() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetFrequency(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetFrequency() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetStatus(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetStatus() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Initialize() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Lock(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Lock() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Play(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Play() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetCurrentPosition(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetCurrentPosition() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetFormat(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetFormat() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetVolume(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetVolume() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetPan(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetPan() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetFrequency(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetFrequency() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Stop(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Stop() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Unlock(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Unlock() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Restore(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Restore() - stub");
			return 0; // DS_OK
		}
	}
}