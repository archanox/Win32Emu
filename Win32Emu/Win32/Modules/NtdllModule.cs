using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// NTDLL.DLL module - provides low-level NT kernel functions.
/// This is the native API layer that sits below kernel32.dll.
/// </summary>
public partial class NtdllModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public NtdllModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "NTDLL.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "NTCURRENTTEB":
				returnValue = NtCurrentTeb();
				return true;

			case "NTALLOCATEVIRTUALMEMORY":
				returnValue = NtAllocateVirtualMemory(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;

			case "NTFREEVIRTUALMEMORY":
				returnValue = NtFreeVirtualMemory(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "NTQUERYOBJECT":
				returnValue = NtQueryObject(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			case "RTLEXITUSERPROCESS":
				RtlExitUserProcess(a.UInt32(0));
				return true;

			case "RTLUNWIND":
				RtlUnwind(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				returnValue = 0;
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Returns the address of the Thread Environment Block (TEB) for the current thread.
	/// Used by applications to access thread-local storage and other thread-specific information.
	/// </summary>
	[DllModuleExport(0)]
	private uint NtCurrentTeb()
	{
		var tebAddress = _env.TebAddress;
		LogNtCurrentTeb(tebAddress);
		return tebAddress;
	}

	/// <summary>
	/// Allocates a region of memory within the virtual address space of a process.
	/// NTSTATUS NtAllocateVirtualMemory(
	///   HANDLE ProcessHandle,
	///   PVOID *BaseAddress,
	///   ULONG_PTR ZeroBits,
	///   PSIZE_T RegionSize,
	///   ULONG AllocationType,
	///   ULONG Protect
	/// );
	/// </summary>
	[DllModuleExport(24, IsStub = true)]
	private uint NtAllocateVirtualMemory(uint ProcessHandle, uint BaseAddress, uint ZeroBits, uint RegionSize, uint AllocationType, uint Protect)
	{
		LogNtAllocateVirtualMemory(ProcessHandle, BaseAddress, ZeroBits, RegionSize, AllocationType, Protect);
		// Return STATUS_NOT_IMPLEMENTED
		return (uint)NativeTypes.NtStatus.STATUS_NOT_IMPLEMENTED;
	}

	/// <summary>
	/// Frees a region of memory within the virtual address space of a process.
	/// NTSTATUS NtFreeVirtualMemory(
	///   HANDLE ProcessHandle,
	///   PVOID *BaseAddress,
	///   PSIZE_T RegionSize,
	///   ULONG FreeType
	/// );
	/// </summary>
	[DllModuleExport(16, IsStub = true)]
	private uint NtFreeVirtualMemory(uint ProcessHandle, uint BaseAddress, uint RegionSize, uint FreeType)
	{
		LogNtFreeVirtualMemory(ProcessHandle, BaseAddress, RegionSize, FreeType);
		// Return STATUS_NOT_IMPLEMENTED
		return (uint)NativeTypes.NtStatus.STATUS_NOT_IMPLEMENTED;
	}

	/// <summary>
	/// Retrieves information about an object.
	/// NTSTATUS NtQueryObject(
	///   HANDLE Handle,
	///   OBJECT_INFORMATION_CLASS ObjectInformationClass,
	///   PVOID ObjectInformation,
	///   ULONG ObjectInformationLength,
	///   PULONG ReturnLength
	/// );
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint NtQueryObject(uint Handle, uint ObjectInformationClass, uint ObjectInformation, uint ObjectInformationLength, uint ReturnLength)
	{
		LogNtQueryObject(Handle, ObjectInformationClass, ObjectInformation, ObjectInformationLength, ReturnLength);
		
		// Write zero to ReturnLength if provided
		if (ReturnLength != 0)
		{
			_env.MemWrite32(ReturnLength, 0);
		}
		
		// Return STATUS_NOT_IMPLEMENTED
		return (uint)NativeTypes.NtStatus.STATUS_NOT_IMPLEMENTED;
	}

	/// <summary>
	/// Terminates the current process.
	/// Similar to ExitProcess but at the NT API level.
	/// </summary>
	[DllModuleExport(4)]
	private void RtlExitUserProcess(uint exitCode)
	{
		LogRtlExitUserProcess(exitCode);
		_env.RequestExit();
	}

	/// <summary>
	/// Initiates an unwind of the stack (exception handling).
	/// </summary>
	[DllModuleExport(16, IsStub = true)]
	private void RtlUnwind(uint targetFrame, uint targetIp, uint exceptionRecord, uint returnValue)
	{
		LogRtlUnwind(targetFrame, targetIp, exceptionRecord, returnValue);
		// Stub: exception unwinding not fully implemented
		// In a real implementation, this would:
		// 1. Walk the exception handler chain
		// 2. Call each handler with EXCEPTION_UNWINDING flag
		// 3. Restore stack frame and registers
		// 4. Jump to target IP
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Ntdll] NtCurrentTeb() -> 0x{TebAddress:X8}")]
	partial void LogNtCurrentTeb(uint tebAddress);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] NtAllocateVirtualMemory(ProcessHandle=0x{ProcessHandle:X8}, BaseAddress=0x{BaseAddress:X8}, ZeroBits={ZeroBits}, RegionSize=0x{RegionSize:X8}, AllocationType=0x{AllocationType:X8}, Protect=0x{Protect:X8})")]
	partial void LogNtAllocateVirtualMemory(uint ProcessHandle, uint BaseAddress, uint ZeroBits, uint RegionSize, uint AllocationType, uint Protect);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] NtFreeVirtualMemory(ProcessHandle=0x{ProcessHandle:X8}, BaseAddress=0x{BaseAddress:X8}, RegionSize=0x{RegionSize:X8}, FreeType=0x{FreeType:X8})")]
	partial void LogNtFreeVirtualMemory(uint ProcessHandle, uint BaseAddress, uint RegionSize, uint FreeType);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] NtQueryObject(Handle=0x{Handle:X8}, ObjectInformationClass={ObjectInformationClass}, ObjectInformation=0x{ObjectInformation:X8}, ObjectInformationLength={ObjectInformationLength}, ReturnLength=0x{ReturnLength:X8})")]
	partial void LogNtQueryObject(uint Handle, uint ObjectInformationClass, uint ObjectInformation, uint ObjectInformationLength, uint ReturnLength);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] RtlExitUserProcess(exitCode={ExitCode})")]
	partial void LogRtlExitUserProcess(uint exitCode);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] RtlUnwind(targetFrame=0x{TargetFrame:X8}, targetIp=0x{TargetIp:X8}, exceptionRecord=0x{ExceptionRecord:X8}, returnValue=0x{ReturnValue:X8})")]
	partial void LogRtlUnwind(uint targetFrame, uint targetIp, uint exceptionRecord, uint returnValue);
}
