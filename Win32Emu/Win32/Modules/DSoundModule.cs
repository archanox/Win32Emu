using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;

namespace Win32Emu.Win32.Modules
{
	public class DSoundModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
	{
		public string Name => "DSOUND.DLL";

		// DirectSound object handles
		private readonly Dictionary<uint, DirectSoundObject> _dsoundObjects = new();
		private readonly Dictionary<uint, DirectSoundBuffer> _buffers = new();
		private uint _nextDSoundHandle = 0x80000000;
		private uint _nextBufferHandle = 0x81000000;

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTSOUNDCREATE":
					returnValue = DirectSoundCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "DIRECTSOUNDENUMERATEA":
					returnValue = DirectSoundEnumerateA(a.UInt32(0), a.UInt32(1));
					return true;
				default:
					Console.WriteLine($"[DSound] Unimplemented export: {export}");
					return false;
			}
		}

		private unsafe uint DirectSoundCreate(uint lpGuid, uint lplpDs, uint pUnkOuter)
		{
			Console.WriteLine($"[DSound] DirectSoundCreate(lpGuid=0x{lpGuid:X8}, lplpDS=0x{lplpDs:X8}, pUnkOuter=0x{pUnkOuter:X8})");

			// Create DirectSound object
			var dsHandle = _nextDSoundHandle++;
			var dsObj = new DirectSoundObject
			{
				Handle = dsHandle
			};
			_dsoundObjects[dsHandle] = dsObj;

			// Initialize audio backend if not already done
			if (env.AudioBackend == null)
			{
				env.AudioBackend = new Sdl3AudioBackend();
				env.AudioBackend.Initialize();
			}

			if (lplpDs != 0)
			{
				env.MemWrite32(lplpDs, dsHandle);
			}

			return 0; // DS_OK
		}

		private unsafe uint DirectSoundEnumerateA(uint lpDsEnumCallback, uint lpContext)
		{
			Console.WriteLine($"[DSound] DirectSoundEnumerateA(lpDSEnumCallback=0x{lpDsEnumCallback:X8}, lpContext=0x{lpContext:X8})");

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

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for DSound - alphabetically ordered
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{ "DIRECTSOUNDCREATE", 1 },
				{ "DIRECTSOUNDENUMERATEA", 2 }
			};
			return exports;
		}
	}
}