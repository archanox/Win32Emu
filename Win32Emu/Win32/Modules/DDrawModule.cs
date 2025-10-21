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
		private readonly Dictionary<uint, uint> _comObjectToHandle = new(); // Maps COM object address to ddraw handle
		private readonly Dictionary<uint, DirectDrawSurface> _surfaces = new();
		private readonly Dictionary<uint, DirectDrawPalette> _palettes = new();
		private uint _nextDDrawHandle = 0x70000000;
		private uint _nextSurfaceHandle = 0x71000000;
		private uint _nextPaletteHandle = 0x72000000;

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
			
			// Store the COM object address in the DirectDraw object for reverse lookup
			ddrawObj.ComObjectAddress = comObjectAddr;
			_comObjectToHandle[comObjectAddr] = ddrawHandle;

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
			
			// Store the COM object address in the DirectDraw object for reverse lookup
			ddrawObj.ComObjectAddress = comObjectAddr;
			_comObjectToHandle[comObjectAddr] = ddrawHandle;

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
			public List<uint> AttachedSurfaces { get; set; } = new List<uint>();
		}

		private sealed class DirectDrawPalette
		{
			public uint Handle { get; set; }
			public uint ComObjectAddress { get; set; }
			public uint[] Entries { get; set; } = Array.Empty<uint>();
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
				return 1; // DDERR_GENERIC
			}

			if (lpdwCaps != 0)
			{
				// Determine caps based on number of entries
				uint caps = 0;
				if (palette.Entries.Length == 2) caps = 0x1; // DDPCAPS_1BIT
				else if (palette.Entries.Length == 4) caps = 0x2; // DDPCAPS_2BIT
				else if (palette.Entries.Length == 16) caps = 0x4; // DDPCAPS_4BIT
				else if (palette.Entries.Length == 256) caps = 0x8; // DDPCAPS_8BIT
				else caps = 0x8; // Default to 8-bit

				_env.MemWrite32(lpdwCaps, caps);
				_logger.LogInformation("[DDraw] Palette caps: 0x{Caps:X8} ({Count} entries)", caps, palette.Entries.Length);
			}

			return 0; // DD_OK
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
				return 1; // DDERR_GENERIC
			}

			if (lpEntries == 0)
			{
				_logger.LogError("[DDraw] GetEntries: lpEntries is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Check bounds
			if (dwBase >= palette.Entries.Length || dwBase + dwNumEntries > palette.Entries.Length)
			{
				_logger.LogError("[DDraw] GetEntries: invalid range (base={Base}, count={Count}, max={Max})", 
					dwBase, dwNumEntries, palette.Entries.Length);
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Write palette entries (PALETTEENTRY is 4 bytes: r,g,b,flags)
			for (var i = 0u; i < dwNumEntries; i++)
			{
				var entry = palette.Entries[dwBase + i];
				_env.MemWrite32(lpEntries + (i * 4), entry);
			}

			_logger.LogInformation("[DDraw] Retrieved {Count} palette entries starting at index {Base}", dwNumEntries, dwBase);
			return 0; // DD_OK
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
				return 1; // DDERR_GENERIC
			}

			if (lpEntries == 0)
			{
				_logger.LogError("[DDraw] SetEntries: lpEntries is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Check bounds
			if (dwStartingEntry >= palette.Entries.Length || dwStartingEntry + dwCount > palette.Entries.Length)
			{
				_logger.LogError("[DDraw] SetEntries: invalid range (start={Start}, count={Count}, max={Max})", 
					dwStartingEntry, dwCount, palette.Entries.Length);
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Read and update palette entries (PALETTEENTRY is 4 bytes: r,g,b,flags)
			for (var i = 0u; i < dwCount; i++)
			{
				var entry = _env.MemRead32(lpEntries + (i * 4));
				palette.Entries[dwStartingEntry + i] = entry;
			}

			_logger.LogInformation("[DDraw] Updated {Count} palette entries starting at index {Start}", dwCount, dwStartingEntry);
			return 0; // DD_OK
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
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpColorTable = args.UInt32(2);
			var lplpDDPalette = args.UInt32(3);
			var pUnkOuter = args.UInt32(4);

			_logger.LogInformation(
				"[DDraw COM] IDirectDraw::CreatePalette(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpColorTable=0x{LpColorTable:X8}, lplpDDPalette=0x{LplpDDPalette:X8}, pUnkOuter=0x{PUnkOuter:X8})",
				thisPtr, dwFlags, lpColorTable, lplpDDPalette, pUnkOuter);

			// Determine number of entries from dwFlags
			int numEntries;
			if ((dwFlags & 0x1) != 0) numEntries = 2; // DDPCAPS_1BIT
			else if ((dwFlags & 0x2) != 0)
				numEntries = 4; // DDPCAPS_2BIT
			else if ((dwFlags & 0x4) != 0)
				numEntries = 16; // DDPCAPS_4BIT
			else if ((dwFlags & 0x8) != 0)
				numEntries = 256; // DDPCAPS_8BIT
			else
				numEntries = 256; // Default

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

			var vtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (c, m) => ComQueryInterface(c, m) },
				{ "AddRef", (c, m) => ComAddRef(c, m) },
				{ "Release", (c, m) => ComRelease(c, m) },
				{ "GetCaps", (c, m) => Palette_GetCaps(c, m) },
				{ "GetEntries", (c, m) => Palette_GetEntries(c, m, paletteHandle) },
				{ "Initialize", (c, m) => Palette_Initialize(c, m) },
				{ "SetEntries", (c, m) => Palette_SetEntries(c, m, paletteHandle) }
			};

			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectDrawPalette", vtableMethods);
			palette.ComObjectAddress = comObjectAddr;

			if (lplpDDPalette != 0)
			{
				_env.MemWrite32(lplpDDPalette, comObjectAddr);
			}

			_logger.LogInformation("[DDraw] Created IDirectDrawPalette COM object at 0x{ComObjectAddr:X8} for palette 0x{PaletteHandle:X8}",
				comObjectAddr, paletteHandle);

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
			var dwSize = _env.MemRead32(lpDDSurfaceDesc);
			var dwFlags = _env.MemRead32(lpDDSurfaceDesc + 4);
			var dwWidth = _env.MemRead32(lpDDSurfaceDesc + 8);
			var dwHeight = _env.MemRead32(lpDDSurfaceDesc + 12);
			
			// Read backbuffer count if DDSD_BACKBUFFERCOUNT flag is set
			var dwBackBufferCount = 0u;
			if ((dwFlags & 0x00000020) != 0) // DDSD_BACKBUFFERCOUNT
			{
				dwBackBufferCount = _env.MemRead32(lpDDSurfaceDesc + 20);
			}
			
			// Read surface capabilities from offset 108
			var dwSurfaceCaps = 0u;
			if (dwSize >= 112)
			{
				dwSurfaceCaps = _env.MemRead32(lpDDSurfaceDesc + 108);
			}
			
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
				IsPrimary = (dwSurfaceCaps & 0x00000200) != 0, // DDSCAPS_PRIMARYSURFACE
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
				{ "SetPalette", (cpu, mem) => Surface_SetPalette(cpu, mem, surfaceHandle) },
				{ "Unlock", (cpu, mem) => Surface_Unlock(cpu, mem, surfaceHandle) },
				{ "UpdateOverlay", (cpu, mem) => Surface_UpdateOverlay(cpu, mem) },
				{ "UpdateOverlayDisplay", (cpu, mem) => Surface_UpdateOverlayDisplay(cpu, mem) },
				{ "UpdateOverlayZOrder", (cpu, mem) => Surface_UpdateOverlayZOrder(cpu, mem) }
			};
			
			// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectDrawSurface", vtableMethods);
			surface.ComObjectAddress = comObjectAddr;
			
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
						Width = (int)dwWidth,
						Height = (int)dwHeight,
						DirectDrawHandle = ddrawHandle,
						IsPrimary = false,
						Pitch = (int)dwWidth * (ddrawObj.BitsPerPixel / 8)
					};
					
					// Allocate memory for the backbuffer
					backBuffer.Bits = new byte[backBuffer.Pitch * backBuffer.Height];
					
					// Store the backbuffer
					_surfaces[backBufferHandle] = backBuffer;
					
					// Create COM vtable for backbuffer
					var backBufferVtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
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
						{ "Lock", (cpu, mem) => Surface_Lock(cpu, mem, backBufferHandle) },
						{ "ReleaseDC", (cpu, mem) => Surface_ReleaseDC(cpu, mem) },
						{ "Restore", (cpu, mem) => Surface_Restore(cpu, mem) },
						{ "SetClipper", (cpu, mem) => Surface_SetClipper(cpu, mem) },
						{ "SetColorKey", (cpu, mem) => Surface_SetColorKey(cpu, mem) },
						{ "SetOverlayPosition", (cpu, mem) => Surface_SetOverlayPosition(cpu, mem) },
						{ "SetPalette", (cpu, mem) => Surface_SetPalette(cpu, mem, backBufferHandle) },
						{ "Unlock", (cpu, mem) => Surface_Unlock(cpu, mem, backBufferHandle) },
						{ "UpdateOverlay", (cpu, mem) => Surface_UpdateOverlay(cpu, mem) },
						{ "UpdateOverlayDisplay", (cpu, mem) => Surface_UpdateOverlayDisplay(cpu, mem) },
						{ "UpdateOverlayZOrder", (cpu, mem) => Surface_UpdateOverlayZOrder(cpu, mem) }
					};
					
					var backBufferComAddr = _env.ComDispatcher.CreateComObject("IDirectDrawSurface", backBufferVtableMethods);
					backBuffer.ComObjectAddress = backBufferComAddr;
					
					// Attach the backbuffer to the primary surface
					surface.AttachedSurfaces.Add(backBufferHandle);
					
					_logger.LogInformation("[DDraw] Created backbuffer {Index} at surface handle 0x{Handle:X8}, COM object at 0x{ComAddr:X8}",
						i + 1, backBufferHandle, backBufferComAddr);
				}
			}
			
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

		private uint Surface_SetPalette(ICpu cpu, VirtualMemory mem, uint surfaceHandle)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDPalette = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::SetPalette(this=0x{ThisPtr:X8}, lpDDPalette=0x{LpDDPalette:X8})", thisPtr,
				lpDDPalette);

			if (!_surfaces.TryGetValue(surfaceHandle, out var surface))
			{
				_logger.LogError("[DDraw] SetPalette: could not find surface with handle 0x{SurfaceHandle:X8}", surfaceHandle);
				return 1; // DDERR_GENERIC
			}

			if (lpDDPalette == 0)
			{
				surface.PaletteHandle = 0;
				_logger.LogInformation("[DDraw] Detached palette from surface 0x{SurfaceHandle:X8}", surfaceHandle);
				return 0; // DD_OK
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
				return 0x887601E6; // DDERR_INVALIDOBJECT
			}

			surface.PaletteHandle = paletteHandle;
			_logger.LogInformation("[DDraw] Surface 0x{SurfaceHandle:X8} palette set to 0x{PaletteHandle:X8}", surfaceHandle, paletteHandle);

			return 0; // DD_OK
		}

		private uint Surface_SetOverlayPosition(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::SetOverlayPosition() - stub");
			return 0; // DD_OK
		}

		private uint Surface_SetColorKey(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpDDColorKey = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::SetColorKey(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpDDColorKey=0x{ColorKey:X8})", 
				thisPtr, dwFlags, lpDDColorKey);

			// Find the surface
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				// For now, find any surface
				// In a complete implementation, we'd match by COM object address
				surface = s;
				break;
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] SetColorKey: could not find surface");
				return 1; // DDERR_GENERIC
			}

			if (lpDDColorKey != 0)
			{
				// Read DDCOLORKEY structure
				var colorKeyLow = _env.MemRead32(lpDDColorKey);
				var colorKeyHigh = _env.MemRead32(lpDDColorKey + 4);

				surface.ColorKeyLow = colorKeyLow;
				surface.ColorKeyHigh = colorKeyHigh;
				surface.HasColorKey = true;

				_logger.LogInformation("[DDraw] Set color key: low=0x{Low:X8}, high=0x{High:X8}", colorKeyLow, colorKeyHigh);
			}
			else
			{
				// Clear color key
				surface.HasColorKey = false;
				_logger.LogInformation("[DDraw] Cleared color key");
			}

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
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var hDC = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::ReleaseDC(this=0x{ThisPtr:X8}, hDC=0x{HDC:X8})", 
				thisPtr, hDC);

			// In a real implementation, this would release the GDI DC
			// For now, just acknowledge the release
			return 0; // DD_OK
		}

		private uint Surface_IsLost(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::IsLost(this=0x{ThisPtr:X8})", thisPtr);
			// Our surfaces are never lost in the emulator
			return 0; // DD_OK
		}

		private uint Surface_Initialize(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::Initialize() - stub");
			return 0; // DD_OK
		}

		private uint Surface_GetSurfaceDesc(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceDesc = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetSurfaceDesc(this=0x{ThisPtr:X8}, lpDDSurfaceDesc=0x{SurfaceDesc:X8})", 
				thisPtr, lpDDSurfaceDesc);

			// Find the surface
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				// For now, use the first surface
				surface = s;
				break;
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetSurfaceDesc: could not find surface");
				return 1; // DDERR_GENERIC
			}

			if (lpDDSurfaceDesc != 0)
			{
				// Find the DirectDraw object to get BPP
				DirectDrawObject? ddrawObj = null;
				if (_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out ddrawObj))
				{
					var dwSize = _env.MemRead32(lpDDSurfaceDesc);

					// Fill DDSURFACEDESC structure
					_env.MemWrite32(lpDDSurfaceDesc + 4, 0x0000100F); // dwFlags: DDSD_WIDTH | DDSD_HEIGHT | DDSD_PITCH | DDSD_PIXELFORMAT
					_env.MemWrite32(lpDDSurfaceDesc + 8, (uint)surface.Width); // dwWidth
					_env.MemWrite32(lpDDSurfaceDesc + 12, (uint)surface.Height); // dwHeight
					_env.MemWrite32(lpDDSurfaceDesc + 16, (uint)surface.Pitch); // lPitch

					// Write pixel format (offset 76)
					if (dwSize >= 108)
					{
						_env.MemWrite32(lpDDSurfaceDesc + 76, 32); // dwSize of DDPIXELFORMAT
						_env.MemWrite32(lpDDSurfaceDesc + 80, 0x00000040); // dwFlags: DDPF_RGB
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
				}
			}

			return 0; // DD_OK
		}

		private uint Surface_GetPixelFormat(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDPixelFormat = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetPixelFormat(this=0x{ThisPtr:X8}, lpDDPixelFormat=0x{PixelFormat:X8})", 
				thisPtr, lpDDPixelFormat);

			// Find the surface
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				surface = s;
				break;
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetPixelFormat: could not find surface");
				return 1; // DDERR_GENERIC
			}

			if (lpDDPixelFormat != 0)
			{
				// Find the DirectDraw object to get BPP
				if (_ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj))
				{
					// Fill DDPIXELFORMAT structure
					_env.MemWrite32(lpDDPixelFormat, 32); // dwSize
					_env.MemWrite32(lpDDPixelFormat + 4, 0x00000040); // dwFlags: DDPF_RGB
					_env.MemWrite32(lpDDPixelFormat + 8, 0); // dwFourCC
					_env.MemWrite32(lpDDPixelFormat + 12, (uint)ddrawObj.BitsPerPixel); // dwRGBBitCount

					// Set RGB masks based on bit depth
					if (ddrawObj.BitsPerPixel == 8)
					{
						// Palettized mode
						_env.MemWrite32(lpDDPixelFormat + 4, 0x00000020); // DDPF_PALETTEINDEXED8
						_env.MemWrite32(lpDDPixelFormat + 16, 0);
						_env.MemWrite32(lpDDPixelFormat + 20, 0);
						_env.MemWrite32(lpDDPixelFormat + 24, 0);
					}
					else if (ddrawObj.BitsPerPixel == 16)
					{
						_env.MemWrite32(lpDDPixelFormat + 16, 0xF800); // Red mask (5 bits)
						_env.MemWrite32(lpDDPixelFormat + 20, 0x07E0); // Green mask (6 bits)
						_env.MemWrite32(lpDDPixelFormat + 24, 0x001F); // Blue mask (5 bits)
					}
					else if (ddrawObj.BitsPerPixel == 24 || ddrawObj.BitsPerPixel == 32)
					{
						_env.MemWrite32(lpDDPixelFormat + 16, 0x00FF0000); // Red mask
						_env.MemWrite32(lpDDPixelFormat + 20, 0x0000FF00); // Green mask
						_env.MemWrite32(lpDDPixelFormat + 24, 0x000000FF); // Blue mask
					}

					_env.MemWrite32(lpDDPixelFormat + 28, 0); // dwRGBAlphaBitMask
				}
			}

			return 0; // DD_OK
		}

		private uint Surface_GetPalette(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lplpDDPalette = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetPalette(this=0x{ThisPtr:X8}, lplpDDPalette=0x{LplpDDPalette:X8})", 
				thisPtr, lplpDDPalette);

			// Find the surface
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				// For now, find any surface - in a complete implementation we'd match by COM object address
				surface = s;
				break;
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetPalette: could not find surface");
				return 1; // DDERR_GENERIC
			}

			if (lplpDDPalette == 0)
			{
				_logger.LogError("[DDraw] GetPalette: lplpDDPalette is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Check if surface has a palette attached
			if (surface.PaletteHandle == 0)
			{
				_env.MemWrite32(lplpDDPalette, 0);
				_logger.LogInformation("[DDraw] Surface has no palette attached");
				return 0x88760165; // DDERR_NOPALETTEATTACHED
			}

			// Find the palette and return its COM object address
			if (_palettes.TryGetValue(surface.PaletteHandle, out var palette))
			{
				_env.MemWrite32(lplpDDPalette, palette.ComObjectAddress);
				_logger.LogInformation("[DDraw] Returning palette COM object at 0x{ComObjectAddr:X8}", palette.ComObjectAddress);
				return 0; // DD_OK
			}

			_logger.LogError("[DDraw] GetPalette: palette handle 0x{PaletteHandle:X8} not found", surface.PaletteHandle);
			return 1; // DDERR_GENERIC
		}

		private uint Surface_GetOverlayPosition(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lplX = args.UInt32(1);
			var lplY = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetOverlayPosition(this=0x{ThisPtr:X8}, lplX=0x{LplX:X8}, lplY=0x{LplY:X8})", 
				thisPtr, lplX, lplY);

			// Overlays are not supported in this implementation
			// Return error indicating this is not an overlay surface
			return 0x88760177; // DDERR_NOTAOVERLAYSURFACE
		}

		private uint Surface_GetFlipStatus(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetFlipStatus(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8})", 
				thisPtr, dwFlags);

			// In an emulator, flips complete instantly
			// Always return DD_OK to indicate no flips are pending
			return 0; // DD_OK
		}

		private uint Surface_GetDC(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lphDC = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetDC(this=0x{ThisPtr:X8}, lphDC=0x{LphDC:X8})", 
				thisPtr, lphDC);

			if (lphDC == 0)
			{
				_logger.LogError("[DDraw] GetDC: lphDC is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Create a fake device context handle
			// In a real implementation, this would create an actual GDI DC
			// For now, we return a non-zero handle to indicate success
			var fakeDC = 0x12340000u;
			_env.MemWrite32(lphDC, fakeDC);

			_logger.LogInformation("[DDraw] Returning fake DC handle: 0x{FakeDC:X8}", fakeDC);
			return 0; // DD_OK
		}

		private uint Surface_GetColorKey(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);
			var lpDDColorKey = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetColorKey(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8}, lpDDColorKey=0x{ColorKey:X8})", 
				thisPtr, dwFlags, lpDDColorKey);

			// Find the surface
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				// For now, find any surface - in a complete implementation we'd match by COM object address
				surface = s;
				break;
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetColorKey: could not find surface");
				return 1; // DDERR_GENERIC
			}

			if (lpDDColorKey == 0)
			{
				_logger.LogError("[DDraw] GetColorKey: lpDDColorKey is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Check if surface has a color key
			if (!surface.HasColorKey)
			{
				_logger.LogInformation("[DDraw] Surface has no color key set");
				return 0x88760168; // DDERR_NOCOLORKEY
			}

			// Write DDCOLORKEY structure (2 DWORDs: dwColorSpaceLowValue and dwColorSpaceHighValue)
			_env.MemWrite32(lpDDColorKey, surface.ColorKeyLow);
			_env.MemWrite32(lpDDColorKey + 4, surface.ColorKeyHigh);

			_logger.LogInformation("[DDraw] Returning color key: low=0x{Low:X8}, high=0x{High:X8}", 
				surface.ColorKeyLow, surface.ColorKeyHigh);

			return 0; // DD_OK
		}

		private uint Surface_GetClipper(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lplpDDClipper = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetClipper(this=0x{ThisPtr:X8}, lplpDDClipper=0x{LplpDDClipper:X8})", 
				thisPtr, lplpDDClipper);

			if (lplpDDClipper == 0)
			{
				_logger.LogError("[DDraw] GetClipper: lplpDDClipper is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// We don't support clippers in this implementation
			// Return null to indicate no clipper is attached
			_env.MemWrite32(lplpDDClipper, 0);
			_logger.LogInformation("[DDraw] No clipper attached to surface");

			return 0x88760169; // DDERR_NOCLIPPERATTACHED
		}

		private uint Surface_GetCaps(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSCaps = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetCaps(this=0x{ThisPtr:X8}, lpDDSCaps=0x{Caps:X8})", 
				thisPtr, lpDDSCaps);

			// Find the surface
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				surface = s;
				break;
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetCaps: could not find surface");
				return 1; // DDERR_GENERIC
			}

			if (lpDDSCaps != 0)
			{
				// Fill DDSCAPS structure
				uint caps = 0;
				if (surface.IsPrimary)
				{
					caps |= 0x00000200; // DDSCAPS_PRIMARYSURFACE
				}
				caps |= 0x00000800; // DDSCAPS_VIDEOMEMORY

				_env.MemWrite32(lpDDSCaps, caps);
			}

			return 0; // DD_OK
		}

		private uint Surface_GetBltStatus(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::GetBltStatus(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8})", 
				thisPtr, dwFlags);

			// In an emulator, blits complete instantly
			// Always return DD_OK to indicate no blits are pending
			return 0; // DD_OK
		}

		private uint Surface_GetAttachedSurface(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSCaps = args.UInt32(1);
			var lplpDDAttachedSurface = args.UInt32(2);

			_logger.LogInformation(
				"[DDraw COM] IDirectDrawSurface::GetAttachedSurface(this=0x{ThisPtr:X8}, lpDDSCaps=0x{LpDDSCaps:X8}, lplp=0x{Lplp:X8})", thisPtr,
				lpDDSCaps, lplpDDAttachedSurface);

			// Find the surface by COM object address
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				if (s.ComObjectAddress == thisPtr)
				{
					surface = s;
					break;
				}
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] GetAttachedSurface: could not find surface with COM address 0x{ThisPtr:X8}", thisPtr);
				if (lplpDDAttachedSurface != 0)
				{
					_env.MemWrite32(lplpDDAttachedSurface, 0);
				}
				return 0x887601C2; // DDERR_NOTFOUND
			}

			// Read the requested capabilities
			var dwCaps = lpDDSCaps != 0 ? _env.MemRead32(lpDDSCaps) : 0;
			_logger.LogInformation("[DDraw] Requested surface caps: 0x{Caps:X8}", dwCaps);

			// Check if there are any attached surfaces
			if (surface.AttachedSurfaces.Count == 0)
			{
				_logger.LogInformation("[DDraw] No attached surfaces found for surface 0x{Handle:X8}", surface.Handle);
				if (lplpDDAttachedSurface != 0)
				{
					_env.MemWrite32(lplpDDAttachedSurface, 0);
				}
				return 0x887601C2; // DDERR_NOTFOUND
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
				return 0; // DD_OK
			}

			_logger.LogError("[DDraw] GetAttachedSurface: attached surface handle 0x{Handle:X8} not found", attachedSurfaceHandle);
			if (lplpDDAttachedSurface != 0)
			{
				_env.MemWrite32(lplpDDAttachedSurface, 0);
			}
			return 0x887601C2; // DDERR_NOTFOUND
		}

		private uint Surface_Flip(ICpu cpu, VirtualMemory mem)
		{
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceTargetOverride = args.UInt32(1);
			var dwFlags = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::Flip(this=0x{ThisPtr:X8}, lpDDSurfaceTargetOverride=0x{Target:X8}, dwFlags=0x{DwFlags:X8})", 
				thisPtr, lpDDSurfaceTargetOverride, dwFlags);

			// Find the surface from the COM object
			DirectDrawSurface? surface = null;
			foreach (var s in _surfaces.Values)
			{
				// We need to match surfaces by their COM object address
				// For now, just use the first primary surface
				if (s.IsPrimary)
				{
					surface = s;
					break;
				}
			}

			if (surface == null)
			{
				_logger.LogError("[DDraw] Flip: could not find primary surface");
				return 1; // DDERR_GENERIC
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
					return 1; // DDERR_GENERIC
				}
			}

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
			var args = new StackArgs(cpu, mem);
			var thisPtr = args.UInt32(0);
			var dwX = args.UInt32(1);
			var dwY = args.UInt32(2);
			var lpDDSrcSurface = args.UInt32(3);
			var lpSrcRect = args.UInt32(4);
			var dwTrans = args.UInt32(5);

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::BltFast(this=0x{ThisPtr:X8}, x={X}, y={Y}, lpDDSrcSurface=0x{SrcSurface:X8}, lpSrcRect=0x{SrcRect:X8}, dwTrans=0x{Trans:X8})", 
				thisPtr, dwX, dwY, lpDDSrcSurface, lpSrcRect, dwTrans);

			// Find destination surface
			DirectDrawSurface? destSurface = null;
			foreach (var s in _surfaces.Values)
			{
				if (s.IsPrimary)
				{
					destSurface = s;
					break;
				}
			}

			if (destSurface == null || destSurface.Bits == null)
			{
				_logger.LogError("[DDraw] BltFast: could not find destination surface");
				return 1; // DDERR_GENERIC
			}

			// Find source surface
			DirectDrawSurface? srcSurface = null;
			if (lpDDSrcSurface != 0)
			{
				if (_surfaces.TryGetValue(lpDDSrcSurface, out var s))
				{
					srcSurface = s;
				}
			}

			if (srcSurface == null || srcSurface.Bits == null)
			{
				_logger.LogError("[DDraw] BltFast: could not find source surface");
				return 1; // DDERR_GENERIC
			}

			// Read source rectangle if provided
			int srcX = 0, srcY = 0, srcWidth = srcSurface.Width, srcHeight = srcSurface.Height;
			if (lpSrcRect != 0)
			{
				srcX = (int)_env.MemRead32(lpSrcRect);
				srcY = (int)_env.MemRead32(lpSrcRect + 4);
				srcWidth = (int)_env.MemRead32(lpSrcRect + 8) - srcX;
				srcHeight = (int)_env.MemRead32(lpSrcRect + 12) - srcY;
			}

			// Get bits per pixel from DirectDraw object
			if (!_ddrawObjects.TryGetValue(destSurface.DirectDrawHandle, out var ddrawObj))
			{
				_logger.LogError("[DDraw] BltFast: could not find DirectDraw object");
				return 1; // DDERR_GENERIC
			}
			var bytesPerPixel = ddrawObj.BitsPerPixel / 8;

			// Perform fast blit
			var destX = (int)dwX;
			var destY = (int)dwY;

			for (var y = 0; y < srcHeight && (destY + y) < destSurface.Height && (srcY + y) < srcSurface.Height; y++)
			{
				for (var x = 0; x < srcWidth && (destX + x) < destSurface.Width && (srcX + x) < srcSurface.Width; x++)
				{
					var destOffset = (destY + y) * destSurface.Pitch + (destX + x) * bytesPerPixel;
					var srcOffset = (srcY + y) * srcSurface.Pitch + (srcX + x) * bytesPerPixel;

					if (destOffset + 1 < destSurface.Bits.Length && srcOffset + 1 < srcSurface.Bits.Length)
					{
						// DDBLTFAST_SRCCOLORKEY = 0x00000001
						if ((dwTrans & 0x00000001) != 0 && srcSurface.HasColorKey)
						{
							// Check for color key transparency
							var srcPixel = (ushort)(srcSurface.Bits[srcOffset] | (srcSurface.Bits[srcOffset + 1] << 8));
							// Check if pixel is within color key range (transparent if it matches)
							if (srcPixel < srcSurface.ColorKeyLow || srcPixel > srcSurface.ColorKeyHigh)
							{
								destSurface.Bits[destOffset] = srcSurface.Bits[srcOffset];
								destSurface.Bits[destOffset + 1] = srcSurface.Bits[srcOffset + 1];
							}
						}
						else
						{
							// No transparency
							destSurface.Bits[destOffset] = srcSurface.Bits[srcOffset];
							destSurface.Bits[destOffset + 1] = srcSurface.Bits[srcOffset + 1];
						}
					}
				}
			}

			return 0; // DD_OK
		}

		private uint Surface_BltBatch(ICpu cpu, VirtualMemory mem)
		{
			_logger.LogInformation("[DDraw COM] IDirectDraw::BltBatch() - stub");
			return 0; // DD_OK
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

			_logger.LogInformation("[DDraw COM] IDirectDrawSurface::Blt(this=0x{ThisPtr:X8}, lpDestRect=0x{DestRect:X8}, lpDDSrcSurface=0x{SrcSurface:X8}, lpSrcRect=0x{SrcRect:X8}, dwFlags=0x{DwFlags:X8}, lpDDBltFx=0x{BltFx:X8})", 
				thisPtr, lpDestRect, lpDDSrcSurface, lpSrcRect, dwFlags, lpDDBltFx);

			// Find destination surface
			DirectDrawSurface? destSurface = null;
			foreach (var s in _surfaces.Values)
			{
				// For now, use the first primary surface as destination
				if (s.IsPrimary)
				{
					destSurface = s;
					break;
				}
			}

			if (destSurface == null || destSurface.Bits == null)
			{
				_logger.LogError("[DDraw] Blt: could not find destination surface");
				return 1; // DDERR_GENERIC
			}

			// Read destination rectangle if provided
			int destX = 0, destY = 0, destWidth = destSurface.Width, destHeight = destSurface.Height;
			if (lpDestRect != 0)
			{
				destX = (int)_env.MemRead32(lpDestRect);
				destY = (int)_env.MemRead32(lpDestRect + 4);
				destWidth = (int)_env.MemRead32(lpDestRect + 8) - destX;
				destHeight = (int)_env.MemRead32(lpDestRect + 12) - destY;
			}

			// Check for color fill operation (DDBLT_COLORFILL = 0x00000400)
			if ((dwFlags & 0x00000400) != 0 && lpDDBltFx != 0)
			{
				// Read fill color from DDBLTFX structure
				var fillColor = _env.MemRead32(lpDDBltFx + 16); // dwFillColor offset

				// Get bits per pixel from DirectDraw object
				if (!_ddrawObjects.TryGetValue(destSurface.DirectDrawHandle, out var ddrawObj))
				{
					_logger.LogError("[DDraw] Blt: could not find DirectDraw object for color fill");
					return 1; // DDERR_GENERIC
				}

				// Perform color fill
				// Determine bytes per pixel based on bit depth
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

				_logger.LogInformation("[DDraw] Performed color fill with color 0x{FillColor:X8}", fillColor);
				return 0; // DD_OK
			}

			// Handle source surface blit
			if (lpDDSrcSurface != 0)
			{
				// Find source surface by COM object address
				DirectDrawSurface? srcSurface = null;
				foreach (var s in _surfaces.Values)
				{
					// Would need to match COM object address
					// For now, this is a simplified implementation
					if (!s.IsPrimary)
					{
						srcSurface = s;
						break;
					}
				}

				if (srcSurface != null && srcSurface.Bits != null)
				{
					// Read source rectangle if provided
					int srcX = 0, srcY = 0, srcWidth = srcSurface.Width, srcHeight = srcSurface.Height;
					if (lpSrcRect != 0)
					{
						srcX = (int)_env.MemRead32(lpSrcRect);
						srcY = (int)_env.MemRead32(lpSrcRect + 4);
						srcWidth = (int)_env.MemRead32(lpSrcRect + 8) - srcX;
						srcHeight = (int)_env.MemRead32(lpSrcRect + 12) - srcY;
					}

					// Perform simple blit (copy pixels)
					for (var y = 0; y < srcHeight && (destY + y) < destSurface.Height && (srcY + y) < srcSurface.Height; y++)
					{
						for (var x = 0; x < srcWidth && (destX + x) < destSurface.Width && (srcX + x) < srcSurface.Width; x++)
						{
							var destOffset = (destY + y) * destSurface.Pitch + (destX + x) * 2;
							var srcOffset = (srcY + y) * srcSurface.Pitch + (srcX + x) * 2;

							if (destOffset + 1 < destSurface.Bits.Length && srcOffset + 1 < srcSurface.Bits.Length)
							{
								destSurface.Bits[destOffset] = srcSurface.Bits[srcOffset];
								destSurface.Bits[destOffset + 1] = srcSurface.Bits[srcOffset + 1];
							}
						}
					}

					_logger.LogInformation("[DDraw] Performed blit from source surface");
				}
			}

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
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpDDDriverCaps = args.UInt32(1);
			var lpDDHELCaps = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetCaps(this=0x{ThisPtr:X8}, lpDDDriverCaps=0x{DriverCaps:X8}, lpDDHELCaps=0x{HELCaps:X8})", 
				thisPtr, lpDDDriverCaps, lpDDHELCaps);

			// Fill in basic capabilities
			if (lpDDDriverCaps != 0)
			{
				var dwSize = _env.MemRead32(lpDDDriverCaps);
				
				// DDCAPS structure - simplified
				_env.MemWrite32(lpDDDriverCaps + 4, 0x00000001); // dwCaps: DDCAPS_BLT
				_env.MemWrite32(lpDDDriverCaps + 8, 0x00000040); // dwCaps2: DDCAPS2_CANRENDERWINDOWED
				_env.MemWrite32(lpDDDriverCaps + 12, 0); // dwCKeyCaps
				_env.MemWrite32(lpDDDriverCaps + 16, 0); // dwFXCaps
				_env.MemWrite32(lpDDDriverCaps + 20, 0); // dwFXAlphaCaps
				_env.MemWrite32(lpDDDriverCaps + 24, 0); // dwPalCaps
				_env.MemWrite32(lpDDDriverCaps + 28, 0x00000001); // dwSVCaps: DDSVCAPS_RESERVED1
				_env.MemWrite32(lpDDDriverCaps + 32, 0); // dwAlphaBltConstBitDepths
				_env.MemWrite32(lpDDDriverCaps + 36, 0); // dwAlphaBltPixelBitDepths
				_env.MemWrite32(lpDDDriverCaps + 40, 0); // dwAlphaBltSurfaceBitDepths
			}

			if (lpDDHELCaps != 0)
			{
				// HEL (Hardware Emulation Layer) caps - can be left empty for now
				var dwSize = _env.MemRead32(lpDDHELCaps);
				_env.MemWrite32(lpDDHELCaps + 4, 0);
			}

			return 0; // DD_OK
		}

		private uint DDraw_GetDisplayMode(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpDDSurfaceDesc = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetDisplayMode(this=0x{ThisPtr:X8}, lpDDSurfaceDesc=0x{SurfaceDesc:X8})", 
				thisPtr, lpDDSurfaceDesc);

			// Find the DirectDraw object
			DirectDrawObject? ddrawObj = null;
			foreach (var obj in _ddrawObjects.Values)
			{
				ddrawObj = obj;
				break;
			}

			if (ddrawObj == null)
			{
				_logger.LogError("[DDraw] GetDisplayMode: could not find DirectDraw object");
				return 1; // DDERR_GENERIC
			}

			if (lpDDSurfaceDesc != 0)
			{
				// Fill DDSURFACEDESC structure
				var dwSize = _env.MemRead32(lpDDSurfaceDesc);

				_env.MemWrite32(lpDDSurfaceDesc + 4, 0x0000100F); // dwFlags: DDSD_WIDTH | DDSD_HEIGHT | DDSD_PITCH | DDSD_PIXELFORMAT
				_env.MemWrite32(lpDDSurfaceDesc + 8, (uint)ddrawObj.Width); // dwWidth
				_env.MemWrite32(lpDDSurfaceDesc + 12, (uint)ddrawObj.Height); // dwHeight
				_env.MemWrite32(lpDDSurfaceDesc + 16, (uint)(ddrawObj.Width * (ddrawObj.BitsPerPixel / 8))); // lPitch

				// Write pixel format (offset 76)
				if (dwSize >= 108)
				{
					_env.MemWrite32(lpDDSurfaceDesc + 76, 32); // dwSize of DDPIXELFORMAT
					_env.MemWrite32(lpDDSurfaceDesc + 80, 0x00000040); // dwFlags: DDPF_RGB
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
			}

			return 0; // DD_OK
		}

		private uint DDraw_GetFourCCCodes(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpNumCodes = args.UInt32(1);
			var lpCodes = args.UInt32(2);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetFourCCCodes(this=0x{ThisPtr:X8}, lpNumCodes=0x{LpNumCodes:X8}, lpCodes=0x{LpCodes:X8})", 
				thisPtr, lpNumCodes, lpCodes);

			if (lpNumCodes == 0)
			{
				_logger.LogError("[DDraw] GetFourCCCodes: lpNumCodes is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// For now, we don't support any hardware FourCC codes
			// Return 0 to indicate no additional formats are supported
			_env.MemWrite32(lpNumCodes, 0);

			_logger.LogInformation("[DDraw] Returning 0 FourCC codes (no additional formats supported)");
			return 0; // DD_OK
		}

		private uint DDraw_GetGDISurface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lplpGDIDDSSurface = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetGDISurface(this=0x{ThisPtr:X8}, lplpGDIDDSSurface=0x{LplpGDIDDSSurface:X8})", 
				thisPtr, lplpGDIDDSSurface);

			if (lplpGDIDDSSurface == 0)
			{
				_logger.LogError("[DDraw] GetGDISurface: lplpGDIDDSSurface is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Find the primary surface (which would be the GDI surface)
			DirectDrawSurface? primarySurface = null;
			foreach (var s in _surfaces.Values)
			{
				if (s.IsPrimary)
				{
					primarySurface = s;
					break;
				}
			}

			if (primarySurface == null)
			{
				_env.MemWrite32(lplpGDIDDSSurface, 0);
				_logger.LogInformation("[DDraw] No GDI surface found");
				return 0x887601C2; // DDERR_NOTFOUND
			}

			// Return the COM object address of the primary surface
			// Note: In a real implementation, we'd need to track the COM object addresses for surfaces
			// For now, we'll return 0 to indicate no GDI surface
			_env.MemWrite32(lplpGDIDDSSurface, 0);
			_logger.LogInformation("[DDraw] GDI surface tracking not fully implemented");

			return 0x887601C2; // DDERR_NOTFOUND
		}

		private uint DDraw_GetMonitorFrequency(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpdwFrequency = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetMonitorFrequency(this=0x{ThisPtr:X8}, lpdwFrequency=0x{Frequency:X8})", 
				thisPtr, lpdwFrequency);

			if (lpdwFrequency != 0)
			{
				// Return typical 60Hz refresh rate
				_env.MemWrite32(lpdwFrequency, 60);
			}

			return 0; // DD_OK
		}

		private uint DDraw_GetScanLine(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpdwScanLine = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetScanLine(this=0x{ThisPtr:X8}, lpdwScanLine=0x{LpdwScanLine:X8})", 
				thisPtr, lpdwScanLine);

			if (lpdwScanLine == 0)
			{
				_logger.LogError("[DDraw] GetScanLine: lpdwScanLine is null");
				return 0x80070057; // DDERR_INVALIDPARAMS
			}

			// Find the DirectDraw object to get display height
			DirectDrawObject? ddrawObj = null;
			foreach (var obj in _ddrawObjects.Values)
			{
				ddrawObj = obj;
				break;
			}

			if (ddrawObj == null)
			{
				_logger.LogError("[DDraw] GetScanLine: could not find DirectDraw object");
				return 1; // DDERR_GENERIC
			}

			// Simulate scan line position based on current time
			// In a real implementation, this would query the actual hardware
			// We'll cycle through all scan lines at approximately 60Hz refresh rate
			var totalScanLines = (uint)(ddrawObj.Height + 40); // Add vertical blanking lines
			var scanLine = (uint)((DateTime.UtcNow.Ticks / 10000) % totalScanLines);

			_env.MemWrite32(lpdwScanLine, scanLine);
			_logger.LogInformation("[DDraw] Returning scan line: {ScanLine} (of {Total})", scanLine, totalScanLines);

			return 0; // DD_OK
		}

		private uint DDraw_GetVerticalBlankStatus(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpbIsInVB = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::GetVerticalBlankStatus(this=0x{ThisPtr:X8}, lpbIsInVB=0x{IsInVB:X8})", 
				thisPtr, lpbIsInVB);

			if (lpbIsInVB != 0)
			{
				// Simulate being in vertical blank 1/60th of the time
				var isInVBlank = (DateTime.UtcNow.Ticks / 10000) % 17 == 0; // Approximately 1/60th
				_env.MemWrite32(lpbIsInVB, isInVBlank ? 1u : 0u);
			}

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
					obj.RenderingBackend = Rendering.BackendFactory.CreateRenderingBackend(_logger);
				}
				
				// Subscribe to UI events from the rendering backend
				// ProcessEnvironment now tracks subscriptions and prevents duplicates automatically
				if (obj.RenderingBackend != null)
				{
					_env.SubscribeToUIEvents(obj.RenderingBackend, null);
					_logger.LogInformation("[DDraw] Subscribed to UI events from rendering backend");
				}
			}
			else
			{
				_logger.LogError("[DDraw] SetCooperativeLevel: Could not find DirectDraw object with handle 0x{Handle:X8}", actualHandle);
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
				
				// Initialize rendering backend with the specified dimensions
				if (obj.RenderingBackend == null)
				{
					obj.RenderingBackend = Rendering.BackendFactory.CreateRenderingBackend(_logger);
				}
				
				// Initialize the window with the specified dimensions
				var title = "Win32Emu DirectDraw";
				if (obj.RenderingBackend.IsInitialized)
				{
					// If already initialized, we would need to recreate with new dimensions
					// For now, we'll just log this situation
					_logger.LogInformation("[DDraw] Display mode changed to {Width}x{Height}x{Bpp}", dwWidth, dwHeight, dwBPP);
				}
				else
				{
					var success = obj.RenderingBackend.Initialize((int)dwWidth, (int)dwHeight, title);
					if (!success)
					{
						_logger.LogError("[DDraw] Failed to initialize rendering backend");
						return 1; // DDERR_GENERIC
					}
				}
				
				// Subscribe to UI events from the rendering backend
				// ProcessEnvironment now tracks subscriptions and prevents duplicates automatically
				if (obj.RenderingBackend != null)
				{
					_env.SubscribeToUIEvents(obj.RenderingBackend, null);
					_logger.LogInformation("[DDraw] Subscribed to UI events from rendering backend");
				}
			}
			else
			{
				_logger.LogError("[DDraw] SetDisplayMode: Could not find DirectDraw object with handle 0x{Handle:X8}", actualHandle);
				return 1; // DDERR_GENERIC
			}

			return 0; // DD_OK
		}

		private uint DDraw_WaitForVerticalBlank(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var dwFlags = args.UInt32(1);

			_logger.LogInformation("[DDraw COM] IDirectDraw::WaitForVerticalBlank(this=0x{ThisPtr:X8}, dwFlags=0x{DwFlags:X8})", 
				thisPtr, dwFlags);

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
			var surfaceMemPtr = _env.VirtualAlloc(0, (uint)(surface.Pitch * surface.Height), 0x1000, 0x04); // MEM_COMMIT, PAGE_READWRITE
			surface.LockedMemoryPtr = surfaceMemPtr;
			
			// Fill the surface description structure
			if (lpDDSurfaceDesc != 0)
			{
				var dwSize = _env.MemRead32(lpDDSurfaceDesc);
				
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
				var data = _env.MemReadBytes(surface.LockedMemoryPtr, surface.Pitch * surface.Height);
				Array.Copy(data, surface.Bits, data.Length);
				
				// We don't actually free memory in this implementation
				// Just mark it as no longer locked
				surface.LockedMemoryPtr = 0;
			}
			
			// Mark the surface as unlocked
			surface.IsLocked = false;
			
			// If this is a primary surface, update the rendering backend texture
			if (surface.IsPrimary && _ddrawObjects.TryGetValue(surface.DirectDrawHandle, out var ddrawObj) && ddrawObj.RenderingBackend != null)
			{
				try
				{
					// Check if surface bits are available
					if (surface.Bits == null)
					{
						_logger.LogWarning("[DDraw] Surface bits are null, skipping flip");
						return 0; // DD_OK
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
						ddrawObj.RenderingBackend.UpdateFrameBuffer(displayData, displayPitch);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[DDraw] Failed to update rendering backend texture for primary surface");
				}
			}
			
			_logger.LogInformation("[DDraw] Unlocked surface 0x{SurfaceHandle:X8}", surfaceHandle);
			return 0; // DD_OK
		}
	}
}