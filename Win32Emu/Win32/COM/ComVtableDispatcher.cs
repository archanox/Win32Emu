using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.COM;

/// <summary>
/// Dispatcher for COM vtable method calls
/// Handles calls to COM interface methods at addresses 0x0E000000-0x0EFFFFFF
/// </summary>
public class ComVtableDispatcher
{
	private readonly ProcessEnvironment _env;
	private readonly ILogger _logger;
	
	// Base address for COM vtable stubs
	private const uint COM_VTABLE_BASE = 0x0D000000;
	private const uint COM_VTABLE_END = 0x0DFFFFFF;
	
	// Map of vtable stub addresses to handler functions
	private readonly Dictionary<uint, Func<ICpu, VirtualMemory, uint>> _vtableHandlers = new();
	
	// Track allocated COM objects
	private readonly Dictionary<uint, ComObjectInfo> _comObjects = new();
	private uint _nextObjectId = 1;
	
	public ComVtableDispatcher(ProcessEnvironment env, ILogger? logger = null)
	{
		_env = env;
		_logger = logger ?? NullLogger.Instance;
	}
	
	/// <summary>
	/// Check if an address is in the COM vtable range
	/// </summary>
	public bool IsComVtableAddress(uint address)
	{
		return address >= COM_VTABLE_BASE && address <= COM_VTABLE_END;
	}
	
	/// <summary>
	/// Try to invoke a COM vtable method
	/// </summary>
	public bool TryInvoke(uint address, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		
		if (!IsComVtableAddress(address))
		{
			return false;
		}
		
		if (_vtableHandlers.TryGetValue(address, out var handler))
		{
			returnValue = handler(cpu, memory);
			return true;
		}
		
		_logger.LogWarning($"[COM] Unhandled COM vtable call at 0x{address:X8}");
		return false;
	}
	
	/// <summary>
	/// Create a COM object with a vtable
	/// </summary>
	public uint CreateComObject(string interfaceName, Dictionary<string, Func<ICpu, VirtualMemory, uint>> methods)
	{
		var objectId = _nextObjectId++;
		
		// Allocate memory for the COM object structure
		// COM object layout: [vtable pointer][object data...]
		var objectAddr = _env.SimpleAlloc(8); // 4 bytes for vtable ptr + 4 bytes for object data
		
		// Allocate memory for the vtable
		var vtableSize = (uint)(methods.Count * 4); // 4 bytes per method pointer
		var vtableAddr = _env.SimpleAlloc(vtableSize);
		
		// Write vtable pointer to object
		_env.MemWrite32(objectAddr, vtableAddr);
		
		// Create vtable stubs and write function pointers
		var stubAddr = COM_VTABLE_BASE + (objectId * 0x1000); // Each object gets 4KB of address space
		uint methodIndex = 0;
		
		foreach (var kvp in methods)
		{
			var methodName = kvp.Key;
			var handler = kvp.Value;
			
			// Calculate stub address for this method
			var methodStubAddr = stubAddr + (methodIndex * 0x10); // 16 bytes per stub
			
			// Write function pointer to vtable
			_env.MemWrite32(vtableAddr + (methodIndex * 4), methodStubAddr);
			
			// Create INT3 stub at the method address
			var stub = new byte[] 
			{ 
				0xCC, // INT3 - breakpoint instruction
				0x90, 0x90, 0x90, // NOP padding
				0x90, 0x90, 0x90, 0x90,
				0x90, 0x90, 0x90, 0x90,
				0x90, 0x90, 0x90, 0x90
			};
			_env.MemWriteBytes(methodStubAddr, stub);
			
			// Register the handler
			_vtableHandlers[methodStubAddr] = handler;
			
			_logger.LogDebug($"[COM] {interfaceName}::{methodName} -> 0x{methodStubAddr:X8}");
			
			methodIndex++;
		}
		
		var objInfo = new ComObjectInfo
		{
			ObjectId = objectId,
			ObjectAddress = objectAddr,
			VtableAddress = vtableAddr,
			InterfaceName = interfaceName
		};
		
		_comObjects[objectAddr] = objInfo;
		
		_logger.LogInformation($"[COM] Created {interfaceName} object at 0x{objectAddr:X8} (vtable at 0x{vtableAddr:X8})");
		
		return objectAddr;
	}
	
	private sealed class ComObjectInfo
	{
		public uint ObjectId { get; set; }
		public uint ObjectAddress { get; set; }
		public uint VtableAddress { get; set; }
		public string InterfaceName { get; set; } = string.Empty;
	}
}
