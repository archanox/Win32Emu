using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.Modules
{
	/// <summary>
	/// TMU (Texture Mapping Unit) vertex data
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GrTmuVertex
	{
		public float sow;  // s texture coordinate (s over w)
		public float tow;  // t texture coordinate (t over w)
		public float oow;  // 1/w for mipmapping
	}
	
	/// <summary>
	/// Glide vertex structure matching the Win32 GrVertex layout
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GrVertex
	{
		public float x, y, z;
		public float r, g, b;
		public float ooz;
		public float a;
		public float oow;
		public GrTmuVertex tmu0;  // TMU 0 texture coordinates
		public GrTmuVertex tmu1;  // TMU 1 texture coordinates (if available)
	}
	
	/// <summary>
	/// Triangle data for batch rendering
	/// </summary>
	public struct Triangle
	{
		public GrVertex v0, v1, v2;
		public uint TextureId;
		public bool HasTexture;
	}
	
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
		
		// Rendering state
		private uint _constantColorValue;
		private uint _fogColorValue;
		private uint _chromakeyValue;
		private bool _depthMaskEnabled = true;
		private bool _ditherEnabled;
		private bool _alphaTestEnabled;
		private byte _alphaTestReference;
		
		// Texture memory tracking
		private uint _nextTextureAddress = 0x1000;
		private const uint TextureMemorySize = 4 * 1024 * 1024; // 4MB texture memory
		
		// Current texture state
		private uint _currentTextureTMU0;
		private uint _currentTextureTMU1;
		
		// Chromakey state
		private bool _chromakeyModeEnabled;
		
		// Cull mode
		private uint _cullMode;
		
		// Render buffer selection
		private uint _renderBuffer;
		
		// Depth buffer state
		private uint _depthBufferFunction;
		private uint _depthBufferMode;
		
		// Triangle batch rendering
		private readonly List<Triangle> _triangleBatch = new();
		private const int MaxBatchSize = 1000;
		
		// Hardware acceleration mode (use GPU rendering instead of CPU rasterization)
		private bool _useHardwareAcceleration = true;
		
		// Memory reference for drawing operations (set during TryInvokeUnsafe)
		private VirtualMemory? _currentMemory;
		
		// Vertex reading helper
		private GrVertex ReadVertex(VirtualMemory memory, uint address)
		{
			Span<byte> data = stackalloc byte[60]; // 9 floats + 2 TMUs * 3 floats = 15 floats * 4 bytes
			memory.ReadBytes(address, data);
			var vertex = new GrVertex
			{
				x = BitConverter.ToSingle(data.Slice(0, 4)),
				y = BitConverter.ToSingle(data.Slice(4, 4)),
				z = BitConverter.ToSingle(data.Slice(8, 4)),
				r = BitConverter.ToSingle(data.Slice(12, 4)),
				g = BitConverter.ToSingle(data.Slice(16, 4)),
				b = BitConverter.ToSingle(data.Slice(20, 4)),
				ooz = BitConverter.ToSingle(data.Slice(24, 4)),
				a = BitConverter.ToSingle(data.Slice(28, 4)),
				oow = BitConverter.ToSingle(data.Slice(32, 4)),
				tmu0 = new GrTmuVertex
				{
					sow = BitConverter.ToSingle(data.Slice(36, 4)),
					tow = BitConverter.ToSingle(data.Slice(40, 4)),
					oow = BitConverter.ToSingle(data.Slice(44, 4))
				},
				tmu1 = new GrTmuVertex
				{
					sow = BitConverter.ToSingle(data.Slice(48, 4)),
					tow = BitConverter.ToSingle(data.Slice(52, 4)),
					oow = BitConverter.ToSingle(data.Slice(56, 4))
				}
			};
			return vertex;
		}

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
			_currentMemory = memory; // Store for use in drawing functions
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
					{
						uint which = a.UInt32(0);
						returnValue = grSstSelect(which);
						return true;
					}

				case "_GRSSTQUERYHARDWARE@4":
					{
						uint hwConfigPtr = a.UInt32(0);
						returnValue = grSstQueryHardware(hwConfigPtr); // Return TRUE to indicate hardware is present
						return true;
					}

				case "_GRSSTWINOPEN@28":
					{
						uint hwnd = a.UInt32(0);
						uint resolution = a.UInt32(1);
						uint refresh = a.UInt32(2);
						uint colorFormat = a.UInt32(3);
						uint origin = a.UInt32(4);
						uint nColBuffers = a.UInt32(5);
						uint nAuxBuffers = a.UInt32(6);
						returnValue = grSstWinOpen(hwnd, resolution, refresh, colorFormat, origin, nColBuffers, nAuxBuffers); // Return TRUE for success
						return true;
					}

				case "_GRSSTWINCLOSE@0":
					returnValue = grSstWinClose();
					return true;

				case "_GRSSTIDLE@0":
					returnValue = grSstIdle();
					return true;

				case "_GRSSTVRETRACEON@0":
					returnValue = grSstVRetraceOn(); // Return TRUE
					return true;

				// Buffer management
				case "_GRBUFFERSWAP@4":
					{
						uint interval = a.UInt32(0);
						returnValue = grBufferSwap(interval);
						return true;
					}

				case "_GRBUFFERCLEAR@12":
					{
						uint color = a.UInt32(0);
						uint alpha = a.UInt32(1);
						uint depth = a.UInt32(2);
						_logger.LogInformation("[Glide2x] grBufferClear(color=0x{Color:X8}, alpha=0x{Alpha:X8}, depth=0x{Depth:X8})", color, alpha, depth);
						returnValue = grBufferClear(color, alpha, depth);
						return true;
					}

				case "_GRRENDERBUFFER@4":
					_logger.LogInformation("[Glide2x] grRenderBuffer({UInt32})", a.UInt32(0));
					returnValue = grRenderBuffer(a.UInt32(0));
					return true;

				// Linear frame buffer
				case "_GRLFBLOCK@24":
					{
						uint type = a.UInt32(0);
						uint buffer = a.UInt32(1);
						uint writeMode = a.UInt32(2);
						uint origin = a.UInt32(3);
						uint pixelPipeline = a.UInt32(4);
						uint infoPtr = a.UInt32(5);
						_logger.LogInformation("[Glide2x] grLfbLock(type={Type}, buffer={Buffer}, writeMode={WriteMode}, origin={Origin}, pixelPipeline={PixelPipeline}, infoPtr=0x{InfoPtr:X8})", 
							type, buffer, writeMode, origin, pixelPipeline, infoPtr);
						returnValue = grLfbLock(type, buffer, writeMode, origin, pixelPipeline, infoPtr); // Return TRUE for success
						return true;
					}

				case "_GRLFBUNLOCK@8":
					{
						uint buffer = a.UInt32(0);
						uint type = a.UInt32(1);
						_logger.LogInformation("[Glide2x] grLfbUnlock(buffer={Buffer}, type={Type})", buffer, type);
						returnValue = grLfbUnlock(buffer, type);
						return true;
					}

				// Texture management
				case "_GUTEXMEMRESET@0":
					_logger.LogInformation("[Glide2x] guTexMemReset()");
					returnValue = guTexMemReset();
					return true;

				case "_GUTEXALLOCATEMEMORY@60":
					{
						uint tmu = a.UInt32(0);
						uint evenOdd = a.UInt32(1);
						uint width = a.UInt32(2);
						uint height = a.UInt32(3);
						uint format = a.UInt32(4);
						uint mipMapMode = a.UInt32(5);
						uint lodMin = a.UInt32(6);
						uint lodMax = a.UInt32(7);
						uint aspect = a.UInt32(8);
						uint smallLodLog2 = a.UInt32(9);
						uint largeLodLog2 = a.UInt32(10);
						uint oddEvenPtr = a.UInt32(11);
						uint mipmapPtr = a.UInt32(12);
						uint startPtr = a.UInt32(13);
						uint endPtr = a.UInt32(14);
						_logger.LogInformation("[Glide2x] guTexAllocateMemory(tmu={Tmu}, evenOdd={EvenOdd}, width={Width}, height={Height}, format={Format}, ...)", 
							tmu, evenOdd, width, height, format);
						returnValue = guTexAllocateMemory(tmu, evenOdd, width, height, format, mipMapMode, lodMin, lodMax, aspect, smallLodLog2, largeLodLog2, oddEvenPtr, mipmapPtr, startPtr, endPtr); // Return a dummy texture memory address
						return true;
					}

				case "_GUTEXDOWNLOADMIPMAP@12":
					{
						uint mmid = a.UInt32(0);
						uint srcPtr = a.UInt32(1);
						uint nccPtr = a.UInt32(2);
						_logger.LogInformation("[Glide2x] guTexDownloadMipMap(mmid=0x{Mmid:X8}, srcPtr=0x{SrcPtr:X8}, nccPtr=0x{NccPtr:X8})", mmid, srcPtr, nccPtr);
						returnValue = guTexDownloadMipMap(mmid, srcPtr, nccPtr);
						return true;
					}

				case "_GRTEXDOWNLOADTABLE@12":
					{
						uint tmu = a.UInt32(0);
						uint type = a.UInt32(1);
						uint dataPtr = a.UInt32(2);
						_logger.LogInformation("[Glide2x] grTexDownloadTable(tmu={Tmu}, type={Type}, dataPtr=0x{DataPtr:X8})", tmu, type, dataPtr);
						returnValue = grTexDownloadTable(tmu, type, dataPtr);
						return true;
					}

				// State management
				case "_GRGLIDEGETSTATE@4":
					{
						uint statePtr = a.UInt32(0);
						_logger.LogInformation("[Glide2x] grGlideGetState(statePtr=0x{StatePtr:X8})", statePtr);
						returnValue = grGlideGetState(statePtr);
						return true;
					}

				case "_GRGLIDESETSTATE@4":
					{
						uint statePtr = a.UInt32(0);
						_logger.LogInformation("[Glide2x] grGlideSetState(statePtr=0x{StatePtr:X8})", statePtr);
						returnValue = grGlideSetState(statePtr);
						return true;
					}

				// Rendering modes
				case "_GRALPHABLENDFUNCTION@16": // _grAlphaBlendFunction@16
					{
						uint rgbSourceFactor = a.UInt32(0);
						uint rgbDestFactor = a.UInt32(1);
						uint alphaSourceFactor = a.UInt32(2);
						uint alphaDestFactor = a.UInt32(3);
						_logger.LogInformation("[Glide2x] grAlphaBlendFunction(rgbSF={RgbSF}, rgbDF={RgbDF}, alphaSF={AlphaSF}, alphaDF={AlphaDF})", 
							rgbSourceFactor, rgbDestFactor, alphaSourceFactor, alphaDestFactor);
						returnValue = grAlphaBlendFunction(rgbSourceFactor, rgbDestFactor, alphaSourceFactor, alphaDestFactor);
						return true;
					}

				case "_GRDEPTHBUFFERFUNCTION@4":
					_logger.LogInformation("[Glide2x] grDepthBufferFunction({UInt32})", a.UInt32(0));
					returnValue = grDepthBufferFunction(a.UInt32(0));
					return true;

				case "_GRDEPTHMASK@4":
					_logger.LogInformation("[Glide2x] grDepthMask({UInt32})", a.UInt32(0));
					returnValue = grDepthMask(a.UInt32(0));
					return true;

				case "_GRDEPTHBUFFERMODE@4":
					_logger.LogInformation("[Glide2x] grDepthBufferMode({UInt32})", a.UInt32(0));
					returnValue = grDepthBufferMode(a.UInt32(0));
					return true;

				case "_GRCHROMAKEYVALUE@4":
					_logger.LogInformation("[Glide2x] grChromakeyValue(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grChromakeyValue(a.UInt32(0));
					return true;

				case "_GRCHROMAKEYMODE@4":
					_logger.LogInformation("[Glide2x] grChromakeyMode({UInt32})", a.UInt32(0));
					returnValue = grChromakeyMode(a.UInt32(0));
					return true;

				case "_GRCULLMODE@4":
					_logger.LogInformation("[Glide2x] grCullMode({UInt32})", a.UInt32(0));
					returnValue = grCullMode(a.UInt32(0));
					return true;

				case "_GRCLIPWINDOW@16":
					{
						uint minx = a.UInt32(0);
						uint miny = a.UInt32(1);
						uint maxx = a.UInt32(2);
						uint maxy = a.UInt32(3);
						_logger.LogInformation("[Glide2x] grClipWindow(minx={Minx}, miny={Miny}, maxx={Maxx}, maxy={Maxy})", minx, miny, maxx, maxy);
						returnValue = grClipWindow(minx, miny, maxx, maxy);
						return true;
					}

				case "_GRCONSTANTCOLORVALUE@4":
					_logger.LogInformation("[Glide2x] grConstantColorValue(0x{UInt32:X8})", a.UInt32(0));
					returnValue = grConstantColorValue(a.UInt32(0));
					return true;

				// GU helper functions
				case "_GUALPHASOURCE@4":
					{
						uint mode = a.UInt32(0);
						_logger.LogInformation("[Glide2x] guAlphaSource(mode={Mode})", mode);
						returnValue = guAlphaSource(mode);
						return true;
					}

				case "_GUCOLORCOMBINEFUNCTION@4":
					{
						uint mode = a.UInt32(0);
						_logger.LogInformation("[Glide2x] guColorCombineFunction(mode={Mode})", mode);
						returnValue = guColorCombineFunction(mode);
						return true;
					}

				case "_GUTEXCOMBINEFUNCTION@8":
					{
						uint tmu = a.UInt32(0);
						uint mode = a.UInt32(1);
						_logger.LogInformation("[Glide2x] guTexCombineFunction(tmu={Tmu}, mode={Mode})", tmu, mode);
						returnValue = guTexCombineFunction(tmu, mode);
						return true;
					}

				case "_GUTEXSOURCE@4":
					_logger.LogInformation("[Glide2x] guTexSource(0x{UInt32:X8})", a.UInt32(0));
					returnValue = guTexSource(a.UInt32(0));
					return true;

				// Drawing primitives
				case "_GRAADRAWLINE@8":
					{
						uint v1Ptr = a.UInt32(0);
						uint v2Ptr = a.UInt32(1);
						_logger.LogInformation("[Glide2x] grAADrawLine(v1Ptr=0x{V1Ptr:X8}, v2Ptr=0x{V2Ptr:X8})", v1Ptr, v2Ptr);
						returnValue = grAADrawLine(v1Ptr, v2Ptr);
						return true;
					}

				case "_GRAADRAWPOINT@4":
					{
						uint vertexPtr = a.UInt32(0);
						_logger.LogInformation("[Glide2x] grAADrawPoint(vertexPtr=0x{VertexPtr:X8})", vertexPtr);
						returnValue = grAADrawPoint(vertexPtr);
						return true;
					}

				case "_GUDRAWTRIANGLEWITHCLIP@12": // _guDrawTriangleWithClip@12
					{
						uint ptrA = a.UInt32(0);
						uint ptrB = a.UInt32(1);
						uint ptrC = a.UInt32(2);
						_logger.LogInformation("[Glide2x] guDrawTriangleWithClip(0x{PtrA:X8}, 0x{PtrB:X8}, 0x{PtrC:X8})", ptrA, ptrB, ptrC);
						returnValue = guDrawTriangleWithClip(ptrA, ptrB, ptrC);
						return true;
					}
				
				case "_GRDRAWTRIANGLE@12":
					{
						uint ptrA = a.UInt32(0);
						uint ptrB = a.UInt32(1);
						uint ptrC = a.UInt32(2);
						_logger.LogInformation("[Glide2x] grDrawTriangle(0x{PtrA:X8}, 0x{PtrB:X8}, 0x{PtrC:X8})", ptrA, ptrB, ptrC);
						returnValue = grDrawTriangle(ptrA, ptrB, ptrC);
						return true;
					}

				default:
					_logger.LogInformation("[Glide2x] Unimplemented export: {Export}", export);
					return false;
			}
		}


		[DllModuleExport(1, entryPoint: 0x00005ED0, Version = "4.90.0.3000", ExportName = "_ConvertAndDownloadRle@64", IsStub = true)]
		public uint ConvertAndDownloadRle(uint tmu, uint startAddress, uint thisLod, uint largeLod, uint aspectRatio, uint format, uint evenOdd, uint bmDataPtr, uint bmHeight, uint u0, uint v0, uint width, uint height, uint destWidth, uint destHeight, uint tlutPtr)
		{
			_logger.LogWarning("[GLIDE2x] ConvertAndDownloadRle(tmu={Tmu}, startAddress=0x{StartAddress:X8}, thisLod={ThisLod}, largeLod={LargeLod}, aspectRatio={AspectRatio}, format={Format}, evenOdd={EvenOdd}, bmDataPtr=0x{BmDataPtr:X8}, bmHeight={BmHeight}, u0={U0}, v0={V0}, width={Width}, height={Height}, destWidth={DestWidth}, destHeight={DestHeight}, tlutPtr=0x{TlutPtr:X8}) - stub", 
				tmu, startAddress, thisLod, largeLod, aspectRatio, format, evenOdd, bmDataPtr, bmHeight, u0, v0, width, height, destWidth, destHeight, tlutPtr);
			// TODO: Implement _ConvertAndDownloadRle@64
			return 0; // DWORD default
		}

		[DllModuleExport(2, entryPoint: 0x00002E70, Version = "4.90.0.3000", ExportName = "_grAADrawLine@8", IsStub = true)]
		public uint grAADrawLine(uint v1Ptr, uint v2Ptr)
		{
			_logger.LogWarning("[GLIDE2x] grAADrawLine called (stub)");
			// Draw an anti-aliased line
			// TODO: Implement line rasterization
			return 0; // Success (void function)
		}

		[DllModuleExport(3, entryPoint: 0x00002EA0, Version = "4.90.0.3000", ExportName = "_grAADrawPoint@4")]
		public uint grAADrawPoint(uint vertexPtr)
		{
			_logger.LogDebug("[GLIDE2x] grAADrawPoint called");
			// Draw an anti-aliased point
			// Parameters: pointer to GrVertex structure
			return 0; // Success (void function)
		}

		[DllModuleExport(4, entryPoint: 0x00002ED0, Version = "4.90.0.3000", ExportName = "_grAADrawPolygon@12")]
		public uint grAADrawPolygon(uint nverts, uint ilistPtr, uint vlistPtr)
		{
			_logger.LogDebug("[GLIDE2x] grAADrawPolygon called");
			// Draw an anti-aliased polygon
			// Parameters: nverts, ilist, vlist
			return 0; // Success (void function)
		}

		[DllModuleExport(5, entryPoint: 0x00002F00, Version = "4.90.0.3000", ExportName = "_grAADrawPolygonVertexList@8")]
		public uint grAADrawPolygonVertexList(uint nverts, uint vlistPtr)
		{
			_logger.LogDebug("[GLIDE2x] grAADrawPolygonVertexList called");
			// Draw an anti-aliased polygon from vertex list
			// Parameters: nverts, vlist
			return 0; // Success (void function)
		}

		[DllModuleExport(6, entryPoint: 0x00002F30, Version = "4.90.0.3000", ExportName = "_grAADrawTriangle@24")]
		public uint grAADrawTriangle(uint v1Ptr, uint v2Ptr, uint v3Ptr, uint aa01, uint aa12, uint aa20)
		{
			_logger.LogDebug("[GLIDE2x] grAADrawTriangle called");
			// Draw an anti-aliased triangle
			// Parameters: pointers to 3 GrVertex structures
			return 0; // Success (void function)
		}

		[DllModuleExport(7, entryPoint: 0x00002980, Version = "4.90.0.3000", ExportName = "_grAlphaBlendFunction@16")]
		public uint grAlphaBlendFunction(uint rgbSourceFactor, uint rgbDestFactor, uint alphaSourceFactor, uint alphaDestFactor)
		{
			_logger.LogDebug("[GLIDE2x] grAlphaBlendFunction called");
			// Set alpha blending function
			// Parameters: rgb_sf (source factor), rgb_df (dest factor), alpha_sf, alpha_df
			// For emulation, we just acknowledge the call
			return 0; // Success (void function)
		}

		[DllModuleExport(8, entryPoint: 0x00002990, Version = "4.90.0.3000", ExportName = "_grAlphaCombine@20")]
		public uint grAlphaCombine(uint function, uint factor, uint local, uint other, uint invert)
		{
			_logger.LogDebug("[GLIDE2x] grAlphaCombine called");
			// Set alpha combine function
			// Parameters: function, factor, local, other, invert
			return 0; // Success (void function)
		}

		[DllModuleExport(9, entryPoint: 0x000029A0, Version = "4.90.0.3000", ExportName = "_grAlphaControlsITRGBLighting@4")]
		public uint grAlphaControlsITRGBLighting(uint enable)
		{
			_logger.LogDebug("[GLIDE2x] grAlphaControlsITRGBLighting called");
			// Enable or disable alpha controlling iterated RGB lighting
			return 0; // Success (void function)
		}

		[DllModuleExport(10, entryPoint: 0x000029B0, Version = "4.90.0.3000", ExportName = "_grAlphaTestFunction@4")]
		public uint grAlphaTestFunction(uint function)
		{
			_logger.LogDebug("[GLIDE2x] grAlphaTestFunction called");
			// Set alpha test comparison function
			return 0; // Success (void function)
		}

		[DllModuleExport(11, entryPoint: 0x000029C0, Version = "4.90.0.3000", ExportName = "_grAlphaTestReferenceValue@4")]
		public uint grAlphaTestReferenceValue(uint value)
		{
			_logger.LogDebug("[GLIDE2x] grAlphaTestReferenceValue: value=0x{Value:X2}", value);
			_alphaTestReference = (byte)(value & 0xFF);
			return 0; // Success (void function)
		}

		[DllModuleExport(12, entryPoint: 0x00001230, Version = "4.90.0.3000", ExportName = "_grBufferClear@12")]
		public uint grBufferClear(uint color, uint alpha, uint depth)
		{
			_logger.LogDebug("[GLIDE2x] grBufferClear: color=0x{Color:X8}, alpha=0x{Alpha:X8}, depth=0x{Depth:X8}", color, alpha, depth);
			
			if (!_windowOpen || _frameBuffer == null)
			{
				_logger.LogWarning("[GLIDE2x] grBufferClear: Window not open or no frame buffer");
				return 0;
			}
			
			// Extract RGBA components from Glide color format
			// Glide uses ARGB format packed into 32-bit integer
			byte r = (byte)((color >> 16) & 0xFF);
			byte g = (byte)((color >> 8) & 0xFF);
			byte b = (byte)(color & 0xFF);
			byte a = (byte)((alpha >> 24) & 0xFF); // Alpha is typically in high byte
			
			// Fill frame buffer with the specified color
			for (int i = 0; i < _frameBuffer.Length; i += 4)
			{
				_frameBuffer[i + 0] = r;
				_frameBuffer[i + 1] = g;
				_frameBuffer[i + 2] = b;
				_frameBuffer[i + 3] = a;
			}
			
			_logger.LogDebug("[GLIDE2x] Buffer cleared to color R={R}, G={G}, B={B}, A={A}", r, g, b, a);
			return 0; // Success (void function)
		}

		[DllModuleExport(13, entryPoint: 0x00001390, Version = "4.90.0.3000", ExportName = "_grBufferNumPending@0")]
		public uint grBufferNumPending()
		{
			_logger.LogDebug("[GLIDE2x] grBufferNumPending called");
			// Return the number of pending buffer swaps
			// For emulation, we have no pending swaps
			return 0;
		}

		[DllModuleExport(14, entryPoint: 0x00001220, Version = "4.90.0.3000", ExportName = "_grBufferSwap@4")]
		public uint grBufferSwap(uint interval)
		{
			_logger.LogDebug("[GLIDE2x] grBufferSwap(interval={Interval})", interval);
			
			if (!_windowOpen || _renderingBackend == null)
			{
				_logger.LogWarning("[GLIDE2x] grBufferSwap: Window not open or no rendering backend");
				return 0;
			}
			
			// Flush any pending triangles before swapping
			FlushTriangleBatch();
			
			// End frame and present to screen
			if (_useHardwareAcceleration)
			{
				_renderingBackend.EndFrame();
			}
			else
			{
				// Software rasterization path - update frame buffer
				if (_frameBuffer != null)
				{
					_renderingBackend.UpdateFrameBuffer(_frameBuffer, _width * 4);
				}
			}
			
			_renderingBackend.ProcessEvents();
			
			// Begin next frame
			if (_useHardwareAcceleration)
			{
				_renderingBackend.BeginFrame();
			}
			
			_logger.LogDebug("[GLIDE2x] Buffer swapped successfully");
			return 0; // Success (void function)
		}

		[DllModuleExport(15, entryPoint: 0x00004090, Version = "4.90.0.3000", ExportName = "_grCheckForRoom@4", IsStub = true)]
		public uint grCheckForRoom(uint size)
		{
			_logger.LogWarning("[GLIDE2x] grCheckForRoom called (stub)");
			// TODO: Implement _grCheckForRoom@4
			return 0; // DWORD default
		}

		[DllModuleExport(16, entryPoint: 0x00003C80, Version = "4.90.0.3000", ExportName = "_grChromakeyMode@4")]
		public uint grChromakeyMode(uint mode)
		{
			_logger.LogDebug("[GLIDE2x] grChromakeyMode: mode={Mode}", mode);
			// Set chromakey mode (for transparency keying)
			// Mode: 0 = disabled, 1 = enabled
			_chromakeyModeEnabled = (mode != 0);
			return 0; // Success (void function)
		}

		[DllModuleExport(17, entryPoint: 0x00003C90, Version = "4.90.0.3000", ExportName = "_grChromakeyValue@4")]
		public uint grChromakeyValue(uint value)
		{
			_logger.LogDebug("[GLIDE2x] grChromakeyValue: value=0x{Value:X8}", value);
			_chromakeyValue = value;
			return 0; // Success (void function)
		}

		[DllModuleExport(18, entryPoint: 0x00003CA0, Version = "4.90.0.3000", ExportName = "_grClipWindow@16")]
		public uint grClipWindow(uint minx, uint miny, uint maxx, uint maxy)
		{
			_logger.LogDebug("[GLIDE2x] grClipWindow called");
			// Set clipping window rectangle (minx, miny, maxx, maxy)
			return 0; // Success (void function)
		}

		[DllModuleExport(19, entryPoint: 0x000029D0, Version = "4.90.0.3000", ExportName = "_grColorCombine@20")]
		public uint grColorCombine(uint function, uint factor, uint local, uint other, uint invert)
		{
			_logger.LogDebug("[GLIDE2x] grColorCombine called");
			// Set color combine function (how colors are blended)
			return 0; // Success (void function)
		}

		[DllModuleExport(20, entryPoint: 0x000029E0, Version = "4.90.0.3000", ExportName = "_grColorMask@8")]
		public uint grColorMask(uint rgb, uint alpha)
		{
			_logger.LogDebug("[GLIDE2x] grColorMask called");
			// Set color write mask (enable/disable writing to RGB and alpha channels)
			return 0; // Success (void function)
		}

		[DllModuleExport(21, entryPoint: 0x00002A00, Version = "4.90.0.3000", ExportName = "_grConstantColorValue4@16")]
		public uint grConstantColorValue4(uint a, uint r, uint g, uint b)
		{
			_logger.LogDebug("[GLIDE2x] grConstantColorValue4 called");
			// Set constant color value using 4 float components (a, r, g, b)
			return 0; // Success (void function)
		}

		[DllModuleExport(22, entryPoint: 0x000029F0, Version = "4.90.0.3000", ExportName = "_grConstantColorValue@4")]
		public uint grConstantColorValue(uint value)
		{
			_logger.LogDebug("[GLIDE2x] grConstantColorValue: value=0x{Value:X8}", value);
			_constantColorValue = value;
			return 0; // Success (void function)
		}

		[DllModuleExport(23, entryPoint: 0x00002E60, Version = "4.90.0.3000", ExportName = "_grCullMode@4")]
		public uint grCullMode(uint mode)
		{
			_logger.LogDebug("[GLIDE2x] grCullMode: mode={Mode}", mode);
			// Set polygon culling mode
			// 0 = GR_CULL_DISABLE, 1 = GR_CULL_NEGATIVE, 2 = GR_CULL_POSITIVE
			_cullMode = mode;
			return 0; // Success (void function)
		}

		[DllModuleExport(24, entryPoint: 0x000013B0, Version = "4.90.0.3000", ExportName = "_grDepthBiasLevel@4")]
		public uint grDepthBiasLevel(uint level)
		{
			_logger.LogDebug("[GLIDE2x] grDepthBiasLevel called");
			// Set depth bias level for polygon offset
			return 0; // Success (void function)
		}

		[DllModuleExport(25, entryPoint: 0x000013C0, Version = "4.90.0.3000", ExportName = "_grDepthBufferFunction@4")]
		public uint grDepthBufferFunction(uint function)
		{
			_logger.LogDebug("[GLIDE2x] grDepthBufferFunction: function={Function}", function);
			// Set depth buffer comparison function
			// 0 = never, 1 = less, 2 = equal, 3 = less or equal, 4 = greater, 5 = not equal, 6 = greater or equal, 7 = always
			_depthBufferFunction = function;
			return 0; // Success (void function)
		}

		[DllModuleExport(26, entryPoint: 0x000013D0, Version = "4.90.0.3000", ExportName = "_grDepthBufferMode@4")]
		public uint grDepthBufferMode(uint mode)
		{
			_logger.LogDebug("[GLIDE2x] grDepthBufferMode: mode={Mode}", mode);
			// Set depth buffer mode
			// 0 = disable, 1 = z-buffering, 2 = w-buffering
			_depthBufferMode = mode;
			return 0; // Success (void function)
		}

		[DllModuleExport(27, entryPoint: 0x00001400, Version = "4.90.0.3000", ExportName = "_grDepthMask@4")]
		public uint grDepthMask(uint mask)
		{
			_logger.LogDebug("[GLIDE2x] grDepthMask: mask={Mask}", mask != 0);
			_depthMaskEnabled = (mask != 0);
			return 0; // Success (void function)
		}

		[DllModuleExport(28, entryPoint: 0x00001410, Version = "4.90.0.3000", ExportName = "_grDisableAllEffects@0")]
		public uint grDisableAllEffects()
		{
			_logger.LogDebug("[GLIDE2x] grDisableAllEffects called");
			// Disable all special effects (fog, chromakey, alpha blend, etc.)
			return 0; // Success (void function)
		}

		[DllModuleExport(29, entryPoint: 0x00003D20, Version = "4.90.0.3000", ExportName = "_grDitherMode@4")]
		public uint grDitherMode(uint mode)
		{
			_logger.LogDebug("[GLIDE2x] grDitherMode called");
			// Set dithering mode for color quantization
			return 0; // Success (void function)
		}

		[DllModuleExport(30, entryPoint: 0x00002FE0, Version = "4.90.0.3000", ExportName = "_grDrawLine@8")]
		public uint grDrawLine(uint v1Ptr, uint v2Ptr)
		{
			_logger.LogDebug("[GLIDE2x] grDrawLine called");
			// Draw a line (non-antialiased)
			// Parameters: pointer to two GrVertex structures
			return 0; // Success (void function)
		}

		[DllModuleExport(31, entryPoint: 0x00002F40, Version = "4.90.0.3000", ExportName = "_grDrawPlanarPolygon@12")]
		public uint grDrawPlanarPolygon(uint nverts, uint ilistPtr, uint vlistPtr)
		{
			_logger.LogDebug("[GLIDE2x] grDrawPlanarPolygon called");
			// Draw a planar polygon
			// Parameters: nverts, ilist, vlist
			return 0; // Success (void function)
		}

		[DllModuleExport(32, entryPoint: 0x00002F50, Version = "4.90.0.3000", ExportName = "_grDrawPlanarPolygonVertexList@8")]
		public uint grDrawPlanarPolygonVertexList(uint nverts, uint vlistPtr)
		{
			_logger.LogDebug("[GLIDE2x] grDrawPlanarPolygonVertexList called");
			// Draw a planar polygon from vertex list
			// Parameters: nverts, vlist
			return 0; // Success (void function)
		}

		[DllModuleExport(33, entryPoint: 0x00002F70, Version = "4.90.0.3000", ExportName = "_grDrawPoint@4")]
		public uint grDrawPoint(uint vertexPtr)
		{
			_logger.LogDebug("[GLIDE2x] grDrawPoint called");
			// Draw a point (non-antialiased)
			// Parameters: pointer to GrVertex structure
			return 0; // Success (void function)
		}

		[DllModuleExport(34, entryPoint: 0x00002F80, Version = "4.90.0.3000", ExportName = "_grDrawPolygon@12")]
		public uint grDrawPolygon(uint nverts, uint ilistPtr, uint vlistPtr)
		{
			_logger.LogDebug("[GLIDE2x] grDrawPolygon called");
			// Draw a polygon
			// Parameters: nverts, ilist, vlist
			return 0; // Success (void function)
		}

		[DllModuleExport(35, entryPoint: 0x00002F50, Version = "4.90.0.3000", ExportName = "_grDrawPolygonVertexList@8")]
		public uint grDrawPolygonVertexList(uint nverts, uint vlistPtr)
		{
			_logger.LogDebug("[GLIDE2x] grDrawPolygonVertexList called");
			// Draw a polygon from vertex list
			// Parameters: nverts, vlist
			return 0; // Success (void function)
		}

		[DllModuleExport(36, entryPoint: 0x00002FF0, Version = "4.90.0.3000", ExportName = "_grDrawTriangle@12")]
		public uint grDrawTriangle(uint ptrA, uint ptrB, uint ptrC)
		{
			_logger.LogDebug("[GLIDE2x] grDrawTriangle: vertices at 0x{PtrA:X8}, 0x{PtrB:X8}, 0x{PtrC:X8}", ptrA, ptrB, ptrC);
			
			if (!_windowOpen || _frameBuffer == null || _currentMemory == null)
			{
				_logger.LogWarning("[GLIDE2x] grDrawTriangle: Window not open or memory not available");
				return 0;
			}
			
			try
			{
				// Read vertices from memory
				var v0 = ReadVertex(_currentMemory, ptrA);
				var v1 = ReadVertex(_currentMemory, ptrB);
				var v2 = ReadVertex(_currentMemory, ptrC);
				
				// Add to batch
				var triangle = new Triangle
				{
					v0 = v0,
					v1 = v1,
					v2 = v2,
					TextureId = _currentTextureTMU0,
					HasTexture = (_currentTextureTMU0 != 0)  // Has texture if a texture is bound
				};
				
				_triangleBatch.Add(triangle);
				
				// Flush if batch is full or rendering to front buffer
				if (_triangleBatch.Count >= MaxBatchSize || _renderBuffer == 0)
				{
					FlushTriangleBatch();
				}
			}
			catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
			{
				_logger.LogError(ex, "[GLIDE2x] Error in grDrawTriangle");
			}
			
			return 0; // Success (void function)
		}

		[DllModuleExport(37, entryPoint: 0x00003E70, Version = "4.90.0.3000", ExportName = "_grErrorSetCallback@4", IsStub = true)]
		public uint grErrorSetCallback(uint callback)
		{
			_logger.LogWarning("[GLIDE2x] grErrorSetCallback called (stub)");
			// TODO: Implement _grErrorSetCallback@4
			return 0; // DWORD default
		}

		[DllModuleExport(38, entryPoint: 0x00003100, Version = "4.90.0.3000", ExportName = "_grFogColorValue@4")]
		public uint grFogColorValue(uint fogcolor)
		{
			_logger.LogDebug("[GLIDE2x] grFogColorValue: color=0x{Color:X8}", fogcolor);
			_fogColorValue = fogcolor;
			return 0; // Success (void function)
		}

		[DllModuleExport(39, entryPoint: 0x00003110, Version = "4.90.0.3000", ExportName = "_grFogMode@4")]
		public uint grFogMode(uint mode)
		{
			_logger.LogDebug("[GLIDE2x] grFogMode called");
			// Set fog mode (disable, enable with or without alpha)
			return 0; // Success (void function)
		}

		[DllModuleExport(40, entryPoint: 0x00003130, Version = "4.90.0.3000", ExportName = "_grFogTable@4")]
		public uint grFogTable(uint tablePtr)
		{
			_logger.LogDebug("[GLIDE2x] grFogTable called");
			// Set fog lookup table (for distance-based fog calculations)
			return 0; // Success (void function)
		}

		[DllModuleExport(41, entryPoint: 0x00003D30, Version = "4.90.0.3000", ExportName = "_grGammaCorrectionValue@4")]
		public uint grGammaCorrectionValue(uint value)
		{
			_logger.LogDebug("[GLIDE2x] grGammaCorrectionValue called");
			// Set gamma correction value for display output
			return 0; // Success (void function)
		}

		[DllModuleExport(42, entryPoint: 0x00004290, Version = "4.90.0.3000", ExportName = "_grGetProcAddressExtXP@4", IsStub = true)]
		public uint grGetProcAddressExtXP(uint funcName)
		{
			_logger.LogWarning("[GLIDE2x] grGetProcAddressExtXP called (stub)");
			// TODO: Implement _grGetProcAddressExtXP@4
			return 0; // DWORD default
		}

		[DllModuleExport(43, entryPoint: 0x000032D0, Version = "4.90.0.3000", ExportName = "_grGlideGetState@4", IsStub = true)]
		public uint grGlideGetState(uint statePtr)
		{
			_logger.LogWarning("[GLIDE2x] grGlideGetState called (stub)");
			// TODO: Implement _grGlideGetState@4
			return 0; // DWORD default
		}

		[DllModuleExport(44, entryPoint: 0x00003290, Version = "4.90.0.3000", ExportName = "_grGlideGetVersion@4")]
		public uint grGlideGetVersion(uint versionPtr)
		{
			_logger.LogDebug("[GLIDE2x] grGlideGetVersion called");
			// Return Glide version string pointer - we'll write a version string to memory
			// Format: "2.60" for Glide 2.60
			// For simplicity, return a constant indicating success
			// The actual implementation would write version info to the provided buffer pointer
			return 0x02600000; // Version 2.60 in BCD format
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
		public uint grGlideSetState(uint statePtr)
		{
			_logger.LogWarning("[GLIDE2x] grGlideSetState called (stub)");
			// TODO: Implement _grGlideSetState@4
			return 0; // DWORD default
		}

		[DllModuleExport(47, entryPoint: 0x000032B0, Version = "4.90.0.3000", ExportName = "_grGlideShamelessPlug@4", IsStub = true)]
		public uint grGlideShamelessPlug(uint mode)
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
		public uint grHints(uint hintType, uint hintMask)
		{
			_logger.LogWarning("[GLIDE2x] grHints called (stub)");
			// TODO: Implement _grHints@8
			return 0; // DWORD default
		}

		[DllModuleExport(50, entryPoint: 0x00001420, Version = "4.90.0.3000", ExportName = "_grLfbConstantAlpha@4")]
		public uint grLfbConstantAlpha(uint alpha)
		{
			_logger.LogDebug("[GLIDE2x] grLfbConstantAlpha called");
			// Set constant alpha value for LFB writes
			return 0; // Success (void function)
		}

		[DllModuleExport(51, entryPoint: 0x00001430, Version = "4.90.0.3000", ExportName = "_grLfbConstantDepth@4")]
		public uint grLfbConstantDepth(uint depth)
		{
			_logger.LogDebug("[GLIDE2x] grLfbConstantDepth called");
			// Set constant depth value for LFB writes
			return 0; // Success (void function)
		}

		[DllModuleExport(52, entryPoint: 0x00001450, Version = "4.90.0.3000", ExportName = "_grLfbLock@24")]
		public uint grLfbLock(uint type, uint buffer, uint writeMode, uint origin, uint pixelPipeline, uint infoPtr)
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
			return _frameBufferAddress; // Return pointer to locked frame buffer
		}

		[DllModuleExport(53, entryPoint: 0x00001460, Version = "4.90.0.3000", ExportName = "_grLfbReadRegion@28")]
		public uint grLfbReadRegion(uint buffer, uint dstX, uint dstY, uint srcX, uint srcY, uint width, uint height)
		{
			_logger.LogDebug("[GLIDE2x] grLfbReadRegion called");
			// Read a region of the frame buffer to memory
			// Parameters: buffer, x, y, width, height, stride, data pointer
			return 1; // TRUE - success
		}

		[DllModuleExport(54, entryPoint: 0x00001470, Version = "4.90.0.3000", ExportName = "_grLfbUnlock@8")]
		public uint grLfbUnlock(uint buffer, uint type)
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

		[DllModuleExport(55, entryPoint: 0x00004090, Version = "4.90.0.3000", ExportName = "_grLfbWriteColorFormat@4")]
		public uint grLfbWriteColorFormat(uint format)
		{
			_logger.LogDebug("[GLIDE2x] grLfbWriteColorFormat called");
			// Set color format for LFB writes
			return 0; // Success (void function)
		}

		[DllModuleExport(56, entryPoint: 0x000014F0, Version = "4.90.0.3000", ExportName = "_grLfbWriteColorSwizzle@8")]
		public uint grLfbWriteColorSwizzle(uint swizzleBytes, uint swapWords)
		{
			_logger.LogDebug("[GLIDE2x] grLfbWriteColorSwizzle called");
			// Set color channel swizzling for LFB writes
			return 0; // Success (void function)
		}

		[DllModuleExport(57, entryPoint: 0x00001480, Version = "4.90.0.3000", ExportName = "_grLfbWriteRegion@32")]
		public uint grLfbWriteRegion(uint buffer, uint dstX, uint dstY, uint srcFormat, uint srcWidth, uint srcHeight, uint srcStride, uint srcPtr)
		{
			_logger.LogDebug("[GLIDE2x] grLfbWriteRegion called");
			// Write a region from memory to the frame buffer
			// Parameters: buffer, x, y, format, width, height, reverse, stride, data pointer
			return 1; // TRUE - success
		}

		[DllModuleExport(58, entryPoint: 0x000014C0, Version = "4.90.0.3000", ExportName = "_grRenderBuffer@4")]
		public uint grRenderBuffer(uint buffer)
		{
			_logger.LogDebug("[GLIDE2x] grRenderBuffer: buffer={Buffer}", buffer);
			// Select which buffer to render to
			// 0 = GR_BUFFER_FRONTBUFFER, 1 = GR_BUFFER_BACKBUFFER
			_renderBuffer = buffer;
			return 0; // Success (void function)
		}

		[DllModuleExport(59, entryPoint: 0x00003E60, Version = "4.90.0.3000", ExportName = "_grResetTriStats@0", IsStub = true)]
		public uint grResetTriStats()
		{
			_logger.LogWarning("[GLIDE2x] grResetTriStats called (stub)");
			// TODO: Implement _grResetTriStats@0
			return 0; // DWORD default
		}

		[DllModuleExport(60, entryPoint: 0x00003E80, Version = "4.90.0.3000", ExportName = "_grSplash@20", IsStub = true)]
		public uint grSplash(uint x, uint y, uint width, uint height, uint frame)
		{
			_logger.LogWarning("[GLIDE2x] grSplash called (stub)");
			// TODO: Implement _grSplash@20
			return 0; // DWORD default
		}

		[DllModuleExport(61, entryPoint: 0x00005260, Version = "4.90.0.3000", ExportName = "_grSstControl@4", IsStub = true)]
		public uint grSstControl(uint code)
		{
			_logger.LogWarning("[GLIDE2x] grSstControl called (stub)");
			// TODO: Implement _grSstControl@4
			return 0; // DWORD default
		}

		[DllModuleExport(62, entryPoint: 0x00005290, Version = "4.90.0.3000", ExportName = "_grSstControlMode@4", IsStub = true)]
		public uint grSstControlMode(uint mode)
		{
			_logger.LogWarning("[GLIDE2x] grSstControlMode called (stub)");
			// TODO: Implement _grSstControlMode@4
			return 0; // DWORD default
		}

		[DllModuleExport(63, entryPoint: 0x000052A0, Version = "4.90.0.3000", ExportName = "_grSstIdle@0")]
		public uint grSstIdle()
		{
			_logger.LogDebug("[GLIDE2x] grSstIdle called");
			// Wait for the graphics engine to go idle
			// For emulation, we're always idle, so just return
			return 0; // Success (void function)
		}

		[DllModuleExport(64, entryPoint: 0x000052B0, Version = "4.90.0.3000", ExportName = "_grSstIsBusy@0")]
		public uint grSstIsBusy()
		{
			_logger.LogDebug("[GLIDE2x] grSstIsBusy called");
			// Check if the graphics engine is busy
			// For emulation, we're always idle, so return FALSE
			return 0; // FALSE - not busy
		}

		[DllModuleExport(65, entryPoint: 0x00004E90, Version = "4.90.0.3000", ExportName = "_grSstOrigin@4", IsStub = true)]
		public uint grSstOrigin(uint origin)
		{
			_logger.LogWarning("[GLIDE2x] grSstOrigin called (stub)");
			// TODO: Implement _grSstOrigin@4
			return 0; // DWORD default
		}

		[DllModuleExport(66, entryPoint: 0x000052D0, Version = "4.90.0.3000", ExportName = "_grSstPerfStats@4", IsStub = true)]
		public uint grSstPerfStats(uint statsPtr)
		{
			_logger.LogWarning("[GLIDE2x] grSstPerfStats called (stub)");
			// TODO: Implement _grSstPerfStats@4
			return 0; // DWORD default
		}

		[DllModuleExport(67, entryPoint: 0x00004EA0, Version = "4.90.0.3000", ExportName = "_grSstQueryBoards@4")]
		public uint grSstQueryBoards(uint hwConfigPtr)
		{
			_logger.LogDebug("[GLIDE2x] grSstQueryBoards called");
			// Return TRUE to indicate one 3Dfx board is present
			// The actual implementation would fill a GrHwConfiguration structure
			// with information about the available 3Dfx hardware
			return 1; // TRUE - one board available
		}

		[DllModuleExport(68, entryPoint: 0x00004EF0, Version = "4.90.0.3000", ExportName = "_grSstQueryHardware@4")]
		public uint grSstQueryHardware(uint hwConfigPtr)
		{
			_logger.LogDebug("[GLIDE2x] grSstQueryHardware(hwConfigPtr=0x{HwConfigPtr:X8})", hwConfigPtr);
			// Return TRUE to indicate hardware is present
			// The actual implementation would fill a GrHwConfiguration structure
			return 1; // TRUE - hardware available
		}

		[DllModuleExport(69, entryPoint: 0x00005320, Version = "4.90.0.3000", ExportName = "_grSstResetPerfStats@0", IsStub = true)]
		public uint grSstResetPerfStats()
		{
			_logger.LogWarning("[GLIDE2x] grSstResetPerfStats called (stub)");
			// TODO: Implement _grSstResetPerfStats@0
			return 0; // DWORD default
		}

		[DllModuleExport(70, entryPoint: 0x00005330, Version = "4.90.0.3000", ExportName = "_grSstScreenHeight@0")]
		public uint grSstScreenHeight()
		{
			_logger.LogDebug("[GLIDE2x] grSstScreenHeight called, returning {Height}", _height);
			return (uint)_height;
		}

		[DllModuleExport(71, entryPoint: 0x00005350, Version = "4.90.0.3000", ExportName = "_grSstScreenWidth@0")]
		public uint grSstScreenWidth()
		{
			_logger.LogDebug("[GLIDE2x] grSstScreenWidth called, returning {Width}", _width);
			return (uint)_width;
		}

		[DllModuleExport(72, entryPoint: 0x00005030, Version = "4.90.0.3000", ExportName = "_grSstSelect@4")]
		public uint grSstSelect(uint which)
		{
			_logger.LogDebug("[GLIDE2x] grSstSelect(which={Which})", which);
			// Select which SST (Scan-line Synchronizer/Transformer) to use
			// Since we're emulating, we only support one virtual SST
			return 0; // Success (void function)
		}

		[DllModuleExport(73, entryPoint: 0x00005370, Version = "4.90.0.3000", ExportName = "_grSstStatus@0")]
		public uint grSstStatus()
		{
			_logger.LogDebug("[GLIDE2x] grSstStatus called");
			// Return status bits:
			// Bit 0: VRetrace active (set when in vertical retrace)
			// Bit 1-5: Number of pending buffer swaps
			// Bit 6: FBI graphics engine busy
			// Bit 7-31: Reserved
			// We return 0 to indicate idle state (not in VRetrace, no pending swaps, not busy)
			return 0;
		}

		[DllModuleExport(74, entryPoint: 0x000053C0, Version = "4.90.0.3000", ExportName = "_grSstVRetraceOn@0")]
		public uint grSstVRetraceOn()
		{
			_logger.LogDebug("[GLIDE2x] grSstVRetraceOn called");
			// Check if vertical retrace is active
			// For emulation, we always return FALSE (not in VRetrace)
			return 0; // FALSE
		}

		[DllModuleExport(75, entryPoint: 0x000053A0, Version = "4.90.0.3000", ExportName = "_grSstVideoLine@0")]
		public uint grSstVideoLine()
		{
			_logger.LogDebug("[GLIDE2x] grSstVideoLine called");
			// Return current video line being scanned (0 to screen height - 1)
			// For emulation, we return 0 (top line)
			return 0;
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
		public uint grSstWinOpen(uint hwnd, uint resolution, uint refresh, uint colorFormat, uint origin, uint nColBuffers, uint nAuxBuffers)
		{
			_logger.LogInformation("[GLIDE2x] grSstWinOpen(hwnd=0x{Hwnd:X8}, resolution={Resolution}, refresh={Refresh}, colorFormat={ColorFormat}, origin={Origin}, nColBuffers={NColBuffers}, nAuxBuffers={NAuxBuffers})", 
				hwnd, resolution, refresh, colorFormat, origin, nColBuffers, nAuxBuffers);
			
			if (_windowOpen)
			{
				_logger.LogWarning("[GLIDE2x] grSstWinOpen: Window already open");
				return 1; // TRUE - already open
			}
			
			// Create rendering backend (prioritize GLFW as requested)
			if (_renderingBackend == null)
			{
				_logger.LogInformation("[GLIDE2x] Creating rendering backend for Glide emulation");
				
				_renderingBackend = Rendering.BackendFactory.CreateRenderingBackendWithHost(_logger, _env.Host);
				if (_env.Host != null)
				{
					_logger.LogInformation("[GLIDE2x] Using Avalonia rendering backend for GUI integration");
				}
				
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
			
			// Allocate frame buffer (still needed for software rasterization fallback and LFB access)
			_frameBuffer = new byte[_width * _height * 4]; // RGBA format
			_windowOpen = true;
			
			// Begin first frame if using hardware acceleration
			if (_useHardwareAcceleration)
			{
				_renderingBackend.BeginFrame();
			}
			
			_logger.LogInformation("[GLIDE2x] Window opened successfully (HW Accel: {HwAccel})", _useHardwareAcceleration);
			return 1; // TRUE - success
		}

		[DllModuleExport(78, entryPoint: 0x00005860, Version = "4.90.0.3000", ExportName = "_grTexCalcMemRequired@16")]
		public uint grTexCalcMemRequired(uint lodMin, uint lodMax, uint aspect, uint format)
		{
			_logger.LogDebug("[GLIDE2x] grTexCalcMemRequired called");
			// Calculate texture memory required
			// Parameters: lodmin, lodmax, aspect, format
			// Return a reasonable size (e.g., 256KB for a typical texture)
			return 256 * 1024; // 256 KB
		}

		[DllModuleExport(79, entryPoint: 0x000058A0, Version = "4.90.0.3000", ExportName = "_grTexClampMode@12")]
		public uint grTexClampMode(uint tmu, uint sClamp, uint tClamp)
		{
			_logger.LogDebug("[GLIDE2x] grTexClampMode called");
			// Set texture clamping mode (wrap, clamp, mirror)
			return 0; // Success (void function)
		}

		[DllModuleExport(80, entryPoint: 0x000058B0, Version = "4.90.0.3000", ExportName = "_grTexCombine@28")]
		public uint grTexCombine(uint tmu, uint rgbFunction, uint rgbFactor, uint alphaFunction, uint alphaFactor, uint rgbInvert, uint alphaInvert)
		{
			_logger.LogDebug("[GLIDE2x] grTexCombine called");
			// Set texture combine function
			// Parameters: tmu, rgb_function, rgb_factor, alpha_function, alpha_factor, rgb_invert, alpha_invert
			return 0; // Success (void function)
		}

		[DllModuleExport(81, entryPoint: 0x00006330, Version = "4.90.0.3000", ExportName = "_grTexCombineFunction@8")]
		public uint grTexCombineFunction()
		{
			_logger.LogDebug("[GLIDE2x] grTexCombineFunction called");
			// Set simplified texture combine function
			return 0; // Success (void function)
		}

		[DllModuleExport(82, entryPoint: 0x00005920, Version = "4.90.0.3000", ExportName = "_grTexDetailControl@16")]
		public uint grTexDetailControl(uint tmu, uint lodBias, uint detailScale, uint detailMax)
		{
			_logger.LogDebug("[GLIDE2x] grTexDetailControl called");
			// Set texture detail control (for detail texture blending)
			return 0; // Success (void function)
		}

		[DllModuleExport(83, entryPoint: 0x00005930, Version = "4.90.0.3000", ExportName = "_grTexDownloadMipMap@16")]
		public uint grTexDownloadMipMap(uint tmu, uint startAddress, uint evenOdd, uint infoPtr)
		{
			_logger.LogDebug("[GLIDE2x] grTexDownloadMipMap called");
			// Download mipmap texture data to texture memory
			return 0; // Success (void function)
		}

		[DllModuleExport(84, entryPoint: 0x00005A20, Version = "4.90.0.3000", ExportName = "_grTexDownloadMipMapLevel@32")]
		public uint grTexDownloadMipMapLevel(uint tmu, uint startAddress, uint lodLevel, uint lodLarge, uint aspectRatio, uint format, uint evenOdd, uint dataPtr)
		{
			_logger.LogDebug("[GLIDE2x] grTexDownloadMipMapLevel called");
			// Download a single mipmap level to texture memory
			return 0; // Success (void function)
		}

		[DllModuleExport(85, entryPoint: 0x000059A0, Version = "4.90.0.3000", ExportName = "_grTexDownloadMipMapLevelPartial@40")]
		public uint grTexDownloadMipMapLevelPartial(uint tmu, uint startAddress, uint lodLevel, uint lodLarge, uint aspectRatio, uint format, uint evenOdd, uint dataPtr, uint start, uint end)
		{
			_logger.LogDebug("[GLIDE2x] grTexDownloadMipMapLevelPartial called");
			// Download a partial mipmap level to texture memory
			return 0; // Success (void function)
		}

		[DllModuleExport(86, entryPoint: 0x00005A60, Version = "4.90.0.3000", ExportName = "_grTexDownloadTable@12")]
		public uint grTexDownloadTable(uint tmu, uint type, uint dataPtr)
		{
			_logger.LogDebug("[GLIDE2x] grTexDownloadTable called");
			// Download a texture table (palette or NCC table)
			return 0; // Success (void function)
		}

		[DllModuleExport(87, entryPoint: 0x00005B10, Version = "4.90.0.3000", ExportName = "_grTexDownloadTablePartial@20")]
		public uint grTexDownloadTablePartial(uint tmu, uint type, uint dataPtr, uint start, uint end)
		{
			_logger.LogDebug("[GLIDE2x] grTexDownloadTablePartial called");
			// Download a partial texture table
			return 0; // Success (void function)
		}

		[DllModuleExport(88, entryPoint: 0x00005BE0, Version = "4.90.0.3000", ExportName = "_grTexFilterMode@12")]
		public uint grTexFilterMode(uint tmu, uint minFilter, uint magFilter)
		{
			_logger.LogDebug("[GLIDE2x] grTexFilterMode called");
			// Set texture filtering mode (point, bilinear, trilinear)
			return 0; // Success (void function)
		}

		[DllModuleExport(89, entryPoint: 0x00005BF0, Version = "4.90.0.3000", ExportName = "_grTexLodBiasValue@8")]
		public uint grTexLodBiasValue(uint tmu, uint bias)
		{
			_logger.LogDebug("[GLIDE2x] grTexLodBiasValue called");
			// Set Level of Detail (LOD) bias for texture mipmap selection
			return 0; // Success (void function)
		}

		[DllModuleExport(90, entryPoint: 0x00005C00, Version = "4.90.0.3000", ExportName = "_grTexMaxAddress@4")]
		public uint grTexMaxAddress(uint tmu)
		{
			_logger.LogDebug("[GLIDE2x] grTexMaxAddress called");
			// Return maximum texture memory address for a TMU
			// Typically 4MB for Voodoo cards
			return 4 * 1024 * 1024; // 4MB
		}

		[DllModuleExport(91, entryPoint: 0x00005C20, Version = "4.90.0.3000", ExportName = "_grTexMinAddress@4")]
		public uint grTexMinAddress(uint tmu)
		{
			_logger.LogDebug("[GLIDE2x] grTexMinAddress called");
			// Return minimum texture memory address for a TMU
			// Typically starts at 0
			return 0;
		}

		[DllModuleExport(92, entryPoint: 0x00005C30, Version = "4.90.0.3000", ExportName = "_grTexMipMapMode@12")]
		public uint grTexMipMapMode(uint tmu, uint mode, uint lodBlend)
		{
			_logger.LogDebug("[GLIDE2x] grTexMipMapMode called");
			// Set mipmap mode (disable, nearest, or blend between levels)
			return 0; // Success (void function)
		}

		[DllModuleExport(93, entryPoint: 0x00005C40, Version = "4.90.0.3000", ExportName = "_grTexMultibase@8")]
		public uint grTexMultibase(uint tmu, uint enable)
		{
			_logger.LogDebug("[GLIDE2x] grTexMultibase called");
			// Enable or disable multibase texture addressing
			return 0; // Success (void function)
		}

		[DllModuleExport(94, entryPoint: 0x00005C50, Version = "4.90.0.3000", ExportName = "_grTexMultibaseAddress@20")]
		public uint grTexMultibaseAddress(uint tmu, uint range, uint startAddress, uint evenOdd, uint infoPtr)
		{
			_logger.LogDebug("[GLIDE2x] grTexMultibaseAddress called");
			// Set multibase texture address
			return 0; // Success (void function)
		}

		[DllModuleExport(95, entryPoint: 0x00005CC0, Version = "4.90.0.3000", ExportName = "_grTexNCCTable@8")]
		public uint grTexNCCTable(uint tmu, uint table)
		{
			_logger.LogDebug("[GLIDE2x] grTexNCCTable called");
			// Set NCC (YIQ color space) texture compression table
			return 0; // Success (void function)
		}

		[DllModuleExport(96, entryPoint: 0x00005D50, Version = "4.90.0.3000", ExportName = "_grTexSource@16")]
		public uint grTexSource(uint tmu, uint startAddress, uint evenOdd, uint infoPtr)
		{
			_logger.LogDebug("[GLIDE2x] grTexSource called");
			// Set active texture source (bind texture)
			return 0; // Success (void function)
		}

		[DllModuleExport(97, entryPoint: 0x00005E70, Version = "4.90.0.3000", ExportName = "_grTexTextureMemRequired@8")]
		public uint grTexTextureMemRequired(uint evenOdd, uint infoPtr)
		{
			_logger.LogDebug("[GLIDE2x] grTexTextureMemRequired called");
			// Calculate texture memory required for a specific texture configuration
			// Return a reasonable size
			return 128 * 1024; // 128 KB
		}

		[DllModuleExport(98, entryPoint: 0x00003E90, Version = "4.90.0.3000", ExportName = "_grTriStats@8", IsStub = true)]
		public uint grTriStats(uint statsPtr, uint reset)
		{
			_logger.LogWarning("[GLIDE2x] grTriStats called (stub)");
			// TODO: Implement _grTriStats@8
			return 0; // DWORD default
		}

		[DllModuleExport(99, entryPoint: 0x00005F60, Version = "4.90.0.3000", ExportName = "_gu3dfGetInfo@8", IsStub = true)]
		public uint gu3dfGetInfo(uint filename, uint info)
		{
			_logger.LogWarning("[GLIDE2x] gu3dfGetInfo called (stub)");
			// TODO: Implement _gu3dfGetInfo@8
			return 0; // DWORD default
		}

		[DllModuleExport(100, entryPoint: 0x00005FA0, Version = "4.90.0.3000", ExportName = "_gu3dfLoad@8", IsStub = true)]
		public uint gu3dfLoad(uint filename, uint data)
		{
			_logger.LogWarning("[GLIDE2x] gu3dfLoad called (stub)");
			// TODO: Implement _gu3dfLoad@8
			return 0; // DWORD default
		}

		[DllModuleExport(101, entryPoint: 0x00001F30, Version = "4.90.0.3000", ExportName = "_guAADrawTriangleWithClip@12", IsStub = true)]
		public uint guAADrawTriangleWithClip(uint v1Ptr, uint v2Ptr, uint v3Ptr)
		{
			_logger.LogWarning("[GLIDE2x] guAADrawTriangleWithClip called (stub)");
			// TODO: Implement _guAADrawTriangleWithClip@12
			return 0; // DWORD default
		}

		[DllModuleExport(102, entryPoint: 0x00006010, Version = "4.90.0.3000", ExportName = "_guAlphaSource@4")]
		public uint guAlphaSource(uint mode)
		{
			_logger.LogDebug("[GLIDE2x] guAlphaSource called");
			// Set alpha source (utility function)
			return 0; // Success (void function)
		}

		[DllModuleExport(103, entryPoint: 0x00006080, Version = "4.90.0.3000", ExportName = "_guColorCombineFunction@4")]
		public uint guColorCombineFunction(uint mode)
		{
			_logger.LogDebug("[GLIDE2x] guColorCombineFunction called");
			// Set color combine function (utility wrapper)
			return 0; // Success (void function)
		}

		[DllModuleExport(104, entryPoint: 0x00002460, Version = "4.90.0.3000", ExportName = "_guDrawPolygonVertexListWithClip@8", IsStub = true)]
		public uint guDrawPolygonVertexListWithClip(uint nverts, uint vlistPtr)
		{
			_logger.LogWarning("[GLIDE2x] guDrawPolygonVertexListWithClip called (stub)");
			// TODO: Implement _guDrawPolygonVertexListWithClip@8
			return 0; // DWORD default
		}

		[DllModuleExport(105, entryPoint: 0x00001500, Version = "4.90.0.3000", ExportName = "_guDrawTriangleWithClip@12")]
		public uint guDrawTriangleWithClip(uint ptrA, uint ptrB, uint ptrC)
		{
			_logger.LogDebug("[GLIDE2x] guDrawTriangleWithClip: vertices at 0x{PtrA:X8}, 0x{PtrB:X8}, 0x{PtrC:X8}", ptrA, ptrB, ptrC);
			// For now, just forward to grDrawTriangle (clipping would be done in a full implementation)
			return grDrawTriangle(ptrA, ptrB, ptrC);
		}

		[DllModuleExport(106, entryPoint: 0x00006400, Version = "4.90.0.3000", ExportName = "_guEncodeRLE16@16", IsStub = true)]
		public uint guEncodeRLE16(uint dstPtr, uint srcPtr, uint width, uint height)
		{
			_logger.LogWarning("[GLIDE2x] guEncodeRLE16 called (stub)");
			// TODO: Implement _guEncodeRLE16@16
			return 0; // DWORD default
		}

		[DllModuleExport(107, entryPoint: 0x00006500, Version = "4.90.0.3000", ExportName = "_guEndianSwapBytes@4", IsStub = true)]
		public uint guEndianSwapBytes(uint size)
		{
			_logger.LogWarning("[GLIDE2x] guEndianSwapBytes called (stub)");
			// TODO: Implement _guEndianSwapBytes@4
			return 0; // DWORD default
		}

		[DllModuleExport(108, entryPoint: 0x000064E0, Version = "4.90.0.3000", ExportName = "_guEndianSwapWords@4", IsStub = true)]
		public uint guEndianSwapWords(uint size)
		{
			_logger.LogWarning("[GLIDE2x] guEndianSwapWords called (stub)");
			// TODO: Implement _guEndianSwapWords@4
			return 0; // DWORD default
		}

		[DllModuleExport(109, entryPoint: 0x00003160, Version = "4.90.0.3000", ExportName = "_guFogGenerateExp2@8", IsStub = true)]
		public uint guFogGenerateExp2(uint fogtablePtr, uint density)
		{
			_logger.LogWarning("[GLIDE2x] guFogGenerateExp2 called (stub)");
			// TODO: Implement _guFogGenerateExp2@8
			return 0; // DWORD default
		}

		[DllModuleExport(110, entryPoint: 0x00003150, Version = "4.90.0.3000", ExportName = "_guFogGenerateExp@8", IsStub = true)]
		public uint guFogGenerateExp(uint fogtablePtr, uint density)
		{
			_logger.LogWarning("[GLIDE2x] guFogGenerateExp called (stub)");
			// TODO: Implement _guFogGenerateExp@8
			return 0; // DWORD default
		}

		[DllModuleExport(111, entryPoint: 0x00003170, Version = "4.90.0.3000", ExportName = "_guFogGenerateLinear@12", IsStub = true)]
		public uint guFogGenerateLinear(uint fogtablePtr, uint nearZ, uint farZ)
		{
			_logger.LogWarning("[GLIDE2x] guFogGenerateLinear called (stub)");
			// TODO: Implement _guFogGenerateLinear@12
			return 0; // DWORD default
		}

		[DllModuleExport(112, entryPoint: 0x00003140, Version = "4.90.0.3000", ExportName = "_guFogTableIndexToW@4", IsStub = true)]
		public uint guFogTableIndexToW(uint index)
		{
			_logger.LogWarning("[GLIDE2x] guFogTableIndexToW called (stub)");
			// TODO: Implement _guFogTableIndexToW@4
			return 0; // DWORD default
		}

		[DllModuleExport(113, entryPoint: 0x00003350, Version = "4.90.0.3000", ExportName = "_guTexAllocateMemory@60")]
		public uint guTexAllocateMemory(uint tmu, uint evenOdd, uint width, uint height, uint format, uint mipMapMode, uint lodMin, uint lodMax, uint aspect, uint smallLodLog2, uint largeLodLog2, uint oddEvenPtr, uint mipmapPtr, uint startPtr, uint endPtr)
		{
			_logger.LogDebug("[GLIDE2x] guTexAllocateMemory called");
			// Allocate texture memory and return mipmap ID
			uint address = _nextTextureAddress;
			_nextTextureAddress += 256 * 1024; // Allocate 256KB per texture
			
			// Wrap around if we exceed texture memory
			if (_nextTextureAddress >= TextureMemorySize)
			{
				_nextTextureAddress = 0x1000;
			}
			
			_logger.LogDebug("[GLIDE2x] guTexAllocateMemory: allocated address=0x{Address:X8}", address);
			return address; // Return mipmap ID (texture address)
		}

		[DllModuleExport(114, entryPoint: 0x000034E0, Version = "4.90.0.3000", ExportName = "_guTexChangeAttributes@48", IsStub = true)]
		public uint guTexChangeAttributes(uint mmid, uint width, uint height, uint format, uint mipMapMode, uint lodMin, uint lodMax, uint aspect, uint smallLodLog2, uint largeLodLog2, uint evenOdd, uint dataPtr)
		{
			_logger.LogWarning("[GLIDE2x] guTexChangeAttributes called (stub)");
			// TODO: Implement _guTexChangeAttributes@48
			return 0; // DWORD default
		}

		[DllModuleExport(115, entryPoint: 0x000061D0, Version = "4.90.0.3000", ExportName = "_guTexCombineFunction@8")]
		public uint guTexCombineFunction(uint tmu, uint mode)
		{
			_logger.LogDebug("[GLIDE2x] guTexCombineFunction called");
			// Set texture combine function (utility wrapper)
			return 0; // Success (void function)
		}

		[DllModuleExport(116, entryPoint: 0x00006340, Version = "4.90.0.3000", ExportName = "_guTexCreateColorMipMap@0", IsStub = true)]
		public uint guTexCreateColorMipMap()
		{
			_logger.LogWarning("[GLIDE2x] guTexCreateColorMipMap called (stub)");
			// TODO: Implement _guTexCreateColorMipMap@0
			return 0; // DWORD default
		}

		[DllModuleExport(117, entryPoint: 0x000035A0, Version = "4.90.0.3000", ExportName = "_guTexDownloadMipMap@12")]
		public uint guTexDownloadMipMap(uint mmid, uint srcPtr, uint nccPtr)
		{
			_logger.LogDebug("[GLIDE2x] guTexDownloadMipMap called");
			// Download mipmap texture data
			// In a real implementation, this would upload texture data to the GPU
			// For emulation, we just acknowledge the call
			return 0; // Success (void function)
		}

		[DllModuleExport(118, entryPoint: 0x00003630, Version = "4.90.0.3000", ExportName = "_guTexDownloadMipMapLevel@12", IsStub = true)]
		public uint guTexDownloadMipMapLevel(uint mmid, uint level, uint srcPtr)
		{
			_logger.LogWarning("[GLIDE2x] guTexDownloadMipMapLevel called (stub)");
			// TODO: Implement _guTexDownloadMipMapLevel@12
			return 0; // DWORD default
		}

		[DllModuleExport(119, entryPoint: 0x000036A0, Version = "4.90.0.3000", ExportName = "_guTexGetCurrentMipMap@4", IsStub = true)]
		public uint guTexGetCurrentMipMap(uint tmu)
		{
			_logger.LogWarning("[GLIDE2x] guTexGetCurrentMipMap called (stub)");
			// TODO: Implement _guTexGetCurrentMipMap@4
			return 0; // DWORD default
		}

		[DllModuleExport(120, entryPoint: 0x000036B0, Version = "4.90.0.3000", ExportName = "_guTexGetMipMapInfo@4", IsStub = true)]
		public uint guTexGetMipMapInfo(uint mmidPtr)
		{
			_logger.LogWarning("[GLIDE2x] guTexGetMipMapInfo called (stub)");
			// TODO: Implement _guTexGetMipMapInfo@4
			return 0; // DWORD default
		}

		[DllModuleExport(121, entryPoint: 0x000036D0, Version = "4.90.0.3000", ExportName = "_guTexMemQueryAvail@4", IsStub = true)]
		public uint guTexMemQueryAvail(uint tmu)
		{
			_logger.LogWarning("[GLIDE2x] guTexMemQueryAvail called (stub)");
			// TODO: Implement _guTexMemQueryAvail@4
			return 0; // DWORD default
		}

		[DllModuleExport(122, entryPoint: 0x000036F0, Version = "4.90.0.3000", ExportName = "_guTexMemReset@0")]
		public uint guTexMemReset()
		{
			_logger.LogDebug("[GLIDE2x] guTexMemReset called");
			// Reset texture memory allocator
			_nextTextureAddress = 0x1000;
			_logger.LogDebug("[GLIDE2x] guTexMemReset: texture memory reset");
			return 0; // Success (void function)
		}

		[DllModuleExport(123, entryPoint: 0x00003770, Version = "4.90.0.3000", ExportName = "_guTexSource@4")]
		public uint guTexSource(uint mmid)
		{
			_logger.LogDebug("[GLIDE2x] guTexSource: mmid=0x{MmId:X8}", mmid);
			// Set the current texture source (bind texture)
			// Assuming TMU0 for simplicity
			_currentTextureTMU0 = mmid;
			return 0; // Success (void function)
		}
		
		/// <summary>
		/// Flush all batched triangles to the rendering backend
		/// </summary>
		private void FlushTriangleBatch()
		{
			if (_triangleBatch.Count == 0 || _renderingBackend == null || !_renderingBackend.IsInitialized)
			{
				return;
			}
			
			_logger.LogDebug("[GLIDE2x] Flushing {Count} triangles to rendering backend (HW Accel: {HwAccel})", 
				_triangleBatch.Count, _useHardwareAcceleration);
			
			if (_useHardwareAcceleration)
			{
				// Hardware-accelerated path: convert triangles to vertices and indices
				var vertices = new List<Rendering.Vertex>();
				var indices = new List<ushort>();
				
				foreach (var tri in _triangleBatch)
				{
					ushort baseIndex = (ushort)vertices.Count;
					
					// Add 3 vertices for this triangle
					vertices.Add(ConvertGrVertexToVertex(tri.v0));
					vertices.Add(ConvertGrVertexToVertex(tri.v1));
					vertices.Add(ConvertGrVertexToVertex(tri.v2));
					
					// Add indices (simple: 0, 1, 2 for each triangle)
					indices.Add(baseIndex);
					indices.Add((ushort)(baseIndex + 1));
					indices.Add((ushort)(baseIndex + 2));
				}
				
				// Set render state from current Glide state
				UpdateRenderState();
				
				// Bind texture if one is active
				if (_currentTextureTMU0 != 0)
				{
					_renderingBackend.BindTexture(_currentTextureTMU0);
				}
				else
				{
					_renderingBackend.BindTexture(0); // No texture
				}
				
				// Draw triangles using hardware acceleration
				_renderingBackend.DrawTriangles(vertices.ToArray(), indices.ToArray());
			}
			else
			{
				// Software rasterization path (fallback)
				foreach (var tri in _triangleBatch)
				{
					RenderTriangle(tri);
				}
				
				// Update the display
				if (_frameBuffer != null)
				{
					_renderingBackend.UpdateFrameBuffer(_frameBuffer, _width * 4);
				}
			}
			
			_triangleBatch.Clear();
		}
		
		/// <summary>
		/// Convert a Glide vertex to a rendering backend vertex
		/// </summary>
		private Rendering.Vertex ConvertGrVertexToVertex(GrVertex v)
		{
			return new Rendering.Vertex
			{
				Position = new System.Numerics.Vector3(v.x, v.y, v.z),
				Color = new System.Numerics.Vector4(
					v.r / 255.0f,
					v.g / 255.0f,
					v.b / 255.0f,
					v.a / 255.0f
				),
				TexCoord = new System.Numerics.Vector2(v.tmu0.sow, v.tmu0.tow),
				Oow = v.oow
			};
		}
		
		/// <summary>
		/// Update rendering backend state from Glide state
		/// </summary>
		private void UpdateRenderState()
		{
			if (_renderingBackend == null)
			{
				return;
			}
			
			// Map Glide blend mode to rendering backend blend mode
			// For simplicity, we'll use alpha blending if alpha is enabled
			var blendMode = Rendering.BlendMode.Alpha; // Default to alpha blending for Glide
			
			// Map Glide depth test to rendering backend depth test
			var depthTest = Rendering.DepthTest.Disabled;
			if (_depthBufferMode != 0) // 0 = disabled
			{
				// Map Glide depth function to our depth test enum
				depthTest = _depthBufferFunction switch
				{
					0 => Rendering.DepthTest.Disabled, // never
					1 => Rendering.DepthTest.Less,      // less
					2 => Rendering.DepthTest.Equal,     // equal
					3 => Rendering.DepthTest.LessEqual, // less or equal
					4 => Rendering.DepthTest.Greater,   // greater
					5 => Rendering.DepthTest.NotEqual,  // not equal
					6 => Rendering.DepthTest.GreaterEqual, // greater or equal
					7 => Rendering.DepthTest.Always,    // always
					_ => Rendering.DepthTest.LessEqual  // default
				};
			}
			
			// Map Glide cull mode to rendering backend cull mode
			var cullMode = _cullMode switch
			{
				0 => Rendering.CullMode.None,   // GR_CULL_DISABLE
				1 => Rendering.CullMode.Front,  // GR_CULL_NEGATIVE
				2 => Rendering.CullMode.Back,   // GR_CULL_POSITIVE
				_ => Rendering.CullMode.None
			};
			
			_renderingBackend.SetRenderState(blendMode, depthTest, cullMode);
		}
		
		/// <summary>
		/// Render a single triangle to the frame buffer using scan-line rasterization
		/// </summary>
		private void RenderTriangle(Triangle tri)
		{
			if (_frameBuffer == null)
			{
				return;
			}
			
			// Sort vertices by Y coordinate (v0.y <= v1.y <= v2.y)
			var v0 = tri.v0;
			var v1 = tri.v1;
			var v2 = tri.v2;
			
			if (v0.y > v1.y) { (v0, v1) = (v1, v0); }
			if (v0.y > v2.y) { (v0, v2) = (v2, v0); }
			if (v1.y > v2.y) { (v1, v2) = (v2, v1); }
			
			// Simple flat-bottom and flat-top triangle rasterization
			RasterizeTriangle(v0, v1, v2);
		}
		
		/// <summary>
		/// Rasterize a triangle using scan-line conversion
		/// </summary>
		private void RasterizeTriangle(GrVertex v0, GrVertex v1, GrVertex v2)
		{
			if (_frameBuffer == null)
			{
				return;
			}
			
			// Skip degenerate triangles
			float totalHeight = v2.y - v0.y;
			if (Math.Abs(totalHeight) < 0.5f)
			{
				return;
			}
			
			// Render flat-bottom triangle (v0 to v1/v2)
			float segmentHeight = v1.y - v0.y + 1;
			if (segmentHeight > 0)
			{
				for (int y = (int)v0.y; y <= (int)v1.y && y < _height; y++)
				{
					if (y < 0) continue;
					
					float alpha = (y - v0.y) / totalHeight;
					float beta = (y - v0.y) / segmentHeight;
					
					// Interpolate X coordinates for left and right edges
					int xA = (int)(v0.x + (v2.x - v0.x) * alpha);
					int xB = (int)(v0.x + (v1.x - v0.x) * beta);
					
					if (xA > xB) { (xA, xB) = (xB, xA); }
					
					// Interpolate colors for short edge (v0 to v1) using beta
					byte rA = (byte)Math.Clamp(v0.r + (v1.r - v0.r) * beta, 0, 255);
					byte gA = (byte)Math.Clamp(v0.g + (v1.g - v0.g) * beta, 0, 255);
					byte bA = (byte)Math.Clamp(v0.b + (v1.b - v0.b) * beta, 0, 255);
					byte aA = (byte)Math.Clamp(v0.a + (v1.a - v0.a) * beta, 0, 255);
					
					// Draw horizontal span
					DrawHorizontalSpan(y, xA, xB, rA, gA, bA, aA);
				}
			}
			
			// Render flat-top triangle (v1 to v2)
			segmentHeight = v2.y - v1.y + 1;
			if (segmentHeight > 0)
			{
				for (int y = (int)v1.y; y <= (int)v2.y && y < _height; y++)
				{
					if (y < 0) continue;
					
					float alpha = (y - v0.y) / totalHeight;
					float beta = (y - v1.y) / segmentHeight;
					
					int xA = (int)(v0.x + (v2.x - v0.x) * alpha);
					int xB = (int)(v1.x + (v2.x - v1.x) * beta);
					
					if (xA > xB) { (xA, xB) = (xB, xA); }
					
					byte rA = (byte)Math.Clamp(v1.r + (v2.r - v1.r) * beta, 0, 255);
					byte gA = (byte)Math.Clamp(v1.g + (v2.g - v1.g) * beta, 0, 255);
					byte bA = (byte)Math.Clamp(v1.b + (v2.b - v1.b) * beta, 0, 255);
					byte aA = (byte)Math.Clamp(v1.a + (v2.a - v1.a) * beta, 0, 255);
					
					DrawHorizontalSpan(y, xA, xB, rA, gA, bA, aA);
				}
			}
		}
		
		/// <summary>
		/// Draw a horizontal span of pixels
		/// </summary>
		private void DrawHorizontalSpan(int y, int xStart, int xEnd, byte r, byte g, byte b, byte a)
		{
			if (_frameBuffer == null || y < 0 || y >= _height)
			{
				return;
			}
			
			xStart = Math.Max(0, Math.Min(xStart, _width - 1));
			xEnd = Math.Max(0, Math.Min(xEnd, _width - 1));
			
			for (int x = xStart; x <= xEnd; x++)
			{
				int offset = (y * _width + x) * 4;
				
				if (offset >= 0 && offset + 3 < _frameBuffer.Length)
				{
					// Simple alpha blending if alpha < 255
					if (a < 255 && _frameBuffer[offset + 3] > 0)
					{
						float srcAlpha = a / 255.0f;
						float dstAlpha = 1.0f - srcAlpha;
						
						_frameBuffer[offset + 0] = (byte)(r * srcAlpha + _frameBuffer[offset + 0] * dstAlpha);
						_frameBuffer[offset + 1] = (byte)(g * srcAlpha + _frameBuffer[offset + 1] * dstAlpha);
						_frameBuffer[offset + 2] = (byte)(b * srcAlpha + _frameBuffer[offset + 2] * dstAlpha);
						_frameBuffer[offset + 3] = (byte)Math.Max(a, _frameBuffer[offset + 3]);
					}
					else
					{
						_frameBuffer[offset + 0] = r;
						_frameBuffer[offset + 1] = g;
						_frameBuffer[offset + 2] = b;
						_frameBuffer[offset + 3] = a;
					}
				}
			}
		}
	}
}