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
		private uint DirectDrawCreate(in uint lpGuid, uint lplpDd, in uint pUnkOuter)
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
		private uint DirectDrawCreateEx(uint lpGuid, uint lplpDd, uint iid, uint pUnkOuter)
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
			public Rendering.Sdl3RenderingBackend? RenderingBackend { get; set; }
			public uint CooperativeLevel { get; set; }
			public IntPtr WindowHandle { get; set; }
		}

		private sealed class DirectDrawSurface
	{
		public uint Handle { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Pitch { get; set; }
		public byte[]? Bits { get; set; }
		public bool IsPrimary { get; set; }
		public bool IsLocked { get; set; }
		public uint DirectDrawHandle { get; set; }
		public IntPtr TexturePtr { get; set; }
		public uint LockedMemoryPtr { get; set; }
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
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceDesc = args.UInt32(1);
			var lplpDDSurface = args.UInt32(2);
			var pUnkOuter = args.UInt32(3);
			
			_logger.LogInformation("[DDraw COM] IDirectDraw::CreateSurface(this=0x{ThisPtr:X8}, lpDDSurfaceDesc=0x{LpDDSurfaceDesc:X8}, lplpDDSurface=0x{LplpDDSurface:X8}, pUnkOuter=0x{PUnkOuter:X8})", thisPtr, lpDDSurfaceDesc, lplpDDSurface, pUnkOuter);
			
			// Read surface description
			uint dwSize = _env.MemRead32(lpDDSurfaceDesc);
			uint dwFlags = _env.MemRead32(lpDDSurfaceDesc + 4);
			uint dwWidth = _env.MemRead32(lpDDSurfaceDesc + 8);
			uint dwHeight = _env.MemRead32(lpDDSurfaceDesc + 12);
			
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
				return 1; // DDERR_GENERIC
			}
			
			// Create a new surface
			var surfaceHandle = _nextSurfaceHandle++;
			var surface = new DirectDrawSurface
			{
				Handle = surfaceHandle,
				Width = (int)dwWidth,
				Height = (int)dwHeight,
				DirectDrawHandle = ddrawHandle,
				IsPrimary = (dwFlags & 0x00000001) != 0, // DDSCAPS_PRIMARYSURFACE
				Pitch = (int)dwWidth * (ddrawObj.BitsPerPixel / 8)
			};
			
			// Allocate memory for the surface
			surface.Bits = new byte[surface.Pitch * surface.Height];
			
			// Store the surface
			_surfaces[surfaceHandle] = surface;
			
			// Create COM vtable for IDirectDrawSurface interface
			var vtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "AddAttachedSurface", (cpu, mem) => Surface_AddAttachedSurface(cpu, mem) },
				{ "AddOverlayDirtyRect", (cpu, mem) => Surface_AddOverlayDirtyRect(cpu, mem) },
				{ "Blt", (cpu, mem) => Surface_Blt(cpu, mem) },
				{ "BltBatch", (cpu, mem) => Surface_BltBatch(cpu, mem) },
				{ "BltFast", (cpu, mem) => Surface_BltFast(cpu, mem) },
				{ "DeleteAttachedSurface", (cpu, mem) => Surface_DeleteAttachedSurface(cpu, mem) },
				{ "EnumAttachedSurfaces", (cpu, mem) => Surface_EnumAttachedSurfaces(cpu, mem) },
				{ "EnumOverlayZOrders", (cpu, mem) => Surface_EnumOverlayZOrders(cpu, mem) },
				{ "Flip", (cpu, mem) => Surface_Flip(cpu, mem) },
				{ "GetAttachedSurface", (cpu, mem) => Surface_GetAttachedSurface(cpu, mem) },
				{ "GetBltStatus", (cpu, mem) => Surface_GetBltStatus(cpu, mem) },
				{ "GetCaps", (cpu, mem) => Surface_GetCaps(cpu, mem) },
				{ "GetClipper", (cpu, mem) => Surface_GetClipper(cpu, mem) },
				{ "GetColorKey", (cpu, mem) => Surface_GetColorKey(cpu, mem) },
				{ "GetDC", (cpu, mem) => Surface_GetDC(cpu, mem) },
				{ "GetFlipStatus", (cpu, mem) => Surface_GetFlipStatus(cpu, mem) },
				{ "GetOverlayPosition", (cpu, mem) => Surface_GetOverlayPosition(cpu, mem) },
				{ "GetPalette", (cpu, mem) => Surface_GetPalette(cpu, mem) },
				{ "GetPixelFormat", (cpu, mem) => Surface_GetPixelFormat(cpu, mem) },
				{ "GetSurfaceDesc", (cpu, mem) => Surface_GetSurfaceDesc(cpu, mem) },
				{ "Initialize", (cpu, mem) => Surface_Initialize(cpu, mem) },
				{ "IsLost", (cpu, mem) => Surface_IsLost(cpu, mem) },
				{ "Lock", (cpu, mem) => Surface_Lock(cpu, mem, surfaceHandle) },
				{ "ReleaseDC", (cpu, mem) => Surface_ReleaseDC(cpu, mem) },
				{ "Restore", (cpu, mem) => Surface_Restore(cpu, mem) },
				{ "SetClipper", (cpu, mem) => Surface_SetClipper(cpu, mem) },
				{ "SetColorKey", (cpu, mem) => Surface_SetColorKey(cpu, mem) },
				{ "SetOverlayPosition", (cpu, mem) => Surface_SetOverlayPosition(cpu, mem) },
				{ "SetPalette", (cpu, mem) => Surface_SetPalette(cpu, mem) },
				{ "Unlock", (cpu, mem) => Surface_Unlock(cpu, mem, surfaceHandle) },
				{ "UpdateOverlay", (cpu, mem) => Surface_UpdateOverlay(cpu, mem) },
				{ "UpdateOverlayDisplay", (cpu, mem) => Surface_UpdateOverlayDisplay(cpu, mem) },
				{ "UpdateOverlayZOrder", (cpu, mem) => Surface_UpdateOverlayZOrder(cpu, mem) }
			};
			
			// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectDrawSurface", vtableMethods);
			
			// Write COM object pointer to output parameter
			if (lplpDDSurface != 0)
			{
				_env.MemWrite32(lplpDDSurface, comObjectAddr);
			}
			
			_logger.LogInformation("[DDraw] Created IDirectDrawSurface COM object at 0x{ComObjectAddr:X8} for surface 0x{SurfaceHandle:X8}", comObjectAddr, surfaceHandle);
			return 0; // DD_OK
		}

		private uint Surface_UpdateOverlayZOrder(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::UpdateOverlayZOrder() - stub");
			return 0; // DD_OK
		}

		private uint Surface_UpdateOverlayDisplay(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::UpdateOverlayDisplay() - stub");
			return 0; // DD_OK
		}

		private uint Surface_UpdateOverlay(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::UpdateOverlay() - stub");
			return 0; // DD_OK
		}

		private uint Surface_SetPalette(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::SetPalette() - stub");
			return 0; // DD_OK
		}

		private uint Surface_SetOverlayPosition(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::SetOverlayPosition() - stub");
			return 0; // DD_OK
		}

		private uint Surface_SetColorKey(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::SetColorKey() - stub");
			return 0; // DD_OK
		}

		private uint Surface_SetClipper(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::SetClipper() - stub");
			return 0; // DD_OK
		}

		private uint Surface_Restore(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Restore() - stub");
			return 0; // DD_OK
		}

		private uint Surface_ReleaseDC(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::ReleaseDC() - stub");
			return 0; // DD_OK
		}

		private uint Surface_IsLost(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::IsLost() - stub");
			return 0; // DD_OK
		}

		private uint Surface_Initialize(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Initialize() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetSurfaceDesc(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetSurfaceDesc() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetPixelFormat(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetPixelFormat() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetPalette(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetPalette() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetOverlayPosition(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetOverlayPosition() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetFlipStatus(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetFlipStatus() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetDC(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetDC() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetColorKey(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetColorKey() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetClipper(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetClipper() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetCaps(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetCaps() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetBltStatus(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetBltStatus() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetAttachedSurface(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::GetAttachedSurface() - stub");
			return 0; // DD_OK
		}

		private uint Surface_Flip(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Flip() - stub");
			return 0; // DD_OK
		}

		private uint Surface_EnumOverlayZOrders(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::EnumOverlayZOrders() - stub");
			return 0; // DD_OK
		}

		private uint Surface_EnumAttachedSurfaces(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::EnumAttachedSurfaces() - stub");
			return 0; // DD_OK
		}

		private uint Surface_DeleteAttachedSurface(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::DeleteAttachedSurface() - stub");
			return 0; // DD_OK
		}

		private uint Surface_BltFast(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::BltFast() - stub");
			return 0; // DD_OK
		}

		private uint Surface_BltBatch(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::BltBatch() - stub");
			return 0; // DD_OK
		}

		private uint Surface_Blt(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Blt() - stub");
			return 0; // DD_OK
		}

		private uint Surface_AddOverlayDirtyRect(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::AddOverlayDirtyRect() - stub");
			return 0; // DD_OK
		}

		private uint Surface_AddAttachedSurface(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::AddAttachedSurface() - stub");
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
				obj.CooperativeLevel = dwFlags;
				obj.WindowHandle = (IntPtr)hWnd;
				
				// Initialize SDL3 backend if not already done
				if (obj.RenderingBackend == null)
				{
					obj.RenderingBackend = new Rendering.Sdl3RenderingBackend(_logger);
				}
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
				
				// Initialize SDL3 backend with the specified dimensions
				if (obj.RenderingBackend == null)
				{
					obj.RenderingBackend = new Rendering.Sdl3RenderingBackend(_logger);
				}
				
				// Initialize the SDL3 window with the specified dimensions
				string title = "Win32Emu DirectDraw";
				if (obj.RenderingBackend.IsInitialized)
				{
					// If already initialized, we would need to recreate with new dimensions
					// For now, we'll just log this situation
					_logger.LogInformation("[DDraw] Display mode changed to {Width}x{Height}x{Bpp}", dwWidth, dwHeight, dwBPP);
				}
				else
				{
					bool success = obj.RenderingBackend.Initialize((int)dwWidth, (int)dwHeight, title);
					if (!success)
					{
						_logger.LogError("[DDraw] Failed to initialize SDL3 backend");
						return 1; // DDERR_GENERIC
					}
				}
			}

			return 0; // DD_OK
		}

		private uint DDraw_WaitForVerticalBlank(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::WaitForVerticalBlank() - stub");
			return 0; // DD_OK
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
				return 1; // DDERR_GENERIC
			}
			
			if (surface.IsLocked)
			{
				_logger.LogWarning("[DDraw] Surface 0x{SurfaceHandle:X8} is already locked", surfaceHandle);
				return 0x8877000A; // DDERR_SURFACEBUSY
			}
			
			// Mark the surface as locked
			surface.IsLocked = true;
			
			// Allocate memory for the surface if not already done
			if (surface.Bits == null)
			{
				surface.Bits = new byte[surface.Pitch * surface.Height];
			}
			
			// Get a pointer to the surface memory
			uint surfaceMemPtr = _env.VirtualAlloc(0, (uint)(surface.Pitch * surface.Height), 0x1000, 0x04); // MEM_COMMIT, PAGE_READWRITE
			surface.LockedMemoryPtr = surfaceMemPtr;
			
			// Fill the surface description structure
			if (lpDDSurfaceDesc != 0)
			{
				uint dwSize = _env.MemRead32(lpDDSurfaceDesc);
				
				// Write the surface description
				_env.MemWrite32(lpDDSurfaceDesc + 4, 0x00001007); // DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PITCH | DDSD_PIXELFORMAT
				_env.MemWrite32(lpDDSurfaceDesc + 8, (uint)surface.Width);  // dwWidth
				_env.MemWrite32(lpDDSurfaceDesc + 12, (uint)surface.Height); // dwHeight
				_env.MemWrite32(lpDDSurfaceDesc + 16, (uint)surface.Pitch);  // lPitch
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
						return 1; // DDERR_GENERIC
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
			return 0; // DD_OK
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
				return 1; // DDERR_GENERIC
			}
			
			if (!surface.IsLocked)
			{
				_logger.LogWarning("[DDraw] Surface 0x{SurfaceHandle:X8} is not locked", surfaceHandle);
				return 0x88770010; // DDERR_NOTLOCKED
			}
			
			// Copy memory from the locked pointer to our surface bits
			if (surface.LockedMemoryPtr != 0 && surface.Bits != null)
			{
				byte[] data = _env.MemReadBytes(surface.LockedMemoryPtr, surface.Pitch * surface.Height);
				Array.Copy(data, surface.Bits, data.Length);
				
				// We don't actually free memory in this implementation
				// Just mark it as no longer locked
				surface.LockedMemoryPtr = 0;
			}
			
			// Mark the surface as unlocked
			surface.IsLocked = false;
			
			// If this is a primary surface, update the SDL3 texture
			if (surface.IsPrimary && _ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj) && ddrawObj.RenderingBackend != null)
			{
				try
				{
					// Update the SDL3 texture with the surface data
					ddrawObj.RenderingBackend.UpdateFrameBuffer(surface.Bits, surface.Pitch);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[DDraw] Failed to update SDL3 texture for primary surface");
				}
			}
			
			_logger.LogInformation("[DDraw] Unlocked surface 0x{SurfaceHandle:X8}", surfaceHandle);
			return 0; // DD_OK
		}
	}
}