using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class Wsock32Module : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public Wsock32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "WSOCK32.DLL";

		public unsafe bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "ACCEPT":
					returnValue = accept(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "BIND":
					returnValue = bind(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "CLOSESOCKET":
					returnValue = closesocket(a.UInt32(0));
					return true;
				case "CONNECT":
					returnValue = connect(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "GETPEERNAME":
					returnValue = getpeername(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GETSOCKNAME":
					returnValue = getsockname(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "IOCTLSOCKET":
					returnValue = ioctlsocket(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "LISTEN":
					returnValue = listen(a.UInt32(0), a.Int32(1));
					return true;
				case "RECV":
					returnValue = recv(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3));
					return true;
				case "RECVFROM":
					returnValue = recvfrom(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3), a.UInt32(4), a.UInt32(5));
					return true;
				case "SELECT":
					returnValue = select(a.Int32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "SEND":
					returnValue = send(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3));
					return true;
				case "SENDTO":
					returnValue = sendto(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3), a.UInt32(4), a.Int32(5));
					return true;
				case "SETSOCKOPT":
					returnValue = setsockopt(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.Int32(4));
					return true;
				case "SHUTDOWN":
					returnValue = shutdown(a.UInt32(0), a.Int32(1));
					return true;
				case "SOCKET":
					returnValue = socket(a.Int32(0), a.Int32(1), a.Int32(2));
					return true;
				case "WSAGETLASTERROR":
					returnValue = WSAGetLastError();
					return true;
				case "WSASTARTUP":
					returnValue = (uint)WSAStartup((ushort)(a.UInt32(0) & 0xFFFF), a.UInt32(1));
					return true;

				default:
					_logger.LogInformation("[WSOCK32] Unimplemented export: {Export}", export);
					return false;
			}
		}

		[DllModuleExport(12)]
		private uint accept(uint s, uint addr, uint addrlen)
		{
			_logger.LogInformation("[WSOCK32] accept(s=0x{S:X8}, addr=0x{Addr:X8}, addrlen=0x{Addrlen:X8})", s, addr, addrlen);
			// Return INVALID_SOCKET
			return 0xFFFFFFFF;
		}

		[DllModuleExport(12)]
		private uint bind(uint s, uint addr, int namelen)
		{
			_logger.LogInformation("[WSOCK32] bind(s=0x{S:X8}, addr=0x{Addr:X8}, namelen={Namelen})", s, addr, namelen);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(4)]
		private uint closesocket(uint s)
		{
			_logger.LogInformation("[WSOCK32] closesocket(s=0x{S:X8})", s);
			return 0; // Success
		}

		[DllModuleExport(12)]
		private uint connect(uint s, uint name, int namelen)
		{
			_logger.LogInformation("[WSOCK32] connect(s=0x{S:X8}, name=0x{Name:X8}, namelen={Namelen})", s, name, namelen);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(12)]
		private uint getpeername(uint s, uint name, uint namelen)
		{
			_logger.LogInformation("[WSOCK32] getpeername(s=0x{S:X8}, name=0x{Name:X8}, namelen=0x{Namelen:X8})", s, name, namelen);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(12)]
		private uint getsockname(uint s, uint name, uint namelen)
		{
			_logger.LogInformation("[WSOCK32] getsockname(s=0x{S:X8}, name=0x{Name:X8}, namelen=0x{Namelen:X8})", s, name, namelen);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(12)]
		private uint ioctlsocket(uint s, int cmd, uint argp)
		{
			_logger.LogInformation("[WSOCK32] ioctlsocket(s=0x{S:X8}, cmd={Cmd}, argp=0x{Argp:X8})", s, cmd, argp);
			return 0; // Success
		}

		[DllModuleExport(8)]
		private uint listen(uint s, int backlog)
		{
			_logger.LogInformation("[WSOCK32] listen(s=0x{S:X8}, backlog={Backlog})", s, backlog);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(16)]
		private uint recv(uint s, uint buf, int len, int flags)
		{
			_logger.LogInformation("[WSOCK32] recv(s=0x{S:X8}, buf=0x{Buf:X8}, len={Len}, flags={Flags})", s, buf, len, flags);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(24)]
		private uint recvfrom(uint s, uint buf, int len, int flags, uint from, uint fromlen)
		{
			_logger.LogInformation("[WSOCK32] recvfrom(s=0x{S:X8}, buf=0x{Buf:X8}, len={Len}, flags={Flags})", s, buf, len, flags);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(20)]
		private uint select(int nfds, uint readfds, uint writefds, uint exceptfds, uint timeout)
		{
			_logger.LogInformation("[WSOCK32] select(nfds={Nfds}, readfds=0x{Readfds:X8}, writefds=0x{Writefds:X8}, exceptfds=0x{Exceptfds:X8})",
				nfds, readfds, writefds, exceptfds);
			// Return 0 (no sockets ready)
			return 0;
		}

		[DllModuleExport(16)]
		private uint send(uint s, uint buf, int len, int flags)
		{
			_logger.LogInformation("[WSOCK32] send(s=0x{S:X8}, buf=0x{Buf:X8}, len={Len}, flags={Flags})", s, buf, len, flags);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(24)]
		private uint sendto(uint s, uint buf, int len, int flags, uint to, int tolen)
		{
			_logger.LogInformation("[WSOCK32] sendto(s=0x{S:X8}, buf=0x{Buf:X8}, len={Len}, flags={Flags})", s, buf, len, flags);
			// Return SOCKET_ERROR
			return 0xFFFFFFFF;
		}

		[DllModuleExport(20)]
		private uint setsockopt(uint s, int level, int optname, uint optval, int optlen)
		{
			_logger.LogInformation("[WSOCK32] setsockopt(s=0x{S:X8}, level={Level}, optname={Optname})", s, level, optname);
			return 0; // Success
		}

		[DllModuleExport(8)]
		private uint shutdown(uint s, int how)
		{
			_logger.LogInformation("[WSOCK32] shutdown(s=0x{S:X8}, how={How})", s, how);
			return 0; // Success
		}

		[DllModuleExport(12)]
		private uint socket(int af, int type, int protocol)
		{
			_logger.LogInformation("[WSOCK32] socket(af={Af}, type={Type}, protocol={Protocol})", af, type, protocol);
			// Return a pseudo socket handle
			return 0xBEEF0000 + (uint)Random.Shared.Next(0x1000);
		}

		[DllModuleExport(0)]
		private uint WSAGetLastError()
		{
			_logger.LogInformation("[WSOCK32] WSAGetLastError()");
			// Return no error
			return 0;
		}

		[DllModuleExport(8)]
		private int WSAStartup(ushort wVersionRequested, uint lpWSAData)
		{
			_logger.LogInformation("[WSOCK32] WSAStartup(wVersionRequested=0x{WVersionRequested:X4}, lpWSAData=0x{LpWSAData:X8})",
				wVersionRequested, lpWSAData);
			// Fill in WSADATA structure (if provided)
			if (lpWSAData != 0)
			{
				_env.MemWrite16(lpWSAData, 0x0202); // wVersion = 2.2
				_env.MemWrite16(lpWSAData + 2, 0x0202); // wHighVersion = 2.2
				// Fill in description string at offset 4 (max 257 bytes)
				var description = "WinSock 2.2\0";
				for (int i = 0; i < description.Length; i++)
				{
					_env.MemWrite8(lpWSAData + 4 + (uint)i, (byte)description[i]);
				}
			}
			return 0; // Success
		}
	}
}
