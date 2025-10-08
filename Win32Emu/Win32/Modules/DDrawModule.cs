using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class DDrawModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

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
		private readonly Dictionary<uint, DirectDrawSurface> _surfaces = new();
		private uint _nextDDrawHandle = 0x70000000;
		private uint _nextSurfaceHandle = 0x71000000;

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTDRAWCREATE":
					returnValue = DirectDrawCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "DIRECTDRAWCREATEEX":
					returnValue = DirectDrawCreateEx(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				default:
					_logger.LogInformation("[DDraw] Unimplemented export: {Export}", export);
					return false;
			}
		}

		/// <summary>
		/// 
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
		[DllModuleExport(9)]
		private unsafe uint DirectDrawCreate(in uint lpGuid, uint lplpDd, in uint pUnkOuter)
		{
			_logger.LogInformation("[DDraw] DirectDrawCreate(lpGuid=0x{LpGuid:X8}, lplpDD=0x{LplpDd:X8}, pUnkOuter=0x{PUnkOuter:X8})", lpGuid, lplpDd, pUnkOuter);

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
			var vtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "Compact", (cpu, mem) => DDraw_Compact(cpu, mem) },
				{ "CreateClipper", (cpu, mem) => DDraw_CreateClipper(cpu, mem) },
				{ "CreatePalette", (cpu, mem) => DDraw_CreatePalette(cpu, mem) },
				{ "CreateSurface", (cpu, mem) => DDraw_CreateSurface(cpu, mem) },
				{ "DuplicateSurface", (cpu, mem) => DDraw_DuplicateSurface(cpu, mem) },
				{ "EnumDisplayModes", (cpu, mem) => DDraw_EnumDisplayModes(cpu, mem) },
				{ "EnumSurfaces", (cpu, mem) => DDraw_EnumSurfaces(cpu, mem) },
				{ "FlipToGDISurface", (cpu, mem) => DDraw_FlipToGDISurface(cpu, mem) },
				{ "GetCaps", (cpu, mem) => DDraw_GetCaps(cpu, mem) },
				{ "GetDisplayMode", (cpu, mem) => DDraw_GetDisplayMode(cpu, mem) },
				{ "GetFourCCCodes", (cpu, mem) => DDraw_GetFourCCCodes(cpu, mem) },
				{ "GetGDISurface", (cpu, mem) => DDraw_GetGDISurface(cpu, mem) },
				{ "GetMonitorFrequency", (cpu, mem) => DDraw_GetMonitorFrequency(cpu, mem) },
				{ "GetScanLine", (cpu, mem) => DDraw_GetScanLine(cpu, mem) },
				{ "GetVerticalBlankStatus", (cpu, mem) => DDraw_GetVerticalBlankStatus(cpu, mem) },
				{ "Initialize", (cpu, mem) => DDraw_Initialize(cpu, mem) },
				{ "RestoreDisplayMode", (cpu, mem) => DDraw_RestoreDisplayMode(cpu, mem) },
				{ "SetCooperativeLevel", (cpu, mem) => DDraw_SetCooperativeLevel(cpu, mem, ddrawHandle) },
				{ "SetDisplayMode", (cpu, mem) => DDraw_SetDisplayMode(cpu, mem, ddrawHandle) },
				{ "WaitForVerticalBlank", (cpu, mem) => DDraw_WaitForVerticalBlank(cpu, mem) }
			};

// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectDraw", vtableMethods);

