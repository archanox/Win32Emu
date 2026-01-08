using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x004052D0
	/// Original name: sub_4052D0
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_004052D0
	{
		private readonly EmulatorEnvironment _env;

		public Function_004052D0(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x004052D0
		/// </summary>
		[OriginalAddress(0x004052D0)]
		public int Execute()
		{
			// TODO: Transpile: int v0; // eax
			// TODO: Transpile: HRESULT v2; // eax
			// TODO: Transpile: HDC DC; // esi
			// TODO: Transpile: int DeviceCaps; // ebx
			// TODO: Transpile: LONG WindowLongA; // eax
			// TODO: Transpile: LONG v6; // eax
			// TODO: Transpile: int v7; // ebp
			// TODO: Transpile: int *v8; // esi
			// TODO: Transpile: int v9; // ecx
			// TODO: Transpile: int v10; // edx
			// TODO: Transpile: int v11; // eax
			// TODO: Transpile: bool v12; // cc
			// TODO: Transpile: int *v13; // eax
			// TODO: Transpile: int v14; // ebx
			// TODO: Transpile: LPDIRECTDRAWSURFACE *v15; // edi
			// TODO: Transpile: int v16; // eax
			// TODO: Transpile: int v17; // ecx
			// TODO: Transpile: int v18; // edx
			// TODO: Transpile: int SystemMetrics; // [esp+4h] [ebp-B8h]
			// TODO: Transpile: HINSTANCE v20; // [esp+10h] [ebp-ACh]
			// TODO: Transpile: BOOL v21; // [esp+10h] [ebp-ACh]
			// TODO: Transpile: LONG v22; // [esp+14h] [ebp-A8h]
			// TODO: Transpile: int v23; // [esp+28h] [ebp-94h] BYREF
			// TODO: Transpile: struct tagRECT rc; // [esp+2Ch] [ebp-90h] BYREF
			// TODO: Transpile: int v25; // [esp+3Ch] [ebp-80h] BYREF
			// TODO: Transpile: LONG pvParam; // [esp+40h] [ebp-7Ch] BYREF
			// TODO: Transpile: LONG v27; // [esp+44h] [ebp-78h]
			// TODO: Transpile: _DWORD v28[27]; // [esp+50h] [ebp-6Ch] BYREF
			CallFunction(0x00404530);
			v20 = hInstance;
			SystemMetrics = GetSystemMetrics;
			v0 = GetSystemMetrics;
			hWnd = _env.CallWin32Api<uint>("CreateWindowExA", 0x40000u, ClassName, WindowName, 0x80080000, 0, 0, v0, SystemMetrics, 0, 0, v20, 0);
			dword_41C7AC = hWnd;
			if (!hWnd)
			return 0;
			_env.CallWin32Api("UpdateWindow", hWnd);
			// TODO: Transpile: SetFocus(hWnd);
			if (_env.CallWin32Api<uint>("DirectDrawCreate", 0,  & lpDD, 0))
			return 0;
			if (dword_41C79C)
			v2 = lpDD.lpVtbl.SetCooperativeLevel(lpDD, hWnd, 83);
			else
			v2 = lpDD.lpVtbl.SetCooperativeLevel(lpDD, hWnd, 8);
			if (v2)
			return 0;
			if (dword_41C79C)
			{
			if (lpDD.lpVtbl.SetDisplayMode(lpDD, dword_41C870, dword_41C874, dword_41C878))
			return 0;
			}
			else
			{
			DC = GetDC;
			DeviceCaps = GetDeviceCaps(DC, 12);
			dword_41C9EC = GetDeviceCaps(DC, 14) * DeviceCaps;
			_env.CallWin32Api("ReleaseDC", 0, DC);
			WindowLongA = GetWindowLongA(hWnd, -16);
			// TODO: Transpile: SetWindowLongA(hWnd, -16, WindowLongA & 0x7F39FFFF | 0xC60000);
			// TODO: Transpile: SetRect(&rc, 0, 0, 640, 480);
			v22 = GetWindowLongA(hWnd, -20);
			v21 = GetMenu != 0;
			v6 = GetWindowLongA(hWnd, -16);
			// TODO: Transpile: AdjustWindowRectEx(&rc, v6, v21, v22);
			// TODO: Transpile: SetWindowPos(hWnd, 0, 0, 0, rc.right - rc.left, rc.bottom - rc.top, 0x16u);
			// TODO: Transpile: SetWindowPos(hWnd, (HWND)0xFFFFFFFE, 0, 0, 0, 0, 0x13u);
			// TODO: Transpile: SystemParametersInfoA(0x30u, 0, &pvParam, 0);
			// TODO: Transpile: GetWindowRect(hWnd, &rc);
			if (rc.left < pvParam)
			rc.left = pvParam;
			if (rc.top < v27)
			rc.top = v27;
			// TODO: Transpile: SetWindowPos(hWnd, 0, rc.left, rc.top, 0, 0, 0x15u);
			}
			// TODO: Transpile: memset(v28, 0, sizeof(v28));
			v28[0] = 108;
			if (dword_41C79C)
			{
			v28[5] = dword_41C87C;
			v28[1] = 33;
			v28[26] = 536;
			if (lpDD.lpVtbl.CreateSurface(lpDD, v28, &dword_43C914, 0))
			return 0;
			dword_43C8E8 = 1;
			dword_43C8EC = 1;
			dword_43C904 = dword_41C870;
			dword_43C908 = dword_41C874;
			dword_43C90C = dword_41C878;
			if (dword_41C87C >= 5)
			uint LABEL_22;
			if (dword_41C87C > 0)
			{
			v7 = 0;
			v8 =  & unk_43C824;
			v23 = dword_43C914;
			v25 = 4;
			while (1)
			{
			if (v8 !=  & unk_43C824)
			v25 = 16;
			if ((*(int (int, int *, int *))(*v23 + 48))(v23, &v25, &v23))
			break;
			v9 = dword_41C870;
			// TODO: Transpile: *v8 = v23;
			v10 = dword_41C874;
			// TODO: Transpile: *(v8 - 11) = 1;
			v11 = dword_41C878;
			// TODO: Transpile: *(v8 - 10) = 1;
			v8 += 12;
			++v7;
			// TODO: Transpile: *(v8 - 16) = v9;
			// TODO: Transpile: *(v8 - 15) = v10;
			v12 = v7 < dword_41C87C;
			// TODO: Transpile: *(v8 - 14) = v11;
			if (!v12)
			uint LABEL_29;
			}
			_env.CallWin32Api("MessageBoxA", hWnd, aBackbufferCoul, 0, 0);
			return 0;
			}
			}
			else
			{
			v28[1] = 1;
			v28[26] = 512;
			if (lpDD.lpVtbl.CreateSurface(lpDD, v28, &dword_43C914, 0))
			return 0;
			dword_43C8E8 = 1;
			dword_43C8EC = 1;
			dword_43C904 = dword_41C870;
			dword_43C908 = dword_41C874;
			v28[1] = 7;
			v28[26] = 64;
			v28[3] = 640;
			v28[2] = 480;
			dword_43C90C = dword_41C878;
			if (dword_41C87C >= 5)
			{
			// TODO: Transpile: LABEL_22:
			_env.CallWin32Api("MessageBoxA", hWnd, Text, 0, 0);
			return 0;
			}
			if (dword_41C87C > 0)
			{
			v14 = 0;
			v15 =  & unk_43C824;
			while (!lpDD.lpVtbl.CreateSurface(lpDD, v28, v15, 0))
			{
			// TODO: Transpile: *(v15 - 11) = (LPDIRECTDRAWSURFACE)1;
			v16 = dword_41C870;
			// TODO: Transpile: *(v15 - 10) = (LPDIRECTDRAWSURFACE)1;
			v17 = dword_41C874;
			// TODO: Transpile: *(v15 - 4) = (LPDIRECTDRAWSURFACE)v16;
			v18 = dword_41C878;
			// TODO: Transpile: *(v15 - 3) = (LPDIRECTDRAWSURFACE)v17;
			v15 += 12;
			++v14;
			// TODO: Transpile: *(v15 - 14) = (LPDIRECTDRAWSURFACE)v18;
			if (v14 >= dword_41C87C)
			uint LABEL_44;
			}
			return 0;
			}
			// TODO: Transpile: LABEL_44:
			if (lpDD.lpVtbl.CreateClipper(lpDD, 0, &dword_41C9E8, 0))
			return 0;
			if ((*(int (int, _DWORD, HWND))(*dword_41C9E8 + 32))(dword_41C9E8, 0, hWnd))
			return 0;
			if ((*(int (int, int))(*dword_43C914 + 112))(dword_43C914, dword_41C9E8))
			return 1;
			}
			// TODO: Transpile: LABEL_29:
			v28[1] = 7;
			v28[26] = 64;
			if (!dword_41C79C)
			uint LABEL_53;
			if (dword_43EF78)
			{
			// TODO: Transpile: (*(void (__stdcall **)(int))(*(_DWORD *)dword_43EF78 + 8))(dword_43EF78);
			dword_43EF78 = 0;
			}
			byte_43EB78 = 0;
			byte_43EB79 = 0;
			byte_43EB7A = 0;
			v13 =  & unk_43EB7C;
			do
			{
			// TODO: Transpile: *(_BYTE *)v13++ = -1;
			// TODO: Transpile: *((_BYTE *)v13 - 3) = -1;
			// TODO: Transpile: *((_BYTE *)v13 - 2) = -1;
			}
			while (v13 <  & dword_43EF78)
			if (lpDD.lpVtbl.CreatePalette(lpDD, 68, &byte_43EB78, &dword_43EF78, 0))
			return 0;
			if ((*(int (int, int))(*dword_43C914 + 124))(dword_43C914, dword_43EF78))
			return 0;
			// TODO: Transpile: LABEL_53:
			CallFunction(0x00406250);
			_env.CallWin32Api("ShowWindow", hWnd, 5);
			return 1;
		}

		/// <summary>
		/// Call another function at the specified address
		/// </summary>
		private uint CallFunction(uint address, params object[] args)
		{
			// TODO: Implement function calling mechanism
			// This would need to interact with the emulator or other generated functions
			_env.Logger?.LogWarning("CallFunction not yet implemented for address 0x{Address:X8}", address);
			return 0;
		}
	}
}
