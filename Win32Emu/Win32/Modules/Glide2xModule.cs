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

		public bool TryInvokeUnsafe(string exp, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (exp?.ToUpperInvariant())
			{
				case "_ConvertAndDownloadRle@64":
					returnValue = ConvertAndDownloadRle();
					return true;
				case "_grAADrawLine@8":
					// FX_ENTRY void FX_CALL grAADrawLine(const GrVertex *v1, const GrVertex *v2);
					return true;
				case "_grAADrawPoint@4":
					// FX_ENTRY void FX_CALL grAADrawPoint(const GrVertex *pt );
					return true;
				case "_grAlphaBlendFunction@16":
					// FX_ENTRY void FX_CALL
					// grAlphaBlendFunction(
					//                      GrAlphaBlendFnc_t rgb_sf,   GrAlphaBlendFnc_t rgb_df,
					//                      GrAlphaBlendFnc_t alpha_sf, GrAlphaBlendFnc_t alpha_df
					//                      );
					return true;
				case "_grBufferClear@12":
					// FX_ENTRY void FX_CALL
					// grBufferClear( GrColor_t color, GrAlpha_t alpha, FxU16 depth );
					return true;
				case "_grBufferSwap@4":
					// FX_ENTRY void FX_CALL
					// grBufferSwap( int swap_interval );
					return true;
				case "_grChromakeyMode@4":
					return true;
				case "_grChromakeyValue@4":
					return true;
				case "_grClipWindow@16":
					return true;
				case "_grConstantColorValue@4":
					return true;
				case "_grCullMode@4":
					return true;
				case "_grDepthBufferFunction@4":
					return true;
				case "_grDepthBufferMode@4":
					return true;
				case "_grDepthMask@4":
					return true;
				case "_grGlideGetState@4":
					return true;
				case "_grGlideInit@0":
					return true;
				case "_grGlideSetState@4":
					return true;
				case "_grGlideShutdown@0":
					return true;
				case "_grLfbLock@24":
					return true;
				case "_grLfbUnlock@8":
					return true;
				case "_grRenderBuffer@4":
					return true;
				case "_grSstIdle@0":
					return true;
				case "_grSstQueryHardware@4":
					return true;
				case "_grSstSelect@4":
					return true;
				case "_grSstVRetraceOn@0":
					return true;
				case "_grSstWinClose@0":
					return true;
				case "_grSstWinOpen@28":
					return true;
				case "_grTexDownloadTable@12":
					return true;
				case "_guAlphaSource@4":
					return true;
				case "_guColorCombineFunction@4":
					return true;
				case "_guDrawTriangleWithClip@12":
					return true;
				case "_guTexAllocateMemory@60":
					return true;
				case "_guTexCombineFunction@8":
					return true;
				case "_guTexDownloadMipMap@12":
					return true;
				case "_guTexMemReset@0":
					return true;
				case "_guTexSource@4":
					return true;
				default:
					_logger.LogInformation($"[Glide2x] Unimplemented export: {exp}");
					return false;
			}
		}

		/// <summary>
		/// FX_ENTRY void FX_CALL 
		/// ConvertAndDownloadRle( GrChipID_t        tmu,
		///	FxU32             startAddress,
		///	GrLOD_t           thisLod,
		///	GrLOD_t           largeLod,
		///	GrAspectRatio_t   aspectRatio,
		///	GrTextureFormat_t format,
		///	FxU32             evenOdd,
		///	FxU8              *bm_data,
		///	long              bm_h,
		///	FxU32             u0,
		///	FxU32             v0,
		///	FxU32             width,
		///	FxU32             height,
		///	FxU32             dest_width,
		///	FxU32             dest_height,
		///	FxU16             *tlut);
		/// </summary>
		/// <returns></returns>
		[DllModuleExport(1, entryPoint: 0x00005ED0, isStub: true)]
		private uint ConvertAndDownloadRle()
		{
			return 0;
		}
	}
}