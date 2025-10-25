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

			case "ORDINAL_17":
				returnValue = Ordinal_17(a.UInt32(0), a.UInt32(1), a.UInt32(2));
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
	[DllModuleExport(4)]
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
	[DllModuleExport(4)]
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

	[DllModuleExport(1)]
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
	[DllModuleExport(4)]
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
	[DllModuleExport(0)]
	private uint Ordinal_17(uint param1, uint param2, uint param3)
	{
		_logger.LogInformation("[Comctl32] Ordinal_17(param1=0x{Param1:X8}, param2=0x{Param2:X8}, param3=0x{Param3:X8})",
			param1, param2, param3);
		
		// InitCommonControls typically doesn't return a value, but we return success
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
