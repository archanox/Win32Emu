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

	// RPC status codes
	private enum RpcStatus : uint
	{
		RPC_S_OK = 0,
		RPC_S_INVALID_ARG = 1,
	}

	// UUID/GUID size
	private const int UUID_SIZE = 16; // 16 bytes for a GUID/UUID

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

			case "UUIDFROMSTRINGA":
				returnValue = UuidFromStringA(a.UInt32(0), a.UInt32(1));
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
			return (uint)RpcStatus.RPC_S_INVALID_ARG;
		}

		// Generate a random UUID (version 4 - random)
		var guid = System.Guid.NewGuid();
		var guidBytes = guid.ToByteArray();

		// Write the UUID to memory
		for (uint i = 0; i < UUID_SIZE; i++)
		{
			_env.MemWrite8(uuid + i, guidBytes[i]);
		}

		_logger.LogDebug("[Rpcrt4] UuidCreate: Created UUID {Guid}", guid);

		return (uint)RpcStatus.RPC_S_OK;
	}

	/// <summary>
	/// Converts a string UUID to a binary UUID.
	/// RPC_STATUS UuidFromStringA(
	///   [in]  RPC_CSTR StringUuid,
	///   [out] UUID     *Uuid
	/// );
	/// </summary>
	[DllModuleExport(2)]
	private uint UuidFromStringA(uint stringUuid, uint uuid)
	{
		// Read the string UUID from memory
		if (stringUuid == 0 || uuid == 0)
		{
			_logger.LogWarning("[Rpcrt4] UuidFromStringA: NULL pointer");
			return (uint)RpcStatus.RPC_S_INVALID_ARG;
		}

		// Read the ANSI string from memory
		var uuidString = _env.ReadAnsiString(stringUuid);
		
		_logger.LogInformation("[Rpcrt4] UuidFromStringA(stringUuid=\"{UuidString}\", uuid=0x{Uuid:X8})",
			uuidString, uuid);

		// Try to parse the UUID string
		if (string.IsNullOrEmpty(uuidString) || !System.Guid.TryParse(uuidString, out var guid))
		{
			_logger.LogWarning("[Rpcrt4] UuidFromStringA: Invalid UUID string format");
			return (uint)RpcStatus.RPC_S_INVALID_ARG;
		}

		// Write the UUID to memory
		var guidBytes = guid.ToByteArray();
		for (uint i = 0; i < UUID_SIZE; i++)
		{
			_env.MemWrite8(uuid + i, guidBytes[i]);
		}

		_logger.LogDebug("[Rpcrt4] UuidFromStringA: Parsed UUID {Guid}", guid);

		return (uint)RpcStatus.RPC_S_OK;
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Rpcrt4] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Rpcrt4] UuidCreate(uuid=0x{Uuid:X8})")]
	partial void LogUuidCreate(uint uuid);
}
