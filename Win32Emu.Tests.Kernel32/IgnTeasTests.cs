using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Kernel32
{
	public class IgnTeasTests
	{
		private readonly TestEnvironment _testEnv;

		public IgnTeasTests()
		{
			_testEnv = new TestEnvironment();
		}
		
		[Fact]
		public unsafe void IgnitionTeaser_ApiMonExpectedOutput()
		{
			var processEnv = new ProcessEnvironment(_testEnv.Memory);
			
			var version = _testEnv.CallKernel32Api("GETVERSION");
			//Assert.Equal((uint)602931718, version);
			
			//#	Time of Day	Thread	TID	Module	Category	API	Return Type	Return Value	Error	Duration
			// 5710	3:21:04.690 PM	1	20904	KERNEL32.DLL	Heaps	HeapCreate ( HEAP_NO_SERIALIZE, 4096, 0 )	HANDLE	0x0a4c0000		0.0001253
			// 5713	3:21:04.690 PM	1	20904	KERNEL32.DLL	Virtual Memory	VirtualAlloc ( NULL, 4194304, MEM_RESERVE, PAGE_READWRITE )	LPVOID	0x0a4d0000		0.0000080
			// 5716	3:21:04.690 PM	1	20904	KERNEL32.DLL	Virtual Memory	VirtualAlloc ( 0x0a4d0000, 65536, MEM_COMMIT, PAGE_READWRITE )	LPVOID	0x0a4d0000		0.0000360
			// 5721	3:21:04.690 PM	1	20904	KERNEL32.DLL	Process	GetStartupInfoA ( 0x001afe34 )	VOID			0.0006725
			
			var stdInputHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", processEnv.StdInputHandle);
			Assert.Equal((uint)0, stdInputHandle);
			//#	Time of Day	Thread	TID	Module	Category	API	Return Type	Return Value	Error	Duration
			// 5732	3:21:04.694 PM	1	20904	KERNEL32.DLL	File Management	GetFileType ( NULL )	DWORD	FILE_TYPE_UNKNOWN		0.0000509
			
			var stdOutputHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", processEnv.StdOutputHandle);
			Assert.Equal((uint)0, stdOutputHandle);
			//#	Time of Day	Thread	TID	Module	Category	API	Return Type	Return Value	Error	Duration
			// 5738	3:21:04.694 PM	1	20904	KERNEL32.DLL	File Management	GetFileType ( NULL )	DWORD	FILE_TYPE_UNKNOWN		0.0000306
			
			var stdErrorHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", processEnv.StdErrorHandle);
			Assert.Equal((uint)0, stdErrorHandle);
			//#	Time of Day	Thread	TID	Module	Category	API	Return Type	Return Value	Error	Duration
			// 5743	3:21:04.694 PM	1	20904	KERNEL32.DLL	File Management	GetFileType ( NULL )	DWORD	FILE_TYPE_UNKNOWN		0.0000016

			var handleCount = _testEnv.Kernel32.SetHandleCount(32u);
			Assert.Equal(32u, handleCount);

			var acp = _testEnv.Kernel32.GetAcp();
			Assert.Equal(CodePage.WestEurope, acp);

			var cpInfoPtr = _testEnv.AllocateMemory(20); // CPINFO structure is 20 bytes
			var cpInfoResult = _testEnv.CallKernel32Api("GETCPINFO", (uint)CodePage.WestEurope, cpInfoPtr);
			Assert.Equal(NativeTypes.Win32Bool.TRUE, cpInfoResult);
			
			var maxCharSize = _testEnv.Memory.Read32(cpInfoPtr + 0);
			Assert.Equal(1u, maxCharSize);
			
			var defaultChar0 = _testEnv.Memory.Read8(cpInfoPtr + 4);
			var defaultChar1 = _testEnv.Memory.Read8(cpInfoPtr + 5);
			Assert.Equal(63, defaultChar0);
			Assert.Equal(0, defaultChar1);
			
			//#	Time of Day	Thread	TID	Module	Category	API	Return Type	Return Value	Error	Duration
			// 5753	3:21:04.695 PM	1	20904	KERNEL32.DLL	Process	GetCommandLineA (  )	LPTSTR	0x028766d0		0.0000012
			var commandLineA = _testEnv.Kernel32.GetCommandLineA();
			// 5754	3:21:04.695 PM	1	20904	KERNEL32.DLL	Process	GetEnvironmentStringsW (  )	LPTCH	0x02892b50		0.0006195
			var environmentStringsW = _testEnv.Kernel32.GetEnvironmentStringsW();
			// 5821	3:21:04.706 PM	1	20904	KERNEL32.DLL	Unicode and Character Sets	WideCharToMultiByte ( CP_ACP, 0, "=::=::\", 3574, NULL, 0, NULL, NULL )	int	3574		0.0000058
			// 5824	3:21:04.706 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 3584 )	LPVOID	0x0a4c0498		0.0005592
			// 5826	3:21:04.708 PM	1	20904	KERNEL32.DLL	Unicode and Character Sets	WideCharToMultiByte ( CP_ACP, 0, "=::=::\", 3574, 0x0a4c0498, 3574, NULL, NULL )	int	3574		0.0000351
			// 5828	3:21:04.708 PM	1	20904	KERNEL32.DLL	Process	FreeEnvironmentStringsW ( "=::=::\" )	BOOL	TRUE		0.0000039
			// 5831	3:21:04.708 PM	1	20904	KERNEL32.DLL	Dynamic-Link Libraries	GetModuleFileNameA ( NULL, 0x00452760, 260 )	DWORD	34		0.0000725


			// -- BELOW IS MISSING --
			// 5844	3:21:04.708 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 1696 )	LPVOID	0x0a4c12b0		0.0000022
			// 5846	3:21:04.708 PM	1	20904	KERNEL32.DLL	Heaps	HeapFree ( 0x0a4c0000, 0, 0x0a4c0498 )	BOOL	TRUE		0.0000016
			// 5847	3:21:04.708 PM	1	20904	KERNEL32.DLL	Dynamic-Link Libraries	GetModuleHandleA ( "KERNEL32" )	HMODULE	0x75680000		0.0000280
			// 5870	3:21:04.710 PM	1	20904	KERNEL32.DLL	Dynamic-Link Libraries	GetProcAddress ( 0x75680000, "IsProcessorFeaturePresent" )	FARPROC	0x756843c0		0.0171529
			// 6141	3:21:04.728 PM	1	20904	KERNEL32.DLL	System Information	IsProcessorFeaturePresent ( PF_FLOATING_POINT_PRECISION_ERRATA )	BOOL	FALSE		0.0000043
			// 6142	3:21:04.728 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, HEAP_ZERO_MEMORY, 2048 )	LPVOID	0x0a4c0498		0.0000027
			// 6143	3:21:04.728 PM	1	20904	KERNEL32.DLL	Process	GetStartupInfoA ( 0x001afe88 )	VOID			0.0000013
			// 6144	3:21:04.728 PM	1	20904	KERNEL32.DLL	Dynamic-Link Libraries	GetModuleHandleA ( NULL )	HMODULE	0x00400000		0.0000263
			// 6145	3:21:04.728 PM	1	20904	USER32.DLL	Cursors	LoadCursorA ( NULL, IDC_ARROW )	HCURSOR	0x00010003		0.0003329
			// 6147	3:21:04.728 PM	1	20904	USER32.DLL	Icons	LoadIconA ( NULL, IDI_APPLICATION )	HICON	0x0001002b		0.0000782
			// 6149	3:21:04.728 PM	1	20904	GDI32.DLL	Device Contexts	GetStockObject ( BLACK_BRUSH )	HGDIOBJ	0x00900011		0.0000045
			// 6151	3:21:04.729 PM	1	20904	USER32.DLL	Window Classes	RegisterClassA ( 0x001afe40 )	ATOM	49860		0.0011049
			// 6188	3:21:04.731 PM	1	20904	WINMM.DLL	Multimedia Timers	timeBeginPeriod ( 1 )	MMRESULT	MMSYSERR_NOERROR		0.0005020
			// 6194	3:21:04.732 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 4096 )	LPVOID	0x0a4c1968		0.0000193
			// 6195	3:21:04.732 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 8416 )	LPVOID	0x0a4c2980		0.0000420
			// 6196	3:21:04.733 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 8416 )	LPVOID	0x0a4c4a78		0.0000008
			// 6197	3:21:04.733 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 8416 )	LPVOID	0x0a4c6b70		0.0000134
			// 6198	3:21:04.733 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 8416 )	LPVOID	0x0a4c8c68		0.0000157
			// 6199	3:21:04.733 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 8416 )	LPVOID	0x0a4cad60		0.0000008
			// 6200	3:21:04.733 PM	1	20904	KERNEL32.DLL	Heaps	HeapAlloc ( 0x0a4c0000, 0, 8416 )	LPVOID	0x0aa60048		0.0000236
			// 6201	3:21:04.733 PM	1	20904	USER32.DLL	System Information	GetSystemMetrics ( SM_CYSCREEN )	int	1460		0.0526187
			// 8237	3:21:04.907 PM	1	20904	USER32.DLL	System Information	GetSystemMetrics ( SM_CXSCREEN )	int	2336		0.0000011
			// 8238	3:21:04.907 PM	1	20904	USER32.DLL	Windows	CreateWindowExA ( WS_EX_APPWINDOW, "Ignition", "Ignition", WS_POPUP | WS_SYSMENU, 0, 0, 2336, 1460, NULL, NULL, 0x00400000, NULL )	HWND			
			// 9369	3:21:05.000 PM	1	20904	USER32.DLL	Window Procedures	DefWindowProcA ( 0x00c408e6, WM_NCCREATE, 0, 1767384 )	LRESULT	1		0.0001007
			// 
		}
	}
}