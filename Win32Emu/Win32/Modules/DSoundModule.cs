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
					_logger.LogInformation("[DSound] Unimplemented export: {Export}", export);
					return false;
			}
		}

private unsafe uint DirectSoundCreate(uint lpGuid, uint lplpDs, uint pUnkOuter)
{
_logger.LogInformation("[DSound] DirectSoundCreate(lpGuid=0x{LpGuid:X8}, lplpDS=0x{LplpDs:X8}, pUnkOuter=0x{PUnkOuter:X8})", lpGuid, lplpDs, pUnkOuter);

// Create DirectSound object with COM vtable
var dsHandle = _nextDSoundHandle++;
var dsObj = new DirectSoundObject
{
Handle = dsHandle,
Frequency = 44100,
BitsPerSample = 16,
Channels = 2
};
_dsoundObjects[dsHandle] = dsObj;

// Initialize audio backend if not already done
if (_env.AudioBackend == null)
{
_env.AudioBackend = new Sdl3AudioBackend();
_env.AudioBackend.Initialize();
}

// Create COM vtable for IDirectSound interface
var vtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
{
{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
{ "CreateSoundBuffer", (cpu, mem) => DSound_CreateSoundBuffer(cpu, mem, dsHandle) },
{ "GetCaps", (cpu, mem) => DSound_GetCaps(cpu, mem) },
{ "DuplicateSoundBuffer", (cpu, mem) => DSound_DuplicateSoundBuffer(cpu, mem) },
{ "SetCooperativeLevel", (cpu, mem) => DSound_SetCooperativeLevel(cpu, mem) },
{ "Compact", (cpu, mem) => DSound_Compact(cpu, mem) },
{ "GetSpeakerConfig", (cpu, mem) => DSound_GetSpeakerConfig(cpu, mem) },
{ "SetSpeakerConfig", (cpu, mem) => DSound_SetSpeakerConfig(cpu, mem) },
{ "Initialize", (cpu, mem) => DSound_Initialize(cpu, mem) }
};

// Create the COM object with vtable
var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectSound", vtableMethods);

// Write COM object pointer to output parameter
if (lplpDs != 0)
{
_env.MemWrite32(lplpDs, comObjectAddr);
}

_logger.LogInformation("[DSound] Created IDirectSound COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
return 0; // DS_OK
}


		private unsafe uint DirectSoundEnumerateA(uint lpDsEnumCallback, uint lpContext)
		{
			_logger.LogInformation("[DSound] DirectSoundEnumerateA(lpDSEnumCallback=0x{LpDsEnumCallback:X8}, lpContext=0x{LpContext:X8})", lpDsEnumCallback, lpContext);

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

		// COM interface methods for IDirectSound
		private uint ComQueryInterface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var riid = args.UInt32(1);
			var ppvObject = args.UInt32(2);
			
			_logger.LogInformation("[DSound COM] IUnknown::QueryInterface(this=0x{ThisPtr:X8}, riid=0x{Riid:X8}, ppvObject=0x{PpvObject:X8})", thisPtr, riid, ppvObject);
			
			// E_NOINTERFACE = 0x80004002
			return 0x80004002;
		}

		private uint ComAddRef(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			
			_logger.LogInformation("[DSound COM] IUnknown::AddRef(this=0x{ThisPtr:X8})", thisPtr);
			return 1; // Reference count
		}

		private uint ComRelease(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			
			_logger.LogInformation("[DSound COM] IUnknown::Release(this=0x{ThisPtr:X8})", thisPtr);
			return 0; // Reference count after release
		}

		private uint DSound_CreateSoundBuffer(ICpu cpu, VirtualMemory memory, uint dsHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var pcDSBufferDesc = args.UInt32(1);
			var lplpDirectSoundBuffer = args.UInt32(2);
			var pUnkOuter = args.UInt32(3);
			
			_logger.LogInformation("[DSound COM] IDirectSound::CreateSoundBuffer(this=0x{ThisPtr:X8}, pcDSBufferDesc=0x{PcDsBufferDesc:X8}, lplpDSBuffer=0x{LplpDirectSoundBuffer:X8}, pUnkOuter=0x{PUnkOuter:X8})", thisPtr, pcDSBufferDesc, lplpDirectSoundBuffer, pUnkOuter);
			
			// Create a sound buffer COM object with its own vtable
			var bufferHandle = _nextBufferHandle++;
			var bufferObj = new DirectSoundBuffer
			{
				Handle = bufferHandle,
				Size = 0,
				IsPrimary = false
			};
			_buffers[bufferHandle] = bufferObj;

			// Create COM vtable for IDirectSoundBuffer interface
			var bufferMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "GetCaps", (cpu, mem) => DSoundBuffer_GetCaps(cpu, mem) },
				{ "GetCurrentPosition", (cpu, mem) => DSoundBuffer_GetCurrentPosition(cpu, mem) },
				{ "GetFormat", (cpu, mem) => DSoundBuffer_GetFormat(cpu, mem) },
				{ "GetVolume", (cpu, mem) => DSoundBuffer_GetVolume(cpu, mem) },
				{ "GetPan", (cpu, mem) => DSoundBuffer_GetPan(cpu, mem) },
				{ "GetFrequency", (cpu, mem) => DSoundBuffer_GetFrequency(cpu, mem) },
				{ "GetStatus", (cpu, mem) => DSoundBuffer_GetStatus(cpu, mem) },
				{ "Initialize", (cpu, mem) => DSoundBuffer_Initialize(cpu, mem) },
				{ "Lock", (cpu, mem) => DSoundBuffer_Lock(cpu, mem) },
				{ "Play", (cpu, mem) => DSoundBuffer_Play(cpu, mem) },
				{ "SetCurrentPosition", (cpu, mem) => DSoundBuffer_SetCurrentPosition(cpu, mem) },
				{ "SetFormat", (cpu, mem) => DSoundBuffer_SetFormat(cpu, mem) },
				{ "SetVolume", (cpu, mem) => DSoundBuffer_SetVolume(cpu, mem) },
				{ "SetPan", (cpu, mem) => DSoundBuffer_SetPan(cpu, mem) },
				{ "SetFrequency", (cpu, mem) => DSoundBuffer_SetFrequency(cpu, mem) },
				{ "Stop", (cpu, mem) => DSoundBuffer_Stop(cpu, mem) },
				{ "Unlock", (cpu, mem) => DSoundBuffer_Unlock(cpu, mem) },
				{ "Restore", (cpu, mem) => DSoundBuffer_Restore(cpu, mem) }
			};

			var bufferComAddr = _env.ComDispatcher.CreateComObject("IDirectSoundBuffer", bufferMethods);

			if (lplpDirectSoundBuffer != 0)
			{
				_env.MemWrite32(lplpDirectSoundBuffer, bufferComAddr);
			}

			_logger.LogInformation("[DSound COM] Created IDirectSoundBuffer COM object at 0x{BufferComAddr:X8}", bufferComAddr);
			return 0; // DS_OK
		}

		private uint DSound_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::GetCaps() - stub");
			return 0; // DS_OK
		}

		private uint DSound_DuplicateSoundBuffer(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::DuplicateSoundBuffer() - stub");
			return 0; // DS_OK
		}

		private uint DSound_SetCooperativeLevel(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var hwnd = args.UInt32(1);
			var dwLevel = args.UInt32(2);
			
			_logger.LogInformation("[DSound COM] IDirectSound::SetCooperativeLevel(this=0x{ThisPtr:X8}, hwnd=0x{Hwnd:X8}, level=0x{DwLevel:X8}) - stub", thisPtr, hwnd, dwLevel);
			return 0; // DS_OK
		}

		private uint DSound_Compact(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::Compact() - stub");
			return 0; // DS_OK
		}

		private uint DSound_GetSpeakerConfig(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::GetSpeakerConfig() - stub");
			return 0; // DS_OK
		}

		private uint DSound_SetSpeakerConfig(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::SetSpeakerConfig() - stub");
			return 0; // DS_OK
		}

		private uint DSound_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSound::Initialize() - stub");
			return 0; // DS_OK
		}

		// IDirectSoundBuffer COM methods
		private uint DSoundBuffer_GetCaps(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetCaps() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetCurrentPosition(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetCurrentPosition() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetFormat(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetFormat() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetVolume(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetVolume() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetPan(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetPan() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetFrequency(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetFrequency() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_GetStatus(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::GetStatus() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Initialize() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Lock(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Lock() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Play(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Play() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetCurrentPosition(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetCurrentPosition() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetFormat(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetFormat() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetVolume(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetVolume() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetPan(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetPan() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_SetFrequency(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::SetFrequency() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Stop(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Stop() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Unlock(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Unlock() - stub");
			return 0; // DS_OK
		}

		private uint DSoundBuffer_Restore(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DSound COM] IDirectSoundBuffer::Restore() - stub");
			return 0; // DS_OK
		}
	}
}