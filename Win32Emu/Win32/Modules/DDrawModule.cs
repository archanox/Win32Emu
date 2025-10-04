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
					_logger.LogInformation($"[DDraw] Unimplemented export: {export}");
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
		private unsafe uint DirectDrawCreate(in uint lpGuid, uint lplpDd, in uint pUnkOuter)
		{
			_logger.LogInformation($"[DDraw] DirectDrawCreate(lpGuid=0x{lpGuid:X8}, lplpDD=0x{lplpDd:X8}, pUnkOuter=0x{pUnkOuter:X8})");

			// Create DirectDraw object
			var ddrawHandle = _nextDDrawHandle++;
			var ddrawObj = new DirectDrawObject
			{
				Handle = ddrawHandle
			};
			_ddrawObjects[ddrawHandle] = ddrawObj;

			// Write vtable pointer back to caller
			if (lplpDd != 0)
			{
				_env.MemWrite32(lplpDd, ddrawHandle);
			}

			_logger.LogInformation($"[DDraw] Created DirectDraw object: 0x{ddrawHandle:X8}");
			return 0; // DD_OK
		}

		private unsafe uint DirectDrawCreateEx(uint lpGuid, uint lplpDd, uint iid, uint pUnkOuter)
		{
			_logger.LogInformation($"[DDraw] DirectDrawCreateEx(lpGuid=0x{lpGuid:X8}, lplpDD=0x{lplpDd:X8}, iid=0x{iid:X8}, pUnkOuter=0x{pUnkOuter:X8})");

			// Similar to DirectDrawCreate but with interface ID
			var ddrawHandle = _nextDDrawHandle++;
			var ddrawObj = new DirectDrawObject
			{
				Handle = ddrawHandle
			};
			_ddrawObjects[ddrawHandle] = ddrawObj;

			if (lplpDd != 0)
			{
				_env.MemWrite32(lplpDd, ddrawHandle);
			}

			_logger.LogInformation($"[DDraw] Created DirectDraw object (Ex): 0x{ddrawHandle:X8}");
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

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for DDraw
			// version 5.3.2600.5512 (XP)
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{ "ACQUIREDDTHREADLOCK", 1 },
				{ "CHECKFULLSCREEN", 2 },
				{ "COMPLETECREATESYSMEMSURFACE", 3 },
				{ "D3DPARSEUNKNOWNCOMMAND", 4 },
				{ "DDGETATTACHEDSURFACELCL", 5 },
				{ "DDINTERNALLOCK", 6 },
				{ "DDINTERNALINTERNALUNLOCK", 7 },
				{ "DSOUNDHELP", 8 },
				{ "DIRECTDRAWCREATE", 9 },
				{ "DIRECTDRAWCREATECLIPPER", 10 },
				{ "DIRECTDRAWCREATEEX", 11 },
				{ "DIRECTDRAWENUMERATEA", 12 },
				{ "DIRECTDRAWENUMERATEEXA", 13 },
				{ "DIRECTDRAWENUMERATEEXW", 14 },
				{ "DIRECTDRAWENUMERATEW", 15 },
				{ "DLLCANUNLOADNOW", 16 },
				{ "DLLGETCLASSOBJECT", 17 },
				{ "GETDDSURFACELOCAL", 18 },
				{ "GETOLETHUNKDATA", 19 },
				{ "GETSURFACEFROMDC", 20 },
				{ "REGISTERSPECIALCASE", 21 },
				{ "RELEASEDDTHREADLOCK", 22 },
			};
			
			return exports;
		}
	}
}