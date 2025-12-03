using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Threading;
using Win32Emu.Win32.COM;
using Win32Emu.Win32.DirectDraw;
using static Win32Emu.Win32.NativeTypes;

namespace Win32Emu.Win32.Modules
{
	public class DDrawModule : IWin32ModuleAsync
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		// Callback execution constants for WASM responsiveness
		// Lower yield interval ensures browser remains responsive during callback execution
		private const int CALLBACK_YIELD_INTERVAL = 10;
		
		// Callback timeout prevents indefinite browser freezing
		// Most callbacks should complete in <100ms; this is a safety net for pathological cases
		private const int CALLBACK_TIMEOUT_MS = 5000;

		// Temporary storage for CPU and memory during callbacks
		// These are set at the start of TryInvokeUnsafe and used by export functions
		private ICpu? _currentCpu;
		private VirtualMemory? _currentMemory;

		public DDrawModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "DDRAW.DLL";

		// DirectDraw object handles
		private readonly Dictionary<uint, DirectDrawObject> _ddrawObjects = new();
		private readonly Dictionary<uint, uint> _comObjectToHandle = new(); // Maps COM object address to ddraw handle
		private readonly Dictionary<uint, DirectDrawSurface> _surfaces = new();
		private readonly Dictionary<uint, DirectDrawPalette> _palettes = new();
		private readonly Dictionary<uint, DirectDrawClipper> _clippers = new();
		private readonly Dictionary<uint, uint> _surfaceDCs = new(); // Maps DC handle to surface COM object address
		private uint _nextDDrawHandle = 0x70000000;
		private uint _nextSurfaceHandle = 0x71000000;
		private uint _nextPaletteHandle = 0x72000000;
		private uint _nextClipperHandle = 0x73000000;
		private uint _nextDCHandle = 0x74000000;

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;

			// Store CPU and memory for use by export functions
			_currentCpu = cpu;
			_currentMemory = memory;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTDRAWCREATE":
					returnValue = DirectDrawCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "DIRECTDRAWCREATEEX":
					returnValue = DirectDrawCreateEx(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "DIRECTDRAWENUMERATEEXA":
					returnValue = DirectDrawEnumerateExA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				default:
					_logger.LogInformation("[DDraw] Unimplemented export: {Export}", export);
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
			_currentCpu = cpu;
			_currentMemory = memory;
			var a = new StackArgs(cpu, memory);

			// Route APIs with callbacks through async paths to avoid .GetAwaiter().GetResult()
			// which throws PlatformNotSupportedException on WASM
			switch (export.ToUpperInvariant())
			{
				case "DIRECTDRAWENUMERATEA":
					return (true, await DirectDrawEnumerateAAsync(a.UInt32(0), a.UInt32(1), cancellationToken).ConfigureAwait(false));
				case "DIRECTDRAWENUMERATEEXA":
					return (true, await DirectDrawEnumerateExAAsync(a.UInt32(0), a.UInt32(1), a.UInt32(2), cancellationToken).ConfigureAwait(false));
				case "DIRECTDRAWENUMERATEW":
					return (true, await DirectDrawEnumerateWAsync(a.UInt32(0), a.UInt32(1), cancellationToken).ConfigureAwait(false));
				case "DIRECTDRAWENUMERATEEXW":
					return (true, await DirectDrawEnumerateExWAsync(a.UInt32(0), a.UInt32(1), a.UInt32(2), cancellationToken).ConfigureAwait(false));
			}

			// For all other APIs, use synchronous implementation
			if (TryInvokeUnsafe(export, cpu, memory, out var syncReturnValue))
			{
				return (true, syncReturnValue);
			}

			// No async work performed; return failure immediately
			return (false, 0);
		}

