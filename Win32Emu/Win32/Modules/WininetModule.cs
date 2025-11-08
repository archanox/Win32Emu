using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// WININET.DLL module - provides Internet functions.
/// This is a stub implementation that returns success for basic functions.
/// </summary>
public partial class WininetModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public WininetModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "WININET.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "INTERNETOPENA":
				returnValue = InternetOpenA(a.LpcStr(0), a.UInt32(1), a.LpcStr(2), a.LpcStr(3), a.UInt32(4));
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Initializes an application's use of the WinINet functions.
	/// HINTERNET InternetOpenA(
	///   LPCSTR lpszAgent,
	///   DWORD  dwAccessType,
	///   LPCSTR lpszProxy,
	///   LPCSTR lpszProxyBypass,
	///   DWORD  dwFlags
	/// );
	/// </summary>
	/// <returns>Handle to internet session (stub - returns 0 to indicate offline)</returns>
	[DllModuleExport(20)]
	private uint InternetOpenA(in LpcStr lpszAgent, uint dwAccessType, in LpcStr lpszProxy, in LpcStr lpszProxyBypass, uint dwFlags)
	{
		var agent = lpszAgent.ToString() ?? "(null)";
		var proxy = lpszProxy.ToString() ?? "(null)";
		var proxyBypass = lpszProxyBypass.ToString() ?? "(null)";

		LogInternetOpenA(agent, dwAccessType, proxy, proxyBypass, dwFlags);

		// Return NULL to indicate offline mode or failure
		// Applications should handle this gracefully and work in offline mode
		return 0;
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Wininet] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Wininet] InternetOpenA(agent=\"{Agent}\", accessType={AccessType}, proxy=\"{Proxy}\", proxyBypass=\"{ProxyBypass}\", flags=0x{Flags:X}) -> NULL (offline mode)")]
	partial void LogInternetOpenA(string agent, uint accessType, string proxy, string proxyBypass, uint flags);
}
