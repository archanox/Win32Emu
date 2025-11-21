using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// RPCRT4.DLL module - provides RPC runtime functions.
/// </summary>
public partial class Rpcrt4Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Rpcrt4Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "RPCRT4.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "UUIDCREATE":
				returnValue = UuidCreate(a.UInt32(0));
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Creates a new UUID.
	/// RPC_STATUS UuidCreate(
	///   [out] UUID *Uuid
	/// );
	/// </summary>
	[DllModuleExport(1)]
	private uint UuidCreate(uint uuid)
	{
		LogUuidCreate(uuid);

		// UUID structure is 16 bytes:
		// typedef struct _GUID {
		//   DWORD Data1;    // 4 bytes
		//   WORD  Data2;    // 2 bytes
		//   WORD  Data3;    // 2 bytes
		//   BYTE  Data4[8]; // 8 bytes
		// } GUID, UUID;

		if (uuid == 0)
		{
			_logger.LogWarning("[Rpcrt4] UuidCreate: NULL pointer");
			return 1; // RPC_S_INVALID_ARG
		}

		// Generate a random UUID (version 4 - random)
		var guid = System.Guid.NewGuid();
		var guidBytes = guid.ToByteArray();

		// Write the UUID to memory
		for (uint i = 0; i < 16; i++)
		{
			_env.MemWrite8(uuid + i, guidBytes[i]);
		}

		_logger.LogDebug("[Rpcrt4] UuidCreate: Created UUID {Guid}", guid);

		return 0; // RPC_S_OK
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Rpcrt4] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Rpcrt4] UuidCreate(uuid=0x{Uuid:X8})")]
	partial void LogUuidCreate(uint uuid);
}