		/// <summary>
		/// </summary>
		/// <param name="lpGuid">A pointer to the globally unique identifier (GUID) that represents the driver to be created. This can be NULL to indicate the active display driver, or you can pass one of the following flags to restrict the active display driver's behavior for debugging purposes:
		/// DDCREATE_EMULATIONONLY
		///	The DirectDraw object uses emulation for all features; it does not take advantage of any hardware-supported features.
		///	DDCREATE_HARDWAREONLY
		/// The DirectDraw object never emulates features not supported by the hardware.Attempts to call methods that require unsupported features fail, returning DDERR_UNSUPPORTED.</param>
		/// <param name="lplpDd">A pointer to a variable to be set to a valid IDirectDraw interface pointer if the call succeeds.</param>
		/// <param name="pUnkOuter">Allows for future compatibility with COM aggregation features. Presently, however, this function returns an error if this parameter is anything but NULL.</param>
		/// <returns>If the function succeeds, the return value is DD_OK.
		/// If it fails, the function can return one of the following error values:
		/// DDERR_DIRECTDRAWALREADYCREATED
		///	DDERR_GENERIC
		/// DDERR_INVALIDDIRECTDRAWGUID
		///	DDERR_INVALIDPARAMS
		/// DDERR_NODIRECTDRAWHW
		///	DDERR_OUTOFMEMORY</returns>
		[DllModuleExport(31, entryPoint: 0x0001DDA5, Version = "4.90.0.3000")]
		[DllModuleExport(9, entryPoint: 0x0002CCA3, Version = "5.1.2600.6532")]
		private uint DirectDrawCreate(uint lpGuid, uint lplpDd, uint pUnkOuter)
		{
			// Fixed: Parameter order now matches MSDN documentation
			// Win32 API: DirectDrawCreate(GUID *lpGUID, LPDIRECTDRAW *lplpDD, IUnknown *pUnkOuter)
			_logger.LogInformation("[DDraw] DirectDrawCreate(lpGuid=0x{LpGuid:X8}, lplpDD=0x{LplpDd:X8}, pUnkOuter=0x{PUnkOuter:X8})", lpGuid, lplpDd, pUnkOuter);

			// Validate output pointer parameter
			if (lplpDd == 0)
			{
				_logger.LogError("[DDraw] DirectDrawCreate: lplpDD is NULL");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Detect if lplpDD looks like a stack pointer (potential parameter handling bug)
			// Check against actual stack range from PE headers
			if (lplpDd >= _env.StackLimit && lplpDd < _env.StackBase)
			{
				_logger.LogWarning("[DDraw] DirectDrawCreate: lplpDD=0x{LplpDd:X8} appears to be a stack address (stack range: 0x{StackLimit:X8}-0x{StackBase:X8}) - this might indicate a parameter handling issue",
					lplpDd, _env.StackLimit, _env.StackBase);
			}

			// Create DirectDraw object with COM vtable
			var ddrawHandle = _nextDDrawHandle++;
			var ddrawObj = new DirectDrawObject
			{
				Handle = ddrawHandle,
				Width = 640,
				Height = 480,
				BitsPerPixel = 16
			};
			_ddrawObjects[ddrawHandle] = ddrawObj;

			// Create COM vtable for IDirectDraw interface
			var vtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
				new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>((cpu, mem) => ComRelease(cpu, mem))),
				new("Compact", ComVtableDispatcher.FromDelegate<IDirectDraw.Compact>((cpu, mem) => DDraw_Compact(cpu, mem))),
				new("CreateClipper", ComVtableDispatcher.FromDelegate<IDirectDraw.CreateClipper>((cpu, mem) => DDraw_CreateClipper(cpu, mem))),
				new("CreatePalette", ComVtableDispatcher.FromDelegate<IDirectDraw.CreatePalette>((cpu, mem) => DDraw_CreatePalette(cpu, mem))),
				new("CreateSurface", ComVtableDispatcher.FromDelegate<IDirectDraw.CreateSurface>((cpu, mem) => DDraw_CreateSurface(cpu, mem))),
				new("DuplicateSurface", ComVtableDispatcher.FromDelegate<IDirectDraw.DuplicateSurface>((cpu, mem) => DDraw_DuplicateSurface(cpu, mem))),
				new("EnumDisplayModes", ComVtableDispatcher.FromDelegate<IDirectDraw.EnumDisplayModes>((cpu, mem) => DDraw_EnumDisplayModes(cpu, mem))),
				new("EnumSurfaces", ComVtableDispatcher.FromDelegate<IDirectDraw.EnumSurfaces>((cpu, mem) => DDraw_EnumSurfaces(cpu, mem))),
				new("FlipToGDISurface", ComVtableDispatcher.FromDelegate<IDirectDraw.FlipToGDISurface>((cpu, mem) => DDraw_FlipToGDISurface(cpu, mem))),
				new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectDraw.GetCaps>((cpu, mem) => DDraw_GetCaps(cpu, mem))),
				new("GetDisplayMode", ComVtableDispatcher.FromDelegate<IDirectDraw.GetDisplayMode>((cpu, mem) => DDraw_GetDisplayMode(cpu, mem))),
				new("GetFourCCCodes", ComVtableDispatcher.FromDelegate<IDirectDraw.GetFourCCCodes>((cpu, mem) => DDraw_GetFourCCCodes(cpu, mem))),
				new("GetGDISurface", ComVtableDispatcher.FromDelegate<IDirectDraw.GetGDISurface>((cpu, mem) => DDraw_GetGDISurface(cpu, mem))),
				new("GetMonitorFrequency", ComVtableDispatcher.FromDelegate<IDirectDraw.GetMonitorFrequency>((cpu, mem) => DDraw_GetMonitorFrequency(cpu, mem))),
				new("GetScanLine", ComVtableDispatcher.FromDelegate<IDirectDraw.GetScanLine>((cpu, mem) => DDraw_GetScanLine(cpu, mem))),
				new("GetVerticalBlankStatus", ComVtableDispatcher.FromDelegate<IDirectDraw.GetVerticalBlankStatus>((cpu, mem) => DDraw_GetVerticalBlankStatus(cpu, mem))),
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDraw.Initialize>((cpu, mem) => DDraw_Initialize(cpu, mem))),
				new("RestoreDisplayMode", ComVtableDispatcher.FromDelegate<IDirectDraw.RestoreDisplayMode>((cpu, mem) => DDraw_RestoreDisplayMode(cpu, mem))),
				new("SetCooperativeLevel", ComVtableDispatcher.FromDelegate<IDirectDraw.SetCooperativeLevel>((cpu, mem) => DDraw_SetCooperativeLevel(cpu, mem, ddrawHandle))),
				new("SetDisplayMode", ComVtableDispatcher.FromDelegate<IDirectDraw.SetDisplayMode>((cpu, mem) => DDraw_SetDisplayMode(cpu, mem, ddrawHandle))),
				new("WaitForVerticalBlank", ComVtableDispatcher.FromDelegate<IDirectDraw.WaitForVerticalBlank>((cpu, mem) => DDraw_WaitForVerticalBlank(cpu, mem)))
			};

			// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDraw", vtableMethods);

			// Store the COM object address in the DirectDraw object for reverse lookup
			ddrawObj.ComObjectAddress = comObjectAddr;
			_comObjectToHandle[comObjectAddr] = ddrawHandle;

			// Write COM object pointer to output parameter with verification
			_logger.LogInformation("[DDraw] Writing COM object 0x{ComObjectAddr:X8} to address 0x{Addr:X8}", comObjectAddr, lplpDd);
			_env.MemWrite32(lplpDd, comObjectAddr);

			// Verify the write succeeded by reading back
			var verification = _env.MemRead32(lplpDd);
			if (verification != comObjectAddr)
			{
				_logger.LogError("[DDraw] Verification failed! Wrote 0x{Expected:X8} but read back 0x{Actual:X8} from address 0x{Addr:X8}", comObjectAddr, verification, lplpDd);
				return (uint)DDResult.DDERR_GENERIC;
			}

			_logger.LogInformation("[DDraw] Verification: Read back 0x{Value:X8} from 0x{Addr:X8} - SUCCESS", verification, lplpDd);

			_logger.LogInformation("[DDraw] Created IDirectDraw COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
			return (uint)DDResult.DD_OK;
		}


		[DllModuleExport(33, entryPoint: 0x0001DDF9, Version = "4.90.0.3000")]
		[DllModuleExport(11, entryPoint: 0x0000CCF6, Version = "5.1.2600.6532")]
		private uint DirectDrawCreateEx(uint lpGuid, uint lplpDd, uint iid, uint pUnkOuter)
		{
			// Fixed: Parameter order now matches MSDN documentation
			// Win32 API: DirectDrawCreateEx(GUID *lpGuid, LPDIRECTDRAW *lplpDD, REFIID iid, IUnknown *pUnkOuter)
			_logger.LogInformation("[DDraw] DirectDrawCreateEx(lpGuid=0x{LpGuid:X8}, lplpDD=0x{LplpDd:X8}, iid=0x{Iid:X8}, pUnkOuter=0x{PUnkOuter:X8})", lpGuid, lplpDd, iid, pUnkOuter);

			// Validate output pointer parameter
			if (lplpDd == 0)
			{
				_logger.LogError("[DDraw] DirectDrawCreateEx: lplpDD is NULL");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Detect if lplpDD looks like a stack pointer (potential parameter handling bug)
			// Check against actual stack range from PE headers
			if (lplpDd >= _env.StackLimit && lplpDd < _env.StackBase)
			{
				_logger.LogWarning("[DDraw] DirectDrawCreateEx: lplpDD=0x{LplpDd:X8} appears to be a stack address (stack range: 0x{StackLimit:X8}-0x{StackBase:X8}) - this might indicate a parameter handling issue", lplpDd, _env.StackLimit, _env.StackBase);
			}

			// Create DirectDraw object with COM vtable (similar to DirectDrawCreate)
			var ddrawHandle = _nextDDrawHandle++;
			var ddrawObj = new DirectDrawObject
			{
				Handle = ddrawHandle,
				Width = 640,
				Height = 480,
				BitsPerPixel = 16
			};
			_ddrawObjects[ddrawHandle] = ddrawObj;

			// Create COM vtable for IDirectDraw interface
			// IMPORTANT: Methods MUST be in exact COM interface order
			// Using List<KeyValuePair> to guarantee insertion order
			var vtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
				new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>((cpu, mem) => ComRelease(cpu, mem))),
				new("Compact", ComVtableDispatcher.FromDelegate<IDirectDraw.Compact>((cpu, mem) => DDraw_Compact(cpu, mem))),
				new("CreateClipper", ComVtableDispatcher.FromDelegate<IDirectDraw.CreateClipper>((cpu, mem) => DDraw_CreateClipper(cpu, mem))),
				new("CreatePalette", ComVtableDispatcher.FromDelegate<IDirectDraw.CreatePalette>((cpu, mem) => DDraw_CreatePalette(cpu, mem))),
				new("CreateSurface", ComVtableDispatcher.FromDelegate<IDirectDraw.CreateSurface>((cpu, mem) => DDraw_CreateSurface(cpu, mem))),
				new("DuplicateSurface", ComVtableDispatcher.FromDelegate<IDirectDraw.DuplicateSurface>((cpu, mem) => DDraw_DuplicateSurface(cpu, mem))),
				new("EnumDisplayModes", ComVtableDispatcher.FromDelegate<IDirectDraw.EnumDisplayModes>((cpu, mem) => DDraw_EnumDisplayModes(cpu, mem))),
				new("EnumSurfaces", ComVtableDispatcher.FromDelegate<IDirectDraw.EnumSurfaces>((cpu, mem) => DDraw_EnumSurfaces(cpu, mem))),
				new("FlipToGDISurface", ComVtableDispatcher.FromDelegate<IDirectDraw.FlipToGDISurface>((cpu, mem) => DDraw_FlipToGDISurface(cpu, mem))),
				new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectDraw.GetCaps>((cpu, mem) => DDraw_GetCaps(cpu, mem))),
				new("GetDisplayMode", ComVtableDispatcher.FromDelegate<IDirectDraw.GetDisplayMode>((cpu, mem) => DDraw_GetDisplayMode(cpu, mem))),
				new("GetFourCCCodes", ComVtableDispatcher.FromDelegate<IDirectDraw.GetFourCCCodes>((cpu, mem) => DDraw_GetFourCCCodes(cpu, mem))),
				new("GetGDISurface", ComVtableDispatcher.FromDelegate<IDirectDraw.GetGDISurface>((cpu, mem) => DDraw_GetGDISurface(cpu, mem))),
				new("GetMonitorFrequency", ComVtableDispatcher.FromDelegate<IDirectDraw.GetMonitorFrequency>((cpu, mem) => DDraw_GetMonitorFrequency(cpu, mem))),
				new("GetScanLine", ComVtableDispatcher.FromDelegate<IDirectDraw.GetScanLine>((cpu, mem) => DDraw_GetScanLine(cpu, mem))),
				new("GetVerticalBlankStatus", ComVtableDispatcher.FromDelegate<IDirectDraw.GetVerticalBlankStatus>((cpu, mem) => DDraw_GetVerticalBlankStatus(cpu, mem))),
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDraw.Initialize>((cpu, mem) => DDraw_Initialize(cpu, mem))),
				new("RestoreDisplayMode", ComVtableDispatcher.FromDelegate<IDirectDraw.RestoreDisplayMode>((cpu, mem) => DDraw_RestoreDisplayMode(cpu, mem))),
				new("SetCooperativeLevel", ComVtableDispatcher.FromDelegate<IDirectDraw.SetCooperativeLevel>((cpu, mem) => DDraw_SetCooperativeLevel(cpu, mem, ddrawHandle))),
				new("SetDisplayMode", ComVtableDispatcher.FromDelegate<IDirectDraw.SetDisplayMode>((cpu, mem) => DDraw_SetDisplayMode(cpu, mem, ddrawHandle))),
				new("WaitForVerticalBlank", ComVtableDispatcher.FromDelegate<IDirectDraw.WaitForVerticalBlank>((cpu, mem) => DDraw_WaitForVerticalBlank(cpu, mem)))
			};

			// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDraw", vtableMethods);

			// Store the COM object address in the DirectDraw object for reverse lookup
			ddrawObj.ComObjectAddress = comObjectAddr;
			_comObjectToHandle[comObjectAddr] = ddrawHandle;

			// Write COM object pointer to output parameter with verification
			_logger.LogInformation("[DDraw] Writing COM object 0x{ComObjectAddr:X8} to address 0x{Addr:X8}", comObjectAddr, lplpDd);
			_env.MemWrite32(lplpDd, comObjectAddr);

			// Verify the write succeeded by reading back
			var verification = _env.MemRead32(lplpDd);
			if (verification != comObjectAddr)
			{
				_logger.LogError("[DDraw] Verification failed! Wrote 0x{Expected:X8} but read back 0x{Actual:X8} from address 0x{Addr:X8}", comObjectAddr, verification, lplpDd);
				return (uint)DDResult.DDERR_GENERIC;
			}

			_logger.LogInformation("[DDraw] Verification: Read back 0x{Value:X8} from 0x{Addr:X8} - SUCCESS", verification, lplpDd);

			_logger.LogInformation("[DDraw] Created IDirectDraw COM object (Ex) at 0x{ComObjectAddr:X8}", comObjectAddr);
			return (uint)DDResult.DD_OK;
		}

		private sealed class DirectDrawObject
		{
			public uint Handle { get; set; }
			public uint ComObjectAddress { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public int BitsPerPixel { get; set; }
			public Rendering.IRenderingBackend? RenderingBackend { get; set; }
			public uint CooperativeLevel { get; set; }
			public IntPtr WindowHandle { get; set; }
		}

		private sealed class DirectDrawSurface
		{
			public uint Handle { get; set; }
			public uint ComObjectAddress { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public int Pitch { get; set; }
			public byte[]? Bits { get; set; }
			public bool IsPrimary { get; set; }
			public bool IsLocked { get; set; }
			public uint DirectDrawHandle { get; set; }
			public IntPtr TexturePtr { get; set; }
			public uint LockedMemoryPtr { get; set; }
			public uint PaletteHandle { get; set; }
			public uint ColorKeyLow { get; set; }
			public uint ColorKeyHigh { get; set; }
			public bool HasColorKey { get; set; }
			public uint ClipperHandle { get; set; }
			public List<uint> AttachedSurfaces { get; set; } = new List<uint>();
			public bool IsTextureDirty { get; set; }
		}

		private sealed class DirectDrawPalette
		{
			public uint Handle { get; set; }
			public uint ComObjectAddress { get; set; }
			public uint[] Entries { get; set; } = Array.Empty<uint>();
		}

		private sealed class DirectDrawClipper
		{
			public uint Handle { get; set; }
			public uint ComObjectAddress { get; set; }
			public uint WindowHandle { get; set; }
			public bool IsWindowedMode { get; set; }
		}

		/// <summary>
		/// Allocates memory for a string and writes it to emulated memory.
		/// Returns the address of the allocated string.
		/// </summary>
		private uint AllocateString(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return 0;
			}

			var bytes = System.Text.Encoding.ASCII.GetBytes(str);
			var size = (uint)(bytes.Length + 1); // +1 for null terminator
			var addr = _env.HeapAlloc(0, size);

			if (addr == 0)
			{
				_logger.LogError("[DDraw] Failed to allocate {Size} bytes for string", size);
				return 0;
			}

			// Write string bytes
			for (int i = 0; i < bytes.Length; i++)
			{
				_env.Memory.Write8(addr + (uint)i, bytes[i]);
			}

			// Write null terminator
			_env.Memory.Write8(addr + (uint)bytes.Length, 0);

			return addr;
		}

		/// <summary>
		/// Allocates memory for a Unicode (UTF-16) string and writes it to emulated memory.
		/// Returns the address of the allocated string.
		/// </summary>
		private uint AllocateUnicodeString(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return 0;
			}

			var size = (uint)((str.Length + 1) * 2); // +1 for null terminator, *2 for UTF-16
			var addr = _env.HeapAlloc(0, size);

			if (addr == 0)
			{
				_logger.LogError("[DDraw] Failed to allocate {Size} bytes for Unicode string", size);
				return 0;
			}

			// Write wide characters to output buffer
			for (int i = 0; i < str.Length; i++)
			{
				_env.Memory.Write16(addr + (uint)(i * 2), str[i]);
			}

			// Write null terminator
			_env.Memory.Write16(addr + (uint)(str.Length * 2), 0);

			return addr;
		}

		/// <summary>
		/// Frees a string that was previously allocated with AllocateString.
		/// </summary>
		private void FreeString(uint addr)
		{
			if (addr != 0)
			{
				_env.HeapFree(0, addr);
			}
		}

		/// <summary>
		/// Allocates a block of memory.
		/// </summary>
		private uint AllocateMemory(uint size)
		{
			return _env.HeapAlloc(0, size);
		}

		/// <summary>
		/// Frees a block of memory.
		/// </summary>
		private void FreeMemory(uint addr)
		{
			if (addr != 0)
			{
				_env.HeapFree(0, addr);
			}
		}

		/// <summary>
		/// Determines the number of palette entries based on DirectDraw palette capability flags.
		/// Checks flags from highest to lowest bit depth to handle multiple flags correctly.
		/// </summary>
		/// <param name="dwFlags">DirectDraw palette capability flags (DDPCAPS_*)</param>
		/// <returns>Number of entries for the palette (2, 4, 16, or 256)</returns>
		public static int DeterminePaletteSizeFromFlags(uint dwFlags)
		{
			// DDPCAPS_ALLOW256 indicates the palette can have all 256 entries defined
			// This overrides the bit depth flags to allow full 256-color palette
			if ((dwFlags & (uint)DDPCaps.DDPCAPS_ALLOW256) != 0)
			{
				return 256;
			}

			// Check from highest to lowest bit depth to handle multiple flags correctly
			// When multiple bit depth flags are set, the palette should be created with the highest bit depth
			if ((dwFlags & (uint)DDPCaps.DDPCAPS_8BIT) != 0)
			{
				return 256;
			}

			if ((dwFlags & (uint)DDPCaps.DDPCAPS_4BIT) != 0)
			{
				return 16;
			}

			if ((dwFlags & (uint)DDPCaps.DDPCAPS_2BIT) != 0)
			{
				return 4;
			}

			if ((dwFlags & (uint)DDPCaps.DDPCAPS_1BIT) != 0)
			{
				return 2;
			}

			return 256; // Default to 8-bit if no flags set
		}

		// COM interface methods (stubs for IDirectDraw)
		private uint ComQueryInterface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var riid = args.UInt32(1);
			var ppvObject = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IUnknown::QueryInterface(this=0x{ThisPtr:X8}, riid=0x{Riid:X8}, ppvObject=0x{PpvObject:X8})", thisPtr, riid, ppvObject);

			// E_NOINTERFACE = 0x80004002
			return (uint)DDResult.E_NOINTERFACE;
		}

		private uint ComAddRef(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DDraw COM] IUnknown::AddRef(this=0x{ThisPtr:X8})", thisPtr);
			return 1; // Reference count
		}

		private uint ComRelease(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DDraw COM] IUnknown::Release(this=0x{ThisPtr:X8})", thisPtr);
			return 0; // Reference count after release
		}

		private uint Palette_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpdwCaps = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawPalette::GetCaps(this=0x{ThisPtr:X8}, lpdwCaps=0x{LpdwCaps:X8})", thisPtr, lpdwCaps);

			// Find the palette based on COM object address
			DirectDrawPalette? palette = null;
			foreach (var p in _palettes.Values)
			{
				if (p.ComObjectAddress == thisPtr)
				{
					palette = p;
					break;
				}
			}

			if (palette == null)
			{
				_logger.LogError("[DDraw] GetCaps: could not find palette with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (lpdwCaps != 0)
			{
				// Determine caps based on number of entries
				uint caps = palette.Entries.Length switch
				{
					2 => (uint)DDPCaps.DDPCAPS_1BIT,
					4 => (uint)DDPCaps.DDPCAPS_2BIT,
					16 => (uint)DDPCaps.DDPCAPS_4BIT,
					256 => (uint)DDPCaps.DDPCAPS_8BIT,
					_ => (uint)DDPCaps.DDPCAPS_8BIT
				};

				_env.MemWrite32(lpdwCaps, caps);
				_logger.LogInformation("[DDraw] Palette caps: 0x{Caps:X8} ({Count} entries)", caps, palette.Entries.Length);
			}

			return (uint)DDResult.DD_OK;
		}

		private uint Palette_GetEntries(ICpu cpu, VirtualMemory memory, uint paletteHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var dwBase = args.UInt32(2);
			var dwNumEntries = args.UInt32(3);
			var lpEntries = args.UInt32(4);

			_logger.LogInformation("[DDraw COM] IDirectDrawPalette::GetEntries(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, dwBase={DwBase}, dwNumEntries={DwNumEntries}, lpEntries=0x{LpEntries:X8})",
				thisPtr, dwFlags, dwBase, dwNumEntries, lpEntries);

			if (!_palettes.TryGetValue(paletteHandle, out var palette))
			{
				_logger.LogError("[DDraw] GetEntries: could not find palette with handle 0x{PaletteHandle:X8}", paletteHandle);
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (lpEntries == 0)
			{
				_logger.LogError("[DDraw] GetEntries: lpEntries is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Check bounds
			if (dwBase >= palette.Entries.Length || dwBase + dwNumEntries > palette.Entries.Length)
			{
				_logger.LogError("[DDraw] GetEntries: invalid range (base={Base}, count={Count}, max={Max})",
					dwBase, dwNumEntries, palette.Entries.Length);
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Write palette entries (PALETTEENTRY is 4 bytes: r,g,b,flags)
			for (var i = 0u; i < dwNumEntries; i++)
			{
				var entry = palette.Entries[dwBase + i];
				_env.MemWrite32(lpEntries + (i * 4), entry);
			}

			_logger.LogInformation("[DDraw] Retrieved {Count} palette entries starting at index {Base}", dwNumEntries, dwBase);
			return (uint)DDResult.DD_OK;
		}

		private uint Palette_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDrawPalette::Initialize() - stub");
			return 0;
		}

		private uint Palette_SetEntries(ICpu cpu, VirtualMemory memory, uint paletteHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var dwStartingEntry = args.UInt32(2);
			var dwCount = args.UInt32(3);
			var lpEntries = args.UInt32(4);

			_logger.LogInformation("[DDraw COM] IDirectDrawPalette::SetEntries(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, dwStartingEntry={DwStartingEntry}, dwCount={DwCount}, lpEntries=0x{LpEntries:X8})",
				thisPtr, dwFlags, dwStartingEntry, dwCount, lpEntries);

			if (!_palettes.TryGetValue(paletteHandle, out var palette))
			{
				_logger.LogError("[DDraw] SetEntries: could not find palette with handle 0x{PaletteHandle:X8}", paletteHandle);
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (lpEntries == 0)
			{
				_logger.LogError("[DDraw] SetEntries: lpEntries is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Check starting entry is valid
			if (dwStartingEntry >= palette.Entries.Length)
			{
				_logger.LogError("[DDraw] SetEntries: starting entry {Start} is beyond palette size {Max}", dwStartingEntry, palette.Entries.Length);
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Clamp count to available entries (for compatibility with games that try to set more entries than palette has)
			var actualCount = dwCount;
			if (dwStartingEntry + dwCount > palette.Entries.Length)
			{
				actualCount = (uint)palette.Entries.Length - dwStartingEntry;
				_logger.LogWarning("[DDraw] SetEntries: clamping count from {RequestedCount} to {ActualCount} (start={Start}, max={Max})", dwCount, actualCount, dwStartingEntry, palette.Entries.Length);
			}

			// Read and update palette entries (PALETTEENTRY is 4 bytes: r,g,b,flags)
			for (var i = 0u; i < actualCount; i++)
			{
				var entry = _env.MemRead32(lpEntries + (i * 4));
				palette.Entries[dwStartingEntry + i] = entry;
			}

			_logger.LogInformation("[DDraw] Updated {Count} palette entries starting at index {Start}", actualCount, dwStartingEntry);
			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_Compact(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Compact() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_CreateClipper(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lplpDDClipper = args.UInt32(2);
			var pUnkOuter = args.UInt32(3);

			_logger.LogInformation("[DDraw COM] IDirectDraw::CreateClipper(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lplpDDClipper=0x{LplpDDClipper:X8}, pUnkOuter=0x{PUnkOuter:X8})", thisPtr, dwFlags, lplpDDClipper, pUnkOuter);

			if (lplpDDClipper == 0)
			{
				_logger.LogError("[DDraw] CreateClipper: lplpDDClipper is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Create a new clipper handle
			var clipperHandle = _nextClipperHandle++;
			var clipper = new DirectDrawClipper
			{
				Handle = clipperHandle,
				IsWindowedMode = true // Typically used for windowed mode
			};

			_clippers[clipperHandle] = clipper;

			// Create COM vtable for IDirectDrawClipper interface
			var clipperVtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
				new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>((cpu, mem) => ComRelease(cpu, mem))),
				new("GetClipList", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.GetClipList>((cpu, mem) => Clipper_GetClipList(cpu, mem))),
				new("GetHWnd", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.GetHWnd>((cpu, mem) => Clipper_GetHWnd(cpu, mem, clipperHandle))),
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.Initialize>((cpu, mem) => Clipper_Initialize(cpu, mem))),
				new("IsClipListChanged", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.IsClipListChanged>((cpu, mem) => Clipper_IsClipListChanged(cpu, mem))),
				new("SetClipList", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.SetClipList>((cpu, mem) => Clipper_SetClipList(cpu, mem))),
				new("SetHWnd", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.SetHWnd>((cpu, mem) => Clipper_SetHWnd(cpu, mem, clipperHandle)))
			};

			var clipperComAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDrawClipper", clipperVtableMethods);
			clipper.ComObjectAddress = clipperComAddr;

			// Write the clipper COM object address to the output pointer
			_env.MemWrite32(lplpDDClipper, clipperComAddr);

			_logger.LogInformation("[DDraw] Created clipper with handle 0x{Handle:X8}, COM object at 0x{ComAddr:X8}", clipperHandle, clipperComAddr);

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_CreatePalette(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpColorTable = args.UInt32(2);
			var lplpDDPalette = args.UInt32(3);
			var pUnkOuter = args.UInt32(4);

			_logger.LogInformation("[DDraw COM] IDirectDraw::CreatePalette(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpColorTable=0x{LpColorTable:X8}, lplpDDPalette=0x{LplpDDPalette:X8}, pUnkOuter=0x{PUnkOuter:X8})", thisPtr, dwFlags, lpColorTable, lplpDDPalette, pUnkOuter);

			// Determine number of entries from dwFlags
			int numEntries = DeterminePaletteSizeFromFlags(dwFlags);

			var paletteEntries = new uint[numEntries];
			if (lpColorTable != 0)
			{
				for (var i = 0; i < numEntries; i++)
				{
					// PALETTEENTRY is 4 bytes (r,g,b,flags)
					paletteEntries[i] = _env.MemRead32(lpColorTable + (uint)(i * 4));
				}
			}

			var paletteHandle = _nextPaletteHandle++;
			var palette = new DirectDrawPalette { Handle = paletteHandle, Entries = paletteEntries };
			_palettes[paletteHandle] = palette;

			var vtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDrawPalette.QueryInterface>((c, m) => ComQueryInterface(c, m))),
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDrawPalette.AddRef>((c, m) => ComAddRef(c, m))),
				new("Release", ComVtableDispatcher.FromDelegate<IDirectDrawPalette.Release>((c, m) => ComRelease(c, m))),
				new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectDrawPalette.GetCaps>((c, m) => Palette_GetCaps(c, m))),
				new("GetEntries", ComVtableDispatcher.FromDelegate<IDirectDrawPalette.GetEntries>((c, m) => Palette_GetEntries(c, m, paletteHandle))),
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDrawPalette.Initialize>((c, m) => Palette_Initialize(c, m))),
				new("SetEntries", ComVtableDispatcher.FromDelegate<IDirectDrawPalette.SetEntries>((c, m) => Palette_SetEntries(c, m, paletteHandle)))
			};

			var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDrawPalette", vtableMethods);
			palette.ComObjectAddress = comObjectAddr;

			if (lplpDDPalette != 0)
			{
				_env.MemWrite32(lplpDDPalette, comObjectAddr);
			}

			_logger.LogInformation("[DDraw] Created IDirectDrawPalette COM object at 0x{ComObjectAddr:X8} for palette 0x{PaletteHandle:X8}", comObjectAddr, paletteHandle);

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_CreateSurface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceDesc = args.UInt32(1);
			var lplpDDSurface = args.UInt32(2);
			var pUnkOuter = args.UInt32(3);

			_logger.LogInformation("[DDraw COM] IDirectDraw::CreateSurface(this=0x{ThisPtr:X8}, lpDDSurfaceDesc=0x{LpDDSurfaceDesc:X8}, lplpDDSurface=0x{LplpDDSurface:X8}, pUnkOuter=0x{PUnkOuter:X8})", thisPtr, lpDDSurfaceDesc, lplpDDSurface, pUnkOuter);

			// Read surface description
			var dwSize = _env.MemRead32(lpDDSurfaceDesc);
			var dwFlags = (DDSD)_env.MemRead32(lpDDSurfaceDesc + 4);
			var dwHeight = _env.MemRead32(lpDDSurfaceDesc + 8);
			var dwWidth = _env.MemRead32(lpDDSurfaceDesc + 12);

			// Read backbuffer count if DDSD_BACKBUFFERCOUNT flag is set
			var dwBackBufferCount = 0u;
			if (dwFlags.HasFlag(DDSD.BACKBUFFERCOUNT))
			{
				dwBackBufferCount = _env.MemRead32(lpDDSurfaceDesc + 20);
			}

			// Read surface capabilities from offset 104 (DDSURFACEDESC.ddsCaps)
			// DDSURFACEDESC is 108 bytes total (0-107), with ddsCaps.dwCaps at offset 104-107
			var dwSurfaceCaps = 0u;
			if (dwSize >= 108)
			{
				dwSurfaceCaps = _env.MemRead32(lpDDSurfaceDesc + 104);
			}

			_logger.LogInformation("[DDraw] Surface creation: dwSize={Size}, flags=0x{Flags:X}, caps=0x{Caps:X8}, width={Width}, height={Height}, backbufferCount={Count}", dwSize, dwFlags, dwSurfaceCaps, dwWidth, dwHeight, dwBackBufferCount);

			// Find the DirectDraw object from the COM object pointer
			uint ddrawHandle = 0;
			foreach (var kvp in _ddrawObjects)
			{
				// For now, just use the first DirectDraw object
				ddrawHandle = kvp.Key;
				break;
			}

			if (ddrawHandle == 0 || !_ddrawObjects.TryGetValue(ddrawHandle, out var ddrawObj))
			{
				_logger.LogError("[DDraw] Failed to find DirectDraw object for CreateSurface");
				return (uint)DDResult.DDERR_GENERIC;
			}

			// Determine if this is a primary surface
			var isPrimary = (dwSurfaceCaps & (uint)DDSCaps.DDSCAPS_PRIMARYSURFACE) != 0;

			// For primary surfaces, if WIDTH/HEIGHT are not specified in the descriptor,
			// use the dimensions from the current display mode (set by SetDisplayMode)
			var surfaceWidth = dwWidth;
			var surfaceHeight = dwHeight;
			
			// Check if dimensions are explicitly specified and valid
			var hasWidthAndHeightFlags = dwFlags.HasFlag(DDSD.WIDTH) && dwFlags.HasFlag(DDSD.HEIGHT);
			var hasValidDimensions = dwWidth > 0 && dwHeight > 0;
			
			if (isPrimary && (!hasWidthAndHeightFlags || !hasValidDimensions))
			{
				// Validate display mode dimensions before using them
				if (ddrawObj.Width <= 0 || ddrawObj.Height <= 0)
				{
					_logger.LogError("[DDraw] Cannot create primary surface: display mode dimensions are invalid ({Width}x{Height})", ddrawObj.Width, ddrawObj.Height);
					return (uint)DDResult.DDERR_GENERIC;
				}
				
				_logger.LogInformation("[DDraw] Primary surface created without explicit dimensions, using display mode: {Width}x{Height}", ddrawObj.Width, ddrawObj.Height);
				// Safe to cast: validation above ensures dimensions are positive
				surfaceWidth = (uint)ddrawObj.Width;
				surfaceHeight = (uint)ddrawObj.Height;
			}

			// Create a new surface
			var surfaceHandle = _nextSurfaceHandle++;
			var surface = new DirectDrawSurface
			{
				Handle = surfaceHandle,
				Width = (int)surfaceWidth,
				Height = (int)surfaceHeight,
				DirectDrawHandle = ddrawHandle,
				IsPrimary = isPrimary,
				Pitch = (int)surfaceWidth * (ddrawObj.BitsPerPixel / 8)
			};

			// Allocate memory for the surface
			surface.Bits = new byte[surface.Pitch * surface.Height];

			// Store the surface
			_surfaces[surfaceHandle] = surface;

			// Create COM vtable for IDirectDrawSurface interface
			// IMPORTANT: Methods MUST be in exact COM interface order
			// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nn-ddraw-idirectdrawsurface
			// See MSDN/DirectX SDK for authoritative method order. Do NOT reorder without verifying against official docs.
			// Using List<KeyValuePair> to guarantee insertion order
			var vtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
				new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>((cpu, mem) => ComRelease(cpu, mem))),
				new("AddAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.AddAttachedSurface>((cpu, mem) => Surface_AddAttachedSurface(cpu, mem))),
				new("AddOverlayDirtyRect", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.AddOverlayDirtyRect>((cpu, mem) => Surface_AddOverlayDirtyRect(cpu, mem))),
				new("Blt", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Blt>((cpu, mem) => Surface_Blt(cpu, mem))),
				new("BltBatch", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.BltBatch>((cpu, mem) => Surface_BltBatch(cpu, mem))),
				new("BltFast", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.BltFast>((cpu, mem) => Surface_BltFast(cpu, mem))),
				new("DeleteAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.DeleteAttachedSurface>((cpu, mem) => Surface_DeleteAttachedSurface(cpu, mem))),
				new("EnumAttachedSurfaces", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.EnumAttachedSurfaces>((cpu, mem) => Surface_EnumAttachedSurfaces(cpu, mem))),
				new("EnumOverlayZOrders", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.EnumOverlayZOrders>((cpu, mem) => Surface_EnumOverlayZOrders(cpu, mem))),
				new("Flip", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Flip>((cpu, mem) => Surface_Flip(cpu, mem))),
				new("GetAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetAttachedSurface>((cpu, mem) => Surface_GetAttachedSurface(cpu, mem))),
				new("GetBltStatus", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetBltStatus>((cpu, mem) => Surface_GetBltStatus(cpu, mem))),
				new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetCaps>((cpu, mem) => Surface_GetCaps(cpu, mem))),
				new("GetClipper", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetClipper>((cpu, mem) => Surface_GetClipper(cpu, mem))),
				new("GetColorKey", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetColorKey>((cpu, mem) => Surface_GetColorKey(cpu, mem))),
				new("GetDC", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetDC>((cpu, mem) => Surface_GetDC(cpu, mem))),
				new("GetFlipStatus", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetFlipStatus>((cpu, mem) => Surface_GetFlipStatus(cpu, mem))),
				new("GetOverlayPosition", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetOverlayPosition>((cpu, mem) => Surface_GetOverlayPosition(cpu, mem))),
				new("GetPalette", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetPalette>((cpu, mem) => Surface_GetPalette(cpu, mem))),
				new("GetPixelFormat", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetPixelFormat>((cpu, mem) => Surface_GetPixelFormat(cpu, mem))),
				new("GetSurfaceDesc", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetSurfaceDesc>((cpu, mem) => Surface_GetSurfaceDesc(cpu, mem))),
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Initialize>((cpu, mem) => Surface_Initialize(cpu, mem))),
				new("IsLost", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.IsLost>((cpu, mem) => Surface_IsLost(cpu, mem))),
				new("Lock", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Lock>((cpu, mem) => Surface_Lock(cpu, mem, surfaceHandle))),
				new("ReleaseDC", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.ReleaseDC>((cpu, mem) => Surface_ReleaseDC(cpu, mem))),
				new("Restore", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Restore>((cpu, mem) => Surface_Restore(cpu, mem))),
				new("SetClipper", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetClipper>((cpu, mem) => Surface_SetClipper(cpu, mem))),
				new("SetColorKey", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetColorKey>((cpu, mem) => Surface_SetColorKey(cpu, mem))),
				new("SetOverlayPosition", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetOverlayPosition>((cpu, mem) => Surface_SetOverlayPosition(cpu, mem))),
				new("SetPalette", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetPalette>((cpu, mem) => Surface_SetPalette(cpu, mem, surfaceHandle))),
				new("Unlock", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Unlock>((cpu, mem) => Surface_Unlock(cpu, mem, surfaceHandle))),
				new("UpdateOverlay", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlay>((cpu, mem) => Surface_UpdateOverlay(cpu, mem))),
				new("UpdateOverlayDisplay", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlayDisplay>((cpu, mem) => Surface_UpdateOverlayDisplay(cpu, mem))),
				new("UpdateOverlayZOrder", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlayZOrder>((cpu, mem) => Surface_UpdateOverlayZOrder(cpu, mem)))
			};

			// Create the COM object with vtable using ordered method list
			var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDrawSurface", vtableMethods);
			surface.ComObjectAddress = comObjectAddr;

			// Check if this is a flipping complex surface that needs backbuffers
			// DDSCAPS_FLIP = 0x00000010, DDSCAPS_COMPLEX = 0x00000008
			var isFlippingChain = (dwSurfaceCaps & (uint)DDSCaps.DDSCAPS_FLIP) != 0 && (dwSurfaceCaps & (uint)DDSCaps.DDSCAPS_COMPLEX) != 0;

			// If this is a primary surface with flipping capabilities but no explicit backbuffer count,
			// default to creating 1 backbuffer (common DirectDraw pattern)
			if (surface.IsPrimary && isFlippingChain && dwBackBufferCount == 0)
			{
				dwBackBufferCount = 1;
				_logger.LogInformation("[DDraw] Primary surface has FLIP+COMPLEX caps but no explicit backbuffer count, defaulting to 1 backbuffer");
			}

			// Create backbuffers if requested
			if (dwBackBufferCount > 0 && surface.IsPrimary)
			{
				_logger.LogInformation("[DDraw] Creating {Count} backbuffer(s) for primary surface", dwBackBufferCount);

				for (var i = 0u; i < dwBackBufferCount; i++)
				{
					var backBufferHandle = _nextSurfaceHandle++;
					var backBuffer = new DirectDrawSurface
					{
						Handle = backBufferHandle,
						Width = (int)surfaceWidth,
						Height = (int)surfaceHeight,
						DirectDrawHandle = ddrawHandle,
						IsPrimary = false,
						Pitch = (int)surfaceWidth * (ddrawObj.BitsPerPixel / 8)
					};

					// Allocate memory for the backbuffer
					backBuffer.Bits = new byte[backBuffer.Pitch * backBuffer.Height];

					// Store the backbuffer
					_surfaces[backBufferHandle] = backBuffer;

					// Create COM vtable for backbuffer
					// IMPORTANT: Methods MUST be in exact COM interface order (same as primary surface)
					// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nn-ddraw-idirectdrawsurface
					// See MSDN/DirectX SDK for authoritative method order. Do NOT reorder without verifying against official docs.
					var backBufferVtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
					{
						new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
						new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
						new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>((cpu, mem) => ComRelease(cpu, mem))),
						new("AddAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.AddAttachedSurface>((cpu, mem) => Surface_AddAttachedSurface(cpu, mem))),
						new("AddOverlayDirtyRect", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.AddOverlayDirtyRect>((cpu, mem) => Surface_AddOverlayDirtyRect(cpu, mem))),
						new("Blt", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Blt>((cpu, mem) => Surface_Blt(cpu, mem))),
						new("BltBatch", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.BltBatch>((cpu, mem) => Surface_BltBatch(cpu, mem))),
						new("BltFast", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.BltFast>((cpu, mem) => Surface_BltFast(cpu, mem))),
						new("DeleteAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.DeleteAttachedSurface>((cpu, mem) => Surface_DeleteAttachedSurface(cpu, mem))),
						new("EnumAttachedSurfaces", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.EnumAttachedSurfaces>((cpu, mem) => Surface_EnumAttachedSurfaces(cpu, mem))),
						new("EnumOverlayZOrders", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.EnumOverlayZOrders>((cpu, mem) => Surface_EnumOverlayZOrders(cpu, mem))),
						new("Flip", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Flip>((cpu, mem) => Surface_Flip(cpu, mem))),
						new("GetAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetAttachedSurface>((cpu, mem) => Surface_GetAttachedSurface(cpu, mem))),
						new("GetBltStatus", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetBltStatus>((cpu, mem) => Surface_GetBltStatus(cpu, mem))),
						new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetCaps>((cpu, mem) => Surface_GetCaps(cpu, mem))),
						new("GetClipper", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetClipper>((cpu, mem) => Surface_GetClipper(cpu, mem))),
						new("GetColorKey", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetColorKey>((cpu, mem) => Surface_GetColorKey(cpu, mem))),
						new("GetDC", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetDC>((cpu, mem) => Surface_GetDC(cpu, mem))),
						new("GetFlipStatus", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetFlipStatus>((cpu, mem) => Surface_GetFlipStatus(cpu, mem))),
						new("GetOverlayPosition", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetOverlayPosition>((cpu, mem) => Surface_GetOverlayPosition(cpu, mem))),
						new("GetPalette", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetPalette>((cpu, mem) => Surface_GetPalette(cpu, mem))),
						new("GetPixelFormat", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetPixelFormat>((cpu, mem) => Surface_GetPixelFormat(cpu, mem))),
						new("GetSurfaceDesc", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetSurfaceDesc>((cpu, mem) => Surface_GetSurfaceDesc(cpu, mem))),
						new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Initialize>((cpu, mem) => Surface_Initialize(cpu, mem))),
						new("IsLost", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.IsLost>((cpu, mem) => Surface_IsLost(cpu, mem))),
						new("Lock", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Lock>((cpu, mem) => Surface_Lock(cpu, mem, backBufferHandle))),
						new("ReleaseDC", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.ReleaseDC>((cpu, mem) => Surface_ReleaseDC(cpu, mem))),
						new("Restore", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Restore>((cpu, mem) => Surface_Restore(cpu, mem))),
						new("SetClipper", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetClipper>((cpu, mem) => Surface_SetClipper(cpu, mem))),
						new("SetColorKey", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetColorKey>((cpu, mem) => Surface_SetColorKey(cpu, mem))),
						new("SetOverlayPosition", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetOverlayPosition>((cpu, mem) => Surface_SetOverlayPosition(cpu, mem))),
						new("SetPalette", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetPalette>((cpu, mem) => Surface_SetPalette(cpu, mem, backBufferHandle))),
						new("Unlock", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Unlock>((cpu, mem) => Surface_Unlock(cpu, mem, backBufferHandle))),
						new("UpdateOverlay", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlay>((cpu, mem) => Surface_UpdateOverlay(cpu, mem))),
						new("UpdateOverlayDisplay", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlayDisplay>((cpu, mem) => Surface_UpdateOverlayDisplay(cpu, mem))),
						new("UpdateOverlayZOrder", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlayZOrder>((cpu, mem) => Surface_UpdateOverlayZOrder(cpu, mem)))
					};

					var backBufferComAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDrawSurface", backBufferVtableMethods);
					backBuffer.ComObjectAddress = backBufferComAddr;

					// Attach the backbuffer to the primary surface
					surface.AttachedSurfaces.Add(backBufferHandle);

					_logger.LogInformation("[DDraw] Created backbuffer {Index} at surface handle 0x{Handle:X8}, COM object at 0x{ComAddr:X8}", i + 1, backBufferHandle, backBufferComAddr);
					_logger.LogInformation("[DDraw] Attached backbuffer 0x{BackBufferHandle:X8} to primary surface 0x{PrimaryHandle:X8} (COM=0x{ComAddr:X8}), AttachedSurfaces.Count={Count}", backBufferHandle, surface.Handle, surface.ComObjectAddress, surface.AttachedSurfaces.Count);
				}
			}

			// Write COM object pointer to output parameter
			if (lplpDDSurface != 0)
			{
				_env.MemWrite32(lplpDDSurface, comObjectAddr);
			}

			_logger.LogInformation("[DDraw] Created IDirectDrawSurface COM object at 0x{ComObjectAddr:X8} for surface 0x{SurfaceHandle:X8}", comObjectAddr, surfaceHandle);
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_UpdateOverlayZOrder(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::UpdateOverlayZOrder() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_UpdateOverlayDisplay(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::UpdateOverlayDisplay() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_UpdateOverlay(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::UpdateOverlay() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_SetPalette(ICpu cpu, VirtualMemory mem, uint surfaceHandle)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDPalette = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::SetPalette(this=0x{ThisPtr:X8}, lpDDPalette=0x{LpDDPalette:X8})", thisPtr, lpDDPalette);

			if (!_surfaces.TryGetValue(surfaceHandle, out var surface))
			{
				_logger.LogError("[DDraw] SetPalette: could not find surface with handle 0x{SurfaceHandle:X8}", surfaceHandle);
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (lpDDPalette == 0)
			{
				surface.PaletteHandle = 0;
				_logger.LogInformation("[DDraw] Detached palette from surface 0x{SurfaceHandle:X8}", surfaceHandle);
				return (uint)DDResult.DD_OK;
			}

			uint paletteHandle = 0;
			foreach (var p in _palettes.Values)
			{
				if (p.ComObjectAddress == lpDDPalette)
				{
					paletteHandle = p.Handle;
					break;
				}
			}

			if (paletteHandle == 0)
			{
				_logger.LogWarning("[DDraw] SetPalette: could not find palette object with address 0x{LpDDPalette:X8}", lpDDPalette);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			surface.PaletteHandle = paletteHandle;
			_logger.LogInformation("[DDraw] Surface 0x{SurfaceHandle:X8} palette set to 0x{PaletteHandle:X8}", surfaceHandle, paletteHandle);

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_SetOverlayPosition(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::SetOverlayPosition() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_SetColorKey(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpDDColorKey = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::SetColorKey(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpDDColorKey=0x{ColorKey:X8})", thisPtr, dwFlags, lpDDColorKey);

			// Find the surface
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] SetColorKey: could not find surface");
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (lpDDColorKey != 0)
			{
				// Read DDCOLORKEY structure
				var colorKey = new DDColorKeyRef(_env.Memory, lpDDColorKey);

				surface.ColorKeyLow = colorKey.dwColorSpaceLowValue;
				surface.ColorKeyHigh = colorKey.dwColorSpaceHighValue;
				surface.HasColorKey = true;

				_logger.LogInformation("[DDraw] Set color key: low=0x{Low:X8}, high=0x{High:X8}", colorKey.dwColorSpaceLowValue, colorKey.dwColorSpaceHighValue);
			}
			else
			{
				// Clear color key
				surface.HasColorKey = false;
				_logger.LogInformation("[DDraw] Cleared color key");
			}

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_SetClipper(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDClipper = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::SetClipper(this=0x{ThisPtr:X8}, lpDDClipper=0x{LpDDClipper:X8})", thisPtr, lpDDClipper);

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] SetClipper: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Find the clipper by COM object address
			uint clipperHandle = 0;
			if (lpDDClipper != 0)
			{
				foreach (var clipper in _clippers.Values)
				{
					if (clipper.ComObjectAddress == lpDDClipper)
					{
						clipperHandle = clipper.Handle;
						break;
					}
				}

				if (clipperHandle == 0)
				{
					_logger.LogError("[DDraw] SetClipper: could not find clipper with COM address 0x{LpDDClipper:X8}", lpDDClipper);
					return (uint)DDResult.DDERR_INVALIDOBJECT;
				}
			}

			// Set the clipper on the surface
			surface.ClipperHandle = clipperHandle;
			_logger.LogInformation("[DDraw] Surface 0x{SurfaceHandle:X8} clipper set to 0x{ClipperHandle:X8}", surface.Handle, clipperHandle);

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_Restore(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Restore() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_ReleaseDC(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var hDC = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::ReleaseDC(this=0x{ThisPtr:X8}, hDC=0x{HDC:X8})", thisPtr, hDC);

			// Validate and release the DC
			// Note: TryGetValue and Remove are not atomic, but this is acceptable
			// as the emulator runs Win32 applications in a single-threaded context
			if (_surfaceDCs.TryGetValue(hDC, out var surfaceAddr))
			{
				if (surfaceAddr != thisPtr)
				{
					_logger.LogError("[DDraw] ReleaseDC: DC 0x{HDC:X8} belongs to surface 0x{SurfaceAddr:X8}, not 0x{ThisPtr:X8}", hDC, surfaceAddr, thisPtr);
					return (uint)DDResult.DDERR_INVALIDOBJECT;
				}

				_surfaceDCs.Remove(hDC);
				_logger.LogInformation("[DDraw] Released DC 0x{HDC:X8} for surface 0x{ThisPtr:X8}", hDC, thisPtr);
				return (uint)DDResult.DD_OK;
			}

			_logger.LogWarning("[DDraw] ReleaseDC: DC 0x{HDC:X8} not found, returning DDERR_INVALIDOBJECT", hDC);
			return (uint)DDResult.DDERR_INVALIDOBJECT;
		}

		private uint Surface_IsLost(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::IsLost(this=0x{ThisPtr:X8})", thisPtr);
			// Our surfaces are never lost in the emulator
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_Initialize(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Initialize() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetSurfaceDesc(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceDesc = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetSurfaceDesc(this=0x{ThisPtr:X8}, lpDDSurfaceDesc=0x{SurfaceDesc:X8})", thisPtr, lpDDSurfaceDesc);

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetSurfaceDesc: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			if (lpDDSurfaceDesc == 0)
			{
				_logger.LogError("[DDraw] GetSurfaceDesc: lpDDSurfaceDesc is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			if (lpDDSurfaceDesc != 0)
			{
				// Find the DirectDraw object to get BPP
				if (_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj))
				{
					var dwSize = _env.MemRead32(lpDDSurfaceDesc);

					// Fill DDSURFACEDESC structure
					_env.MemWrite32(lpDDSurfaceDesc + 4, (uint)(DDSD.CAPS | DDSD.WIDTH | DDSD.HEIGHT | DDSD.PITCH | DDSD.PIXELFORMAT));
					_env.MemWrite32(lpDDSurfaceDesc + 8, (uint)surface.Height); // dwHeight
					_env.MemWrite32(lpDDSurfaceDesc + 12, (uint)surface.Width); // dwWidth
					_env.MemWrite32(lpDDSurfaceDesc + 16, (uint)surface.Pitch); // lPitch

					// Write pixel format (offset 76 for DDSURFACEDESC, 72 for actual spec)
					if (dwSize >= 108)
					{
						_env.MemWrite32(lpDDSurfaceDesc + 76, 32); // dwSize of DDPIXELFORMAT
						_env.MemWrite32(lpDDSurfaceDesc + 80, (uint)DDPFFlags.DDPF_RGB);
						_env.MemWrite32(lpDDSurfaceDesc + 84, 0); // dwFourCC
						_env.MemWrite32(lpDDSurfaceDesc + 88, (uint)ddrawObj.BitsPerPixel); // dwRGBBitCount

						// Set RGB masks based on bit depth
						if (ddrawObj.BitsPerPixel == 16)
						{
							_env.MemWrite32(lpDDSurfaceDesc + 92, 0xF800); // Red mask (5 bits)
							_env.MemWrite32(lpDDSurfaceDesc + 96, 0x07E0); // Green mask (6 bits)
							_env.MemWrite32(lpDDSurfaceDesc + 100, 0x001F); // Blue mask (5 bits)
						}
						else if (ddrawObj.BitsPerPixel == 24 || ddrawObj.BitsPerPixel == 32)
						{
							_env.MemWrite32(lpDDSurfaceDesc + 92, 0x00FF0000); // Red mask
							_env.MemWrite32(lpDDSurfaceDesc + 96, 0x0000FF00); // Green mask
							_env.MemWrite32(lpDDSurfaceDesc + 100, 0x000000FF); // Blue mask
						}

						_env.MemWrite32(lpDDSurfaceDesc + 104, 0); // dwRGBAlphaBitMask
					}

					// Write ddsCaps (offset 108)
					// For primary surfaces, set DDSCAPS_PRIMARYSURFACE; for others, set DDSCAPS_OFFSCREENPLAIN
					if (dwSize >= 112)
					{
						const uint DDSCAPS_PRIMARYSURFACE = (uint)DDSCaps.DDSCAPS_PRIMARYSURFACE;
						const uint DDSCAPS_OFFSCREENPLAIN = (uint)DDSCaps.DDSCAPS_OFFSCREENPLAIN;
						var caps = surface.IsPrimary ? DDSCAPS_PRIMARYSURFACE : DDSCAPS_OFFSCREENPLAIN;
						_env.MemWrite32(lpDDSurfaceDesc + 108, caps); // ddsCaps.dwCaps
					}
				}
			}

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetPixelFormat(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDPixelFormat = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetPixelFormat(this=0x{ThisPtr:X8}, lpDDPixelFormat=0x{PixelFormat:X8})", thisPtr, lpDDPixelFormat);

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetPixelFormat: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			if (lpDDPixelFormat == 0)
			{
				_logger.LogError("[DDraw] GetPixelFormat: lpDDPixelFormat is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			if (lpDDPixelFormat != 0)
			{
				// Find the DirectDraw object to get BPP
				if (_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj))
				{
					// Fill DDPIXELFORMAT structure
					var pf = new DDPixelFormatRef(_env.Memory, lpDDPixelFormat);
					pf.dwSize = 32;
					pf.dwFlags = (uint)DDPFFlags.DDPF_RGB;
					pf.dwFourCC = 0;
					pf.dwRGBBitCount = (uint)ddrawObj.BitsPerPixel;

					// Set RGB masks based on bit depth
					if (ddrawObj.BitsPerPixel == 8)
					{
						// Palettized mode
						pf.dwFlags = (uint)DDPFFlags.DDPF_PALETTEINDEXED8;
						pf.dwRBitMask = 0;
						pf.dwGBitMask = 0;
						pf.dwBBitMask = 0;
					}
					else if (ddrawObj.BitsPerPixel == 16)
					{
						pf.dwRBitMask = 0xF800; // Red mask (5 bits)
						pf.dwGBitMask = 0x07E0; // Green mask (6 bits)
						pf.dwBBitMask = 0x001F; // Blue mask (5 bits)
					}
					else if (ddrawObj.BitsPerPixel == 24 || ddrawObj.BitsPerPixel == 32)
					{
						pf.dwRBitMask = 0x00FF0000; // Red mask
						pf.dwGBitMask = 0x0000FF00; // Green mask
						pf.dwBBitMask = 0x000000FF; // Blue mask
					}

					pf.dwRGBAlphaBitMask = 0;
				}
			}

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetPalette(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lplpDDPalette = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetPalette(this=0x{ThisPtr:X8}, lplpDDPalette=0x{LplpDDPalette:X8})", thisPtr, lplpDDPalette);

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetPalette: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			if (lplpDDPalette == 0)
			{
				_logger.LogError("[DDraw] GetPalette: lplpDDPalette is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Check if surface has a palette attached
			if (surface.PaletteHandle == 0)
			{
				_env.MemWrite32(lplpDDPalette, 0);
				_logger.LogInformation("[DDraw] Surface has no palette attached");
				return (uint)DDResult.DDERR_NOPALETTEATTACHED;
			}

			// Find the palette and return its COM object address
			if (_palettes.TryGetValue(surface.PaletteHandle, out var palette))
			{
				_env.MemWrite32(lplpDDPalette, palette.ComObjectAddress);
				_logger.LogInformation("[DDraw] Returning palette COM object at 0x{ComObjectAddr:X8}", palette.ComObjectAddress);
				return (uint)DDResult.DD_OK;
			}

			_logger.LogError("[DDraw] GetPalette: palette handle 0x{PaletteHandle:X8} not found", surface.PaletteHandle);
			return (uint)DDResult.DDERR_GENERIC;
		}

		private uint Surface_GetOverlayPosition(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lplX = args.UInt32(1);
			var lplY = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetOverlayPosition(this=0x{ThisPtr:X8}, lplX=0x{LplX:X8}, lplY=0x{LplY:X8})", thisPtr, lplX, lplY);

			// Overlays are not supported in this implementation
			// Return error indicating this is not an overlay surface
			return (uint)DDResult.DDERR_NOTAOVERLAYSURFACE;
		}

		private uint Surface_GetFlipStatus(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetFlipStatus(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8})", thisPtr, dwFlags);

			// In an emulator, flips complete instantly
			// Always return DD_OK to indicate no flips are pending
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetDC(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lphDC = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetDC(this=0x{ThisPtr:X8}, lphDC=0x{LphDC:X8})", thisPtr, lphDC);

			if (lphDC == 0)
			{
				_logger.LogError("[DDraw] GetDC: lphDC is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Create a device context handle and track which surface it belongs to
			// This allows ReleaseDC to properly validate and clean up
			// Note: The emulator runs Win32 applications in a single-threaded context,
			// so thread safety for _nextDCHandle++ and _surfaceDCs is not a concern
			var dcHandle = _nextDCHandle++;
			_surfaceDCs[dcHandle] = thisPtr;
			_env.MemWrite32(lphDC, dcHandle);

			_logger.LogInformation("[DDraw] Created DC handle 0x{DcHandle:X8} for surface COM object 0x{ThisPtr:X8}", dcHandle, thisPtr);
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetColorKey(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpDDColorKey = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetColorKey(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpDDColorKey=0x{ColorKey:X8})", thisPtr, dwFlags, lpDDColorKey);

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetColorKey: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			if (lpDDColorKey == 0)
			{
				_logger.LogError("[DDraw] GetColorKey: lpDDColorKey is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Check if surface has a color key
			if (!surface.HasColorKey)
			{
				_logger.LogInformation("[DDraw] Surface has no color key set");
				return (uint)DDResult.DDERR_NOCOLORKEY;
			}

			// Write DDCOLORKEY structure (2 DWORDs: dwColorSpaceLowValue and dwColorSpaceHighValue)
			_env.MemWrite32(lpDDColorKey, surface.ColorKeyLow);
			_env.MemWrite32(lpDDColorKey + 4, surface.ColorKeyHigh);

			_logger.LogInformation("[DDraw] Returning color key: low=0x{Low:X8}, high=0x{High:X8}", surface.ColorKeyLow, surface.ColorKeyHigh);

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetClipper(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lplpDDClipper = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetClipper(this=0x{ThisPtr:X8}, lplpDDClipper=0x{LplpDDClipper:X8})", thisPtr, lplpDDClipper);

			if (lplpDDClipper == 0)
			{
				_logger.LogError("[DDraw] GetClipper: lplpDDClipper is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetClipper: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Check if a clipper is attached to the surface
			if (surface.ClipperHandle == 0)
			{
				// Return null to indicate no clipper is attached
				_env.MemWrite32(lplpDDClipper, 0);
				_logger.LogInformation("[DDraw] No clipper attached to surface 0x{SurfaceHandle:X8}", surface.Handle);
				return (uint)DDResult.DDERR_NOCLIPPERATTACHED;
			}

			// Get the clipper and return its COM object address
			if (_clippers.TryGetValue(surface.ClipperHandle, out var clipper))
			{
				_env.MemWrite32(lplpDDClipper, clipper.ComObjectAddress);
				_logger.LogInformation("[DDraw] Returning clipper COM object 0x{ComAddr:X8} for surface 0x{SurfaceHandle:X8}", clipper.ComObjectAddress, surface.Handle);
				return (uint)DDResult.DD_OK;
			}

			// Clipper handle is set but clipper not found
			_logger.LogError("[DDraw] GetClipper: clipper handle 0x{ClipperHandle:X8} not found", surface.ClipperHandle);
			_env.MemWrite32(lplpDDClipper, 0);
			return (uint)DDResult.DDERR_INVALIDOBJECT;
		}

		private uint Surface_GetCaps(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSCaps = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetCaps(this=0x{ThisPtr:X8}, lpDDSCaps=0x{Caps:X8})", thisPtr, lpDDSCaps);

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetCaps: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			if (lpDDSCaps == 0)
			{
				_logger.LogError("[DDraw] GetCaps: lpDDSCaps is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Fill DDSCAPS structure
			uint caps = 0;

			// Primary surface flag
			if (surface.IsPrimary)
			{
				caps |= (uint)DDSCaps.DDSCAPS_PRIMARYSURFACE;
			}
			else
			{
				caps |= (uint)DDSCaps.DDSCAPS_OFFSCREENPLAIN;
			}

			// Memory location (always system memory in emulator)
			caps |= (uint)DDSCaps.DDSCAPS_VIDEOMEMORY; // Emulated

			// Complex surface (has attached surfaces)
			if (surface.AttachedSurfaces.Count > 0)
			{
				caps |= (uint)DDSCaps.DDSCAPS_COMPLEX;
			}

			// Flipping capability
			if (surface.IsPrimary && surface.AttachedSurfaces.Count > 0)
			{
				caps |= (uint)DDSCaps.DDSCAPS_FLIP;
			}

			_env.MemWrite32(lpDDSCaps, caps);
			_logger.LogInformation("[DDraw] Surface caps: 0x{Caps:X8}", caps);

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetBltStatus(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetBltStatus(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8})", thisPtr, dwFlags);

			// In an emulator, blits complete instantly
			// Always return DD_OK to indicate no blits are pending
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_GetAttachedSurface(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSCaps = args.UInt32(1);
			var lplpDDAttachedSurface = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetAttachedSurface(this=0x{ThisPtr:X8}, lpDDSCaps=0x{LpDDSCaps:X8}, lplp=0x{Lplp:X8})", thisPtr, lpDDSCaps, lplpDDAttachedSurface);

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetAttachedSurface: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				if (lplpDDAttachedSurface != 0)
				{
					_env.MemWrite32(lplpDDAttachedSurface, 0);
				}

				return (uint)DDResult.DDERR_NOTFOUND;
			}

			// Log diagnostic information about the found surface
			_logger.LogInformation("[DDraw] Found surface: Handle=0x{Handle:X8}, ComAddr=0x{ComAddr:X8}, IsPrimary={IsPrimary}, AttachedSurfaces.Count={Count}", surface.Handle, surface.ComObjectAddress, surface.IsPrimary, surface.AttachedSurfaces.Count);

			// Log all attached surface handles for debugging
			if (surface.AttachedSurfaces.Count > 0)
			{
				_logger.LogInformation("[DDraw] Attached surfaces: {Handles}", string.Join(", ", surface.AttachedSurfaces.Select(h => $"0x{h:X8}")));
			}

			// Read the requested capabilities
			var dwCaps = lpDDSCaps != 0 ? _env.MemRead32(lpDDSCaps) : 0;
			_logger.LogInformation("[DDraw] Requested surface caps: 0x{Caps:X8}", dwCaps);

			// Check if there are any attached surfaces
			if (surface.AttachedSurfaces.Count == 0)
			{
				_logger.LogInformation("[DDraw] No attached surfaces found for surface 0x{Handle:X8}", surface.Handle);

				// If a backbuffer is requested, create one on-demand
				// DDSCAPS_BACKBUFFER = 0x00000004
				const uint DDSCAPS_BACKBUFFER = (uint)DDSCaps.DDSCAPS_BACKBUFFER;

				// Log diagnostic information
				_logger.LogInformation("[DDraw] Surface diagnostic: IsPrimary={IsPrimary}, dwCaps=0x{Caps:X8}, backbuffer requested={BackbufferRequested}", surface.IsPrimary, dwCaps, (dwCaps & DDSCAPS_BACKBUFFER) != 0);

				// Create backbuffer on-demand if requested, regardless of whether the surface was
				// originally marked as primary. Some applications may not set the primary flag correctly,
				// or may request a backbuffer for any surface that needs flipping capabilities.
				if ((dwCaps & DDSCAPS_BACKBUFFER) != 0)
				{
					_logger.LogInformation("[DDraw] Backbuffer requested, creating on-demand for surface 0x{Handle:X8}", surface.Handle);

					// Get the DirectDraw object to determine bits per pixel
					if (_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj))
					{
						// Create a backbuffer surface
						var backBufferHandle = _nextSurfaceHandle++;
						var backBuffer = new DirectDrawSurface
						{
							Handle = backBufferHandle,
							Width = surface.Width,
							Height = surface.Height,
							DirectDrawHandle = surface.DirectDrawHandle,
							IsPrimary = false,
							Pitch = surface.Width * (ddrawObj.BitsPerPixel / 8)
						};

						// Allocate memory for the backbuffer
						backBuffer.Bits = new byte[backBuffer.Pitch * backBuffer.Height];

						// Store the backbuffer
						_surfaces[backBufferHandle] = backBuffer;

						// Create COM vtable for backbuffer
						// IMPORTANT: Methods MUST be in exact COM interface order to match primary surface vtable
						// Using List<KeyValuePair> to guarantee insertion order
						var backBufferVtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
						{
							new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
							new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
							new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>((cpu, mem) => ComRelease(cpu, mem))),
							new("AddAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.AddAttachedSurface>((cpu, mem) => Surface_AddAttachedSurface(cpu, mem))),
							new("AddOverlayDirtyRect", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.AddOverlayDirtyRect>((cpu, mem) => Surface_AddOverlayDirtyRect(cpu, mem))),
							new("Blt", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Blt>((cpu, mem) => Surface_Blt(cpu, mem))),
							new("BltBatch", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.BltBatch>((cpu, mem) => Surface_BltBatch(cpu, mem))),
							new("BltFast", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.BltFast>((cpu, mem) => Surface_BltFast(cpu, mem))),
							new("DeleteAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.DeleteAttachedSurface>((cpu, mem) => Surface_DeleteAttachedSurface(cpu, mem))),
							new("EnumAttachedSurfaces", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.EnumAttachedSurfaces>((cpu, mem) => Surface_EnumAttachedSurfaces(cpu, mem))),
							new("EnumOverlayZOrders", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.EnumOverlayZOrders>((cpu, mem) => Surface_EnumOverlayZOrders(cpu, mem))),
							new("Flip", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Flip>((cpu, mem) => Surface_Flip(cpu, mem))),
							new("GetAttachedSurface", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetAttachedSurface>((cpu, mem) => Surface_GetAttachedSurface(cpu, mem))),
							new("GetBltStatus", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetBltStatus>((cpu, mem) => Surface_GetBltStatus(cpu, mem))),
							new("GetCaps", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetCaps>((cpu, mem) => Surface_GetCaps(cpu, mem))),
							new("GetClipper", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetClipper>((cpu, mem) => Surface_GetClipper(cpu, mem))),
							new("GetColorKey", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetColorKey>((cpu, mem) => Surface_GetColorKey(cpu, mem))),
							new("GetDC", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetDC>((cpu, mem) => Surface_GetDC(cpu, mem))),
							new("GetFlipStatus", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetFlipStatus>((cpu, mem) => Surface_GetFlipStatus(cpu, mem))),
							new("GetOverlayPosition", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetOverlayPosition>((cpu, mem) => Surface_GetOverlayPosition(cpu, mem))),
							new("GetPalette", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetPalette>((cpu, mem) => Surface_GetPalette(cpu, mem))),
							new("GetPixelFormat", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetPixelFormat>((cpu, mem) => Surface_GetPixelFormat(cpu, mem))),
							new("GetSurfaceDesc", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.GetSurfaceDesc>((cpu, mem) => Surface_GetSurfaceDesc(cpu, mem))),
							new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Initialize>((cpu, mem) => Surface_Initialize(cpu, mem))),
							new("IsLost", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.IsLost>((cpu, mem) => Surface_IsLost(cpu, mem))),
							new("Lock", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Lock>((cpu, mem) => Surface_Lock(cpu, mem, backBufferHandle))),
							new("ReleaseDC", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.ReleaseDC>((cpu, mem) => Surface_ReleaseDC(cpu, mem))),
							new("Restore", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Restore>((cpu, mem) => Surface_Restore(cpu, mem))),
							new("SetClipper", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetClipper>((cpu, mem) => Surface_SetClipper(cpu, mem))),
							new("SetColorKey", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetColorKey>((cpu, mem) => Surface_SetColorKey(cpu, mem))),
							new("SetOverlayPosition", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetOverlayPosition>((cpu, mem) => Surface_SetOverlayPosition(cpu, mem))),
							new("SetPalette", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.SetPalette>((cpu, mem) => Surface_SetPalette(cpu, mem, backBufferHandle))),
							new("Unlock", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.Unlock>((cpu, mem) => Surface_Unlock(cpu, mem, backBufferHandle))),
							new("UpdateOverlay", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlay>((cpu, mem) => Surface_UpdateOverlay(cpu, mem))),
							new("UpdateOverlayDisplay", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlayDisplay>((cpu, mem) => Surface_UpdateOverlayDisplay(cpu, mem))),
							new("UpdateOverlayZOrder", ComVtableDispatcher.FromDelegate<IDirectDrawSurface.UpdateOverlayZOrder>((cpu, mem) => Surface_UpdateOverlayZOrder(cpu, mem)))
						};

						var backBufferComAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDrawSurface", backBufferVtableMethods);
						backBuffer.ComObjectAddress = backBufferComAddr;

						// Attach the backbuffer to the primary surface
						surface.AttachedSurfaces.Add(backBufferHandle);

						_logger.LogInformation("[DDraw] Created on-demand backbuffer at surface handle 0x{Handle:X8}, COM object at 0x{ComAddr:X8}", backBufferHandle, backBufferComAddr);

						// Return the newly created backbuffer
						if (lplpDDAttachedSurface != 0)
						{
							_env.MemWrite32(lplpDDAttachedSurface, backBuffer.ComObjectAddress);
						}

						_logger.LogInformation("[DDraw] Returning on-demand backbuffer COM object at 0x{ComAddr:X8}", backBuffer.ComObjectAddress);
						return (uint)DDResult.DD_OK;
					}
					else
					{
						_logger.LogError("[DDraw] Could not find DirectDraw object for on-demand backbuffer creation");
					}
				}

				if (lplpDDAttachedSurface != 0)
				{
					_env.MemWrite32(lplpDDAttachedSurface, 0);
				}

				return (uint)DDResult.DDERR_NOTFOUND;
			}

			// For now, return the first attached surface (typically the backbuffer)
			// In a complete implementation, we would filter by the requested capabilities
			var attachedSurfaceHandle = surface.AttachedSurfaces[0];
			if (_surfaces.TryGetValue(attachedSurfaceHandle, out var attachedSurface))
			{
				if (lplpDDAttachedSurface != 0)
				{
					_env.MemWrite32(lplpDDAttachedSurface, attachedSurface.ComObjectAddress);
				}

				_logger.LogInformation("[DDraw] Returning attached surface COM object at 0x{ComAddr:X8}", attachedSurface.ComObjectAddress);
				return (uint)DDResult.DD_OK;
			}

			_logger.LogError("[DDraw] GetAttachedSurface: attached surface handle 0x{Handle:X8} not found", attachedSurfaceHandle);
			if (lplpDDAttachedSurface != 0)
			{
				_env.MemWrite32(lplpDDAttachedSurface, 0);
			}

			return (uint)DDResult.DDERR_NOTFOUND;
		}

		private uint Surface_Flip(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceTargetOverride = args.UInt32(1);
			var dwFlags = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::Flip(this=0x{ThisPtr:X8}, lpDDSurfaceTargetOverride=0x{Target:X8}, dwFlags=0x{DwFlags:X8})", thisPtr, lpDDSurfaceTargetOverride, dwFlags);

			// Find the surface by COM object address (thisPtr)
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] Flip: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// If this is a primary surface, present the frame to the rendering backend
			if (_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj) && ddrawObj.RenderingBackend != null)
			{
				try
				{
					// Process events to keep the window responsive
					ddrawObj.RenderingBackend.ProcessEvents();

					// The frame was already updated in Surface_Unlock, so Flip just ensures presentation
					_logger.LogInformation("[DDraw] Flipped primary surface");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[DDraw] Failed to flip surface");
					return (uint)DDResult.DDERR_GENERIC;
				}
			}

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_EnumOverlayZOrders(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpContext = args.UInt32(2);
			var lpfnCallback = args.UInt32(3);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::EnumOverlayZOrders(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpContext=0x{LpContext:X8}, lpfnCallback=0x{LpfnCallback:X8})", thisPtr, dwFlags, lpContext, lpfnCallback);

			if (lpfnCallback == 0)
			{
				_logger.LogError("[DDraw] EnumOverlayZOrders: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] EnumOverlayZOrders: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Callback signature: HRESULT WINAPI EnumSurfacesCallback(LPDIRECTDRAWSURFACE lpDDSurface, LPDDSURFACEDESC lpDDSurfaceDesc, LPVOID lpContext)
			// dwFlags: DDENUMOVERLAYZ_BACKTOFRONT (0) or DDENUMOVERLAYZ_FRONTTOBACK (1)

			// In our emulator, we don't support overlay surfaces, so there are no overlays to enumerate.
			// According to DirectX documentation, if there are no overlays, we should simply return DD_OK
			// without calling the callback at all.

			_logger.LogInformation("[DDraw] No overlay surfaces to enumerate (overlay surfaces not implemented in emulator)");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_EnumAttachedSurfaces(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpContext = args.UInt32(1);
			var lpEnumSurfacesCallback = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::EnumAttachedSurfaces(this=0x{ThisPtr:X8}, lpContext=0x{LpContext:X8}, lpEnumSurfacesCallback=0x{LpEnumSurfacesCallback:X8})", thisPtr, lpContext, lpEnumSurfacesCallback);

			if (lpEnumSurfacesCallback == 0)
			{
				_logger.LogError("[DDraw] EnumAttachedSurfaces: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Find the surface by COM object address
			var surface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (surface == null)
			{
				_logger.LogError("[DDraw] EnumAttachedSurfaces: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Callback signature: HRESULT WINAPI EnumSurfacesCallback(LPDIRECTDRAWSURFACE lpDDSurface, LPDDSURFACEDESC lpDDSurfaceDesc, LPVOID lpContext)
			// Enumerate all surfaces attached to this surface

			try
			{
				_logger.LogInformation("[DDraw] Enumerating {Count} attached surface(s) for surface 0x{Handle:X8}", surface.AttachedSurfaces.Count, surface.Handle);

				var callbackHelper = new CallbackHelper(_currentCpu!, _currentMemory!, _logger);

				// Enumerate each attached surface
				foreach (var attachedSurfaceHandle in surface.AttachedSurfaces)
				{
					if (!_surfaces.TryGetValue(attachedSurfaceHandle, out var attachedSurface))
					{
						_logger.LogWarning("[DDraw] EnumAttachedSurfaces: could not find attached surface 0x{Handle:X8}, skipping", attachedSurfaceHandle);
						continue;
					}

					// Allocate DDSURFACEDESC structure (108 bytes minimum)
					var surfaceDescPtr = AllocateMemory(108);
					var surfaceDesc = new DDSurfaceDescRef(_env.Memory, surfaceDescPtr);

					// Get DirectDraw object to determine BPP
					_ddrawObjects.TryGetValue(attachedSurface.DirectDrawHandle, out var ddrawObj);

					// Fill in the structure using ref struct
					surfaceDesc.dwSize = 108;
					surfaceDesc.dwFlags = DDSD.CAPS | DDSD.WIDTH | DDSD.HEIGHT | DDSD.PITCH | DDSD.PIXELFORMAT;
					surfaceDesc.dwHeight = (uint)attachedSurface.Height;
					surfaceDesc.dwWidth = (uint)attachedSurface.Width;
					surfaceDesc.lPitch = (uint)attachedSurface.Pitch;

					// Fill in pixel format using nested ref struct
					if (ddrawObj != null)
					{
						var pixelFormat = surfaceDesc.ddpfPixelFormat;
						pixelFormat.dwSize = 32;
						pixelFormat.dwFlags = (uint)DDPFFlags.DDPF_RGB;
						pixelFormat.dwFourCC = 0;
						pixelFormat.dwRGBBitCount = (uint)ddrawObj.BitsPerPixel;

						// Set RGB masks based on bit depth
						if (ddrawObj.BitsPerPixel == 8)
						{
							// Palettized mode
							pixelFormat.dwFlags = (uint)DDPFFlags.DDPF_PALETTEINDEXED8;
							pixelFormat.dwRBitMask = 0;
							pixelFormat.dwGBitMask = 0;
							pixelFormat.dwBBitMask = 0;
						}
						else if (ddrawObj.BitsPerPixel == 16)
						{
							pixelFormat.dwRBitMask = 0xF800;
							pixelFormat.dwGBitMask = 0x07E0;
							pixelFormat.dwBBitMask = 0x001F;
						}
						else if (ddrawObj.BitsPerPixel == 24 || ddrawObj.BitsPerPixel == 32)
						{
							pixelFormat.dwRBitMask = 0x00FF0000;
							pixelFormat.dwGBitMask = 0x0000FF00;
							pixelFormat.dwBBitMask = 0x000000FF;
						}

						pixelFormat.dwRGBAlphaBitMask = 0;
					}

					// Set surface caps
					uint caps = 0;
					if (attachedSurface.IsPrimary)
					{
						caps |= (uint)DDSCaps.DDSCAPS_PRIMARYSURFACE;
					}
					else
					{
						// Attached surfaces are typically backbuffers
						caps |= (uint)DDSCaps.DDSCAPS_BACKBUFFER;
						caps |= (uint)DDSCaps.DDSCAPS_FLIP;
					}

					// If there are more attached surfaces, this is a complex surface
					if (attachedSurface.AttachedSurfaces.Count > 0)
					{
						caps |= (uint)DDSCaps.DDSCAPS_COMPLEX;
					}

					surfaceDesc.dwSurfaceCaps = caps;

					_logger.LogDebug("[DDraw] Enumerating attached surface: 0x{Handle:X8} (COM=0x{ComAddr:X8}, {Width}x{Height}, caps=0x{Caps:X8})", attachedSurface.Handle, attachedSurface.ComObjectAddress, attachedSurface.Width, attachedSurface.Height, caps);

					// Invoke callback: EnumSurfacesCallback(lpDDSurface, lpDDSurfaceDesc, lpContext)
					var parameters = new uint[] { attachedSurface.ComObjectAddress, surfaceDescPtr, lpContext };
					var result = callbackHelper.InvokeStdcallCallback(lpEnumSurfacesCallback, parameters);

					// Free allocated structure
					FreeMemory(surfaceDescPtr);

					if (result == null)
					{
						_logger.LogError("[DDraw] EnumAttachedSurfaces: callback invocation failed");
						return (uint)DDResult.DDERR_GENERIC;
					}

					// Callback returns DDENUMRET_OK (1) to continue, DDENUMRET_CANCEL (0) to stop
					if (result.Value == 0)
					{
						_logger.LogInformation("[DDraw] EnumAttachedSurfaces: callback requested cancellation");
						break;
					}
				}

				return (uint)DDResult.DD_OK;
			}
			catch (OutOfMemoryException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (StackOverflowException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (AccessViolationException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (System.Threading.ThreadAbortException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] EnumAttachedSurfaces: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		private uint Surface_DeleteAttachedSurface(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpDDSAttachedSurface = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::DeleteAttachedSurface(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpDDSAttachedSurface=0x{LpDDSAttachedSurface:X8})", thisPtr, dwFlags, lpDDSAttachedSurface);

			if (lpDDSAttachedSurface == 0)
			{
				_logger.LogError("[DDraw] DeleteAttachedSurface: lpDDSAttachedSurface is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Find the destination surface by COM object address
			var destSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (destSurface == null)
			{
				_logger.LogError("[DDraw] DeleteAttachedSurface: could not find destination surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Find the surface to detach by COM object address
			var detachSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == lpDDSAttachedSurface);

			if (detachSurface == null)
			{
				_logger.LogError("[DDraw] DeleteAttachedSurface: could not find surface to detach with COM address 0x{LpDDSAttachedSurface:X8}", lpDDSAttachedSurface);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Check if it's attached
			if (!destSurface.AttachedSurfaces.Contains(detachSurface.Handle))
			{
				_logger.LogWarning("[DDraw] Surface 0x{DetachHandle:X8} is not attached to surface 0x{DestHandle:X8}", detachSurface.Handle, destSurface.Handle);
				return (uint)DDResult.DDERR_SURFACENOTATTACHED;
			}

			// Detach the surface
			destSurface.AttachedSurfaces.Remove(detachSurface.Handle);
			_logger.LogInformation("[DDraw] Detached surface 0x{DetachHandle:X8} from surface 0x{DestHandle:X8}", detachSurface.Handle, destSurface.Handle);

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_BltFast(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwX = args.UInt32(1);
			var dwY = args.UInt32(2);
			var lpDDSrcSurface = args.UInt32(3);
			var lpSrcRect = args.UInt32(4);
			var dwTrans = args.UInt32(5);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::BltFast(this=0x{ThisPtr:X8}, x={X}, y={Y}, lpDDSrcSurface=0x{SrcSurface:X8}, lpSrcRect=0x{SrcRect:X8}, dwTrans=0x{Trans:X8})", thisPtr, dwX, dwY, lpDDSrcSurface, lpSrcRect, dwTrans);

			// Find destination surface by COM object address
			var destSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (destSurface == null || destSurface.Bits == null)
			{
				_logger.LogError("[DDraw] BltFast: could not find destination surface");
				return (uint)DDResult.DDERR_GENERIC;
			}

			// Find source surface by COM object address
			DirectDrawSurface? srcSurface = null;
			if (lpDDSrcSurface != 0)
			{
				srcSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == lpDDSrcSurface);
			}

			if (srcSurface?.Bits == null)
			{
				_logger.LogError("[DDraw] BltFast: could not find source surface");
				return (uint)DDResult.DDERR_GENERIC;
			}

			// Read source rectangle if provided
			int srcX = 0, srcY = 0, srcWidth = srcSurface.Width, srcHeight = srcSurface.Height;
			if (lpSrcRect != 0)
			{
				var srcRect = new RectRef(_env.Memory, lpSrcRect);
				srcX = srcRect.left;
				srcY = srcRect.top;
				srcWidth = srcRect.right - srcX;
				srcHeight = srcRect.bottom - srcY;
			}

			// Get bits per pixel from DirectDraw object
			if (!_ddrawObjects.TryGetValue(destSurface.DirectDrawHandle, out var ddrawObj))
			{
				_logger.LogError("[DDraw] BltFast: could not find DirectDraw object");
				return (uint)DDResult.DDERR_GENERIC;
			}

			var bytesPerPixel = ddrawObj.BitsPerPixel / 8;

			// Calculate destination position and clipping
			var destX = (int)dwX;
			var destY = (int)dwY;

			_logger.LogDebug("[DDraw] BltFast: dest={DestW}x{DestH}, src=({SrcX},{SrcY},{SrcW}x{SrcH}), destPos=({DestX},{DestY})", 
				destSurface.Width, destSurface.Height, srcX, srcY, srcWidth, srcHeight, destX, destY);

			// Check if destination is completely out of bounds (right, bottom, left, or top)
			if (destX >= destSurface.Width || destY >= destSurface.Height ||
				destX + srcWidth <= 0 || destY + srcHeight <= 0)
			{
				_logger.LogDebug("[DDraw] BltFast: destination region ({X},{Y},{W}x{H}) is completely outside destination surface ({Width}x{Height})", destX, destY, srcWidth, srcHeight, destSurface.Width, destSurface.Height);
				return (uint)DDResult.DD_OK;
			}

			// Clip the source rectangle if destination goes out of bounds
			if (destX < 0)
			{
				srcX -= destX;
				srcWidth += destX;
				destX = 0;
			}

			if (destY < 0)
			{
				srcY -= destY;
				srcHeight += destY;
				destY = 0;
			}

			if (destX + srcWidth > destSurface.Width)
			{
				srcWidth = destSurface.Width - destX;
			}

			if (destY + srcHeight > destSurface.Height)
			{
				srcHeight = destSurface.Height - destY;
			}

			// Validate source rectangle is within source surface bounds
			if (srcX < 0 || srcY < 0 || srcX + srcWidth > srcSurface.Width || srcY + srcHeight > srcSurface.Height)
			{
				_logger.LogError("[DDraw] BltFast: source rectangle out of bounds");
				return (uint)DDResult.DDERR_GENERIC;
			}

			// Check if width/height are valid after clipping
			if (srcWidth <= 0 || srcHeight <= 0)
			{
				_logger.LogDebug("[DDraw] BltFast: nothing to blit after clipping (srcWidth={W}, srcHeight={H})", srcWidth, srcHeight);
				return (uint)DDResult.DD_OK;
			}

			// Calculate offsets in the source and destination buffers
			var srcOffset = srcY * srcSurface.Pitch + srcX * bytesPerPixel;
			var destOffset = destY * destSurface.Pitch + destX * bytesPerPixel;

			// Get spans for the blitter
			var srcSpan = srcSurface.Bits.AsSpan(srcOffset);
			var destSpan = destSurface.Bits.AsSpan(destOffset);

			// DDBLTFAST_SRCCOLORKEY = 0x00000001
			var useSrcColorKey = (dwTrans & 0x00000001) != 0 && srcSurface.HasColorKey;

			// Use OptimizedBlitter for high-performance blitting
			if (useSrcColorKey)
			{
				OptimizedBlitter.BltWithSourceColorKey(
					destSpan,
					srcSpan,
					destSurface.Pitch,
					srcSurface.Pitch,
					srcWidth,
					srcHeight,
					bytesPerPixel,
					srcSurface.ColorKeyLow,
					srcSurface.ColorKeyHigh);
			}
			else
			{
				OptimizedBlitter.BltFast(
					destSpan,
					srcSpan,
					destSurface.Pitch,
					srcSurface.Pitch,
					srcWidth,
					srcHeight,
					bytesPerPixel);
			}

			// Mark destination surface as dirty
			destSurface.IsTextureDirty = true;

			return (uint)DDResult.DD_OK;
		}


		private uint Surface_BltBatch(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::BltBatch() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_Blt(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDestRect = args.UInt32(1);
			var lpDDSrcSurface = args.UInt32(2);
			var lpSrcRect = args.UInt32(3);
			var dwFlags = args.UInt32(4);
			var lpDDBltFx = args.UInt32(5);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::Blt(this=0x{ThisPtr:X8}, lpDestRect=0x{DestRect:X8}, lpDDSrcSurface=0x{SrcSurface:X8}, lpSrcRect=0x{SrcRect:X8}, dwFlags=0x{DwFlags:X8}, lpDDBltFx=0x{BltFx:X8})", thisPtr, lpDestRect, lpDDSrcSurface, lpSrcRect, dwFlags, lpDDBltFx);

			// Find destination surface by COM object address
			var destSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (destSurface?.Bits == null)
			{
				_logger.LogError("[DDraw] Blt: could not find destination surface");
				return (uint)DDResult.DDERR_GENERIC;
			}

			// Read destination rectangle if provided
			int destX = 0, destY = 0, destWidth = destSurface.Width, destHeight = destSurface.Height;
			if (lpDestRect != 0)
			{
				var destRect = new RectRef(_env.Memory, lpDestRect);
				destX = destRect.left;
				destY = destRect.top;
				destWidth = destRect.right - destX;
				destHeight = destRect.bottom - destY;
			}

			// Check for color fill operation (DDBLT_COLORFILL = 0x00000400)
			if ((dwFlags & (uint)DDBlt.DDBLT_COLORFILL) != 0 && lpDDBltFx != 0)
			{
				// Read fill color from DDBLTFX structure
				var fillColor = _env.MemRead32(lpDDBltFx + 16); // dwFillColor offset

				// Get bits per pixel from DirectDraw object
				if (!_ddrawObjects.TryGetValue(destSurface.DirectDrawHandle, out var ddrawObj))
				{
					_logger.LogError("[DDraw] Blt: could not find DirectDraw object for color fill");
					return (uint)DDResult.DDERR_GENERIC;
				}

				// Perform color fill
				var bytesPerPixel = ddrawObj.BitsPerPixel / 8;
				for (var y = destY; y < destY + destHeight && y < destSurface.Height; y++)
				{
					for (var x = destX; x < destX + destWidth && x < destSurface.Width; x++)
					{
						var offset = y * destSurface.Pitch + x * bytesPerPixel;
						if (offset + bytesPerPixel - 1 < destSurface.Bits.Length)
						{
							switch (bytesPerPixel)
							{
								case 1: // 8-bit
									destSurface.Bits[offset] = (byte)(fillColor & 0xFF);
									break;
								case 2: // 16-bit
									destSurface.Bits[offset] = (byte)(fillColor & 0xFF);
									destSurface.Bits[offset + 1] = (byte)((fillColor >> 8) & 0xFF);
									break;
								case 3: // 24-bit
									destSurface.Bits[offset] = (byte)(fillColor & 0xFF);
									destSurface.Bits[offset + 1] = (byte)((fillColor >> 8) & 0xFF);
									destSurface.Bits[offset + 2] = (byte)((fillColor >> 16) & 0xFF);
									break;
								case 4: // 32-bit
									destSurface.Bits[offset] = (byte)(fillColor & 0xFF);
									destSurface.Bits[offset + 1] = (byte)((fillColor >> 8) & 0xFF);
									destSurface.Bits[offset + 2] = (byte)((fillColor >> 16) & 0xFF);
									destSurface.Bits[offset + 3] = (byte)((fillColor >> 24) & 0xFF);
									break;
							}
						}
					}
				}

				// Mark destination surface as dirty
				destSurface.IsTextureDirty = true;

				_logger.LogInformation("[DDraw] Performed color fill with color 0x{FillColor:X8}", fillColor);
				return (uint)DDResult.DD_OK;
			}

			// Handle source surface blit
			if (lpDDSrcSurface == 0)
			{
				return (uint)DDResult.DD_OK;
			}

			// Find source surface by COM object address
			var srcSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == lpDDSrcSurface);

			if (srcSurface == null || srcSurface.Bits == null)
			{
				return (uint)DDResult.DD_OK;
			}

			// Read source rectangle if provided
			int srcX = 0, srcY = 0, srcWidth = srcSurface.Width, srcHeight = srcSurface.Height;
			if (lpSrcRect != 0)
			{
				var srcRect = new RectRef(_env.Memory, lpSrcRect);
				srcX = srcRect.left;
				srcY = srcRect.top;
				srcWidth = srcRect.right - srcX;
				srcHeight = srcRect.bottom - srcY;
			}

			// Get bits per pixel from DirectDraw object
			if (!_ddrawObjects.TryGetValue(destSurface.DirectDrawHandle, out var ddrawObj2))
			{
				_logger.LogError("[DDraw] Blt: could not find DirectDraw object");
				return (uint)DDResult.DDERR_GENERIC;
			}

			var bytesPerPixel2 = ddrawObj2.BitsPerPixel / 8;

			// Clip rectangles to surface bounds
			if (destX < 0)
			{
				srcX -= destX;
				destWidth += destX;
				destX = 0;
			}

			if (destY < 0)
			{
				srcY -= destY;
				destHeight += destY;
				destY = 0;
			}

			if (destX + destWidth > destSurface.Width)
			{
				destWidth = destSurface.Width - destX;
			}

			if (destY + destHeight > destSurface.Height)
			{
				destHeight = destSurface.Height - destY;
			}

			// Validate source rectangle
			if (srcX < 0 || srcY < 0 || srcX + srcWidth > srcSurface.Width || srcY + srcHeight > srcSurface.Height)
			{
				_logger.LogError("[DDraw] Blt: source rectangle out of bounds");
				return (uint)DDResult.DDERR_GENERIC;
			}

			// Check if there's anything to blit
			if (destWidth <= 0 || destHeight <= 0)
			{
				_logger.LogDebug("[DDraw] Blt: nothing to blit after clipping");
				return (uint)DDResult.DD_OK;
			}

			// Calculate offsets
			var srcOffset = srcY * srcSurface.Pitch + srcX * bytesPerPixel2;
			var destOffset = destY * destSurface.Pitch + destX * bytesPerPixel2;

			// Get spans for blitter
			var srcSpan = srcSurface.Bits.AsSpan(srcOffset);
			var destSpan = destSurface.Bits.AsSpan(destOffset);

			// DDBLT_KEYSRC = 0x00008000
			var useSrcColorKey = (dwFlags & (uint)DDBlt.DDBLT_KEYSRC) != 0 && srcSurface.HasColorKey;

			// Use OptimizedBlitter for high-performance blitting
			if (useSrcColorKey)
			{
				OptimizedBlitter.BltWithSourceColorKey(
					destSpan,
					srcSpan,
					destSurface.Pitch,
					srcSurface.Pitch,
					destWidth,
					destHeight,
					bytesPerPixel2,
					srcSurface.ColorKeyLow,
					srcSurface.ColorKeyHigh);
			}
			else
			{
				OptimizedBlitter.BltFast(
					destSpan,
					srcSpan,
					destSurface.Pitch,
					srcSurface.Pitch,
					destWidth,
					destHeight,
					bytesPerPixel2);
			}

			// Mark destination surface as dirty
			destSurface.IsTextureDirty = true;

			_logger.LogInformation("[DDraw] Performed blit from source surface");

			return (uint)DDResult.DD_OK;
		}


		private uint Surface_AddOverlayDirtyRect(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::AddOverlayDirtyRect() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_AddAttachedSurface(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSAttachedSurface = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::AddAttachedSurface(this=0x{ThisPtr:X8}, lpDDSAttachedSurface=0x{LpDDSAttachedSurface:X8})", thisPtr, lpDDSAttachedSurface);

			if (lpDDSAttachedSurface == 0)
			{
				_logger.LogError("[DDraw] AddAttachedSurface: lpDDSAttachedSurface is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Find the destination surface by COM object address
			var destSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == thisPtr);

			if (destSurface == null)
			{
				_logger.LogError("[DDraw] AddAttachedSurface: could not find destination surface with COM address 0x{ThisPtr:X8}", thisPtr);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Find the surface to attach by COM object address
			var attachSurface = _surfaces.Values.FirstOrDefault(s => s.ComObjectAddress == lpDDSAttachedSurface);

			if (attachSurface == null)
			{
				_logger.LogError("[DDraw] AddAttachedSurface: could not find surface to attach with COM address 0x{LpDDSAttachedSurface:X8}", lpDDSAttachedSurface);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			// Check if already attached
			if (destSurface.AttachedSurfaces.Contains(attachSurface.Handle))
			{
				_logger.LogWarning("[DDraw] Surface 0x{AttachHandle:X8} is already attached to surface 0x{DestHandle:X8}", attachSurface.Handle, destSurface.Handle);
				return (uint)DDResult.DDERR_SURFACEALREADYATTACHED;
			}

			// Attach the surface
			destSurface.AttachedSurfaces.Add(attachSurface.Handle);
			_logger.LogInformation("[DDraw] Attached surface 0x{AttachHandle:X8} to surface 0x{DestHandle:X8}", attachSurface.Handle, destSurface.Handle);

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_DuplicateSurface(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::DuplicateSurface() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_EnumDisplayModes(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpDDSurfaceDesc = args.UInt32(2);
			var lpContext = args.UInt32(3);
			var lpEnumModesCallback = args.UInt32(4);

			_logger.LogInformation("[DDraw COM] IDirectDraw::EnumDisplayModes(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpDDSurfaceDesc=0x{LpDDSurfaceDesc:X8}, lpContext=0x{LpContext:X8}, lpEnumModesCallback=0x{LpEnumModesCallback:X8})", thisPtr, dwFlags, lpDDSurfaceDesc, lpContext, lpEnumModesCallback);

			if (lpEnumModesCallback == 0)
			{
				_logger.LogError("[DDraw] EnumDisplayModes: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Callback signature: HRESULT WINAPI EnumModesCallback(LPDDSURFACEDESC lpDDSurfaceDesc, LPVOID lpContext)
			// Enumerate common display modes for emulation

			try
			{
				// Common display modes to enumerate
				var displayModes = new[]
				{
					new { Width = 640, Height = 480, Bpp = 8 },
					new { Width = 640, Height = 480, Bpp = 16 },
					new { Width = 640, Height = 480, Bpp = 32 },
					new { Width = 800, Height = 600, Bpp = 8 },
					new { Width = 800, Height = 600, Bpp = 16 },
					new { Width = 800, Height = 600, Bpp = 32 },
					new { Width = 1024, Height = 768, Bpp = 16 },
					new { Width = 1024, Height = 768, Bpp = 32 },
				};

				var callbackHelper = new CallbackHelper(_currentCpu!, _currentMemory!, _logger);

				foreach (var mode in displayModes)
				{
					// Allocate DDSURFACEDESC structure (108 bytes minimum)
					var surfaceDescPtr = AllocateMemory(108);
					var surfaceDesc = new DDSurfaceDescRef(_env.Memory, surfaceDescPtr)
					{
						// Fill in the structure using ref struct
						dwSize = 108,
						dwFlags = DDSD.WIDTH | DDSD.HEIGHT | DDSD.PIXELFORMAT,
						dwHeight = (uint)mode.Height,
						dwWidth = (uint)mode.Width,
						lPitch = (uint)(mode.Width * (mode.Bpp / 8))
					};

					// Fill in pixel format using nested ref struct
					var pixelFormat = surfaceDesc.ddpfPixelFormat;
					pixelFormat.dwSize = 32;
					pixelFormat.dwFlags = (uint)DDPFFlags.DDPF_RGB;
					pixelFormat.dwFourCC = 0;
					pixelFormat.dwRGBBitCount = (uint)mode.Bpp;

					// Set RGB masks based on bit depth
					if (mode.Bpp == 16)
					{
						pixelFormat.dwRBitMask = 0xF800;
						pixelFormat.dwGBitMask = 0x07E0;
						pixelFormat.dwBBitMask = 0x001F;
					}
					else if (mode.Bpp == 24 || mode.Bpp == 32)
					{
						pixelFormat.dwRBitMask = 0x00FF0000;
						pixelFormat.dwGBitMask = 0x0000FF00;
						pixelFormat.dwBBitMask = 0x000000FF;
					}

					_logger.LogDebug("[DDraw] Enumerating mode: {Width}x{Height}x{Bpp}", mode.Width, mode.Height, mode.Bpp);

					// Invoke callback: EnumModesCallback(lpDDSurfaceDesc, lpContext)
					var parameters = new uint[] { surfaceDescPtr, lpContext };
					var result = callbackHelper.InvokeStdcallCallback(lpEnumModesCallback, parameters);

					// Free allocated structure
					FreeMemory(surfaceDescPtr);

					if (result == null)
					{
						_logger.LogError("[DDraw] EnumDisplayModes: callback invocation failed");
						return (uint)DDResult.DDERR_GENERIC;
					}

					// Callback returns DDENUMRET_OK (1) to continue, DDENUMRET_CANCEL (0) to stop
					if (result.Value == 0)
					{
						_logger.LogInformation("[DDraw] EnumDisplayModes: callback requested cancellation");
						break;
					}
				}

				return (uint)DDResult.DD_OK;
			}
			catch (OutOfMemoryException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (StackOverflowException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] EnumDisplayModes: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		private uint DDraw_EnumSurfaces(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpDDSD = args.UInt32(2);
			var lpContext = args.UInt32(3);
			var lpEnumSurfacesCallback = args.UInt32(4);

			_logger.LogInformation("[DDraw COM] IDirectDraw::EnumSurfaces(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpDDSD=0x{LpDDSD:X8}, lpContext=0x{LpContext:X8}, lpEnumSurfacesCallback=0x{LpEnumSurfacesCallback:X8})", thisPtr, dwFlags, lpDDSD, lpContext, lpEnumSurfacesCallback);

			if (lpEnumSurfacesCallback == 0)
			{
				_logger.LogError("[DDraw] EnumSurfaces: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Callback signature: HRESULT WINAPI EnumSurfacesCallback(LPDIRECTDRAWSURFACE lpDDSurface, LPDDSURFACEDESC lpDDSurfaceDesc, LPVOID lpContext)
			// Enumerate all surfaces that match the criteria

			try
			{
				var callbackHelper = new CallbackHelper(_currentCpu!, _currentMemory!, _logger);

				// Enumerate existing surfaces
				foreach (var surface in _surfaces.Values)
				{
					// Allocate DDSURFACEDESC structure (108 bytes minimum)
					var surfaceDescPtr = AllocateMemory(108);
					var surfaceDesc = new DDSurfaceDescRef(_env.Memory, surfaceDescPtr);

					// Get DirectDraw object to determine BPP
					_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj);

					// Fill in the structure using ref struct
					surfaceDesc.dwSize = 108;
					surfaceDesc.dwFlags = DDSD.CAPS | DDSD.WIDTH | DDSD.HEIGHT | DDSD.PITCH | DDSD.PIXELFORMAT;
					surfaceDesc.dwHeight = (uint)surface.Height;
					surfaceDesc.dwWidth = (uint)surface.Width;
					surfaceDesc.lPitch = (uint)surface.Pitch;

					// Fill in pixel format using nested ref struct
					if (ddrawObj != null)
					{
						var pixelFormat = surfaceDesc.ddpfPixelFormat;
						pixelFormat.dwSize = 32;
						pixelFormat.dwFlags = (uint)DDPFFlags.DDPF_RGB;
						pixelFormat.dwFourCC = 0;
						pixelFormat.dwRGBBitCount = (uint)ddrawObj.BitsPerPixel;

						switch (ddrawObj.BitsPerPixel)
						{
							// Set RGB masks based on bit depth
							case 16:
								pixelFormat.dwRBitMask = 0xF800;
								pixelFormat.dwGBitMask = 0x07E0;
								pixelFormat.dwBBitMask = 0x001F;
								break;
							case 24:
							case 32:
								pixelFormat.dwRBitMask = 0x00FF0000;
								pixelFormat.dwGBitMask = 0x0000FF00;
								pixelFormat.dwBBitMask = 0x000000FF;
								break;
						}
					}

					// Set surface caps
					uint caps = 0;
					if (surface.IsPrimary)
					{
						caps |= (uint)DDSCaps.DDSCAPS_PRIMARYSURFACE;
					}
					else
					{
						caps |= (uint)DDSCaps.DDSCAPS_OFFSCREENPLAIN;
					}

					surfaceDesc.dwSurfaceCaps = caps;

					_logger.LogDebug("[DDraw] Enumerating surface: 0x{Handle:X8} ({Width}x{Height})", surface.Handle, surface.Width, surface.Height);

					// Invoke callback: EnumSurfacesCallback(lpDDSurface, lpDDSurfaceDesc, lpContext)
					var parameters = new uint[] { surface.ComObjectAddress, surfaceDescPtr, lpContext };
					var result = callbackHelper.InvokeStdcallCallback(lpEnumSurfacesCallback, parameters);

					// Free allocated structure
					FreeMemory(surfaceDescPtr);

					if (result == null)
					{
						_logger.LogError("[DDraw] EnumSurfaces: callback invocation failed");
						return (uint)DDResult.DDERR_GENERIC;
					}

					// Callback returns DDENUMRET_OK (1) to continue, DDENUMRET_CANCEL (0) to stop
					if (result.Value == 0)
					{
						_logger.LogInformation("[DDraw] EnumSurfaces: callback requested cancellation");
						break;
					}
				}

				return (uint)DDResult.DD_OK;
			}
			catch (OutOfMemoryException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (StackOverflowException)
			{
				throw; // Rethrow critical exceptions
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] EnumSurfaces: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		private uint DDraw_FlipToGDISurface(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::FlipToGDISurface() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpDDDriverCaps = args.UInt32(1);
			var lpDDHELCaps = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetCaps(this=0x{ThisPtr:X8}, lpDDDriverCaps=0x{DriverCaps:X8}, lpDDHELCaps=0x{HELCaps:X8})", thisPtr, lpDDDriverCaps, lpDDHELCaps);

			// Fill in driver capabilities
			if (lpDDDriverCaps != 0)
			{
				var dwSize = _env.MemRead32(lpDDDriverCaps);

				// DDCAPS structure - comprehensive implementation
				// dwCaps: General capabilities
				uint caps = 0;
				caps |= (uint)DDCaps.DDCAPS_BLT;
				caps |= (uint)DDCaps.DDCAPS_BLTCOLORFILL;
				caps |= (uint)DDCaps.DDCAPS_BLTQUEUE;
				caps |= (uint)DDCaps.DDCAPS_BLTSTRETCH;
				caps |= (uint)DDCaps.DDCAPS_COLORKEY;
				caps |= (uint)DDCaps.DDCAPS_GDI;
				caps |= (uint)DDCaps.DDCAPS_PALETTE;
				caps |= (uint)DDCaps.DDCAPS_PALETTEVSYNC;
				_env.MemWrite32(lpDDDriverCaps + 4, caps);

				// dwCaps2: Extended capabilities  
				uint caps2 = 0;
				caps2 |= (uint)DDCaps2.DDCAPS2_CERTIFIED;
				caps2 |= (uint)DDCaps2.DDCAPS2_CANRENDERWINDOWED;
				caps2 |= (uint)DDCaps2.DDCAPS2_WIDESURFACES;
				caps2 |= (uint)DDCaps2.DDCAPS2_CANBOBHARDWARE;
				_env.MemWrite32(lpDDDriverCaps + 8, caps2);

				// dwCKeyCaps: Color key capabilities
				uint ckeyCaps = 0;
				ckeyCaps |= (uint)DDCKeyCaps.DDCKEYCAPS_DESTBLT;
				ckeyCaps |= (uint)DDCKeyCaps.DDCKEYCAPS_DESTBLTCLRSPACE;
				ckeyCaps |= (uint)DDCKeyCaps.DDCKEYCAPS_SRCBLT;
				ckeyCaps |= (uint)DDCKeyCaps.DDCKEYCAPS_SRCBLTCLRSPACE;
				_env.MemWrite32(lpDDDriverCaps + 12, ckeyCaps);

				// dwFXCaps: Blt effects capabilities
				uint fxCaps = 0;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTARITHSTRETCHY;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTARITHSTRETCHYN;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTMIRRORLEFTRIGHT;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTMIRRORUPDOWN;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTROTATION;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTSHRINKX;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTSHRINKY;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTSTRETCHX;
				fxCaps |= (uint)DDFXCaps.DDFXCAPS_BLTSTRETCHY;
				_env.MemWrite32(lpDDDriverCaps + 16, fxCaps);

				// dwFXAlphaCaps: Alpha blt capabilities
				_env.MemWrite32(lpDDDriverCaps + 20, 0);

				// dwPalCaps: Palette capabilities
				uint palCaps = 0;
				palCaps |= (uint)DDPCaps.DDPCAPS_8BIT;
				palCaps |= (uint)DDPCaps.DDPCAPS_PRIMARYSURFACE;
				palCaps |= (uint)DDPCaps.DDPCAPS_ALLOW256;
				_env.MemWrite32(lpDDDriverCaps + 24, palCaps);

				// dwSVCaps: Surface capabilities (system memory)
				uint svCaps = 0;
				svCaps |= (uint)DDSVCaps.DDSVCAPS_RESERVED1;
				_env.MemWrite32(lpDDDriverCaps + 28, svCaps);

				// Remaining fields
				_env.MemWrite32(lpDDDriverCaps + 32, 0); // dwAlphaBltConstBitDepths
				_env.MemWrite32(lpDDDriverCaps + 36, 0); // dwAlphaBltPixelBitDepths
				_env.MemWrite32(lpDDDriverCaps + 40, 0); // dwAlphaBltSurfaceBitDepths

				// Video memory information (if structure is large enough)
				if (dwSize >= 128)
				{
					_env.MemWrite32(lpDDDriverCaps + 44, 0); // dwVidMemTotal (0 = unspecified)
					_env.MemWrite32(lpDDDriverCaps + 48, 0); // dwVidMemFree (0 = unspecified)
				}
			}

			if (lpDDHELCaps != 0)
			{
				// HEL (Hardware Emulation Layer) caps - report same capabilities
				// Most emulators report the same caps for HEL as for HAL
				var dwSize = _env.MemRead32(lpDDHELCaps);

				// Basic capabilities for HEL
				_env.MemWrite32(lpDDHELCaps + 4, (uint)DDCaps.DDCAPS_BLT);
				_env.MemWrite32(lpDDHELCaps + 8, (uint)DDCaps2.DDCAPS2_CANRENDERWINDOWED);
			}

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_GetDisplayMode(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceDesc = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetDisplayMode(this=0x{ThisPtr:X8}, lpDDSurfaceDesc=0x{SurfaceDesc:X8})", thisPtr, lpDDSurfaceDesc);

			// Find the DirectDraw object
			var ddrawObj = _ddrawObjects.Values.FirstOrDefault();

			if (ddrawObj == null)
			{
				_logger.LogError("[DDraw] GetDisplayMode: could not find DirectDraw object");
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (lpDDSurfaceDesc != 0)
			{
				// Fill DDSURFACEDESC structure
				var dwSize = _env.MemRead32(lpDDSurfaceDesc);

				_env.MemWrite32(lpDDSurfaceDesc + 4, (uint)(DDSD.CAPS | DDSD.WIDTH | DDSD.HEIGHT | DDSD.PITCH | DDSD.PIXELFORMAT));
				_env.MemWrite32(lpDDSurfaceDesc + 8, (uint)ddrawObj.Height); // dwHeight
				_env.MemWrite32(lpDDSurfaceDesc + 12, (uint)ddrawObj.Width); // dwWidth
				_env.MemWrite32(lpDDSurfaceDesc + 16, (uint)(ddrawObj.Width * (ddrawObj.BitsPerPixel / 8))); // lPitch

				// Write pixel format (offset 76)
				if (dwSize >= 108)
				{
					_env.MemWrite32(lpDDSurfaceDesc + 76, 32); // dwSize of DDPIXELFORMAT
					_env.MemWrite32(lpDDSurfaceDesc + 80, (uint)DDPFFlags.DDPF_RGB);
					_env.MemWrite32(lpDDSurfaceDesc + 84, 0); // dwFourCC
					_env.MemWrite32(lpDDSurfaceDesc + 88, (uint)ddrawObj.BitsPerPixel); // dwRGBBitCount

					// Set RGB masks based on bit depth
					if (ddrawObj.BitsPerPixel == 16)
					{
						_env.MemWrite32(lpDDSurfaceDesc + 92, 0xF800); // Red mask (5 bits)
						_env.MemWrite32(lpDDSurfaceDesc + 96, 0x07E0); // Green mask (6 bits)
						_env.MemWrite32(lpDDSurfaceDesc + 100, 0x001F); // Blue mask (5 bits)
					}
					else if (ddrawObj.BitsPerPixel == 24 || ddrawObj.BitsPerPixel == 32)
					{
						_env.MemWrite32(lpDDSurfaceDesc + 92, 0x00FF0000); // Red mask
						_env.MemWrite32(lpDDSurfaceDesc + 96, 0x0000FF00); // Green mask
						_env.MemWrite32(lpDDSurfaceDesc + 100, 0x000000FF); // Blue mask
					}

					_env.MemWrite32(lpDDSurfaceDesc + 104, 0); // dwRGBAlphaBitMask
				}

				// Write ddsCaps (offset 108)
				// For display mode, set DDSCAPS_PRIMARYSURFACE
				if (dwSize >= 112)
				{
					const uint DDSCAPS_PRIMARYSURFACE = (uint)DDSCaps.DDSCAPS_PRIMARYSURFACE;
					_env.MemWrite32(lpDDSurfaceDesc + 108, DDSCAPS_PRIMARYSURFACE); // ddsCaps.dwCaps
				}
			}

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_GetFourCCCodes(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpNumCodes = args.UInt32(1);
			var lpCodes = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetFourCCCodes(this=0x{ThisPtr:X8}, lpNumCodes=0x{LpNumCodes:X8}, lpCodes=0x{LpCodes:X8})", thisPtr, lpNumCodes, lpCodes);

			if (lpNumCodes == 0)
			{
				_logger.LogError("[DDraw] GetFourCCCodes: lpNumCodes is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// For now, we don't support any hardware FourCC codes
			// Return 0 to indicate no additional formats are supported
			_env.MemWrite32(lpNumCodes, 0);

			_logger.LogInformation("[DDraw] Returning 0 FourCC codes (no additional formats supported)");
			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_GetGDISurface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lplpGDIDDSSurface = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetGDISurface(this=0x{ThisPtr:X8}, lplpGDIDDSSurface=0x{LplpGDIDDSSurface:X8})", thisPtr, lplpGDIDDSSurface);

			if (lplpGDIDDSSurface == 0)
			{
				_logger.LogError("[DDraw] GetGDISurface: lplpGDIDDSSurface is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Find the primary surface (which would be the GDI surface)
			// Note: Linear search is acceptable here as there are typically only 1-3 surfaces
			// (primary, backbuffer, and possibly one more) in a DirectDraw application
			var primarySurface = _surfaces.Values.FirstOrDefault(s => s.IsPrimary);

			if (primarySurface == null)
			{
				_env.MemWrite32(lplpGDIDDSSurface, 0);
				_logger.LogInformation("[DDraw] No GDI surface found");
				return (uint)DDResult.DDERR_NOTFOUND;
			}

			// Return the COM object address of the primary surface
			// The COM object addresses are already being tracked in the DirectDrawSurface.ComObjectAddress field
			_env.MemWrite32(lplpGDIDDSSurface, primarySurface.ComObjectAddress);
			_logger.LogInformation("[DDraw] Returning GDI surface COM object at 0x{ComObjectAddr:X8}", primarySurface.ComObjectAddress);

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_GetMonitorFrequency(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpdwFrequency = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetMonitorFrequency(this=0x{ThisPtr:X8}, lpdwFrequency=0x{Frequency:X8})", thisPtr, lpdwFrequency);

			if (lpdwFrequency != 0)
			{
				// Return typical 60Hz refresh rate
				_env.MemWrite32(lpdwFrequency, 60);
			}

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_GetScanLine(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpdwScanLine = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetScanLine(this=0x{ThisPtr:X8}, lpdwScanLine=0x{LpdwScanLine:X8})", thisPtr, lpdwScanLine);

			if (lpdwScanLine == 0)
			{
				_logger.LogError("[DDraw] GetScanLine: lpdwScanLine is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Find the DirectDraw object to get display height
			var ddrawObj = _ddrawObjects.Values.FirstOrDefault();

			if (ddrawObj == null)
			{
				_logger.LogError("[DDraw] GetScanLine: could not find DirectDraw object");
				return (uint)DDResult.DDERR_GENERIC;
			}

			// Simulate scan line position based on current time
			// In a real implementation, this would query the actual hardware
			// We'll cycle through all scan lines at approximately 60Hz refresh rate
			var totalScanLines = (uint)(ddrawObj.Height + 40); // Add vertical blanking lines
			var scanLine = (uint)((DateTime.UtcNow.Ticks / 10000) % totalScanLines);

			_env.MemWrite32(lpdwScanLine, scanLine);
			_logger.LogInformation("[DDraw] Returning scan line: {ScanLine} (of {Total})", scanLine, totalScanLines);

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_GetVerticalBlankStatus(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpbIsInVB = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetVerticalBlankStatus(this=0x{ThisPtr:X8}, lpbIsInVB=0x{IsInVB:X8})", thisPtr, lpbIsInVB);

			if (lpbIsInVB != 0)
			{
				// Simulate being in vertical blank 1/60th of the time
				var isInVBlank = (DateTime.UtcNow.Ticks / 10000) % 17 == 0; // Approximately 1/60th
				_env.MemWrite32(lpbIsInVB, isInVBlank ? 1u : 0u);
			}

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Initialize() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_RestoreDisplayMode(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::RestoreDisplayMode() - stub");
			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_SetCooperativeLevel(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var hWnd = args.UInt32(1);
			var dwFlags = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDraw::SetCooperativeLevel(this=0x{ThisPtr:X8}, hWnd=0x{HWnd:X8}, flags=0x{DwFlags:X8})", thisPtr, hWnd, dwFlags);

			// Decode and log flags for better debugging
			var flagsStr = new List<string>();
			if ((dwFlags & 0x00000008) != 0)
			{
				flagsStr.Add("DDSCL_FULLSCREEN");
			}

			if ((dwFlags & 0x00000010) != 0)
			{
				flagsStr.Add("DDSCL_EXCLUSIVE");
			}

			if ((dwFlags & 0x00000002) != 0)
			{
				flagsStr.Add("DDSCL_NORMAL");
			}

			if ((dwFlags & 0x00000020) != 0)
			{
				flagsStr.Add("DDSCL_ALLOWMODEX");
			}

			if ((dwFlags & 0x00000040) != 0)
			{
				flagsStr.Add("DDSCL_ALLOWREBOOT");
			}

			if ((dwFlags & 0x00000080) != 0)
			{
				flagsStr.Add("DDSCL_NOWINDOWCHANGES");
			}

			_logger.LogDebug("[DDraw COM] SetCooperativeLevel flags: {Flags}", string.Join(" | ", flagsStr));

			// Look up the actual handle from the COM object address
			if (!_comObjectToHandle.TryGetValue(thisPtr, out var actualHandle))
			{
				_logger.LogWarning("[DDraw] SetCooperativeLevel: Could not find DirectDraw handle for COM object 0x{ThisPtr:X8}, using captured handle 0x{Handle:X8}", thisPtr, ddrawHandle);
				actualHandle = ddrawHandle;
			}

			// Store cooperation level settings
			if (_ddrawObjects.TryGetValue(actualHandle, out var obj))
			{
				obj.CooperativeLevel = dwFlags;
				obj.WindowHandle = (IntPtr)hWnd;

				// Initialize rendering backend if not already done
				if (obj.RenderingBackend == null)
				{
					if (_env.BackendFactory != null)
					{
						obj.RenderingBackend = _env.BackendFactory.CreateRenderingBackendWithHost(_logger, _env.Host);
						if (_env.Host != null)
						{
							_logger.LogInformation("[DDraw] Using Avalonia rendering backend for GUI integration");
						}
					}
					else
					{
						_logger.LogWarning("[DDraw] BackendFactory not available, rendering backend not created");
					}
				}

				// Subscribe to UI events from the rendering backend
				// ProcessEnvironment now tracks subscriptions and prevents duplicates automatically
				if (obj.RenderingBackend != null)
				{
					_env.SubscribeToUIEvents(obj.RenderingBackend, null);
					_logger.LogInformation("[DDraw] Subscribed to UI events from rendering backend");
				}

				_logger.LogInformation("[DDraw COM] SetCooperativeLevel succeeded, returning DD_OK (0)");
			}
			else
			{
				_logger.LogError("[DDraw] SetCooperativeLevel: Could not find DirectDraw object with handle 0x{Handle:X8}", actualHandle);
				_logger.LogError("[DDraw COM] SetCooperativeLevel failed, returning DDERR_GENERIC (1)");
				return (uint)DDResult.DDERR_GENERIC;
			}

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_SetDisplayMode(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwWidth = args.UInt32(1);
			var dwHeight = args.UInt32(2);
			var dwBPP = args.UInt32(3);

			_logger.LogInformation("[DDraw COM] IDirectDraw::SetDisplayMode(this=0x{ThisPtr:X8}, width={DwWidth}, height={DwHeight}, bpp={DwBpp})", thisPtr, dwWidth, dwHeight, dwBPP);

			// Validate parameters
			if (dwWidth == 0 || dwHeight == 0)
			{
				_logger.LogError("[DDraw COM] SetDisplayMode: Invalid dimensions ({Width}x{Height})", dwWidth, dwHeight);
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			if (dwBPP != 8 && dwBPP != 16 && dwBPP != 24 && dwBPP != 32)
			{
				_logger.LogWarning("[DDraw COM] SetDisplayMode: Unusual BPP value {Bpp}, accepting anyway", dwBPP);
			}

			// Look up the actual handle from the COM object address
			if (!_comObjectToHandle.TryGetValue(thisPtr, out var actualHandle))
			{
				_logger.LogWarning("[DDraw] SetDisplayMode: Could not find DirectDraw handle for COM object 0x{ThisPtr:X8}, using captured handle 0x{Handle:X8}", thisPtr, ddrawHandle);
				actualHandle = ddrawHandle;
			}

			// Store display mode settings
			if (_ddrawObjects.TryGetValue(actualHandle, out var obj))
			{
				obj.Width = (int)dwWidth;
				obj.Height = (int)dwHeight;
				obj.BitsPerPixel = (int)dwBPP;

				// Update ProcessEnvironment with display mode for GetSystemMetrics
				_env.DisplayWidth = (int)dwWidth;
				_env.DisplayHeight = (int)dwHeight;
				_env.DisplayBitsPerPixel = (int)dwBPP;
				_logger.LogInformation("[DDraw] Updated ProcessEnvironment display mode to {Width}x{Height}x{Bpp}", dwWidth, dwHeight, dwBPP);

				// Initialize rendering backend with the specified dimensions
				if (obj.RenderingBackend == null)
				{
					if (_env.BackendFactory != null)
					{
						obj.RenderingBackend = _env.BackendFactory.CreateRenderingBackendWithHost(_logger, _env.Host);
						if (_env.Host != null)
						{
							_logger.LogInformation("[DDraw] Using Avalonia rendering backend for GUI integration");
						}
					}
					else
					{
						_logger.LogWarning("[DDraw] BackendFactory not available, rendering backend not created");
					}
				}

				// Initialize the window with the specified dimensions
				var title = "Win32Emu DirectDraw";
				if (obj.RenderingBackend?.IsInitialized == true)
				{
					// If already initialized, we would need to recreate with new dimensions
					// For now, we'll just log this situation
					_logger.LogInformation("[DDraw] Display mode changed to {Width}x{Height}x{Bpp}", dwWidth, dwHeight, dwBPP);
				}
				else if (obj.RenderingBackend != null)
				{
					// In WASM mode, we cannot block on async operations (Monitor.Wait is not supported).
					// Fire-and-forget the initialization - the backend will self-mark as initialized.
					// Subsequent render calls will check IsInitialized before attempting to render.
					if (PlatformHelpers.IsWasm)
					{
						// Use ContinueWith to properly handle any exceptions from the async initialization
						// In WASM, continuations run on the synchronization context, so we don't specify TaskScheduler
						_ = obj.RenderingBackend.InitializeAsync((int)dwWidth, (int)dwHeight, title)
							.ContinueWith(t =>
							{
								if (t.IsFaulted)
								{
									_logger.LogError(t.Exception?.GetBaseException(), "[DDraw] Rendering backend initialization failed (WASM mode)");
								}
								else if (t.Result)
								{
									_logger.LogInformation("[DDraw] Rendering backend initialized successfully with {Width}x{Height} (WASM mode)", dwWidth, dwHeight);
								}
								else
								{
									_logger.LogWarning("[DDraw] Rendering backend initialization returned false (WASM mode)");
								}
							});
						_logger.LogInformation("[DDraw] Rendering backend initialization started asynchronously with {Width}x{Height} (WASM mode)", dwWidth, dwHeight);
					}
					else
					{
						var success = obj.RenderingBackend.InitializeAsync((int)dwWidth, (int)dwHeight, title).GetAwaiter().GetResult();
						if (!success)
						{
							// In headless/nogui mode (Host == null), rendering backend initialization may fail
							// due to lack of video device. This is expected and should not cause the application to crash.
							// We log the failure but still return success to allow headless testing.
							if (_env.Host == null)
							{
								_logger.LogWarning("[DDraw] Failed to initialize rendering backend in headless mode (expected - no video device)");
								_logger.LogInformation("[DDraw] SetDisplayMode succeeded in headless mode (rendering disabled)");
							}
							else
							{
								// In GUI mode, initialization failure is an actual error
								_logger.LogError("[DDraw] Failed to initialize rendering backend");
								_logger.LogError("[DDraw COM] SetDisplayMode failed, returning DDERR_GENERIC (1)");
								return (uint)DDResult.DDERR_GENERIC;
							}
						}
						else
						{
							_logger.LogInformation("[DDraw] Rendering backend initialized successfully with {Width}x{Height}", dwWidth, dwHeight);
						}
					}
				}

				// Subscribe to UI events from the rendering backend
				// ProcessEnvironment now tracks subscriptions and prevents duplicates automatically
				if (obj.RenderingBackend != null)
				{
					_env.SubscribeToUIEvents(obj.RenderingBackend, null);
					_logger.LogInformation("[DDraw] Subscribed to UI events from rendering backend");
				}

				_logger.LogInformation("[DDraw COM] SetDisplayMode succeeded, returning DD_OK (0)");
			}
			else
			{
				_logger.LogError("[DDraw] SetDisplayMode: Could not find DirectDraw object with handle 0x{Handle:X8}", actualHandle);
				_logger.LogError("[DDraw COM] SetDisplayMode failed, returning DDERR_GENERIC (1)");
				return (uint)DDResult.DDERR_GENERIC;
			}

			return (uint)DDResult.DD_OK;
		}

		private uint DDraw_WaitForVerticalBlank(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::WaitForVerticalBlank(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8})", thisPtr, dwFlags);

			// DDWAITVB_BLOCKBEGIN = 0x00000001 - Wait for vertical blank to begin
			// DDWAITVB_BLOCKBEGINEVENT = 0x00000002 - Triggers when vertical blank begins
			// DDWAITVB_BLOCKEND = 0x00000004 - Wait for vertical blank to end

			if ((dwFlags & 0x00000001) != 0 || (dwFlags & 0x00000004) != 0)
			{
				// Simulate a small wait for vertical blank
				// In reality, at 60Hz, vertical blank lasts about 1-2ms
				// We don't actually wait in the emulator to avoid slowing down
				_logger.LogInformation("[DDraw] Simulated wait for vertical blank");
			}

			return (uint)DDResult.DD_OK;
		}

		private uint Surface_Lock(ICpu cpu, VirtualMemory memory, uint surfaceHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpDestRect = args.UInt32(1);
			var lpDDSurfaceDesc = args.UInt32(2);
			var dwFlags = args.UInt32(3);
			var hEvent = args.UInt32(4);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::Lock(this=0x{ThisPtr:X8}, lpDestRect=0x{LpDestRect:X8}, lpDDSurfaceDesc=0x{LpDDSurfaceDesc:X8}, dwFlags=0x{DwFlags:X8}, hEvent=0x{HEvent:X8})", thisPtr, lpDestRect, lpDDSurfaceDesc, dwFlags, hEvent);

			if (!_surfaces.TryGetValue(surfaceHandle, out var surface))
			{
				_logger.LogError("[DDraw] Failed to find surface 0x{SurfaceHandle:X8} for Lock", surfaceHandle);
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (surface.IsLocked)
			{
				_logger.LogWarning("[DDraw] Surface 0x{SurfaceHandle:X8} is already locked", surfaceHandle);
				return (uint)DDResult.DDERR_SURFACEBUSY;
			}

			// Mark the surface as locked
			surface.IsLocked = true;

			// Allocate memory for the surface if not already done
			if (surface.Bits == null)
			{
				surface.Bits = new byte[surface.Pitch * surface.Height];
			}

			// Get a pointer to the surface memory
			var surfaceMemPtr = _env.VirtualAlloc(0, (uint)(surface.Pitch * surface.Height), 0x1000, 0x04); // MEM_COMMIT, PAGE_READWRITE
			surface.LockedMemoryPtr = surfaceMemPtr;

			// Fill the surface description structure
			if (lpDDSurfaceDesc != 0)
			{
				var dwSize = _env.MemRead32(lpDDSurfaceDesc);

				// Write the surface description
				_env.MemWrite32(lpDDSurfaceDesc + 4, (uint)(DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT));
				_env.MemWrite32(lpDDSurfaceDesc + 8, (uint)surface.Height); // dwHeight
				_env.MemWrite32(lpDDSurfaceDesc + 12, (uint)surface.Width); // dwWidth
				_env.MemWrite32(lpDDSurfaceDesc + 16, (uint)surface.Pitch); // lPitch
				_env.MemWrite32(lpDDSurfaceDesc + 20, 0); // dwBackBufferCount
				_env.MemWrite32(lpDDSurfaceDesc + 24, 0); // dwMipMapCount
				_env.MemWrite32(lpDDSurfaceDesc + 28, 0); // dwRefreshRate
				_env.MemWrite32(lpDDSurfaceDesc + 32, 0); // dwAlphaBitDepth
				_env.MemWrite32(lpDDSurfaceDesc + 36, 0); // dwReserved
				_env.MemWrite32(lpDDSurfaceDesc + 40, surfaceMemPtr); // lpSurface

				// Write pixel format if needed (offset 76)
				if (dwSize >= 108)
				{
					if (!_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj))
					{
						_logger.LogError("[DDraw] Failed to find DirectDraw object for surface 0x{SurfaceHandle:X8}", surfaceHandle);
						return (uint)DDResult.DDERR_GENERIC;
					}

					// Write pixel format structure
					_env.MemWrite32(lpDDSurfaceDesc + 76, 32); // dwSize of DDPIXELFORMAT
					_env.MemWrite32(lpDDSurfaceDesc + 80, 0x00000040); // DDPF_RGB
					_env.MemWrite32(lpDDSurfaceDesc + 84, 0); // dwRGBBitCount
					_env.MemWrite32(lpDDSurfaceDesc + 88, (uint)ddrawObj.BitsPerPixel); // dwRGBBitCount

					// Set RGB masks based on bit depth
					if (ddrawObj.BitsPerPixel == 16)
					{
						_env.MemWrite32(lpDDSurfaceDesc + 92, 0xF800); // Red mask (5 bits)
						_env.MemWrite32(lpDDSurfaceDesc + 96, 0x07E0); // Green mask (6 bits)
						_env.MemWrite32(lpDDSurfaceDesc + 100, 0x001F); // Blue mask (5 bits)
					}
					else if (ddrawObj.BitsPerPixel == 24 || ddrawObj.BitsPerPixel == 32)
					{
						_env.MemWrite32(lpDDSurfaceDesc + 92, 0x00FF0000); // Red mask
						_env.MemWrite32(lpDDSurfaceDesc + 96, 0x0000FF00); // Green mask
						_env.MemWrite32(lpDDSurfaceDesc + 100, 0x000000FF); // Blue mask
					}

					_env.MemWrite32(lpDDSurfaceDesc + 104, 0); // dwRGBAlphaBitMask
				}
			}

			_logger.LogInformation("[DDraw] Locked surface 0x{SurfaceHandle:X8}, memory at 0x{SurfaceMemPtr:X8}", surfaceHandle, surfaceMemPtr);
			return (uint)DDResult.DD_OK;
		}

		private uint Surface_Unlock(ICpu cpu, VirtualMemory memory, uint surfaceHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpRect = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::Unlock(this=0x{ThisPtr:X8}, lpRect=0x{LpRect:X8})", thisPtr, lpRect);

			if (!_surfaces.TryGetValue(surfaceHandle, out var surface))
			{
				_logger.LogError("[DDraw] Failed to find surface 0x{SurfaceHandle:X8} for Unlock", surfaceHandle);
				return (uint)DDResult.DDERR_GENERIC;
			}

			if (!surface.IsLocked)
			{
				_logger.LogWarning("[DDraw] Surface 0x{SurfaceHandle:X8} is not locked", surfaceHandle);
				return (uint)DDResult.DDERR_NOTLOCKED;
			}

			// Copy memory from the locked pointer to our surface bits
			if (surface.LockedMemoryPtr != 0 && surface.Bits != null)
			{
				var data = _env.MemReadBytes(surface.LockedMemoryPtr, surface.Pitch * surface.Height);
				Array.Copy(data, surface.Bits, data.Length);

				// We don't actually free memory in this implementation
				// Just mark it as no longer locked
				surface.LockedMemoryPtr = 0;
			}

			// Mark the surface as unlocked
			surface.IsLocked = false;

			// If this is a primary surface, update the rendering backend texture
			if (surface.IsPrimary && _ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj) && ddrawObj.RenderingBackend != null && ddrawObj.RenderingBackend.IsInitialized)
			{
				try
				{
					// Check if surface bits are available
					if (surface.Bits == null)
					{
						_logger.LogWarning("[DDraw] Surface bits are null, skipping flip");
						return (uint)DDResult.DD_OK;
					}

					byte[] displayData;

					// Check if we need to convert the surface data based on bit depth
					if (ddrawObj.BitsPerPixel == 8)
					{
						// 8-bit palettized mode
						if (surface.PaletteHandle != 0 && _palettes.TryGetValue(surface.PaletteHandle, out var palette))
						{
							// Convert palettized (8-bit indexed) to RGBA using attached palette
							_logger.LogDebug("[DDraw] Converting 8-bit palettized surface to RGBA");
							displayData = ddrawObj.RenderingBackend.ConvertPalettizedToRGBA(
								surface.Bits,
								palette.Entries,
								surface.Width,
								surface.Height,
								surface.Pitch);
						}
						else
						{
							// No palette set yet - use a default grayscale palette
							_logger.LogWarning("[DDraw] No palette set for 8-bit surface, using grayscale");
							var grayscalePalette = new uint[256];
							for (var i = 0; i < 256; i++)
							{
								grayscalePalette[i] = (0xFFu << 24) | ((uint)i << 16) | ((uint)i << 8) | (uint)i; // RGBA: opaque grayscale
							}

							displayData = ddrawObj.RenderingBackend.ConvertPalettizedToRGBA(
								surface.Bits,
								grayscalePalette,
								surface.Width,
								surface.Height,
								surface.Pitch);
						}
					}
					else if (ddrawObj.BitsPerPixel == 16)
					{
						// Convert 16-bit RGB565 to RGBA
						_logger.LogInformation("[DDraw] Converting 16-bit RGB565 surface to RGBA");
						displayData = ddrawObj.RenderingBackend.Convert16BitToRGBA(
							surface.Bits,
							surface.Width,
							surface.Height,
							surface.Pitch);
					}
					else if (ddrawObj.BitsPerPixel == 24)
					{
						// Convert 24-bit RGB/BGR to RGBA
						_logger.LogDebug("[DDraw] Converting 24-bit surface to RGBA");
						displayData = ddrawObj.RenderingBackend.Convert24BitToRGBA(
							surface.Bits,
							surface.Width,
							surface.Height,
							surface.Pitch);
					}
					else if (ddrawObj.BitsPerPixel == 32)
					{
						// 32-bit RGBA - pass through
						displayData = surface.Bits;
					}
					else
					{
						// Unknown format - treat as RGBA
						_logger.LogWarning("[DDraw] Unknown bit depth {Bpp}, treating as RGBA", ddrawObj.BitsPerPixel);
						displayData = surface.Bits;
					}

					// Update the rendering backend texture with the converted surface data
					if (displayData != null)
					{
						var displayPitch = surface.Width * 4; // RGBA format
						_logger.LogDebug("[DDraw] Calling UpdateFrameBuffer: surface={SurfaceHandle:X8}, width={Width}, height={Height}, pitch={Pitch}, dataLength={DataLength}", 
							surfaceHandle, surface.Width, surface.Height, displayPitch, displayData.Length);
						var updateResult = ddrawObj.RenderingBackend.UpdateFrameBuffer(displayData, displayPitch);
						_logger.LogDebug("[DDraw] UpdateFrameBuffer result: {Result}", updateResult);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[DDraw] Failed to update rendering backend texture for primary surface");
				}
			}

			_logger.LogInformation("[DDraw] Unlocked surface 0x{SurfaceHandle:X8}", surfaceHandle);
			return (uint)DDResult.DD_OK;
		}

		// IDirectDrawClipper interface methods
		private uint Clipper_GetClipList(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpRect = args.UInt32(1);
			var lpClipList = args.UInt32(2);
			var lpdwSize = args.UInt32(3);

			_logger.LogInformation("[DDraw COM] IDirectDrawClipper::GetClipList(this=0x{ThisPtr:X8}, lpRect=0x{LpRect:X8}, lpClipList=0x{LpClipList:X8}, lpdwSize=0x{LpdwSize:X8})", thisPtr, lpRect, lpClipList, lpdwSize);

			// For windowed mode, we typically don't need a complex clip list
			// Return that no clip list is available
			// Do not write to lpdwSize when returning DDERR_NOCLIPLIST, per DirectDraw documentation.

			return (uint)DDResult.DDERR_NOCLIPLIST;
		}

		private uint Clipper_GetHWnd(ICpu cpu, VirtualMemory memory, uint clipperHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lphWnd = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawClipper::GetHWnd(this=0x{ThisPtr:X8}, lphWnd=0x{LphWnd:X8})", thisPtr, lphWnd);

			if (lphWnd == 0)
			{
				_logger.LogError("[DDraw] GetHWnd: lphWnd is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			if (!_clippers.TryGetValue(clipperHandle, out var clipper))
			{
				_logger.LogError("[DDraw] GetHWnd: clipper not found");
				_env.MemWrite32(lphWnd, 0);
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			_env.MemWrite32(lphWnd, clipper.WindowHandle);
			_logger.LogInformation("[DDraw] Returning window handle 0x{WindowHandle:X8}", clipper.WindowHandle);

			return (uint)DDResult.DD_OK;
		}

		private uint Clipper_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDrawClipper::Initialize() - stub");
			// Already initialized by CreateClipper
			return (uint)DDResult.DD_OK;
		}

		private uint Clipper_IsClipListChanged(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpbChanged = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawClipper::IsClipListChanged(this=0x{ThisPtr:X8}, lpbChanged=0x{LpbChanged:X8})", thisPtr, lpbChanged);

			if (lpbChanged != 0)
			{
				_env.MemWrite32(lpbChanged, 0); // FALSE - not changed
			}

			return (uint)DDResult.DD_OK;
		}

		private uint Clipper_SetClipList(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpClipList = args.UInt32(1);
			var dwFlags = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawClipper::SetClipList(this=0x{ThisPtr:X8}, lpClipList=0x{LpClipList:X8}, dwFlags=0x{DwFlags:X8})", thisPtr, lpClipList, dwFlags);

			// For now, we accept but don't process clip lists
			return (uint)DDResult.DD_OK;
		}

		private uint Clipper_SetHWnd(ICpu cpu, VirtualMemory memory, uint clipperHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var hWnd = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawClipper::SetHWnd(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, hWnd=0x{HWnd:X8})", thisPtr, dwFlags, hWnd);

			if (!_clippers.TryGetValue(clipperHandle, out var clipper))
			{
				_logger.LogError("[DDraw] SetHWnd: clipper not found");
				return (uint)DDResult.DDERR_INVALIDOBJECT;
			}

			clipper.WindowHandle = hWnd;
			_logger.LogInformation("[DDraw] Set clipper window handle to 0x{WindowHandle:X8}", hWnd);

			return (uint)DDResult.DD_OK;
		}


		[DllModuleExport(1, entryPoint: 0x000178E9, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(1, entryPoint: 0x0000475A, Version = "5.1.2600.6532", IsStub = true)]
		public uint AcquireDDThreadLock()
		{
			_logger.LogWarning("[ddraw] AcquireDDThreadLock called (stub)");
			// TODO: Implement AcquireDDThreadLock
			return 0; // DWORD default
		}

		[DllModuleExport(2, entryPoint: 0x0002A9D9, Version = "5.1.2600.6532", IsStub = true)]
		public uint CheckFullscreen()
		{
			_logger.LogWarning("[ddraw] CheckFullscreen called (stub)");
			// TODO: Implement CheckFullscreen
			return 0; // DWORD default
		}

		[DllModuleExport(2, entryPoint: 0x0001B178, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(4, entryPoint: 0x0002C8DA, Version = "5.1.2600.6532", IsStub = true)]
		public uint D3DParseUnknownCommand()
		{
			_logger.LogWarning("[ddraw] D3DParseUnknownCommand called (stub)");
			// TODO: Implement D3DParseUnknownCommand
			return 0; // DWORD default
		}

		[DllModuleExport(3, entryPoint: 0x0002B960, Version = "5.1.2600.6532", IsStub = true)]
		public uint CompleteCreateSysmemSurface()
		{
			_logger.LogWarning("[ddraw] CompleteCreateSysmemSurface called (stub)");
			// TODO: Implement CompleteCreateSysmemSurface
			return 0; // DWORD default
		}

		[DllModuleExport(3, entryPoint: 0x0001E270, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(5, entryPoint: 0x0002CF19, Version = "5.1.2600.6532", IsStub = true)]
		public uint DDGetAttachedSurfaceLcl()
		{
			_logger.LogWarning("[ddraw] DDGetAttachedSurfaceLcl called (stub)");
			// TODO: Implement DDGetAttachedSurfaceLcl
			return 0; // DWORD default
		}

		[DllModuleExport(4, entryPoint: 0x0001ED2D, Version = "4.90.0.3000", IsStub = true)]
		public uint DDHAL32_VidMemAlloc()
		{
			_logger.LogWarning("[ddraw] DDHAL32_VidMemAlloc called (stub)");
			// TODO: Implement DDHAL32_VidMemAlloc
			return 0; // DWORD default
		}

		[DllModuleExport(5, entryPoint: 0x0001EDC1, Version = "4.90.0.3000", IsStub = true)]
		public uint DDHAL32_VidMemFree()
		{
			_logger.LogWarning("[ddraw] DDHAL32_VidMemFree called (stub)");
			// TODO: Implement DDHAL32_VidMemFree
			return 0; // DWORD default
		}

		[DllModuleExport(6, entryPoint: 0x00018A38, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(6, entryPoint: 0x0002A63F, Version = "5.1.2600.6532", IsStub = true)]
		public uint DDInternalLock()
		{
			_logger.LogWarning("[ddraw] DDInternalLock called (stub)");
			// TODO: Implement DDInternalLock
			return 0; // DWORD default
		}

		[DllModuleExport(7, entryPoint: 0x00017F6E, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(7, entryPoint: 0x0002A5F4, Version = "5.1.2600.6532", IsStub = true)]
		public uint DDInternalUnlock()
		{
			_logger.LogWarning("[ddraw] DDInternalUnlock called (stub)");
			// TODO: Implement DDInternalUnlock
			return 0; // DWORD default
		}

		[DllModuleExport(8, entryPoint: 0x000201D4, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(8, entryPoint: 0x0002E4B6, Version = "5.1.2600.6532", IsStub = true)]
		public uint DSoundHelp()
		{
			_logger.LogWarning("[ddraw] DSoundHelp called (stub)");
			// TODO: Implement DSoundHelp
			return 0; // DWORD default
		}

		[DllModuleExport(9, entryPoint: 0x00022368, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry10()
		{
			_logger.LogWarning("[ddraw] DdEntry10 called (stub)");
			// TODO: Implement DdEntry10
			return 0; // DWORD default
		}

		[DllModuleExport(32, entryPoint: 0x0002A461, Version = "4.90.0.3000")]
		[DllModuleExport(10, entryPoint: 0x0002E921, Version = "5.1.2600.6532")]
		public uint DirectDrawCreateClipper(uint dwFlags, uint lplpDDClipper, uint pUnkOuter)
		{
			_logger.LogInformation("[DDraw] DirectDrawCreateClipper(dwFlags=0x{DwFlags:X8}, lplpDDClipper=0x{LplpDDClipper:X8}, pUnkOuter=0x{PUnkOuter:X8})", dwFlags, lplpDDClipper, pUnkOuter);

			if (lplpDDClipper == 0)
			{
				_logger.LogError("[DDraw] DirectDrawCreateClipper: lplpDDClipper is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			if (pUnkOuter != 0)
			{
				_logger.LogError("[DDraw] DirectDrawCreateClipper: pUnkOuter must be NULL");
				return (uint)DDResult.CLASS_E_NOAGGREGATION;
			}

			// Create a new clipper handle
			var clipperHandle = _nextClipperHandle++;
			var clipper = new DirectDrawClipper
			{
				Handle = clipperHandle,
				IsWindowedMode = true
			};

			_clippers[clipperHandle] = clipper;

			// Create COM vtable for IDirectDrawClipper interface
			var clipperVtableMethods = new List<KeyValuePair<string, ComMethodInfo>>
			{
				new("QueryInterface", ComVtableDispatcher.FromDelegate<IDirectDraw.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
				new("AddRef", ComVtableDispatcher.FromDelegate<IDirectDraw.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
				new("Release", ComVtableDispatcher.FromDelegate<IDirectDraw.Release>((cpu, mem) => ComRelease(cpu, mem))),
				new("GetClipList", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.GetClipList>((cpu, mem) => Clipper_GetClipList(cpu, mem))),
				new("GetHWnd", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.GetHWnd>((cpu, mem) => Clipper_GetHWnd(cpu, mem, clipperHandle))),
				new("Initialize", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.Initialize>((cpu, mem) => Clipper_Initialize(cpu, mem))),
				new("IsClipListChanged", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.IsClipListChanged>((cpu, mem) => Clipper_IsClipListChanged(cpu, mem))),
				new("SetClipList", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.SetClipList>((cpu, mem) => Clipper_SetClipList(cpu, mem))),
				new("SetHWnd", ComVtableDispatcher.FromDelegate<IDirectDrawClipper.SetHWnd>((cpu, mem) => Clipper_SetHWnd(cpu, mem, clipperHandle)))
			};

			var clipperComAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectDrawClipper", clipperVtableMethods);
			clipper.ComObjectAddress = clipperComAddr;

			// Write the clipper COM object address to the output pointer
			_env.MemWrite32(lplpDDClipper, clipperComAddr);

			_logger.LogInformation("[DDraw] Created standalone clipper with handle 0x{Handle:X8}, COM object at 0x{ComAddr:X8}", clipperHandle, clipperComAddr);

			return (uint)DDResult.DD_OK;
		}

		[DllModuleExport(10, entryPoint: 0x00025999, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry11()
		{
			_logger.LogWarning("[ddraw] DdEntry11 called (stub)");
			// TODO: Implement DdEntry11
			return 0; // DWORD default
		}

		[DllModuleExport(11, entryPoint: 0x00022414, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry12()
		{
			_logger.LogWarning("[ddraw] DdEntry12 called (stub)");
			// TODO: Implement DdEntry12
			return 0; // DWORD default
		}

		[DllModuleExport(34, entryPoint: 0x0001DC21, Version = "4.90.0.3000")]
		[DllModuleExport(12, entryPoint: 0x0002CB1B, Version = "5.1.2600.6532")]
		public uint DirectDrawEnumerateA(uint lpCallback, uint lpContext)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateA(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8})", lpCallback, lpContext);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateA: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Callback signature: BOOL WINAPI DDEnumCallback(GUID FAR *lpGUID, LPSTR lpDriverDescription, LPSTR lpDriverName, LPVOID lpContext)
			// For an emulator, we enumerate one primary display driver (the emulated one)

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateString(driverDescription);

				// Allocate and write driver name string
				var driverName = "display";
				var namePtr = AllocateString(driverName);

				_logger.LogDebug("[DDraw] Allocated strings: desc=0x{Desc:X8}, name=0x{Name:X8}", descPtr, namePtr);

				// Create callback helper
				var callbackHelper = new CallbackHelper(_currentCpu!, _currentMemory!, _logger);

				// Invoke callback: DDEnumCallback(lpGUID, lpDriverDescription, lpDriverName, lpContext)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext };
				var result = callbackHelper.InvokeStdcallCallback(lpCallback, parameters);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (result == null)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateA: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateA: callback returned {Result}", result.Value);

				// Callback returns FALSE (0) to stop enumeration, non-zero to continue
				// Since we only have one device, we always return success
				return (uint)DDResult.DD_OK;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateA: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		[DllModuleExport(12, entryPoint: 0x00022626, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry13()
		{
			_logger.LogWarning("[ddraw] DdEntry13 called (stub)");
			// TODO: Implement DdEntry13
			return 0; // DWORD default
		}

		[DllModuleExport(35, entryPoint: 0x0001A8F5, Version = "4.90.0.3000")]
		[DllModuleExport(13, entryPoint: 0x00001A57, Version = "5.1.2600.6532")]
		public uint DirectDrawEnumerateExA(uint lpCallback, uint lpContext, uint dwFlags)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateExA(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8}, dwFlags=0x{DwFlags:X8})", lpCallback, lpContext, dwFlags);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateExA: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Extended enumeration flags:
			// DDENUM_ATTACHEDSECONDARYDEVICES (0x00000001) - Enumerate secondary devices
			// DDENUM_DETACHEDSECONDARYDEVICES (0x00000002) - Enumerate detached devices
			// DDENUM_NONDISPLAYDEVICES (0x00000004) - Enumerate non-display devices

			// Callback signature: BOOL WINAPI DDEnumCallbackEx(GUID FAR *lpGUID, LPSTR lpDriverDescription, LPSTR lpDriverName, LPVOID lpContext, HMONITOR hm)
			// For an emulator, we enumerate one primary display driver (the emulated one)

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateString(driverDescription);

				// Allocate and write driver name string
				var driverName = "display";
				var namePtr = AllocateString(driverName);

				// Monitor handle (just use a dummy value for primary monitor)
				uint hMonitor = 0x00010001;

				_logger.LogDebug("[DDraw] Allocated strings: desc=0x{Desc:X8}, name=0x{Name:X8}, hMonitor=0x{HMonitor:X8}", descPtr, namePtr, hMonitor);

				// Create callback helper
				var callbackHelper = new CallbackHelper(_currentCpu!, _currentMemory!, _logger);

				// Invoke callback: DDEnumCallbackEx(lpGUID, lpDriverDescription, lpDriverName, lpContext, hMonitor)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext, hMonitor };
				var result = callbackHelper.InvokeStdcallCallback(lpCallback, parameters);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (result == null)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateExA: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateExA: callback returned {Result}", result.Value);

				// Callback returns FALSE (0) to stop enumeration, non-zero to continue
				// Since we only have one device, we always return success
				return (uint)DDResult.DD_OK;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateExA: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		[DllModuleExport(13, entryPoint: 0x00022467, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry16()
		{
			_logger.LogWarning("[ddraw] DdEntry16 called (stub)");
			// TODO: Implement DdEntry16
			return 0; // DWORD default
		}

		[DllModuleExport(36, entryPoint: 0x0001AD3A, Version = "4.90.0.3000")]
		[DllModuleExport(14, entryPoint: 0x0002C629, Version = "5.1.2600.6532")]
		public uint DirectDrawEnumerateExW(uint lpCallback, uint lpContext, uint dwFlags)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateExW(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8}, dwFlags=0x{DwFlags:X8})", lpCallback, lpContext, dwFlags);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateExW: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Extended enumeration flags:
			// DDENUM_ATTACHEDSECONDARYDEVICES (0x00000001) - Enumerate secondary devices
			// DDENUM_DETACHEDSECONDARYDEVICES (0x00000002) - Enumerate detached devices
			// DDENUM_NONDISPLAYDEVICES (0x00000004) - Enumerate non-display devices

			// Callback signature: BOOL WINAPI DDEnumCallbackExW(GUID FAR *lpGUID, LPWSTR lpDriverDescription, LPWSTR lpDriverName, LPVOID lpContext, HMONITOR hm)
			// For an emulator, we enumerate one primary display driver (the emulated one)

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string (Unicode)
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateUnicodeString(driverDescription);

				// Allocate and write driver name string (Unicode)
				var driverName = "display";
				var namePtr = AllocateUnicodeString(driverName);

				// Monitor handle (just use a dummy value for primary monitor)
				uint hMonitor = 0x00010001;

				_logger.LogDebug("[DDraw] Allocated Unicode strings: desc=0x{Desc:X8}, name=0x{Name:X8}, hMonitor=0x{HMonitor:X8}", descPtr, namePtr, hMonitor);

				// Create callback helper
				var callbackHelper = new CallbackHelper(_currentCpu!, _currentMemory!, _logger);

				// Invoke callback: DDEnumCallbackExW(lpGUID, lpDriverDescription, lpDriverName, lpContext, hMonitor)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext, hMonitor };
				var result = callbackHelper.InvokeStdcallCallback(lpCallback, parameters);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (result == null)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateExW: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateExW: callback returned {Result}", result.Value);

				// Callback returns FALSE (0) to stop enumeration, non-zero to continue
				// Since we only have one device, we always return success
				return (uint)DDResult.DD_OK;
			}
			catch (System.Runtime.InteropServices.ExternalException ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateExW: COM/Interop exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
			catch (SystemException ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateExW: system exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, "[DDraw] DirectDrawEnumerateExW: unexpected exception during enumeration");
				throw;
			}
		}

		[DllModuleExport(14, entryPoint: 0x0002273B, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry17()
		{
			_logger.LogWarning("[ddraw] DdEntry17 called (stub)");
			// TODO: Implement DdEntry17
			return 0; // DWORD default
		}

		[DllModuleExport(37, entryPoint: 0x0001DC5F, Version = "4.90.0.3000")]
		[DllModuleExport(15, entryPoint: 0x0002CAF6, Version = "5.1.2600.6532")]
		public uint DirectDrawEnumerateW(uint lpCallback, uint lpContext)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateW(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8})", lpCallback, lpContext);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateW: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			// Callback signature: BOOL WINAPI DDEnumCallbackW(GUID FAR *lpGUID, LPWSTR lpDriverDescription, LPWSTR lpDriverName, LPVOID lpContext)
			// For an emulator, we enumerate one primary display driver (the emulated one)

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string (Unicode)
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateUnicodeString(driverDescription);

				// Allocate and write driver name string (Unicode)
				var driverName = "display";
				var namePtr = AllocateUnicodeString(driverName);

				_logger.LogDebug("[DDraw] Allocated Unicode strings: desc=0x{Desc:X8}, name=0x{Name:X8}", descPtr, namePtr);

				// Create callback helper
				var callbackHelper = new CallbackHelper(_currentCpu!, _currentMemory!, _logger);

				// Invoke callback: DDEnumCallbackW(lpGUID, lpDriverDescription, lpDriverName, lpContext)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext };
				var result = callbackHelper.InvokeStdcallCallback(lpCallback, parameters);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (result == null)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateW: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateW: callback returned {Result}", result.Value);

				// Callback returns FALSE (0) to stop enumeration, non-zero to continue
				// Since we only have one device, we always return success
				return (uint)DDResult.DD_OK;
			}
			catch (OutOfMemoryException)
			{
				throw; // Rethrow non-recoverable exceptions
			}
			catch (StackOverflowException)
			{
				throw; // Rethrow non-recoverable exceptions
			}
			catch (System.Threading.ThreadAbortException)
			{
				throw; // Rethrow non-recoverable exceptions
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateW: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		#region Async Enumerate Functions

		/// <summary>
		/// Async implementation of DirectDrawEnumerateA.
		/// </summary>
		private async Task<uint> DirectDrawEnumerateAAsync(uint lpCallback, uint lpContext, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateAAsync(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8})", lpCallback, lpContext);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateAAsync: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateString(driverDescription);

				// Allocate and write driver name string
				var driverName = "display";
				var namePtr = AllocateString(driverName);

				_logger.LogDebug("[DDraw] Allocated strings: desc=0x{Desc:X8}, name=0x{Name:X8}", descPtr, namePtr);

				// Invoke callback asynchronously: DDEnumCallback(lpGUID, lpDriverDescription, lpDriverName, lpContext)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext };
				var result = await InvokeCallbackAsync(lpCallback, parameters, 4, cancellationToken).ConfigureAwait(false);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (!result.success)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateAAsync: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateAAsync: callback returned {Result}", result.returnValue);
				return (uint)DDResult.DD_OK;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateAAsync: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		/// <summary>
		/// Async implementation of DirectDrawEnumerateExA.
		/// </summary>
		private async Task<uint> DirectDrawEnumerateExAAsync(uint lpCallback, uint lpContext, uint dwFlags, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateExAAsync(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8}, dwFlags=0x{DwFlags:X8})", lpCallback, lpContext, dwFlags);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateExAAsync: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateString(driverDescription);

				// Allocate and write driver name string
				var driverName = "display";
				var namePtr = AllocateString(driverName);

				// Monitor handle (just use a dummy value for primary monitor)
				uint hMonitor = 0x00010001;

				_logger.LogDebug("[DDraw] Allocated strings: desc=0x{Desc:X8}, name=0x{Name:X8}, hMonitor=0x{HMonitor:X8}", descPtr, namePtr, hMonitor);

				// Invoke callback asynchronously: DDEnumCallbackEx(lpGUID, lpDriverDescription, lpDriverName, lpContext, hMonitor)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext, hMonitor };
				var result = await InvokeCallbackAsync(lpCallback, parameters, 5, cancellationToken).ConfigureAwait(false);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (!result.success)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateExAAsync: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateExAAsync: callback returned {Result}", result.returnValue);
				return (uint)DDResult.DD_OK;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateExAAsync: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		/// <summary>
		/// Async implementation of DirectDrawEnumerateW.
		/// </summary>
		private async Task<uint> DirectDrawEnumerateWAsync(uint lpCallback, uint lpContext, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateWAsync(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8})", lpCallback, lpContext);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateWAsync: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string (Unicode)
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateUnicodeString(driverDescription);

				// Allocate and write driver name string (Unicode)
				var driverName = "display";
				var namePtr = AllocateUnicodeString(driverName);

				_logger.LogDebug("[DDraw] Allocated Unicode strings: desc=0x{Desc:X8}, name=0x{Name:X8}", descPtr, namePtr);

				// Invoke callback asynchronously: DDEnumCallbackW(lpGUID, lpDriverDescription, lpDriverName, lpContext)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext };
				var result = await InvokeCallbackAsync(lpCallback, parameters, 4, cancellationToken).ConfigureAwait(false);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (!result.success)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateWAsync: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateWAsync: callback returned {Result}", result.returnValue);
				return (uint)DDResult.DD_OK;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateWAsync: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		/// <summary>
		/// Async implementation of DirectDrawEnumerateExW.
		/// </summary>
		private async Task<uint> DirectDrawEnumerateExWAsync(uint lpCallback, uint lpContext, uint dwFlags, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[DDraw] DirectDrawEnumerateExWAsync(lpCallback=0x{LpCallback:X8}, lpContext=0x{LpContext:X8}, dwFlags=0x{DwFlags:X8})", lpCallback, lpContext, dwFlags);

			if (lpCallback == 0)
			{
				_logger.LogError("[DDraw] DirectDrawEnumerateExWAsync: callback is null");
				return (uint)DDResult.DDERR_INVALIDPARAMS;
			}

			try
			{
				// Allocate memory for GUID (NULL for primary display driver)
				uint guidPtr = 0; // NULL indicates primary display driver

				// Allocate and write driver description string (Unicode)
				var driverDescription = "Primary Display Driver";
				var descPtr = AllocateUnicodeString(driverDescription);

				// Allocate and write driver name string (Unicode)
				var driverName = "display";
				var namePtr = AllocateUnicodeString(driverName);

				// Monitor handle (just use a dummy value for primary monitor)
				uint hMonitor = 0x00010001;

				_logger.LogDebug("[DDraw] Allocated Unicode strings: desc=0x{Desc:X8}, name=0x{Name:X8}, hMonitor=0x{HMonitor:X8}", descPtr, namePtr, hMonitor);

				// Invoke callback asynchronously: DDEnumCallbackExW(lpGUID, lpDriverDescription, lpDriverName, lpContext, hMonitor)
				var parameters = new uint[] { guidPtr, descPtr, namePtr, lpContext, hMonitor };
				var result = await InvokeCallbackAsync(lpCallback, parameters, 5, cancellationToken).ConfigureAwait(false);

				// Free allocated strings
				FreeString(descPtr);
				FreeString(namePtr);

				if (!result.success)
				{
					_logger.LogError("[DDraw] DirectDrawEnumerateExWAsync: callback invocation failed");
					return (uint)DDResult.DDERR_GENERIC;
				}

				_logger.LogInformation("[DDraw] DirectDrawEnumerateExWAsync: callback returned {Result}", result.returnValue);
				return (uint)DDResult.DD_OK;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] DirectDrawEnumerateExWAsync: exception during enumeration");
				return (uint)DDResult.DDERR_GENERIC;
			}
		}

		/// <summary>
		/// Async version of callback invocation that uses CpuHelpers.ExecuteAsync for WASM compatibility.
		/// This eliminates the need for GetAwaiter().GetResult() which throws PlatformNotSupportedException on WASM.
		/// </summary>
		/// <param name="callbackAddress">Address of the callback function in emulated memory</param>
		/// <param name="parameters">Parameters to pass to the callback (pushed right-to-left)</param>
		/// <param name="paramCount">Number of parameters for stack cleanup calculation</param>
		/// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
		/// <returns>Tuple containing success flag and return value from EAX</returns>
		private async Task<(bool success, uint returnValue)> InvokeCallbackAsync(
			uint callbackAddress, 
			uint[] parameters, 
			int paramCount,
			CancellationToken cancellationToken = default)
		{
			if (_currentCpu == null || _currentMemory == null)
			{
				_logger.LogWarning("[DDraw] InvokeCallbackAsync: CPU or Memory not available");
				return (false, 0);
			}

			_logger.LogInformation("[DDraw] InvokeCallbackAsync: Calling 0x{CallbackAddress:X8}", callbackAddress);

			// Validate callback address
			if (callbackAddress == 0)
			{
				_logger.LogWarning("[DDraw] InvokeCallbackAsync: Callback address is NULL (0x00000000), aborting");
				return (false, 0);
			}

			// Save current CPU state
			var savedEip = _currentCpu.GetEip();
			var savedEsp = _currentCpu.GetRegister("ESP");
			var savedEbp = _currentCpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			var esp = savedEsp;

			// Push return address first
			esp -= 4;
			_currentMemory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			for (int i = parameters.Length - 1; i >= 0; i--)
			{
				esp -= 4;
				_currentMemory.Write32(esp, parameters[i]);
			}

			// Update CPU registers
			_currentCpu.SetRegister("ESP", esp);
			_currentCpu.SetEip(callbackAddress);

			// Execute until we hit the return address with cancellation support
			// WASM: Use lower yield interval to keep browser responsive during callbacks
			var steps = 0;
			var executionSuccessful = true;
			var lastCheckEip = _currentCpu.GetEip();
			var stuckCounter = 0;
			
			// Emergency timeout for callbacks - prevent browser freeze
			var startTime = DateTime.UtcNow;

			try
			{
				while (true)
				{
					// Emergency timeout check for WASM to prevent browser freeze
					// Check every 100 iterations to reduce DateTime.UtcNow overhead
					if (PlatformHelpers.IsWasm && steps % 100 == 0)
					{
						var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
						if (elapsed > CALLBACK_TIMEOUT_MS)
						{
							_logger.LogError("[DDraw] InvokeCallbackAsync: Callback execution timeout after {Ms}ms, aborting", elapsed);
							executionSuccessful = false;
							break;
						}
					}
					// Check for cancellation at regular intervals
					if (steps % CpuHelpers.CANCELLATION_CHECK_INTERVAL == 0)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							_logger.LogInformation("[DDraw] InvokeCallbackAsync: Cancellation requested at step {Steps}", steps);
							executionSuccessful = false;
							break;
						}

											// Suspend execution to preserve CPU state across async boundary
					var cpuState = CpuHelpers.SuspendExecution(_currentCpu);
					
					// Yield to allow other async operations to proceed
					// Use Task.Delay(1) in WASM to actually return control to browser event loop
					// Task.Yield() only yields to .NET scheduler, not to JavaScript
					if (PlatformHelpers.IsWasm)
					{
						await Task.Delay(1);
					}
					else
					{
						await Task.Yield();
					}
					
					// Resume execution with preserved state
					CpuHelpers.ResumeExecution(_currentCpu, cpuState);
					}

					var eip = _currentCpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogWarning("[DDraw] InvokeCallbackAsync: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting");
						executionSuccessful = false;
						break;
					}

					// Check for other invalid low addresses
					if (eip < CpuHelpers.MINIMUM_VALID_EIP && eip != RETURN_ADDRESS)
					{
						_logger.LogError("[DDraw] InvokeCallbackAsync: Execution jumped to invalid low address 0x{Eip:X8}", eip);
						executionSuccessful = false;
						break;
					}

					// Detect potential infinite loops
					if (steps > 0 && steps % CpuHelpers.INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = _currentCpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= CpuHelpers.STUCK_COUNTER_THRESHOLD)
							{
								_logger.LogWarning("[DDraw] InvokeCallbackAsync: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting", 
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
					await CpuHelpers.ExecuteAsync(_currentCpu, _currentMemory);
					steps++;

					// Periodically yield for cooperative multitasking
					// In WASM, use Task.Delay(1) to return control to browser event loop
					// Task.Yield() doesn't work in WASM - only yields to .NET scheduler
					if (steps % CALLBACK_YIELD_INTERVAL == 0)
					{
						if (PlatformHelpers.IsWasm)
						{
							await Task.Delay(1);
						}
						else
						{
							await Task.Yield();
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DDraw] InvokeCallbackAsync: Exception during execution: {ExMessage}", ex.Message);
				executionSuccessful = false;
			}

			// Get return value from EAX, but only if execution was successful
			var returnValue = executionSuccessful ? _currentCpu.GetRegister("EAX") : 0u;

			// Restore CPU state
			_currentCpu.SetEip(savedEip);
			_currentCpu.SetRegister("ESP", savedEsp);
			_currentCpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[DDraw] InvokeCallbackAsync: Completed with return value 0x{ReturnValue:X8}", returnValue);

			return (executionSuccessful, returnValue);
		}

		#endregion

		[DllModuleExport(15, entryPoint: 0x00022768, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry18()
		{
			_logger.LogWarning("[ddraw] DdEntry18 called (stub)");
			// TODO: Implement DdEntry18
			return 0; // DWORD default
		}

		[DllModuleExport(38, entryPoint: 0x0002AC54, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(16, entryPoint: 0x0002F32D, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllCanUnloadNow()
		{
			_logger.LogWarning("[ddraw] DllCanUnloadNow called (stub)");
			// TODO: Implement DllCanUnloadNow
			return 0; // DWORD default
		}

		[DllModuleExport(16, entryPoint: 0x00025EA7, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry19()
		{
			_logger.LogWarning("[ddraw] DdEntry19 called (stub)");
			// TODO: Implement DdEntry19
			return 0; // DWORD default
		}

		[DllModuleExport(39, entryPoint: 0x0002AB29, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(17, entryPoint: 0x0002F1BF, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllGetClassObject()
		{
			_logger.LogWarning("[ddraw] DllGetClassObject called (stub)");
			// TODO: Implement DllGetClassObject
			return 0; // DWORD default
		}

		[DllModuleExport(17, entryPoint: 0x00021EB8, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry1()
		{
			_logger.LogWarning("[ddraw] DdEntry1 called (stub)");
			// TODO: Implement DdEntry1
			return 0; // DWORD default
		}

		[DllModuleExport(41, entryPoint: 0x0002B513, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(18, entryPoint: 0x0002B7E9, Version = "5.1.2600.6532", IsStub = true)]
		public uint GetDDSurfaceLocal()
		{
			_logger.LogWarning("[ddraw] GetDDSurfaceLocal called (stub)");
			// TODO: Implement GetDDSurfaceLocal
			return 0; // DWORD default
		}

		[DllModuleExport(18, entryPoint: 0x00022786, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry20()
		{
			_logger.LogWarning("[ddraw] DdEntry20 called (stub)");
			// TODO: Implement DdEntry20
			return 0; // DWORD default
		}

		[DllModuleExport(43, entryPoint: 0x0003308E, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(19, entryPoint: 0x0002B619, Version = "5.1.2600.6532", IsStub = true)]
		public uint GetOLEThunkData()
		{
			_logger.LogWarning("[ddraw] GetOLEThunkData called (stub)");
			// TODO: Implement GetOLEThunkData
			return 0; // DWORD default
		}

		[DllModuleExport(19, entryPoint: 0x000227ED, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry21()
		{
			_logger.LogWarning("[ddraw] DdEntry21 called (stub)");
			// TODO: Implement DdEntry21
			return 0; // DWORD default
		}

		[DllModuleExport(44, entryPoint: 0x000302AB, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(20, entryPoint: 0x0003089F, Version = "5.1.2600.6532", IsStub = true)]
		public uint GetSurfaceFromDC()
		{
			_logger.LogWarning("[ddraw] GetSurfaceFromDC called (stub)");
			// TODO: Implement GetSurfaceFromDC
			return 0; // DWORD default
		}

		[DllModuleExport(20, entryPoint: 0x00028B59, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry22()
		{
			_logger.LogWarning("[ddraw] DdEntry22 called (stub)");
			// TODO: Implement DdEntry22
			return 0; // DWORD default
		}

		[DllModuleExport(50, entryPoint: 0x000087E7, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(21, entryPoint: 0x0002009C, Version = "5.1.2600.6532", IsStub = true)]
		public uint RegisterSpecialCase()
		{
			_logger.LogWarning("[ddraw] RegisterSpecialCase called (stub)");
			// TODO: Implement RegisterSpecialCase
			return 0; // DWORD default
		}

		[DllModuleExport(21, entryPoint: 0x00028383, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry23()
		{
			_logger.LogWarning("[ddraw] DdEntry23 called (stub)");
			// TODO: Implement DdEntry23
			return 0; // DWORD default
		}

		[DllModuleExport(51, entryPoint: 0x000178FB, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(22, entryPoint: 0x00004789, Version = "5.1.2600.6532", IsStub = true)]
		public uint ReleaseDDThreadLock()
		{
			_logger.LogWarning("[ddraw] ReleaseDDThreadLock called (stub)");
			// TODO: Implement ReleaseDDThreadLock
			return 0; // DWORD default
		}

		[DllModuleExport(22, entryPoint: 0x0002851F, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry24()
		{
			_logger.LogWarning("[ddraw] DdEntry24 called (stub)");
			// TODO: Implement DdEntry24
			return 0; // DWORD default
		}

		[DllModuleExport(23, entryPoint: 0x00026758, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry2()
		{
			_logger.LogWarning("[ddraw] DdEntry2 called (stub)");
			// TODO: Implement DdEntry2
			return 0; // DWORD default
		}

		[DllModuleExport(24, entryPoint: 0x000257F3, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry3()
		{
			_logger.LogWarning("[ddraw] DdEntry3 called (stub)");
			// TODO: Implement DdEntry3
			return 0; // DWORD default
		}

		[DllModuleExport(25, entryPoint: 0x0002585D, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry4()
		{
			_logger.LogWarning("[ddraw] DdEntry4 called (stub)");
			// TODO: Implement DdEntry4
			return 0; // DWORD default
		}

		[DllModuleExport(26, entryPoint: 0x00022115, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry5()
		{
			_logger.LogWarning("[ddraw] DdEntry5 called (stub)");
			// TODO: Implement DdEntry5
			return 0; // DWORD default
		}

		[DllModuleExport(27, entryPoint: 0x00022050, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry6()
		{
			_logger.LogWarning("[ddraw] DdEntry6 called (stub)");
			// TODO: Implement DdEntry6
			return 0; // DWORD default
		}

		[DllModuleExport(28, entryPoint: 0x0002590A, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry7()
		{
			_logger.LogWarning("[ddraw] DdEntry7 called (stub)");
			// TODO: Implement DdEntry7
			return 0; // DWORD default
		}

		[DllModuleExport(29, entryPoint: 0x00022171, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry8()
		{
			_logger.LogWarning("[ddraw] DdEntry8 called (stub)");
			// TODO: Implement DdEntry8
			return 0; // DWORD default
		}

		[DllModuleExport(30, entryPoint: 0x00022354, Version = "4.90.0.3000", IsStub = true)]
		public uint DdEntry9()
		{
			_logger.LogWarning("[ddraw] DdEntry9 called (stub)");
			// TODO: Implement DdEntry9
			return 0; // DWORD default
		}

		[DllModuleExport(40, entryPoint: 0x0002AEF8, Version = "4.90.0.3000", IsStub = true)]
		public uint GetAliasedVidMem()
		{
			_logger.LogWarning("[ddraw] GetAliasedVidMem called (stub)");
			// TODO: Implement GetAliasedVidMem
			return 0; // DWORD default
		}

		[DllModuleExport(42, entryPoint: 0x00030A18, Version = "4.90.0.3000", IsStub = true)]
		public uint GetNextMipMap()
		{
			_logger.LogWarning("[ddraw] GetNextMipMap called (stub)");
			// TODO: Implement GetNextMipMap
			return 0; // DWORD default
		}

		[DllModuleExport(45, entryPoint: 0x00033908, Version = "4.90.0.3000", IsStub = true)]
		public uint HeapVidMemAllocAligned()
		{
			_logger.LogWarning("[ddraw] HeapVidMemAllocAligned called (stub)");
			// TODO: Implement HeapVidMemAllocAligned
			return 0; // DWORD default
		}

		[DllModuleExport(46, entryPoint: 0x00017FB9, Version = "4.90.0.3000", IsStub = true)]
		public uint InternalLock()
		{
			_logger.LogWarning("[ddraw] InternalLock called (stub)");
			// TODO: Implement InternalLock
			return 0; // DWORD default
		}

		[DllModuleExport(47, entryPoint: 0x0001798B, Version = "4.90.0.3000", IsStub = true)]
		public uint InternalUnlock()
		{
			_logger.LogWarning("[ddraw] InternalUnlock called (stub)");
			// TODO: Implement InternalUnlock
			return 0; // DWORD default
		}

		[DllModuleExport(48, entryPoint: 0x00030A54, Version = "4.90.0.3000", IsStub = true)]
		public uint LateAllocateSurfaceMem()
		{
			_logger.LogWarning("[ddraw] LateAllocateSurfaceMem called (stub)");
			// TODO: Implement LateAllocateSurfaceMem
			return 0; // DWORD default
		}

		[DllModuleExport(49, entryPoint: 0x0002312E, Version = "4.90.0.3000", IsStub = true)]
		public uint LockCB()
		{
			_logger.LogWarning("[ddraw] LockCB called (stub)");
			// TODO: Implement LockCB
			return 0; // DWORD default
		}

		[DllModuleExport(52, entryPoint: 0x00021469, Version = "4.90.0.3000", IsStub = true)]
		public uint UnlockCB()
		{
			_logger.LogWarning("[ddraw] UnlockCB called (stub)");
			// TODO: Implement UnlockCB
			return 0; // DWORD default
		}

		[DllModuleExport(53, entryPoint: 0x00033413, Version = "4.90.0.3000", IsStub = true)]
		public uint VidMemAlloc()
		{
			_logger.LogWarning("[ddraw] VidMemAlloc called (stub)");
			// TODO: Implement VidMemAlloc
			return 0; // DWORD default
		}

		[DllModuleExport(54, entryPoint: 0x00033483, Version = "4.90.0.3000", IsStub = true)]
		public uint VidMemAmountFree()
		{
			_logger.LogWarning("[ddraw] VidMemAmountFree called (stub)");
			// TODO: Implement VidMemAmountFree
			return 0; // DWORD default
		}

		[DllModuleExport(55, entryPoint: 0x00033398, Version = "4.90.0.3000", IsStub = true)]
		public uint VidMemFini()
		{
			_logger.LogWarning("[ddraw] VidMemFini called (stub)");
			// TODO: Implement VidMemFini
			return 0; // DWORD default
		}

		[DllModuleExport(56, entryPoint: 0x00033437, Version = "4.90.0.3000", IsStub = true)]
		public uint VidMemFree()
		{
			_logger.LogWarning("[ddraw] VidMemFree called (stub)");
			// TODO: Implement VidMemFree
			return 0; // DWORD default
		}

		[DllModuleExport(57, entryPoint: 0x0003332E, Version = "4.90.0.3000", IsStub = true)]
		public uint VidMemInit()
		{
			_logger.LogWarning("[ddraw] VidMemInit called (stub)");
			// TODO: Implement VidMemInit
			return 0; // DWORD default
		}

		[DllModuleExport(58, entryPoint: 0x000334A7, Version = "4.90.0.3000", IsStub = true)]
		public uint VidMemLargestFree()
		{
			_logger.LogWarning("[ddraw] VidMemLargestFree called (stub)");
			// TODO: Implement VidMemLargestFree
			return 0; // DWORD default
		}

		[DllModuleExport(59, entryPoint: 0x00057648, Version = "4.90.0.3000", IsStub = true)]
		public uint thk1632_ThunkData32()
		{
			_logger.LogWarning("[ddraw] thk1632_ThunkData32 called (stub)");
			// TODO: Implement thk1632_ThunkData32
			return 0; // DWORD default
		}

		[DllModuleExport(60, entryPoint: 0x0005766C, Version = "4.90.0.3000", IsStub = true)]
		public uint thk3216_ThunkData32()
		{
			_logger.LogWarning("[ddraw] thk3216_ThunkData32 called (stub)");
			// TODO: Implement thk3216_ThunkData32
			return 0; // DWORD default
		}
	}
}