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
		
		// Rendering backend for 3Dfx Glide emulation
		private Rendering.IRenderingBackend? _renderingBackend;
		
		// Glide state
		private bool _glideInitialized;
		private bool _windowOpen;
		private int _width = 640;
		private int _height = 480;
		private byte[]? _frameBuffer;
		private bool _frameBufferLocked;
		private uint _frameBufferAddress;
		private const uint FrameBufferBaseAddress = 0xA0000000; // Dummy address for locked frame buffer

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
					returnValue = grGlideInit(); // Success
					return true;

				case "_GRGLIDESHUTDOWN@0":
					_logger.LogInformation("[Glide2x] grGlideShutdown()");
					returnValue = grGlideShutdown();
					return true;

				case "_GRSSTSELECT@4":
					_logger.LogInformation("[Glide2x] grSstSelect({UInt32})", a.UInt32(0));
					returnValue = grSstSelect();
					return true;

				case "_GRSSTQUERYHARDWARE@4":
					_logger.LogInformation("[Glide2x] grSstQueryHardware(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grSstQueryHardware(); // Return TRUE to indicate hardware is present
					return true;

				case "_GRSSTWINOPEN@28":
					_logger.LogInformation("[Glide2x] grSstWinOpen(hwnd=0x{UInt32:X8}, res={U}, refresh={UInt33}, ...)", a.UInt32(0), a.UInt32(1), a.UInt32(2));
					returnValue = grSstWinOpen(); // Return TRUE for success
					return true;

				case "_GRSSTWINCLOSE@0":
					_logger.LogInformation("[Glide2x] grSstWinClose()");
					returnValue = grSstWinClose();
					return true;

				case "_GRSSTIDLE@0":
					_logger.LogInformation("[Glide2x] grSstIdle()");
					returnValue = grSstIdle();
					return true;

				case "_GRSSTVRETRACEON@0":
					_logger.LogInformation("[Glide2x] grSstVRetraceOn()");
					returnValue = grSstVRetraceOn(); // Return TRUE
					return true;

				// Buffer management
				case "_GRBUFFERSWAP@4":
					_logger.LogInformation("[Glide2x] grBufferSwap({UInt32})", a.UInt32(0));
					returnValue = grBufferSwap();
					return true;

				case "_GRBUFFERCLEAR@12":
					_logger.LogInformation("[Glide2x] grBufferClear(color=0x{UInt32:X8}, alpha={U}, depth={UInt33})", a.UInt32(0), a.UInt32(1), a.UInt32(2));
					returnValue = grBufferClear();
					return true;

				case "_GRRENDERBUFFER@4":
					_logger.LogInformation("[Glide2x] grRenderBuffer({UInt32})", a.UInt32(0));
					returnValue = grRenderBuffer();
					return true;

				// Linear frame buffer
				case "_GRLFBLOCK@24":
					_logger.LogInformation("[Glide2x] grLfbLock({UInt32}, {U}, ...)", a.UInt32(0), a.UInt32(1));
					returnValue = grLfbLock(); // Return TRUE for success
					return true;

				case "_GRLFBUNLOCK@8":
					_logger.LogInformation("[Glide2x] grLfbUnlock({UInt32}, {U})", a.UInt32(0), a.UInt32(1));
					returnValue = grLfbUnlock();
					return true;

				// Texture management
				case "_GUTEXMEMRESET@0":
					_logger.LogInformation("[Glide2x] guTexMemReset()");
					returnValue = guTexMemReset();
					return true;

				case "_GUTEXALLOCATEMEMORY@60":
					_logger.LogInformation("[Glide2x] guTexAllocateMemory(...)");
					returnValue = guTexAllocateMemory(); // Return a dummy texture memory address
					return true;

				case "_GUTEXDOWNLOADMIPMAP@12":
					_logger.LogInformation("[Glide2x] guTexDownloadMipMap(0x{UInt32:X8}, 0x{U:X8}, 0x{UInt33:X8})", a.UInt32(0), a.UInt32(1), a.UInt32(2));
					returnValue = guTexDownloadMipMap();
					return true;

				case "_GRTEXDOWNLOADTABLE@12":
					_logger.LogInformation("[Glide2x] grTexDownloadTable({UInt32}, 0x{U:X8}, 0x{UInt33:X8})", a.UInt32(0), a.UInt32(1), a.UInt32(2));
					returnValue = grTexDownloadTable();
					return true;

				// State management
				case "_GRGLIDEGETSTATE@4":
					_logger.LogInformation("[Glide2x] grGlideGetState(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grGlideGetState();
					return true;

				case "_GRGLIDESETSTATE@4":
					_logger.LogInformation("[Glide2x] grGlideSetState(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grGlideSetState();
					return true;

				// Rendering modes
				case "_GRALPHABLENDFUNCTION@16": // _grAlphaBlendFunction@16
					_logger.LogInformation("[Glide2x] grAlphaBlendFunction({UInt32}, {U}, {UInt33}, {U1})", a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					returnValue = grAlphaBlendFunction();
					return true;

				case "_GRDEPTHBUFFERFUNCTION@4":
					_logger.LogInformation("[Glide2x] grDepthBufferFunction({UInt32})", a.UInt32(0));
					returnValue = grDepthBufferFunction();
					return true;

				case "_GRDEPTHMASK@4":
					_logger.LogInformation("[Glide2x] grDepthMask({UInt32})", a.UInt32(0));
					returnValue = grDepthMask();
					return true;

				case "_GRDEPTHBUFFERMODE@4":
					_logger.LogInformation("[Glide2x] grDepthBufferMode({UInt32})", a.UInt32(0));
					returnValue = grDepthBufferMode();
					return true;

				case "_GRCHROMAKEYVALUE@4":
					_logger.LogInformation("[Glide2x] grChromakeyValue(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grChromakeyValue();
					return true;

				case "_GRCHROMAKEYMODE@4":
					_logger.LogInformation("[Glide2x] grChromakeyMode({UInt32})", a.UInt32(0));
					returnValue = grChromakeyMode();
					return true;

				case "_GRCULLMODE@4":
					_logger.LogInformation("[Glide2x] grCullMode({UInt32})", a.UInt32(0));
					returnValue = grCullMode();
					return true;

				case "_GRCLIPWINDOW@16":
					_logger.LogInformation("[Glide2x] grClipWindow({UInt32}, {U}, {UInt33}, {U1})", a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					returnValue = grClipWindow();
					return true;

				case "_GRCONSTANTCOLORVALUE@4":
					_logger.LogInformation("[Glide2x] grConstantColorValue(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grConstantColorValue();
					return true;

				// GU helper functions
				case "_GUALPHASOURCE@4":
					_logger.LogInformation("[Glide2x] guAlphaSource({UInt32})", a.UInt32(0));
					returnValue = guAlphaSource();
					return true;

				case "_GUCOLORCOMBINEFUNCTION@4":
					_logger.LogInformation("[Glide2x] guColorCombineFunction({UInt32})", a.UInt32(0));
					returnValue = guColorCombineFunction();
					return true;

				case "_GUTEXCOMBINEFUNCTION@8":
					_logger.LogInformation("[Glide2x] guTexCombineFunction({UInt32}, {U})", a.UInt32(0), a.UInt32(1));
					returnValue = guTexCombineFunction();
					return true;

				case "_GUTEXSOURCE@4":
					_logger.LogInformation("[Glide2x] guTexSource(0x{UInt32:X8})", a.UInt32(0));
					returnValue = guTexSource();
					return true;

				// Drawing primitives
				case "_GRAADRAWLINE@8":
					_logger.LogInformation("[Glide2x] grAADrawLine(0x{UInt32:X8}, 0x{U:X8})", a.UInt32(0), a.UInt32(1));
					returnValue = grAADrawLine();
					return true;

				case "_GRAADRAWPOINT@4":
					_logger.LogInformation("[Glide2x] grAADrawPoint(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grAADrawPoint();
					return true;

				case "_GUDRAWTRIANGLEWITHCLIP@12": // _guDrawTriangleWithClip@12
					_logger.LogInformation("[Glide2x] guDrawTriangleWithClip(0x{UInt32:X8}, 0x{U:X8}, 0x{UInt33:X8})", a.UInt32(0), a.UInt32(1), a.UInt32(2));
					returnValue = guDrawTriangleWithClip();
					return true;

				default:
					_logger.LogInformation("[Glide2x] Unimplemented export: {Export}", export);
					return false;
			}
		}


		[DllModuleExport(1, entryPoint: 0x00005ED0, Version = "4.90.0.3000", ExportName = "_ConvertAndDownloadRle@64", IsStub = true)]
		public uint ConvertAndDownloadRle()
		{
			_logger.LogWarning("[GLIDE2x] ConvertAndDownloadRle called (stub)");
			// TODO: Implement _ConvertAndDownloadRle@64
			return 0; // DWORD default
		}

		[DllModuleExport(2, entryPoint: 0x00002E70, Version = "4.90.0.3000", ExportName = "_grAADrawLine@8", IsStub = true)]
		public uint grAADrawLine()
		{
			_logger.LogWarning("[GLIDE2x] grAADrawLine called (stub)");
			// TODO: Implement _grAADrawLine@8
			return 0; // DWORD default
		}

		[DllModuleExport(3, entryPoint: 0x00002EA0, Version = "4.90.0.3000", ExportName = "_grAADrawPoint@4", IsStub = true)]
		public uint grAADrawPoint()
		{
			_logger.LogWarning("[GLIDE2x] grAADrawPoint called (stub)");
			// TODO: Implement _grAADrawPoint@4
			return 0; // DWORD default
		}

		[DllModuleExport(4, entryPoint: 0x00002ED0, Version = "4.90.0.3000", ExportName = "_grAADrawPolygon@12", IsStub = true)]
		public uint grAADrawPolygon()
		{
			_logger.LogWarning("[GLIDE2x] grAADrawPolygon called (stub)");
			// TODO: Implement _grAADrawPolygon@12
			return 0; // DWORD default
		}

		[DllModuleExport(5, entryPoint: 0x00002F00, Version = "4.90.0.3000", ExportName = "_grAADrawPolygonVertexList@8", IsStub = true)]
		public uint grAADrawPolygonVertexList()
		{
			_logger.LogWarning("[GLIDE2x] grAADrawPolygonVertexList called (stub)");
			// TODO: Implement _grAADrawPolygonVertexList@8
			return 0; // DWORD default
		}

		[DllModuleExport(6, entryPoint: 0x00002F30, Version = "4.90.0.3000", ExportName = "_grAADrawTriangle@24", IsStub = true)]
		public uint grAADrawTriangle()
		{
			_logger.LogWarning("[GLIDE2x] grAADrawTriangle called (stub)");
			// TODO: Implement _grAADrawTriangle@24
			return 0; // DWORD default
		}

		[DllModuleExport(7, entryPoint: 0x00002980, Version = "4.90.0.3000", ExportName = "_grAlphaBlendFunction@16", IsStub = true)]
		public uint grAlphaBlendFunction()
		{
			_logger.LogWarning("[GLIDE2x] grAlphaBlendFunction called (stub)");
			// TODO: Implement _grAlphaBlendFunction@16
			return 0; // DWORD default
		}

		[DllModuleExport(8, entryPoint: 0x00002990, Version = "4.90.0.3000", ExportName = "_grAlphaCombine@20", IsStub = true)]
		public uint grAlphaCombine()
		{
			_logger.LogWarning("[GLIDE2x] grAlphaCombine called (stub)");
			// TODO: Implement _grAlphaCombine@20
			return 0; // DWORD default
		}

		[DllModuleExport(9, entryPoint: 0x000029A0, Version = "4.90.0.3000", ExportName = "_grAlphaControlsITRGBLighting@4", IsStub = true)]
		public uint grAlphaControlsITRGBLighting()
		{
			_logger.LogWarning("[GLIDE2x] grAlphaControlsITRGBLighting called (stub)");
			// TODO: Implement _grAlphaControlsITRGBLighting@4
			return 0; // DWORD default
		}

		[DllModuleExport(10, entryPoint: 0x000029B0, Version = "4.90.0.3000", ExportName = "_grAlphaTestFunction@4", IsStub = true)]
		public uint grAlphaTestFunction()
		{
			_logger.LogWarning("[GLIDE2x] grAlphaTestFunction called (stub)");
			// TODO: Implement _grAlphaTestFunction@4
			return 0; // DWORD default
		}

		[DllModuleExport(11, entryPoint: 0x000029C0, Version = "4.90.0.3000", ExportName = "_grAlphaTestReferenceValue@4", IsStub = true)]
		public uint grAlphaTestReferenceValue()
		{
			_logger.LogWarning("[GLIDE2x] grAlphaTestReferenceValue called (stub)");
			// TODO: Implement _grAlphaTestReferenceValue@4
			return 0; // DWORD default
		}

		[DllModuleExport(12, entryPoint: 0x00001230, Version = "4.90.0.3000", ExportName = "_grBufferClear@12")]
		public uint grBufferClear()
		{
			_logger.LogDebug("[GLIDE2x] grBufferClear called");
			
			if (!_windowOpen || _frameBuffer == null)
			{
				_logger.LogWarning("[GLIDE2x] grBufferClear: Window not open or no frame buffer");
				return 0;
			}
			
			// Read color, alpha, and depth from stack (stdcall: arguments pushed right-to-left)
			uint color = _env.Cpu.PeekStack32(0);   // First argument (color)
			uint alpha = _env.Cpu.PeekStack32(4);   // Second argument (alpha)
			uint depth = _env.Cpu.PeekStack32(8);   // Third argument (depth)

			_logger.LogDebug($"[GLIDE2x] grBufferClear params: color=0x{color:X8}, alpha=0x{alpha:X8}, depth=0x{depth:X8}");

			// Fill frame buffer with the specified color (assuming 32bpp RGBA)
			byte r = (byte)((color >> 16) & 0xFF);
			byte g = (byte)((color >> 8) & 0xFF);
			byte b = (byte)(color & 0xFF);
			byte a = (byte)((alpha) & 0xFF); // Use alpha argument if needed

			// Assuming _frameBuffer is a byte[] representing RGBA pixels
			for (int i = 0; i < _frameBuffer.Length; i += 4)
			{
				_frameBuffer[i + 0] = r;
				_frameBuffer[i + 1] = g;
				_frameBuffer[i + 2] = b;
				_frameBuffer[i + 3] = a;
			}
			_logger.LogDebug("[GLIDE2x] Buffer cleared successfully");
			return 0; // Success (void function)
		}

		[DllModuleExport(13, entryPoint: 0x00001390, Version = "4.90.0.3000", ExportName = "_grBufferNumPending@0", IsStub = true)]
		public uint grBufferNumPending()
		{
			_logger.LogWarning("[GLIDE2x] grBufferNumPending called (stub)");
			// TODO: Implement _grBufferNumPending@0
			return 0; // DWORD default
		}

		[DllModuleExport(14, entryPoint: 0x00001220, Version = "4.90.0.3000", ExportName = "_grBufferSwap@4")]
		public uint grBufferSwap()
		{
			_logger.LogDebug("[GLIDE2x] grBufferSwap called");
			
			if (!_windowOpen || _renderingBackend == null || _frameBuffer == null)
			{
				_logger.LogWarning("[GLIDE2x] grBufferSwap: Window not open or no frame buffer");
				return 0;
			}
			
			// Update the display with the frame buffer
			_renderingBackend.UpdateFrameBuffer(_frameBuffer, _width * 4);
			_renderingBackend.ProcessEvents();
			
			_logger.LogDebug("[GLIDE2x] Buffer swapped successfully");
			return 0; // Success (void function)
		}

		[DllModuleExport(15, entryPoint: 0x00004090, Version = "4.90.0.3000", ExportName = "_grCheckForRoom@4", IsStub = true)]
		public uint grCheckForRoom()
		{
			_logger.LogWarning("[GLIDE2x] grCheckForRoom called (stub)");
			// TODO: Implement _grCheckForRoom@4
			return 0; // DWORD default
		}

		[DllModuleExport(16, entryPoint: 0x00003C80, Version = "4.90.0.3000", ExportName = "_grChromakeyMode@4", IsStub = true)]
		public uint grChromakeyMode()
		{
			_logger.LogWarning("[GLIDE2x] grChromakeyMode called (stub)");
			// TODO: Implement _grChromakeyMode@4
			return 0; // DWORD default
		}

		[DllModuleExport(17, entryPoint: 0x00003C90, Version = "4.90.0.3000", ExportName = "_grChromakeyValue@4", IsStub = true)]
		public uint grChromakeyValue()
		{
			_logger.LogWarning("[GLIDE2x] grChromakeyValue called (stub)");
			// TODO: Implement _grChromakeyValue@4
			return 0; // DWORD default
		}

		[DllModuleExport(18, entryPoint: 0x00003CA0, Version = "4.90.0.3000", ExportName = "_grClipWindow@16", IsStub = true)]
		public uint grClipWindow()
		{
			_logger.LogWarning("[GLIDE2x] grClipWindow called (stub)");
			// TODO: Implement _grClipWindow@16
			return 0; // DWORD default
		}

		[DllModuleExport(19, entryPoint: 0x000029D0, Version = "4.90.0.3000", ExportName = "_grColorCombine@20", IsStub = true)]
		public uint grColorCombine()
		{
			_logger.LogWarning("[GLIDE2x] grColorCombine called (stub)");
			// TODO: Implement _grColorCombine@20
			return 0; // DWORD default
		}

		[DllModuleExport(20, entryPoint: 0x000029E0, Version = "4.90.0.3000", ExportName = "_grColorMask@8", IsStub = true)]
		public uint grColorMask()
		{
			_logger.LogWarning("[GLIDE2x] grColorMask called (stub)");
			// TODO: Implement _grColorMask@8
			return 0; // DWORD default
		}

		[DllModuleExport(21, entryPoint: 0x00002A00, Version = "4.90.0.3000", ExportName = "_grConstantColorValue4@16", IsStub = true)]
		public uint grConstantColorValue4()
		{
			_logger.LogWarning("[GLIDE2x] grConstantColorValue4 called (stub)");
			// TODO: Implement _grConstantColorValue4@16
			return 0; // DWORD default
		}

		[DllModuleExport(22, entryPoint: 0x000029F0, Version = "4.90.0.3000", ExportName = "_grConstantColorValue@4", IsStub = true)]
		public uint grConstantColorValue()
		{
			_logger.LogWarning("[GLIDE2x] grConstantColorValue called (stub)");
			// TODO: Implement _grConstantColorValue@4
			return 0; // DWORD default
		}

		[DllModuleExport(23, entryPoint: 0x00002E60, Version = "4.90.0.3000", ExportName = "_grCullMode@4", IsStub = true)]
		public uint grCullMode()
		{
			_logger.LogWarning("[GLIDE2x] grCullMode called (stub)");
			// TODO: Implement _grCullMode@4
			return 0; // DWORD default
		}

		[DllModuleExport(24, entryPoint: 0x000013B0, Version = "4.90.0.3000", ExportName = "_grDepthBiasLevel@4", IsStub = true)]
		public uint grDepthBiasLevel()
		{
			_logger.LogWarning("[GLIDE2x] grDepthBiasLevel called (stub)");
			// TODO: Implement _grDepthBiasLevel@4
			return 0; // DWORD default
		}

		[DllModuleExport(25, entryPoint: 0x000013C0, Version = "4.90.0.3000", ExportName = "_grDepthBufferFunction@4", IsStub = true)]
		public uint grDepthBufferFunction()
		{
			_logger.LogWarning("[GLIDE2x] grDepthBufferFunction called (stub)");
			// TODO: Implement _grDepthBufferFunction@4
			return 0; // DWORD default
		}

		[DllModuleExport(26, entryPoint: 0x000013D0, Version = "4.90.0.3000", ExportName = "_grDepthBufferMode@4", IsStub = true)]
		public uint grDepthBufferMode()
		{
			_logger.LogWarning("[GLIDE2x] grDepthBufferMode called (stub)");
			// TODO: Implement _grDepthBufferMode@4
			return 0; // DWORD default
		}

		[DllModuleExport(27, entryPoint: 0x00001400, Version = "4.90.0.3000", ExportName = "_grDepthMask@4", IsStub = true)]
		public uint grDepthMask()
		{
			_logger.LogWarning("[GLIDE2x] grDepthMask called (stub)");
			// TODO: Implement _grDepthMask@4
			return 0; // DWORD default
		}

		[DllModuleExport(28, entryPoint: 0x00001410, Version = "4.90.0.3000", ExportName = "_grDisableAllEffects@0", IsStub = true)]
		public uint grDisableAllEffects()
		{
			_logger.LogWarning("[GLIDE2x] grDisableAllEffects called (stub)");
			// TODO: Implement _grDisableAllEffects@0
			return 0; // DWORD default
		}

		[DllModuleExport(29, entryPoint: 0x00003D20, Version = "4.90.0.3000", ExportName = "_grDitherMode@4", IsStub = true)]
		public uint grDitherMode()
		{
			_logger.LogWarning("[GLIDE2x] grDitherMode called (stub)");
			// TODO: Implement _grDitherMode@4
			return 0; // DWORD default
		}

		[DllModuleExport(30, entryPoint: 0x00002FE0, Version = "4.90.0.3000", ExportName = "_grDrawLine@8", IsStub = true)]
		public uint grDrawLine()
		{
			_logger.LogWarning("[GLIDE2x] grDrawLine called (stub)");
			// TODO: Implement _grDrawLine@8
			return 0; // DWORD default
		}

		[DllModuleExport(31, entryPoint: 0x00002F40, Version = "4.90.0.3000", ExportName = "_grDrawPlanarPolygon@12", IsStub = true)]
		public uint grDrawPlanarPolygon()
		{
			_logger.LogWarning("[GLIDE2x] grDrawPlanarPolygon called (stub)");
			// TODO: Implement _grDrawPlanarPolygon@12
			return 0; // DWORD default
		}

		[DllModuleExport(32, entryPoint: 0x00002F50, Version = "4.90.0.3000", ExportName = "_grDrawPlanarPolygonVertexList@8", IsStub = true)]
		public uint grDrawPlanarPolygonVertexList()
		{
			_logger.LogWarning("[GLIDE2x] grDrawPlanarPolygonVertexList called (stub)");
			// TODO: Implement _grDrawPlanarPolygonVertexList@8
			return 0; // DWORD default
		}

		[DllModuleExport(33, entryPoint: 0x00002F70, Version = "4.90.0.3000", ExportName = "_grDrawPoint@4", IsStub = true)]
		public uint grDrawPoint()
		{
			_logger.LogWarning("[GLIDE2x] grDrawPoint called (stub)");
			// TODO: Implement _grDrawPoint@4
			return 0; // DWORD default
		}

		[DllModuleExport(34, entryPoint: 0x00002F80, Version = "4.90.0.3000", ExportName = "_grDrawPolygon@12", IsStub = true)]
		public uint grDrawPolygon()
		{
			_logger.LogWarning("[GLIDE2x] grDrawPolygon called (stub)");
			// TODO: Implement _grDrawPolygon@12
			return 0; // DWORD default
		}

		[DllModuleExport(35, entryPoint: 0x00002F50, Version = "4.90.0.3000", ExportName = "_grDrawPolygonVertexList@8", IsStub = true)]
		public uint grDrawPolygonVertexList()
		{
			_logger.LogWarning("[GLIDE2x] grDrawPolygonVertexList called (stub)");
			// TODO: Implement _grDrawPolygonVertexList@8
			return 0; // DWORD default
		}

		[DllModuleExport(36, entryPoint: 0x00002FF0, Version = "4.90.0.3000", ExportName = "_grDrawTriangle@12", IsStub = true)]
		public uint grDrawTriangle()
		{
			_logger.LogWarning("[GLIDE2x] grDrawTriangle called (stub)");
			// TODO: Implement _grDrawTriangle@12
			return 0; // DWORD default
		}

		[DllModuleExport(37, entryPoint: 0x00003E70, Version = "4.90.0.3000", ExportName = "_grErrorSetCallback@4", IsStub = true)]
		public uint grErrorSetCallback()
		{
			_logger.LogWarning("[GLIDE2x] grErrorSetCallback called (stub)");
			// TODO: Implement _grErrorSetCallback@4
			return 0; // DWORD default
		}

		[DllModuleExport(38, entryPoint: 0x00003100, Version = "4.90.0.3000", ExportName = "_grFogColorValue@4", IsStub = true)]
		public uint grFogColorValue()
		{
			_logger.LogWarning("[GLIDE2x] grFogColorValue called (stub)");
			// TODO: Implement _grFogColorValue@4
			return 0; // DWORD default
		}

		[DllModuleExport(39, entryPoint: 0x00003110, Version = "4.90.0.3000", ExportName = "_grFogMode@4", IsStub = true)]
		public uint grFogMode()
		{
			_logger.LogWarning("[GLIDE2x] grFogMode called (stub)");
			// TODO: Implement _grFogMode@4
			return 0; // DWORD default
		}

		[DllModuleExport(40, entryPoint: 0x00003130, Version = "4.90.0.3000", ExportName = "_grFogTable@4", IsStub = true)]
		public uint grFogTable()
		{
			_logger.LogWarning("[GLIDE2x] grFogTable called (stub)");
			// TODO: Implement _grFogTable@4
			return 0; // DWORD default
		}

		[DllModuleExport(41, entryPoint: 0x00003D30, Version = "4.90.0.3000", ExportName = "_grGammaCorrectionValue@4", IsStub = true)]
		public uint grGammaCorrectionValue()
		{
			_logger.LogWarning("[GLIDE2x] grGammaCorrectionValue called (stub)");
			// TODO: Implement _grGammaCorrectionValue@4
			return 0; // DWORD default
		}

		[DllModuleExport(42, entryPoint: 0x00004290, Version = "4.90.0.3000", ExportName = "_grGetProcAddressExtXP@4", IsStub = true)]
		public uint grGetProcAddressExtXP()
		{
			_logger.LogWarning("[GLIDE2x] grGetProcAddressExtXP called (stub)");
			// TODO: Implement _grGetProcAddressExtXP@4
			return 0; // DWORD default
		}

		[DllModuleExport(43, entryPoint: 0x000032D0, Version = "4.90.0.3000", ExportName = "_grGlideGetState@4", IsStub = true)]
		public uint grGlideGetState()
		{
			_logger.LogWarning("[GLIDE2x] grGlideGetState called (stub)");
			// TODO: Implement _grGlideGetState@4
			return 0; // DWORD default
		}

		[DllModuleExport(44, entryPoint: 0x00003290, Version = "4.90.0.3000", ExportName = "_grGlideGetVersion@4", IsStub = true)]
		public uint grGlideGetVersion()
		{
			_logger.LogWarning("[GLIDE2x] grGlideGetVersion called (stub)");
			// TODO: Implement _grGlideGetVersion@4
			return 0; // DWORD default
		}

		[DllModuleExport(45, entryPoint: 0x00003220, Version = "4.90.0.3000", ExportName = "_grGlideInit@0")]
		public uint grGlideInit()
		{
			_logger.LogInformation("[GLIDE2x] grGlideInit called");
			
			if (_glideInitialized)
			{
				_logger.LogWarning("[GLIDE2x] grGlideInit: Already initialized");
				return 0; // Already initialized
			}
			
			_glideInitialized = true;
			_logger.LogInformation("[GLIDE2x] Glide initialized successfully");
			return 0; // Success (void function)
		}

		[DllModuleExport(46, entryPoint: 0x00003300, Version = "4.90.0.3000", ExportName = "_grGlideSetState@4", IsStub = true)]
		public uint grGlideSetState()
		{
			_logger.LogWarning("[GLIDE2x] grGlideSetState called (stub)");
			// TODO: Implement _grGlideSetState@4
			return 0; // DWORD default
		}

		[DllModuleExport(47, entryPoint: 0x000032B0, Version = "4.90.0.3000", ExportName = "_grGlideShamelessPlug@4", IsStub = true)]
		public uint grGlideShamelessPlug()
		{
			_logger.LogWarning("[GLIDE2x] grGlideShamelessPlug called (stub)");
			// TODO: Implement _grGlideShamelessPlug@4
			return 0; // DWORD default
		}

		[DllModuleExport(48, entryPoint: 0x00003280, Version = "4.90.0.3000", ExportName = "_grGlideShutdown@0")]
		public uint grGlideShutdown()
		{
			_logger.LogInformation("[GLIDE2x] grGlideShutdown called");
			
			if (!_glideInitialized)
			{
				_logger.LogWarning("[GLIDE2x] grGlideShutdown: Not initialized");
				return 0;
			}
			
			// Close window if open
			if (_windowOpen)
			{
				grSstWinClose();
			}
			
			_glideInitialized = false;
			_logger.LogInformation("[GLIDE2x] Glide shutdown successfully");
			return 0; // Success (void function)
		}

		[DllModuleExport(49, entryPoint: 0x00003D60, Version = "4.90.0.3000", ExportName = "_grHints@8", IsStub = true)]
		public uint grHints()
		{
			_logger.LogWarning("[GLIDE2x] grHints called (stub)");
			// TODO: Implement _grHints@8
			return 0; // DWORD default
		}

		[DllModuleExport(50, entryPoint: 0x00001420, Version = "4.90.0.3000", ExportName = "_grLfbConstantAlpha@4", IsStub = true)]
		public uint grLfbConstantAlpha()
		{
			_logger.LogWarning("[GLIDE2x] grLfbConstantAlpha called (stub)");
			// TODO: Implement _grLfbConstantAlpha@4
			return 0; // DWORD default
		}

		[DllModuleExport(51, entryPoint: 0x00001430, Version = "4.90.0.3000", ExportName = "_grLfbConstantDepth@4", IsStub = true)]
		public uint grLfbConstantDepth()
		{
			_logger.LogWarning("[GLIDE2x] grLfbConstantDepth called (stub)");
			// TODO: Implement _grLfbConstantDepth@4
			return 0; // DWORD default
		}

		[DllModuleExport(52, entryPoint: 0x00001450, Version = "4.90.0.3000", ExportName = "_grLfbLock@24")]
		public uint grLfbLock()
		{
			_logger.LogDebug("[GLIDE2x] grLfbLock called");
			
			if (!_windowOpen || _frameBuffer == null)
			{
				_logger.LogWarning("[GLIDE2x] grLfbLock: Window not open or no frame buffer");
				return 0; // FALSE - failed
			}
			
			if (_frameBufferLocked)
			{
				_logger.LogWarning("[GLIDE2x] grLfbLock: Frame buffer already locked");
				return 0; // FALSE - already locked
			}
			
			// Allocate a memory region for the frame buffer if not already done
			if (_frameBufferAddress == 0)
			{
				_frameBufferAddress = FrameBufferBaseAddress;
				
				// Map the frame buffer to emulated memory so the application can write to it
				// We need to copy data back and forth between _frameBuffer and emulated memory
				_logger.LogInformation("[GLIDE2x] Frame buffer mapped to address 0x{Address:X8}", _frameBufferAddress);
			}
			
			_frameBufferLocked = true;
			_logger.LogDebug("[GLIDE2x] Frame buffer locked at address 0x{Address:X8}", _frameBufferAddress);
			return 1; // TRUE - success (note: real implementation returns pointer, but we return success flag)
		}

		[DllModuleExport(53, entryPoint: 0x00001460, Version = "4.90.0.3000", ExportName = "_grLfbReadRegion@28", IsStub = true)]
		public uint grLfbReadRegion()
		{
			_logger.LogWarning("[GLIDE2x] grLfbReadRegion called (stub)");
			// TODO: Implement _grLfbReadRegion@28
			return 0; // DWORD default
		}

		[DllModuleExport(54, entryPoint: 0x00001470, Version = "4.90.0.3000", ExportName = "_grLfbUnlock@8")]
		public uint grLfbUnlock()
		{
			_logger.LogDebug("[GLIDE2x] grLfbUnlock called");
			
			if (!_frameBufferLocked)
			{
				_logger.LogWarning("[GLIDE2x] grLfbUnlock: Frame buffer not locked");
				return 1; // TRUE - success (not an error to unlock when not locked)
			}
			
			_frameBufferLocked = false;
			_logger.LogDebug("[GLIDE2x] Frame buffer unlocked");
			return 1; // TRUE - success
		}

		[DllModuleExport(55, entryPoint: 0x00004090, Version = "4.90.0.3000", ExportName = "_grLfbWriteColorFormat@4", IsStub = true)]
		public uint grLfbWriteColorFormat()
		{
			_logger.LogWarning("[GLIDE2x] grLfbWriteColorFormat called (stub)");
			// TODO: Implement _grLfbWriteColorFormat@4
			return 0; // DWORD default
		}

		[DllModuleExport(56, entryPoint: 0x000014F0, Version = "4.90.0.3000", ExportName = "_grLfbWriteColorSwizzle@8", IsStub = true)]
		public uint grLfbWriteColorSwizzle()
		{
			_logger.LogWarning("[GLIDE2x] grLfbWriteColorSwizzle called (stub)");
			// TODO: Implement _grLfbWriteColorSwizzle@8
			return 0; // DWORD default
		}

		[DllModuleExport(57, entryPoint: 0x00001480, Version = "4.90.0.3000", ExportName = "_grLfbWriteRegion@32", IsStub = true)]
		public uint grLfbWriteRegion()
		{
			_logger.LogWarning("[GLIDE2x] grLfbWriteRegion called (stub)");
			// TODO: Implement _grLfbWriteRegion@32
			return 0; // DWORD default
		}

		[DllModuleExport(58, entryPoint: 0x000014C0, Version = "4.90.0.3000", ExportName = "_grRenderBuffer@4", IsStub = true)]
		public uint grRenderBuffer()
		{
			_logger.LogWarning("[GLIDE2x] grRenderBuffer called (stub)");
			// TODO: Implement _grRenderBuffer@4
			return 0; // DWORD default
		}

		[DllModuleExport(59, entryPoint: 0x00003E60, Version = "4.90.0.3000", ExportName = "_grResetTriStats@0", IsStub = true)]
		public uint grResetTriStats()
		{
			_logger.LogWarning("[GLIDE2x] grResetTriStats called (stub)");
			// TODO: Implement _grResetTriStats@0
			return 0; // DWORD default
		}

		[DllModuleExport(60, entryPoint: 0x00003E80, Version = "4.90.0.3000", ExportName = "_grSplash@20", IsStub = true)]
		public uint grSplash()
		{
			_logger.LogWarning("[GLIDE2x] grSplash called (stub)");
			// TODO: Implement _grSplash@20
			return 0; // DWORD default
		}

		[DllModuleExport(61, entryPoint: 0x00005260, Version = "4.90.0.3000", ExportName = "_grSstControl@4", IsStub = true)]
		public uint grSstControl()
		{
			_logger.LogWarning("[GLIDE2x] grSstControl called (stub)");
			// TODO: Implement _grSstControl@4
			return 0; // DWORD default
		}

		[DllModuleExport(62, entryPoint: 0x00005290, Version = "4.90.0.3000", ExportName = "_grSstControlMode@4", IsStub = true)]
		public uint grSstControlMode()
		{
			_logger.LogWarning("[GLIDE2x] grSstControlMode called (stub)");
			// TODO: Implement _grSstControlMode@4
			return 0; // DWORD default
		}

		[DllModuleExport(63, entryPoint: 0x000052A0, Version = "4.90.0.3000", ExportName = "_grSstIdle@0", IsStub = true)]
		public uint grSstIdle()
		{
			_logger.LogWarning("[GLIDE2x] grSstIdle called (stub)");
			// TODO: Implement _grSstIdle@0
			return 0; // DWORD default
		}

		[DllModuleExport(64, entryPoint: 0x000052B0, Version = "4.90.0.3000", ExportName = "_grSstIsBusy@0", IsStub = true)]
		public uint grSstIsBusy()
		{
			_logger.LogWarning("[GLIDE2x] grSstIsBusy called (stub)");
			// TODO: Implement _grSstIsBusy@0
			return 0; // DWORD default
		}

		[DllModuleExport(65, entryPoint: 0x00004E90, Version = "4.90.0.3000", ExportName = "_grSstOrigin@4", IsStub = true)]
		public uint grSstOrigin()
		{
			_logger.LogWarning("[GLIDE2x] grSstOrigin called (stub)");
			// TODO: Implement _grSstOrigin@4
			return 0; // DWORD default
		}

		[DllModuleExport(66, entryPoint: 0x000052D0, Version = "4.90.0.3000", ExportName = "_grSstPerfStats@4", IsStub = true)]
		public uint grSstPerfStats()
		{
			_logger.LogWarning("[GLIDE2x] grSstPerfStats called (stub)");
			// TODO: Implement _grSstPerfStats@4
			return 0; // DWORD default
		}

		[DllModuleExport(67, entryPoint: 0x00004EA0, Version = "4.90.0.3000", ExportName = "_grSstQueryBoards@4", IsStub = true)]
		public uint grSstQueryBoards()
		{
			_logger.LogWarning("[GLIDE2x] grSstQueryBoards called (stub)");
			// TODO: Implement _grSstQueryBoards@4
			return 0; // DWORD default
		}

		[DllModuleExport(68, entryPoint: 0x00004EF0, Version = "4.90.0.3000", ExportName = "_grSstQueryHardware@4", IsStub = true)]
		public uint grSstQueryHardware()
		{
			_logger.LogWarning("[GLIDE2x] grSstQueryHardware called (stub)");
			// TODO: Implement _grSstQueryHardware@4
			return 0; // DWORD default
		}

		[DllModuleExport(69, entryPoint: 0x00005320, Version = "4.90.0.3000", ExportName = "_grSstResetPerfStats@0", IsStub = true)]
		public uint grSstResetPerfStats()
		{
			_logger.LogWarning("[GLIDE2x] grSstResetPerfStats called (stub)");
			// TODO: Implement _grSstResetPerfStats@0
			return 0; // DWORD default
		}

		[DllModuleExport(70, entryPoint: 0x00005330, Version = "4.90.0.3000", ExportName = "_grSstScreenHeight@0", IsStub = true)]
		public uint grSstScreenHeight()
		{
			_logger.LogWarning("[GLIDE2x] grSstScreenHeight called (stub)");
			// TODO: Implement _grSstScreenHeight@0
			return 0; // DWORD default
		}

		[DllModuleExport(71, entryPoint: 0x00005350, Version = "4.90.0.3000", ExportName = "_grSstScreenWidth@0", IsStub = true)]
		public uint grSstScreenWidth()
		{
			_logger.LogWarning("[GLIDE2x] grSstScreenWidth called (stub)");
			// TODO: Implement _grSstScreenWidth@0
			return 0; // DWORD default
		}

		[DllModuleExport(72, entryPoint: 0x00005030, Version = "4.90.0.3000", ExportName = "_grSstSelect@4", IsStub = true)]
		public uint grSstSelect()
		{
			_logger.LogWarning("[GLIDE2x] grSstSelect called (stub)");
			// TODO: Implement _grSstSelect@4
			return 0; // DWORD default
		}

		[DllModuleExport(73, entryPoint: 0x00005370, Version = "4.90.0.3000", ExportName = "_grSstStatus@0", IsStub = true)]
		public uint grSstStatus()
		{
			_logger.LogWarning("[GLIDE2x] grSstStatus called (stub)");
			// TODO: Implement _grSstStatus@0
			return 0; // DWORD default
		}

		[DllModuleExport(74, entryPoint: 0x000053C0, Version = "4.90.0.3000", ExportName = "_grSstVRetraceOn@0", IsStub = true)]
		public uint grSstVRetraceOn()
		{
			_logger.LogWarning("[GLIDE2x] grSstVRetraceOn called (stub)");
			// TODO: Implement _grSstVRetraceOn@0
			return 0; // DWORD default
		}

		[DllModuleExport(75, entryPoint: 0x000053A0, Version = "4.90.0.3000", ExportName = "_grSstVideoLine@0", IsStub = true)]
		public uint grSstVideoLine()
		{
			_logger.LogWarning("[GLIDE2x] grSstVideoLine called (stub)");
			// TODO: Implement _grSstVideoLine@0
			return 0; // DWORD default
		}

		[DllModuleExport(76, entryPoint: 0x00005210, Version = "4.90.0.3000", ExportName = "_grSstWinClose@0")]
		public uint grSstWinClose()
		{
			_logger.LogInformation("[GLIDE2x] grSstWinClose called");
			
			if (!_windowOpen)
			{
				_logger.LogWarning("[GLIDE2x] grSstWinClose: Window not open");
				return 0;
			}
			
			// Unsubscribe from UI events
			if (_renderingBackend != null)
			{
				_env.UnsubscribeFromUIEvents(_renderingBackend, null);
				_renderingBackend.Dispose();
				_renderingBackend = null;
			}
			
			_frameBuffer = null;
			_windowOpen = false;
			_frameBufferLocked = false;
			
			_logger.LogInformation("[GLIDE2x] Window closed successfully");
			return 0; // Success (void function)
		}

		[DllModuleExport(77, entryPoint: 0x00005080, Version = "4.90.0.3000", ExportName = "_grSstWinOpen@28")]
		public uint grSstWinOpen()
		{
			_logger.LogInformation("[GLIDE2x] grSstWinOpen called");
			
			if (_windowOpen)
			{
				_logger.LogWarning("[GLIDE2x] grSstWinOpen: Window already open");
				return 1; // TRUE - already open
			}
			
			// Create rendering backend (prioritize GLFW as requested)
			if (_renderingBackend == null)
			{
				_logger.LogInformation("[GLIDE2x] Creating GLFW rendering backend for Glide emulation");
				_renderingBackend = Rendering.BackendFactory.CreateRenderingBackend(_logger);
				
				if (_renderingBackend == null)
				{
					_logger.LogError("[GLIDE2x] Failed to create rendering backend");
					return 0; // FALSE - failed
				}
			}
			
			// Initialize the rendering backend
			if (!_renderingBackend.IsInitialized)
			{
				var title = "Win32Emu - 3Dfx Glide";
				var success = _renderingBackend.Initialize(_width, _height, title);
				
				if (!success)
				{
					_logger.LogError("[GLIDE2x] Failed to initialize rendering backend");
					return 0; // FALSE - failed
				}
				
				_logger.LogInformation("[GLIDE2x] Rendering backend initialized: {Width}x{Height}", _width, _height);
				
				// Subscribe to UI events
				_env.SubscribeToUIEvents(_renderingBackend, null);
			}
			
			// Allocate frame buffer
			_frameBuffer = new byte[_width * _height * 4]; // RGBA format
			_windowOpen = true;
			
			_logger.LogInformation("[GLIDE2x] Window opened successfully");
			return 1; // TRUE - success
		}

		[DllModuleExport(78, entryPoint: 0x00005860, Version = "4.90.0.3000", ExportName = "_grTexCalcMemRequired@16", IsStub = true)]
		public uint grTexCalcMemRequired()
		{
			_logger.LogWarning("[GLIDE2x] grTexCalcMemRequired called (stub)");
			// TODO: Implement _grTexCalcMemRequired@16
			return 0; // DWORD default
		}

		[DllModuleExport(79, entryPoint: 0x000058A0, Version = "4.90.0.3000", ExportName = "_grTexClampMode@12", IsStub = true)]
		public uint grTexClampMode()
		{
			_logger.LogWarning("[GLIDE2x] grTexClampMode called (stub)");
			// TODO: Implement _grTexClampMode@12
			return 0; // DWORD default
		}

		[DllModuleExport(80, entryPoint: 0x000058B0, Version = "4.90.0.3000", ExportName = "_grTexCombine@28", IsStub = true)]
		public uint grTexCombine()
		{
			_logger.LogWarning("[GLIDE2x] grTexCombine called (stub)");
			// TODO: Implement _grTexCombine@28
			return 0; // DWORD default
		}

		[DllModuleExport(81, entryPoint: 0x00006330, Version = "4.90.0.3000", ExportName = "_grTexCombineFunction@8", IsStub = true)]
		public uint grTexCombineFunction()
		{
			_logger.LogWarning("[GLIDE2x] grTexCombineFunction called (stub)");
			// TODO: Implement _grTexCombineFunction@8
			return 0; // DWORD default
		}

		[DllModuleExport(82, entryPoint: 0x00005920, Version = "4.90.0.3000", ExportName = "_grTexDetailControl@16", IsStub = true)]
		public uint grTexDetailControl()
		{
			_logger.LogWarning("[GLIDE2x] grTexDetailControl called (stub)");
			// TODO: Implement _grTexDetailControl@16
			return 0; // DWORD default
		}

		[DllModuleExport(83, entryPoint: 0x00005930, Version = "4.90.0.3000", ExportName = "_grTexDownloadMipMap@16", IsStub = true)]
		public uint grTexDownloadMipMap()
		{
			_logger.LogWarning("[GLIDE2x] grTexDownloadMipMap called (stub)");
			// TODO: Implement _grTexDownloadMipMap@16
			return 0; // DWORD default
		}

		[DllModuleExport(84, entryPoint: 0x00005A20, Version = "4.90.0.3000", ExportName = "_grTexDownloadMipMapLevel@32", IsStub = true)]
		public uint grTexDownloadMipMapLevel()
		{
			_logger.LogWarning("[GLIDE2x] grTexDownloadMipMapLevel called (stub)");
			// TODO: Implement _grTexDownloadMipMapLevel@32
			return 0; // DWORD default
		}

		[DllModuleExport(85, entryPoint: 0x000059A0, Version = "4.90.0.3000", ExportName = "_grTexDownloadMipMapLevelPartial@40", IsStub = true)]
		public uint grTexDownloadMipMapLevelPartial()
		{
			_logger.LogWarning("[GLIDE2x] grTexDownloadMipMapLevelPartial called (stub)");
			// TODO: Implement _grTexDownloadMipMapLevelPartial@40
			return 0; // DWORD default
		}

		[DllModuleExport(86, entryPoint: 0x00005A60, Version = "4.90.0.3000", ExportName = "_grTexDownloadTable@12", IsStub = true)]
		public uint grTexDownloadTable()
		{
			_logger.LogWarning("[GLIDE2x] grTexDownloadTable called (stub)");
			// TODO: Implement _grTexDownloadTable@12
			return 0; // DWORD default
		}

		[DllModuleExport(87, entryPoint: 0x00005B10, Version = "4.90.0.3000", ExportName = "_grTexDownloadTablePartial@20", IsStub = true)]
		public uint grTexDownloadTablePartial()
		{
			_logger.LogWarning("[GLIDE2x] grTexDownloadTablePartial called (stub)");
			// TODO: Implement _grTexDownloadTablePartial@20
			return 0; // DWORD default
		}

		[DllModuleExport(88, entryPoint: 0x00005BE0, Version = "4.90.0.3000", ExportName = "_grTexFilterMode@12", IsStub = true)]
		public uint grTexFilterMode()
		{
			_logger.LogWarning("[GLIDE2x] grTexFilterMode called (stub)");
			// TODO: Implement _grTexFilterMode@12
			return 0; // DWORD default
		}

		[DllModuleExport(89, entryPoint: 0x00005BF0, Version = "4.90.0.3000", ExportName = "_grTexLodBiasValue@8", IsStub = true)]
		public uint grTexLodBiasValue()
		{
			_logger.LogWarning("[GLIDE2x] grTexLodBiasValue called (stub)");
			// TODO: Implement _grTexLodBiasValue@8
			return 0; // DWORD default
		}

		[DllModuleExport(90, entryPoint: 0x00005C00, Version = "4.90.0.3000", ExportName = "_grTexMaxAddress@4", IsStub = true)]
		public uint grTexMaxAddress()
		{
			_logger.LogWarning("[GLIDE2x] grTexMaxAddress called (stub)");
			// TODO: Implement _grTexMaxAddress@4
			return 0; // DWORD default
		}

		[DllModuleExport(91, entryPoint: 0x00005C20, Version = "4.90.0.3000", ExportName = "_grTexMinAddress@4", IsStub = true)]
		public uint grTexMinAddress()
		{
			_logger.LogWarning("[GLIDE2x] grTexMinAddress called (stub)");
			// TODO: Implement _grTexMinAddress@4
			return 0; // DWORD default
		}

		[DllModuleExport(92, entryPoint: 0x00005C30, Version = "4.90.0.3000", ExportName = "_grTexMipMapMode@12", IsStub = true)]
		public uint grTexMipMapMode()
		{
			_logger.LogWarning("[GLIDE2x] grTexMipMapMode called (stub)");
			// TODO: Implement _grTexMipMapMode@12
			return 0; // DWORD default
		}

		[DllModuleExport(93, entryPoint: 0x00005C40, Version = "4.90.0.3000", ExportName = "_grTexMultibase@8", IsStub = true)]
		public uint grTexMultibase()
		{
			_logger.LogWarning("[GLIDE2x] grTexMultibase called (stub)");
			// TODO: Implement _grTexMultibase@8
			return 0; // DWORD default
		}

		[DllModuleExport(94, entryPoint: 0x00005C50, Version = "4.90.0.3000", ExportName = "_grTexMultibaseAddress@20", IsStub = true)]
		public uint grTexMultibaseAddress()
		{
			_logger.LogWarning("[GLIDE2x] grTexMultibaseAddress called (stub)");
			// TODO: Implement _grTexMultibaseAddress@20
			return 0; // DWORD default
		}

		[DllModuleExport(95, entryPoint: 0x00005CC0, Version = "4.90.0.3000", ExportName = "_grTexNCCTable@8", IsStub = true)]
		public uint grTexNCCTable()
		{
			_logger.LogWarning("[GLIDE2x] grTexNCCTable called (stub)");
			// TODO: Implement _grTexNCCTable@8
			return 0; // DWORD default
		}

		[DllModuleExport(96, entryPoint: 0x00005D50, Version = "4.90.0.3000", ExportName = "_grTexSource@16", IsStub = true)]
		public uint grTexSource()
		{
			_logger.LogWarning("[GLIDE2x] grTexSource called (stub)");
			// TODO: Implement _grTexSource@16
			return 0; // DWORD default
		}

		[DllModuleExport(97, entryPoint: 0x00005E70, Version = "4.90.0.3000", ExportName = "_grTexTextureMemRequired@8", IsStub = true)]
		public uint grTexTextureMemRequired()
		{
			_logger.LogWarning("[GLIDE2x] grTexTextureMemRequired called (stub)");
			// TODO: Implement _grTexTextureMemRequired@8
			return 0; // DWORD default
		}

		[DllModuleExport(98, entryPoint: 0x00003E90, Version = "4.90.0.3000", ExportName = "_grTriStats@8", IsStub = true)]
		public uint grTriStats()
		{
			_logger.LogWarning("[GLIDE2x] grTriStats called (stub)");
			// TODO: Implement _grTriStats@8
			return 0; // DWORD default
		}

		[DllModuleExport(99, entryPoint: 0x00005F60, Version = "4.90.0.3000", ExportName = "_gu3dfGetInfo@8", IsStub = true)]
		public uint gu3dfGetInfo()
		{
			_logger.LogWarning("[GLIDE2x] gu3dfGetInfo called (stub)");
			// TODO: Implement _gu3dfGetInfo@8
			return 0; // DWORD default
		}

		[DllModuleExport(100, entryPoint: 0x00005FA0, Version = "4.90.0.3000", ExportName = "_gu3dfLoad@8", IsStub = true)]
		public uint gu3dfLoad()
		{
			_logger.LogWarning("[GLIDE2x] gu3dfLoad called (stub)");
			// TODO: Implement _gu3dfLoad@8
			return 0; // DWORD default
		}

		[DllModuleExport(101, entryPoint: 0x00001F30, Version = "4.90.0.3000", ExportName = "_guAADrawTriangleWithClip@12", IsStub = true)]
		public uint guAADrawTriangleWithClip()
		{
			_logger.LogWarning("[GLIDE2x] guAADrawTriangleWithClip called (stub)");
			// TODO: Implement _guAADrawTriangleWithClip@12
			return 0; // DWORD default
		}

		[DllModuleExport(102, entryPoint: 0x00006010, Version = "4.90.0.3000", ExportName = "_guAlphaSource@4", IsStub = true)]
		public uint guAlphaSource()
		{
			_logger.LogWarning("[GLIDE2x] guAlphaSource called (stub)");
			// TODO: Implement _guAlphaSource@4
			return 0; // DWORD default
		}

		[DllModuleExport(103, entryPoint: 0x00006080, Version = "4.90.0.3000", ExportName = "_guColorCombineFunction@4", IsStub = true)]
		public uint guColorCombineFunction()
		{
			_logger.LogWarning("[GLIDE2x] guColorCombineFunction called (stub)");
			// TODO: Implement _guColorCombineFunction@4
			return 0; // DWORD default
		}

		[DllModuleExport(104, entryPoint: 0x00002460, Version = "4.90.0.3000", ExportName = "_guDrawPolygonVertexListWithClip@8", IsStub = true)]
		public uint guDrawPolygonVertexListWithClip()
		{
			_logger.LogWarning("[GLIDE2x] guDrawPolygonVertexListWithClip called (stub)");
			// TODO: Implement _guDrawPolygonVertexListWithClip@8
			return 0; // DWORD default
		}

		[DllModuleExport(105, entryPoint: 0x00001500, Version = "4.90.0.3000", ExportName = "_guDrawTriangleWithClip@12", IsStub = true)]
		public uint guDrawTriangleWithClip()
		{
			_logger.LogWarning("[GLIDE2x] guDrawTriangleWithClip called (stub)");
			// TODO: Implement _guDrawTriangleWithClip@12
			return 0; // DWORD default
		}

		[DllModuleExport(106, entryPoint: 0x00006400, Version = "4.90.0.3000", ExportName = "_guEncodeRLE16@16", IsStub = true)]
		public uint guEncodeRLE16()
		{
			_logger.LogWarning("[GLIDE2x] guEncodeRLE16 called (stub)");
			// TODO: Implement _guEncodeRLE16@16
			return 0; // DWORD default
		}

		[DllModuleExport(107, entryPoint: 0x00006500, Version = "4.90.0.3000", ExportName = "_guEndianSwapBytes@4", IsStub = true)]
		public uint guEndianSwapBytes()
		{
			_logger.LogWarning("[GLIDE2x] guEndianSwapBytes called (stub)");
			// TODO: Implement _guEndianSwapBytes@4
			return 0; // DWORD default
		}

		[DllModuleExport(108, entryPoint: 0x000064E0, Version = "4.90.0.3000", ExportName = "_guEndianSwapWords@4", IsStub = true)]
		public uint guEndianSwapWords()
		{
			_logger.LogWarning("[GLIDE2x] guEndianSwapWords called (stub)");
			// TODO: Implement _guEndianSwapWords@4
			return 0; // DWORD default
		}

		[DllModuleExport(109, entryPoint: 0x00003160, Version = "4.90.0.3000", ExportName = "_guFogGenerateExp2@8", IsStub = true)]
		public uint guFogGenerateExp2()
		{
			_logger.LogWarning("[GLIDE2x] guFogGenerateExp2 called (stub)");
			// TODO: Implement _guFogGenerateExp2@8
			return 0; // DWORD default
		}

		[DllModuleExport(110, entryPoint: 0x00003150, Version = "4.90.0.3000", ExportName = "_guFogGenerateExp@8", IsStub = true)]
		public uint guFogGenerateExp()
		{
			_logger.LogWarning("[GLIDE2x] guFogGenerateExp called (stub)");
			// TODO: Implement _guFogGenerateExp@8
			return 0; // DWORD default
		}

		[DllModuleExport(111, entryPoint: 0x00003170, Version = "4.90.0.3000", ExportName = "_guFogGenerateLinear@12", IsStub = true)]
		public uint guFogGenerateLinear()
		{
			_logger.LogWarning("[GLIDE2x] guFogGenerateLinear called (stub)");
			// TODO: Implement _guFogGenerateLinear@12
			return 0; // DWORD default
		}

		[DllModuleExport(112, entryPoint: 0x00003140, Version = "4.90.0.3000", ExportName = "_guFogTableIndexToW@4", IsStub = true)]
		public uint guFogTableIndexToW()
		{
			_logger.LogWarning("[GLIDE2x] guFogTableIndexToW called (stub)");
			// TODO: Implement _guFogTableIndexToW@4
			return 0; // DWORD default
		}

		[DllModuleExport(113, entryPoint: 0x00003350, Version = "4.90.0.3000", ExportName = "_guTexAllocateMemory@60", IsStub = true)]
		public uint guTexAllocateMemory()
		{
			_logger.LogWarning("[GLIDE2x] guTexAllocateMemory called (stub)");
			// TODO: Implement _guTexAllocateMemory@60
			return 0; // DWORD default
		}

		[DllModuleExport(114, entryPoint: 0x000034E0, Version = "4.90.0.3000", ExportName = "_guTexChangeAttributes@48", IsStub = true)]
		public uint guTexChangeAttributes()
		{
			_logger.LogWarning("[GLIDE2x] guTexChangeAttributes called (stub)");
			// TODO: Implement _guTexChangeAttributes@48
			return 0; // DWORD default
		}

		[DllModuleExport(115, entryPoint: 0x000061D0, Version = "4.90.0.3000", ExportName = "_guTexCombineFunction@8", IsStub = true)]
		public uint guTexCombineFunction()
		{
			_logger.LogWarning("[GLIDE2x] guTexCombineFunction called (stub)");
			// TODO: Implement _guTexCombineFunction@8
			return 0; // DWORD default
		}

		[DllModuleExport(116, entryPoint: 0x00006340, Version = "4.90.0.3000", ExportName = "_guTexCreateColorMipMap@0", IsStub = true)]
		public uint guTexCreateColorMipMap()
		{
			_logger.LogWarning("[GLIDE2x] guTexCreateColorMipMap called (stub)");
			// TODO: Implement _guTexCreateColorMipMap@0
			return 0; // DWORD default
		}

		[DllModuleExport(117, entryPoint: 0x000035A0, Version = "4.90.0.3000", ExportName = "_guTexDownloadMipMap@12", IsStub = true)]
		public uint guTexDownloadMipMap()
		{
			_logger.LogWarning("[GLIDE2x] guTexDownloadMipMap called (stub)");
			// TODO: Implement _guTexDownloadMipMap@12
			return 0; // DWORD default
		}

		[DllModuleExport(118, entryPoint: 0x00003630, Version = "4.90.0.3000", ExportName = "_guTexDownloadMipMapLevel@12", IsStub = true)]
		public uint guTexDownloadMipMapLevel()
		{
			_logger.LogWarning("[GLIDE2x] guTexDownloadMipMapLevel called (stub)");
			// TODO: Implement _guTexDownloadMipMapLevel@12
			return 0; // DWORD default
		}

		[DllModuleExport(119, entryPoint: 0x000036A0, Version = "4.90.0.3000", ExportName = "_guTexGetCurrentMipMap@4", IsStub = true)]
		public uint guTexGetCurrentMipMap()
		{
			_logger.LogWarning("[GLIDE2x] guTexGetCurrentMipMap called (stub)");
			// TODO: Implement _guTexGetCurrentMipMap@4
			return 0; // DWORD default
		}

		[DllModuleExport(120, entryPoint: 0x000036B0, Version = "4.90.0.3000", ExportName = "_guTexGetMipMapInfo@4", IsStub = true)]
		public uint guTexGetMipMapInfo()
		{
			_logger.LogWarning("[GLIDE2x] guTexGetMipMapInfo called (stub)");
			// TODO: Implement _guTexGetMipMapInfo@4
			return 0; // DWORD default
		}

		[DllModuleExport(121, entryPoint: 0x000036D0, Version = "4.90.0.3000", ExportName = "_guTexMemQueryAvail@4", IsStub = true)]
		public uint guTexMemQueryAvail()
		{
			_logger.LogWarning("[GLIDE2x] guTexMemQueryAvail called (stub)");
			// TODO: Implement _guTexMemQueryAvail@4
			return 0; // DWORD default
		}

		[DllModuleExport(122, entryPoint: 0x000036F0, Version = "4.90.0.3000", ExportName = "_guTexMemReset@0", IsStub = true)]
		public uint guTexMemReset()
		{
			_logger.LogWarning("[GLIDE2x] guTexMemReset called (stub)");
			// TODO: Implement _guTexMemReset@0
			return 0; // DWORD default
		}

		[DllModuleExport(123, entryPoint: 0x00003770, Version = "4.90.0.3000", ExportName = "_guTexSource@4", IsStub = true)]
		public uint guTexSource()
		{
			_logger.LogWarning("[GLIDE2x] guTexSource called (stub)");
			// TODO: Implement _guTexSource@4
			return 0; // DWORD default
		}
	}
}