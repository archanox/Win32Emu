using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class Glide2XModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public Glide2XModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "GLIDE2X.DLL";

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				// Glide initialization/shutdown
				case "_GRGLIDEINIT@0":
					_logger.LogInformation("[Glide2x] grGlideInit()");
					returnValue = 0; // Success
					return true;

				case "_GRGLIDESHUTDOWN@0":
					_logger.LogInformation("[Glide2x] grGlideShutdown()");
					returnValue = 0;
					return true;

				case "_GRSSTSELECT@4":
					_logger.LogInformation($"[Glide2x] grSstSelect({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GRSSTQUERYHARDWARE@4":
					_logger.LogInformation($"[Glide2x] grSstQueryHardware(0x{a.UInt32(0):X8})");
					returnValue = 1; // Return TRUE to indicate hardware is present
					return true;

				case "_GRSSTWINOPEN@28":
					_logger.LogInformation($"[Glide2x] grSstWinOpen(hwnd=0x{a.UInt32(0):X8}, res={a.UInt32(1)}, refresh={a.UInt32(2)}, ...)");
					returnValue = 1; // Return TRUE for success
					return true;

				case "_GRSSTWINCLOSE@0":
					_logger.LogInformation("[Glide2x] grSstWinClose()");
					returnValue = 0;
					return true;

				case "_GRSSTIDLE@0":
					_logger.LogInformation("[Glide2x] grSstIdle()");
					returnValue = 0;
					return true;

				case "_GRSSTVRETRACEON@0":
					_logger.LogInformation("[Glide2x] grSstVRetraceOn()");
					returnValue = 1; // Return TRUE
					return true;

				// Buffer management
				case "_GRBUFFERSWAP@4":
					_logger.LogInformation($"[Glide2x] grBufferSwap({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GRBUFFERCLEAR@12":
					_logger.LogInformation($"[Glide2x] grBufferClear(color=0x{a.UInt32(0):X8}, alpha={a.UInt32(1)}, depth={a.UInt32(2)})");
					returnValue = 0;
					return true;

				case "_GRRENDERBUFFER@4":
					_logger.LogInformation($"[Glide2x] grRenderBuffer({a.UInt32(0)})");
					returnValue = 0;
					return true;

				// Linear frame buffer
				case "_GRLFBLOCK@24":
					_logger.LogInformation($"[Glide2x] grLfbLock({a.UInt32(0)}, {a.UInt32(1)}, ...)");
					returnValue = 1; // Return TRUE for success
					return true;

				case "_GRLFBUNLOCK@8":
					_logger.LogInformation($"[Glide2x] grLfbUnlock({a.UInt32(0)}, {a.UInt32(1)})");
					returnValue = 1;
					return true;

				// Texture management
				case "_GUTEXMEMRESET@0":
					_logger.LogInformation("[Glide2x] guTexMemReset()");
					returnValue = 0;
					return true;

				case "_GUTEXALLOCATEMEMORY@60":
					_logger.LogInformation("[Glide2x] guTexAllocateMemory(...)");
					returnValue = 0x100000; // Return a dummy texture memory address
					return true;

				case "_GUTEXDOWNLOADMIPMAP@12":
					_logger.LogInformation($"[Glide2x] guTexDownloadMipMap(0x{a.UInt32(0):X8}, 0x{a.UInt32(1):X8}, 0x{a.UInt32(2):X8})");
					returnValue = 0;
					return true;

				case "_GRTEXDOWNLOADTABLE@12":
					_logger.LogInformation($"[Glide2x] grTexDownloadTable({a.UInt32(0)}, 0x{a.UInt32(1):X8}, 0x{a.UInt32(2):X8})");
					returnValue = 0;
					return true;

				// State management
				case "_GRGLIDEGETSTATE@4":
					_logger.LogInformation($"[Glide2x] grGlideGetState(0x{a.UInt32(0):X8})");
					returnValue = 0;
					return true;

				case "_GRGLIDESETSTATE@4":
					_logger.LogInformation($"[Glide2x] grGlideSetState(0x{a.UInt32(0):X8})");
					returnValue = 0;
					return true;

				// Rendering modes
				case "_GRALPHABLENDFUNCTI" + "ON@16": // _grAlphaBlendFunction@16
					_logger.LogInformation($"[Glide2x] grAlphaBlendFunction({a.UInt32(0)}, {a.UInt32(1)}, {a.UInt32(2)}, {a.UInt32(3)})");
					returnValue = 0;
					return true;

				case "_GRDEPTHBUFFERFUNCTION@4":
					_logger.LogInformation($"[Glide2x] grDepthBufferFunction({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GRDEPTHMASK@4":
					_logger.LogInformation($"[Glide2x] grDepthMask({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GRDEPTHBUFFERMODE@4":
					_logger.LogInformation($"[Glide2x] grDepthBufferMode({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GRCHROMAKEYVALUE@4":
					_logger.LogInformation($"[Glide2x] grChromakeyValue(0x{a.UInt32(0):X8})");
					returnValue = 0;
					return true;

				case "_GRCHROMAKEYMODE@4":
					_logger.LogInformation($"[Glide2x] grChromakeyMode({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GRCULLMODE@4":
					_logger.LogInformation($"[Glide2x] grCullMode({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GRCLIPWINDOW@16":
					_logger.LogInformation($"[Glide2x] grClipWindow({a.UInt32(0)}, {a.UInt32(1)}, {a.UInt32(2)}, {a.UInt32(3)})");
					returnValue = 0;
					return true;

				case "_GRCONSTANTCOLORVALUE@4":
					_logger.LogInformation($"[Glide2x] grConstantColorValue(0x{a.UInt32(0):X8})");
					returnValue = 0;
					return true;

				// GU helper functions
				case "_GUALPHASOURCE@4":
					_logger.LogInformation($"[Glide2x] guAlphaSource({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GUCOLORCOMBINEFUNCTION@4":
					_logger.LogInformation($"[Glide2x] guColorCombineFunction({a.UInt32(0)})");
					returnValue = 0;
					return true;

				case "_GUTEXCOMBINEFUNCTION@8":
					_logger.LogInformation($"[Glide2x] guTexCombineFunction({a.UInt32(0)}, {a.UInt32(1)})");
					returnValue = 0;
					return true;

				case "_GUTEXSOURCE@4":
					_logger.LogInformation($"[Glide2x] guTexSource(0x{a.UInt32(0):X8})");
					returnValue = 0;
					return true;

				// Drawing primitives
				case "_GRAADRAWLINE@8":
					_logger.LogInformation($"[Glide2x] grAADrawLine(0x{a.UInt32(0):X8}, 0x{a.UInt32(1):X8})");
					returnValue = 0;
					return true;

				case "_GRAADRAWPOINT@4":
					_logger.LogInformation($"[Glide2x] grAADrawPoint(0x{a.UInt32(0):X8})");
					returnValue = 0;
					return true;

				case "_GUDRAWTRIANGLEWITHCL" + "IP@12": // _guDrawTriangleWithClip@12
					_logger.LogInformation($"[Glide2x] guDrawTriangleWithClip(0x{a.UInt32(0):X8}, 0x{a.UInt32(1):X8}, 0x{a.UInt32(2):X8})");
					returnValue = 0;
					return true;

				default:
					_logger.LogInformation($"[Glide2x] Unimplemented export: {export}");
					return false;
			}
		}

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for Glide2x - currently no implemented exports
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{"_ConvertAndDownloadRle@64", 1}
			};
			return exports;
		}
	}
}