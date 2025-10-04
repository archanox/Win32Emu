using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
namespace Win32Emu.Win32.Modules
{
	public class DSoundModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public DSoundModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}
		public string Name => "DSOUND.DLL";

		// DirectSound object handles
		private readonly Dictionary<uint, DirectSoundObject> _dsoundObjects = new();
		private readonly Dictionary<uint, DirectSoundBuffer> _buffers = new();
		private uint _nextDSoundHandle = 0x80000000;
		private uint _nextBufferHandle = 0x81000000;

		public bool TryInvokeUnsafe(string exp, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (exp.ToUpperInvariant())
			{
				case "DIRECTSOUNDCREATE":
					returnValue = DirectSoundCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "DIRECTSOUNDENUMERATEA":
					returnValue = DirectSoundEnumerateA(a.UInt32(0), a.UInt32(1));
					return true;
				default:
					_logger.LogInformation($"[DSound] Unimplemented export: {exp}");
					return false;
			}
		}

		private uint DirectSoundCreate(uint lpGuid, uint lplpDs, uint pUnkOuter)
		{
			_logger.LogInformation($"[DSound] DirectSoundCreate(lpGuid=0x{lpGuid:X8}, lplpDS=0x{lplpDs:X8}, pUnkOuter=0x{pUnkOuter:X8})");

			// Create DirectSound object
			var dsHandle = _nextDSoundHandle++;
			var dsObj = new DirectSoundObject
			{
				Handle = dsHandle
			};
			_dsoundObjects[dsHandle] = dsObj;

			// Initialize audio backend if not already done
			if (_env.AudioBackend == null)
			{
				_env.AudioBackend = new Sdl3AudioBackend();
				_env.AudioBackend.Initialize();
			}

			if (lplpDs != 0)
			{
				_env.MemWrite32(lplpDs, dsHandle);
			}

			return 0; // DS_OK
		}

		private uint DirectSoundEnumerateA(uint lpDsEnumCallback, uint lpContext)
		{
			_logger.LogInformation($"[DSound] DirectSoundEnumerateA(lpDSEnumCallback=0x{lpDsEnumCallback:X8}, lpContext=0x{lpContext:X8})");

			// For now, just report no devices found (return DS_OK)
			// In a full implementation, we would enumerate audio devices and call the callback
			return 0; // DS_OK
		}

		private sealed class DirectSoundObject
		{
			public uint Handle { get; set; }
			public int Frequency { get; set; } = 44100;
			public int BitsPerSample { get; set; } = 16;
			public int Channels { get; set; } = 2;
		}

		private sealed class DirectSoundBuffer
		{
			public uint Handle { get; set; }
			public uint AudioStreamId { get; set; }
			public int Size { get; set; }
			public byte[]? Data { get; set; }
			public bool IsPrimary { get; set; }
		}
	}
}