// Write COM object pointer to output parameter
			if (lplpDd != 0)
			{
				_env.MemWrite32(lplpDd, comObjectAddr);
			}

			_logger.LogInformation("[DDraw] Created IDirectDraw COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
			return 0; // DD_OK
		}


		[DllModuleExport(11)]
		private unsafe uint DirectDrawCreateEx(uint lpGuid, uint lplpDd, uint iid, uint pUnkOuter)
		{
			_logger.LogInformation("[DDraw] DirectDrawCreateEx(lpGuid=0x{LpGuid:X8}, lplpDD=0x{LplpDd:X8}, iid=0x{Iid:X8}, pUnkOuter=0x{PUnkOuter:X8})", lpGuid, lplpDd, iid, pUnkOuter);

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
			var vtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "Compact", (cpu, mem) => DDraw_Compact(cpu, mem) },
				{ "CreateClipper", (cpu, mem) => DDraw_CreateClipper(cpu, mem) },
				{ "CreatePalette", (cpu, mem) => DDraw_CreatePalette(cpu, mem) },
				{ "CreateSurface", (cpu, mem) => DDraw_CreateSurface(cpu, mem) },
				{ "DuplicateSurface", (cpu, mem) => DDraw_DuplicateSurface(cpu, mem) },
				{ "EnumDisplayModes", (cpu, mem) => DDraw_EnumDisplayModes(cpu, mem) },
				{ "EnumSurfaces", (cpu, mem) => DDraw_EnumSurfaces(cpu, mem) },
				{ "FlipToGDISurface", (cpu, mem) => DDraw_FlipToGDISurface(cpu, mem) },
				{ "GetCaps", (cpu, mem) => DDraw_GetCaps(cpu, mem) },
				{ "GetDisplayMode", (cpu, mem) => DDraw_GetDisplayMode(cpu, mem) },
				{ "GetFourCCCodes", (cpu, mem) => DDraw_GetFourCCCodes(cpu, mem) },
				{ "GetGDISurface", (cpu, mem) => DDraw_GetGDISurface(cpu, mem) },
				{ "GetMonitorFrequency", (cpu, mem) => DDraw_GetMonitorFrequency(cpu, mem) },
				{ "GetScanLine", (cpu, mem) => DDraw_GetScanLine(cpu, mem) },
				{ "GetVerticalBlankStatus", (cpu, mem) => DDraw_GetVerticalBlankStatus(cpu, mem) },
				{ "Initialize", (cpu, mem) => DDraw_Initialize(cpu, mem) },
				{ "RestoreDisplayMode", (cpu, mem) => DDraw_RestoreDisplayMode(cpu, mem) },
				{ "SetCooperativeLevel", (cpu, mem) => DDraw_SetCooperativeLevel(cpu, mem, ddrawHandle) },
				{ "SetDisplayMode", (cpu, mem) => DDraw_SetDisplayMode(cpu, mem, ddrawHandle) },
				{ "WaitForVerticalBlank", (cpu, mem) => DDraw_WaitForVerticalBlank(cpu, mem) }
			};

			// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectDraw", vtableMethods);

			// Write COM object pointer to output parameter
			if (lplpDd != 0)
			{
				_env.MemWrite32(lplpDd, comObjectAddr);
			}

			_logger.LogInformation("[DDraw] Created IDirectDraw COM object (Ex) at 0x{ComObjectAddr:X8}", comObjectAddr);
			return 0; // DD_OK
		}

		private sealed class DirectDrawObject
		{
			public uint Handle { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public int BitsPerPixel { get; set; }
		}

		private sealed class DirectDrawSurface
		{
			public uint Handle { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public int Pitch { get; set; }
			public byte[]? Bits { get; set; }
			public bool IsPrimary { get; set; }
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
			return 0x80004002;
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

		private uint DDraw_Compact(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Compact() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_CreateClipper(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::CreateClipper() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_CreatePalette(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::CreatePalette() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_CreateSurface(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::CreateSurface() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_DuplicateSurface(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::DuplicateSurface() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_EnumDisplayModes(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::EnumDisplayModes() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_EnumSurfaces(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::EnumSurfaces() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_FlipToGDISurface(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::FlipToGDISurface() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetCaps() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_GetDisplayMode(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetDisplayMode() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_GetFourCCCodes(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetFourCCCodes() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_GetGDISurface(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetGDISurface() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_GetMonitorFrequency(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetMonitorFrequency() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_GetScanLine(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetScanLine() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_GetVerticalBlankStatus(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetVerticalBlankStatus() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Initialize() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_RestoreDisplayMode(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::RestoreDisplayMode() - stub");
			return 0; // DD_OK
		}

		private uint DDraw_SetCooperativeLevel(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var hWnd = args.UInt32(1);
			var dwFlags = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDraw::SetCooperativeLevel(this=0x{ThisPtr:X8}, hWnd=0x{HWnd:X8}, flags=0x{DwFlags:X8})", thisPtr, hWnd, dwFlags);

			// Store cooperation level settings
			if (_ddrawObjects.TryGetValue(ddrawHandle, out var obj))
			{
				// Store flags for future reference
			}

			return 0; // DD_OK
		}

		private uint DDraw_SetDisplayMode(ICpu cpu, VirtualMemory memory, uint ddrawHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwWidth = args.UInt32(1);
			var dwHeight = args.UInt32(2);
			var dwBPP = args.UInt32(3);

			_logger.LogInformation("[DDraw COM] IDirectDraw::SetDisplayMode(this=0x{ThisPtr:X8}, width={DwWidth}, height={DwHeight}, bpp={DwBpp})", thisPtr, dwWidth, dwHeight, dwBPP);

			// Store display mode settings
			if (_ddrawObjects.TryGetValue(ddrawHandle, out var obj))
			{
				obj.Width = (int)dwWidth;
				obj.Height = (int)dwHeight;
				obj.BitsPerPixel = (int)dwBPP;
			}

			return 0; // DD_OK
		}

		private uint DDraw_WaitForVerticalBlank(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::WaitForVerticalBlank() - stub");
			return 0; // DD_OK
		}
	}
}