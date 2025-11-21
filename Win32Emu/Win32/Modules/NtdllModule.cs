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

	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] RtlExitUserProcess(exitCode={ExitCode})")]
	partial void LogRtlExitUserProcess(uint exitCode);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Ntdll] RtlUnwind(targetFrame=0x{TargetFrame:X8}, targetIp=0x{TargetIp:X8}, exceptionRecord=0x{ExceptionRecord:X8}, returnValue=0x{ReturnValue:X8})")]
	partial void LogRtlUnwind(uint targetFrame, uint targetIp, uint exceptionRecord, uint returnValue);
}
