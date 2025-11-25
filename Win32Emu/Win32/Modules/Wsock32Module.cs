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

				// Ordinal-based exports (used by some older programs)
				case "ORDINAL_2":
					returnValue = bind(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "ORDINAL_8":
					returnValue = getsockname(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "ORDINAL_9":
					returnValue = getsockopt(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "ORDINAL_10":
					returnValue = htonl(a.UInt32(0));
					return true;
				case "ORDINAL_11":
					returnValue = htons(a.UInt32(0));
					return true;
				case "ORDINAL_14":
					returnValue = listen(a.UInt32(0), a.Int32(1));
					return true;
				case "ORDINAL_15":
					returnValue = ntohl(a.UInt32(0));
					return true;
				case "ORDINAL_109":
					WSASetLastError(a.Int32(0));
					returnValue = 0;
					return true;
				case "ORDINAL_110":
					returnValue = WSAGetLastError();
					return true;
				case "ORDINAL_113":
					returnValue = __WSAFDIsSet(a.UInt32(0), a.UInt32(1));
					return true;

				// Named versions of the above ordinals
				case "HTONL":
					returnValue = htonl(a.UInt32(0));
					return true;
				case "HTONS":
					returnValue = htons(a.UInt32(0));
					return true;
				case "NTOHL":
					returnValue = ntohl(a.UInt32(0));
					return true;
				case "NTOHS":
					returnValue = ntohs(a.UInt32(0));
					return true;
				case "GETSOCKOPT":
					returnValue = getsockopt(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "WSASETLASTERROR":
					WSASetLastError(a.Int32(0));
					returnValue = 0;
					return true;
				case "__WSAFDISSET":
					returnValue = __WSAFDIsSet(a.UInt32(0), a.UInt32(1));
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

		// Track WSA last error
		private int _wsaLastError = 0;

		[DllModuleExport(0)]
		private uint WSAGetLastError()
		{
			_logger.LogInformation("[WSOCK32] WSAGetLastError() -> {WsaLastError}", _wsaLastError);
			return (uint)_wsaLastError;
		}

		/// <summary>
		/// Sets the error code that can be retrieved through WSAGetLastError.
		/// </summary>
		[DllModuleExport(4)]
		private void WSASetLastError(int iError)
		{
			_logger.LogInformation("[WSOCK32] WSASetLastError(iError={IError})", iError);
			_wsaLastError = iError;
		}

		/// <summary>
		/// Gets a socket option.
		/// </summary>
		[DllModuleExport(20)]
		private uint getsockopt(uint s, int level, int optname, uint optval, uint optlen)
		{
			_logger.LogInformation("[WSOCK32] getsockopt(s=0x{S:X8}, level={Level}, optname={Optname}, optval=0x{Optval:X8}, optlen=0x{Optlen:X8})",
				s, level, optname, optval, optlen);
			// Return SOCKET_ERROR
			_wsaLastError = 10014; // WSAEFAULT
			return 0xFFFFFFFF;
		}

		/// <summary>
		/// Converts a u_long from host to network byte order (big-endian).
		/// </summary>
		[DllModuleExport(4)]
		private uint htonl(uint hostlong)
		{
			_logger.LogInformation("[WSOCK32] htonl(hostlong=0x{Hostlong:X8})", hostlong);
			// Convert from host to network byte order (big-endian)
			return ((hostlong & 0x000000FF) << 24) |
			       ((hostlong & 0x0000FF00) << 8) |
			       ((hostlong & 0x00FF0000) >> 8) |
			       ((hostlong & 0xFF000000) >> 24);
		}

		/// <summary>
		/// Converts a u_short from host to network byte order (big-endian).
		/// </summary>
		[DllModuleExport(4)]
		private uint htons(uint hostshort)
		{
			_logger.LogInformation("[WSOCK32] htons(hostshort=0x{Hostshort:X4})", hostshort);
			// Convert from host to network byte order (big-endian)
			var s = (ushort)hostshort;
			return (uint)(((s & 0x00FF) << 8) | ((s & 0xFF00) >> 8));
		}

		/// <summary>
		/// Converts a u_long from network to host byte order.
		/// </summary>
		[DllModuleExport(4)]
		private uint ntohl(uint netlong)
		{
			_logger.LogInformation("[WSOCK32] ntohl(netlong=0x{Netlong:X8})", netlong);
			// Convert from network to host byte order (same as htonl on little-endian)
			return ((netlong & 0x000000FF) << 24) |
			       ((netlong & 0x0000FF00) << 8) |
			       ((netlong & 0x00FF0000) >> 8) |
			       ((netlong & 0xFF000000) >> 24);
		}

		/// <summary>
		/// Converts a u_short from network to host byte order.
		/// </summary>
		[DllModuleExport(4)]
		private uint ntohs(uint netshort)
		{
			_logger.LogInformation("[WSOCK32] ntohs(netshort=0x{Netshort:X4})", netshort);
			// Convert from network to host byte order (same as htons on little-endian)
			var s = (ushort)netshort;
			return (uint)(((s & 0x00FF) << 8) | ((s & 0xFF00) >> 8));
		}

		/// <summary>
		/// Determines whether a socket is a member of a fd_set structure.
		/// </summary>
		[DllModuleExport(8)]
		private uint __WSAFDIsSet(uint fd, uint set)
		{
			_logger.LogInformation("[WSOCK32] __WSAFDIsSet(fd=0x{Fd:X8}, set=0x{Set:X8})", fd, set);

			if (set == 0)
			{
				return 0; // Not in set (set is null)
			}

			// fd_set structure:
			// u_int fd_count (4 bytes)
			// SOCKET fd_array[FD_SETSIZE] (variable)
			var fd_count = _env.MemRead32(set);
			for (uint i = 0; i < fd_count; i++)
			{
				var socket = _env.MemRead32(set + 4 + (i * 4));
				if (socket == fd)
				{
					return 1; // Found in set
				}
			}

			return 0; // Not found in set
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
