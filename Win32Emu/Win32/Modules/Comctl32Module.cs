using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// COMCTL32.DLL module - provides Windows Common Controls functionality.
/// </summary>
public class Comctl32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;
	private uint _nextImageListHandle = 0x90000000;
	private readonly Dictionary<uint, ImageListData> _imageLists = new();

	public Comctl32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "COMCTL32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "IMAGELIST_CREATE":
				returnValue = ImageList_Create(a.Int32(0), a.Int32(1), a.UInt32(2), a.Int32(3), a.Int32(4));
				return true;

			case "IMAGELIST_ADDMASKED":
				returnValue = ImageList_AddMasked(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "IMAGELIST_REPLACEICON":
				returnValue = ImageList_ReplaceIcon(a.UInt32(0), a.Int32(1), a.UInt32(2));
				return true;

			case "IMAGELIST_DESTROY":
				returnValue = ImageList_Destroy(a.UInt32(0));
				return true;

			case "IMAGELIST_LOADIMAGEA":
				returnValue = ImageList_LoadImageA(a.UInt32(0), a.LpcStr(1), a.Int32(2), a.Int32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6));
				return true;

			case "IMAGELIST_SETOVERLAYIMAGE":
				returnValue = ImageList_SetOverlayImage(a.UInt32(0), a.Int32(1), a.Int32(2));
				return true;

			case "ORDINAL_17":
				returnValue = InitCommonControls(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "CREATEPROPERTYSHEETPAGEA":
				returnValue = CreatePropertySheetPageA(a.UInt32(0));
				return true;

			case "DESTROYPROPERTYSHEETPAGE":
				returnValue = DestroyPropertySheetPage(a.UInt32(0));
				return true;

			case "PROPERTYSHEETA":
				returnValue = PropertySheetA(a.UInt32(0));
				return true;

			case "IMAGELIST_SETBKCOLOR":
				returnValue = ImageList_SetBkColor(a.UInt32(0), a.UInt32(1));
				return true;

			case "MENUHELP":
				returnValue = MenuHelp(a.UInt32(0));
				return true;

			case "GETEFFECTIVECLIENTRECT":
				returnValue = GetEffectiveClientRect(a.UInt32(0), a.UInt32(1));
				return true;

			case "CREATESTATUSWINDOWA":
				returnValue = CreateStatusWindowA(a.UInt32(0));
				return true;

			case "ORDINAL_234":
			case "ORDINAL_329":
			case "ORDINAL_334":
			case "ORDINAL_337":
			case "ORDINAL_338":
			case "ORDINAL_340":
			case "ORDINAL_350":
			case "ORDINAL_351":
			case "ORDINAL_355":
				_logger.LogInformation("[Comctl32] {Export}(...)", export);
				returnValue = 1; // Generic stub for ordinals
				return true;

			default:
				_logger.LogInformation("[Comctl32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Creates an image list.
	/// HIMAGELIST ImageList_Create(
	///   int cx,
	///   int cy,
	///   UINT flags,
	///   int cInitial,
	///   int cGrow
	/// );
	/// </summary>
	[DllModuleExport(45, Version = "5.81.4916.400")]
	private uint ImageList_Create(int cx, int cy, uint flags, int cInitial, int cGrow)
	{
		_logger.LogInformation("[Comctl32] ImageList_Create(cx={Cx}, cy={Cy}, flags=0x{Flags:X}, cInitial={CInitial}, cGrow={CGrow})",
			cx, cy, flags, cInitial, cGrow);

		var handle = _nextImageListHandle++;
		_imageLists[handle] = new ImageListData
		{
			Width = cx,
			Height = cy,
			Flags = flags,
			InitialCount = cInitial,
			GrowBy = cGrow
		};

		return handle;
	}

	/// <summary>
	/// Adds an image or images to an image list, generating a mask from the specified bitmap.
	/// int ImageList_AddMasked(
	///   HIMAGELIST himl,
	///   HBITMAP    hbmImage,
	///   COLORREF   crMask
	/// );
	/// </summary>
	[DllModuleExport(42, Version = "5.81.4916.400")]
	private uint ImageList_AddMasked(uint himl, uint hbmImage, uint crMask)
	{
		_logger.LogInformation("[Comctl32] ImageList_AddMasked(himl=0x{Himl:X8}, hbmImage=0x{HbmImage:X8}, crMask=0x{CrMask:X8})",
			himl, hbmImage, crMask);

		if (_imageLists.TryGetValue(himl, out var imageList))
		{
			var index = imageList.Images.Count;
			imageList.Images.Add(hbmImage);
			return (uint)index;
		}

		return 0xFFFFFFFF; // -1 on error
	}

	[DllModuleExport(75, Version = "5.81.4916.400")]
	private uint ImageList_ReplaceIcon(uint himl, int i, uint hicon)
	{
		_logger.LogInformation("[Comctl32] ImageList_ReplaceIcon(himl=0x{Himl:X8}, i={I}, hicon=0x{Hicon:X8})",
			himl, i, hicon);

		if (_imageLists.TryGetValue(himl, out var imageList))
		{
			if (i == -1)
			{
				// Add new icon
				var index = imageList.Images.Count;
				imageList.Images.Add(hicon);
				return (uint)index;
			}
			else if (i >= 0 && i < imageList.Images.Count)
			{
				// Replace existing icon
				imageList.Images[i] = hicon;
				return (uint)i;
			}
		}

		return 0xFFFFFFFF; // -1 on error
	}

	/// <summary>
	/// Destroys an image list.
	/// BOOL ImageList_Destroy(
	///   HIMAGELIST himl
	/// );
	/// </summary>
	[DllModuleExport(46, Version = "5.81.4916.400")]
	private uint ImageList_Destroy(uint himl)
	{
		_logger.LogInformation("[Comctl32] ImageList_Destroy(himl=0x{Himl:X8})", himl);

		if (_imageLists.Remove(himl))
		{
			return 1; // TRUE
		}

		return 0; // FALSE
	}

	/// <summary>
	/// Ordinal 17 - InitCommonControls or similar initialization function.
	/// void InitCommonControls();
	/// </summary>
	[DllModuleExport(17, Version = "5.81.4916.400")]
	private uint InitCommonControls(uint param1, uint param2, uint param3)
	{
		_logger.LogInformation("[Comctl32] Ordinal_17(param1=0x{Param1:X8}, param2=0x{Param2:X8}, param3=0x{Param3:X8})",
			param1, param2, param3);

		// InitCommonControls typically doesn't return a value, but we return success
		return 1;
	}

	/// <summary>
	/// Creates a property sheet page.
	/// HPROPSHEETPAGE CreatePropertySheetPageA(LPCPROPSHEETPAGEA lppsp);
	/// </summary>
	[DllModuleExport(19, Version = "5.81.4916.400")]
	private uint CreatePropertySheetPageA(uint lppsp)
	{
		_logger.LogInformation("[Comctl32] CreatePropertySheetPageA(lppsp=0x{Lppsp:X8})", lppsp);

		// Stub: Return a fake handle for the property sheet page
		// A real implementation would parse the PROPSHEETPAGE structure and create a page
		return lppsp != 0 ? 0x90010000 : 0;
	}

	/// <summary>
	/// Destroys a property sheet page.
	/// BOOL DestroyPropertySheetPage(HPROPSHEETPAGE hPSPage);
	/// </summary>
	[DllModuleExport(24, Version = "5.81.4916.400")]
	private uint DestroyPropertySheetPage(uint hPSPage)
	{
		_logger.LogInformation("[Comctl32] DestroyPropertySheetPage(hPSPage=0x{HPSPage:X8})", hPSPage);

		// Stub: Always return TRUE (success)
		return 1;
	}

	/// <summary>
	/// Creates and displays a property sheet.
	/// INT_PTR PropertySheetA(LPCPROPSHEETHEADERA lppsh);
	/// </summary>
	[DllModuleExport(88, Version = "5.81.4916.400")]
	private uint PropertySheetA(uint lppsh)
	{
		_logger.LogInformation("[Comctl32] PropertySheetA(lppsh=0x{Lppsh:X8})", lppsh);

		// Stub: Return 0 (user cancelled or closed the property sheet)
		// A real implementation would parse PROPSHEETHEADER and display the property sheet dialog
		return 0;
	}

	/// <summary>
	/// Loads an image list from a bitmap, cursor, or icon resource.
	/// HIMAGELIST ImageList_LoadImageA(
	///   [in] HINSTANCE hi,
	///   [in] LPCSTR    lpbmp,
	///   [in] int       cx,
	///   [in] int       cGrow,
	///   [in] COLORREF  crMask,
	///   [in] UINT      uType,
	///   [in] UINT      uFlags
	/// );
	/// </summary>
	[DllModuleExport(65, Version = "5.81.4916.400")]
	private uint ImageList_LoadImageA(uint hi, in LpcStr lpbmpPtr, int cx, int cGrow, uint crMask, uint uType, uint uFlags)
	{
		var lpbmp = lpbmpPtr.ToString() ?? string.Empty;

		_logger.LogInformation("[Comctl32] ImageList_LoadImageA(hi=0x{Hi:X8}, lpbmp=\"{Lpbmp}\", cx={Cx}, cGrow={CGrow}, crMask=0x{CrMask:X8}, uType={UType}, uFlags=0x{UFlags:X8})",
			hi, lpbmp, cx, cGrow, crMask, uType, uFlags);

		// Create an image list with estimated dimensions
		var handle = _nextImageListHandle++;

		// Estimate height based on type - icons are typically square
		int cy = uType == (uint)NativeTypes.ImageType.IMAGE_ICON ? cx : 16; // Default height for bitmaps

		_imageLists[handle] = new ImageListData
		{
			Width = cx,
			Height = cy,
			Flags = uFlags,
			InitialCount = 1,
			GrowBy = cGrow
		};

		_logger.LogInformation("[Comctl32] ImageList_LoadImageA: Created image list handle 0x{Handle:X8}", handle);

		return handle;
	}

	/// <summary>
	/// Adds an image to the list of images used as overlay masks.
	/// BOOL ImageList_SetOverlayImage(
	///   [in] HIMAGELIST himl,
	///   [in] int        iImage,
	///   [in] int        iOverlay
	/// );
	/// </summary>
	[DllModuleExport(82, Version = "5.81.4916.400")]
	private uint ImageList_SetOverlayImage(uint himl, int iImage, int iOverlay)
	{
		_logger.LogInformation("[Comctl32] ImageList_SetOverlayImage(himl=0x{Himl:X8}, iImage={IImage}, iOverlay={IOverlay})",
			himl, iImage, iOverlay);

		// Overlay image indices are 1-based and limited to 1-15 (4 bits)
		if (iOverlay < 1 || iOverlay > 15)
		{
			_logger.LogWarning("[Comctl32] ImageList_SetOverlayImage: Invalid overlay index {IOverlay}", iOverlay);
			return 0; // FALSE
		}

		if (!_imageLists.TryGetValue(himl, out var imageList))
		{
			_logger.LogWarning("[Comctl32] ImageList_SetOverlayImage: Invalid image list handle 0x{Himl:X8}", himl);
			return 0; // FALSE
		}

		if (iImage < 0 || iImage >= imageList.Images.Count)
		{
			_logger.LogWarning("[Comctl32] ImageList_SetOverlayImage: Invalid image index {IImage}", iImage);
			return 0; // FALSE
		}

		// For stub implementation, just return success
		// A real implementation would store the overlay image mapping
		return 1; // TRUE
	}


	/// <summary>
	/// Sets the background color in an image list.
	/// </summary>
	[DllModuleExport(76, Version = "5.81.4916.400")]
	private uint ImageList_SetBkColor(uint himl, uint clrBk)
	{
		_logger.LogInformation("[Comctl32] ImageList_SetBkColor(himl=0x{Himl:X8}, clrBk=0x{ClrBk:X8})", himl, clrBk);
		return 0xFFFFFFFF; // CLR_NONE - transparent background
	}

	[DllModuleExport(1, Version = "5.81.4916.400", IsStub = true)]
	private uint MenuHelp(uint param1)
	{
		_logger.LogInformation("[Comctl32] MenuHelp(param1=0x{Param1:X8})", param1);
		return 1;
	}

	[DllModuleExport(8, Version = "5.81.4916.400", IsStub = true)]
	private uint GetEffectiveClientRect(uint param1, uint param2)
	{
		_logger.LogInformation("[Comctl32] GetEffectiveClientRect(param1=0x{Param1:X8}, param2=0x{Param2:X8})", param1, param2);
		return 1;
	}

	[DllModuleExport(6, Version = "5.81.4916.400", IsStub = true)]
	private uint CreateStatusWindowA(uint param1)
	{
		_logger.LogInformation("[Comctl32] CreateStatusWindowA(param1=0x{Param1:X8})", param1);
		return 1;
	}

	private class ImageListData
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public uint Flags { get; set; }
		public int InitialCount { get; set; }
		public int GrowBy { get; set; }
		public List<uint> Images { get; } = new();
	}
}
