using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x00403D20
	/// Original name: sub_403D20
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_00403D20
	{
		private readonly EmulatorEnvironment _env;

		public Function_00403D20(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x00403D20
		/// </summary>
		[OriginalAddress(0x00403D20)]
		public int Execute()
		{
			// TODO: Transpile: int i; // edi
			// TODO: Transpile: __int16 v1; // cx
			// TODO: Transpile: int v2; // esi
			// TODO: Transpile: unsigned int v3; // ebp
			// TODO: Transpile: int v4; // eax
			// TODO: Transpile: int v5; // ebx
			// TODO: Transpile: int v6; // ecx
			// TODO: Transpile: int v7; // eax
			// TODO: Transpile: int j; // ebx
			// TODO: Transpile: int v10; // edi
			// TODO: Transpile: int v11; // ebp
			// TODO: Transpile: unsigned int v12; // ecx
			// TODO: Transpile: unsigned int v13; // esi
			// TODO: Transpile: int v14; // ecx
			// TODO: Transpile: int v15; // eax
			// TODO: Transpile: int v16; // esi
			// TODO: Transpile: __int16 v17; // [esp+40h] [ebp-E8h] BYREF
			// TODO: Transpile: __int16 v18; // [esp+42h] [ebp-E6h]
			// TODO: Transpile: unsigned int v19; // [esp+44h] [ebp-E4h]
			// TODO: Transpile: int v20; // [esp+48h] [ebp-E0h]
			// TODO: Transpile: unsigned __int16 v21; // [esp+4Ch] [ebp-DCh]
			// TODO: Transpile: __int16 v22; // [esp+4Eh] [ebp-DAh]
			// TODO: Transpile: unsigned int v23; // [esp+50h] [ebp-D8h]
			// TODO: Transpile: int v24; // [esp+54h] [ebp-D4h]
			// TODO: Transpile: int v25; // [esp+58h] [ebp-D0h] BYREF
			// TODO: Transpile: int v26; // [esp+5Ch] [ebp-CCh]
			// TODO: Transpile: int v27; // [esp+60h] [ebp-C8h]
			// TODO: Transpile: int v28; // [esp+64h] [ebp-C4h]
			// TODO: Transpile: __int16 *v29; // [esp+68h] [ebp-C0h]
			// TODO: Transpile: _DWORD v30[6]; // [esp+6Ch] [ebp-BCh]
			// TODO: Transpile: _DWORD v31[6]; // [esp+84h] [ebp-A4h]
			// TODO: Transpile: _DWORD v32[6]; // [esp+9Ch] [ebp-8Ch]
			// TODO: Transpile: _DWORD v33[2]; // [esp+B4h] [ebp-74h] BYREF
			// TODO: Transpile: int v34; // [esp+BCh] [ebp-6Ch]
			// TODO: Transpile: _DWORD v35[24]; // [esp+C8h] [ebp-60h] BYREF
			// TODO: Transpile: v30[1] = 2;
			// TODO: Transpile: v30[4] = 2;
			// TODO: Transpile: v30[5] = 2;
			// TODO: Transpile: v31[0] = 22050;
			// TODO: Transpile: v31[1] = 22050;
			// TODO: Transpile: v31[3] = 22050;
			// TODO: Transpile: v31[2] = 44100;
			// TODO: Transpile: v31[4] = 22050;
			// TODO: Transpile: v31[5] = 44100;
			// TODO: Transpile: v32[0] = 16;
			// TODO: Transpile: v32[1] = 16;
			// TODO: Transpile: v30[0] = 1;
			// TODO: Transpile: v30[2] = 1;
			// TODO: Transpile: v30[3] = 1;
			// TODO: Transpile: v32[2] = 16;
			// TODO: Transpile: v32[3] = 8;
			// TODO: Transpile: v32[4] = 8;
			// TODO: Transpile: v32[5] = 8;
			if (dword_41C848 == 1)
			return 0;
			dword_453094 = 4;
			dword_4530C0 = 0;
			dword_4530A0 = 0;
			dword_43C7D8 = 0;
			dword_41C848 = 0;
			if (_env.CallWin32Api<uint>("DirectSoundCreate", 0, &ppDS, 0))
			return 0;
			if (ppDS.lpVtbl.SetCooperativeLevel(ppDS, (HWND)dword_41C7AC, 4))
			{
			if (!ppDS.lpVtbl.SetCooperativeLevel(ppDS, (HWND)dword_41C7AC, 3))
			{
			v28 = 0;
			v25 = 20;
			v26 = 1;
			v27 = 0;
			v29 = 0;
			if (!ppDS.lpVtbl.CreateSoundBuffer(ppDS, (LPCDSBUFFERDESC)&v25, (LPDIRECTSOUNDBUFFER * )&dword_4530C0, 0))
			{
			// TODO: Transpile: for ( i = 0; i < 6; ++i )
			{
			v1 = v30[i];
			v2 = v32[i];
			v3 = v31[i];
			v17 = 0;
			v19 = 0;
			v20 = 0;
			v21 = 0;
			v18 = v1;
			v22 = v2;
			v4 = v30[i] * v2;
			v17 = 1;
			v19 = v3;
			// TODO: Transpile: LOWORD(v24) = v4 / 8;
			v21 = v24;
			dword_43C7F0 = (unsigned __int16)v24;
			v5 = v3 * (unsigned __int16)v24;
			v23 = v3 / 0x64;
			v20 = v5;
			v6 = ( * (int (__stdcall *  * )(int, __int16 * ))( * (_DWORD * )dword_4530C0 + 56))(dword_4530C0, &v17);
			dword_453080 = v3;
			v7 = v30[i];
			dword_453084 = v2;
			dword_453088 = v7 - 1;
			dword_453090 = v2 / 8 - 1;
			if (!v6)
			{
			v18 = v30[i];
			v21 = v24;
			v22 = v2;
			v19 = v3;
			v20 = v5;
			dword_43C7F4 = v5;
			v28 = 0;
			v27 = v5;
			v29 = &v17;
			v25 = 20;
			v26 = 232;
			v17 = 1;
			v6 = ppDS.lpVtbl.CreateSoundBuffer(ppDS, (LPCDSBUFFERDESC)&v25, (LPDIRECTSOUNDBUFFER * )&dword_4530A0, 0);
			if (v6)
			return 0;
			i = 6;
			}
			}
			if (!v6)
			{
			dword_45308C = v23;
			// TODO: Transpile: v33[0] = 20;
			if (!( * (int (__stdcall *  * )(int, _DWORD * ))( * (_DWORD * )dword_4530A0 + 12))(dword_4530A0, v33))
			{
			dword_43C7F4 = v34;
			dword_43C7C4 = dword_43C7F0 * v23 * (dword_453098 + 27);
			// TODO: Transpile: memset(v35, 0, sizeof(v35));
			// TODO: Transpile: v35[0] = 96;
			if (!ppDS.lpVtbl.GetCaps(ppDS, (LPDSCAPS)v35) && (v35[1] & 0x20) == 0)
			dword_43C7C4 = dword_43C7F0 * v23 * (dword_453098 + 2);
			if ((int)(dword_43C7F4 - dword_43C7F0 * v23) < dword_43C7C4)
			dword_43C7C4 = dword_43C7F4 - dword_43C7F0 * v23;
			dword_43C7E0 = 0;
			// TODO: Transpile: (*(void (__stdcall **)(int, _DWORD, _DWORD, int))(*(_DWORD *)dword_4530C0 + 48))(dword_4530C0, 0, 0, 1);
			// TODO: Transpile: (*(void (__stdcall **)(int, _DWORD, _DWORD, int))(*(_DWORD *)dword_4530A0 + 48))(dword_4530A0, 0, 0, 1);
			dword_41C844 = 1;
			dword_41C848 = 1;
			return 1;
			}
			}
			}
			}
			return 0;
			}
			dword_43C7D8 = 1;
			v28 = 0;
			v25 = 20;
			v26 = 1;
			v27 = 0;
			v29 = 0;
			if (ppDS.lpVtbl.CreateSoundBuffer(ppDS, (LPCDSBUFFERDESC)&v25, (LPDIRECTSOUNDBUFFER * )&dword_4530C0, 0))
			return 0;
			// TODO: Transpile: for ( j = 0; j < 6; ++j )
			{
			v10 = v30[j];
			v11 = v32[j];
			v12 = v31[j];
			v18 = v10;
			v22 = v11;
			v19 = v12;
			v17 = 1;
			v21 = v10 * v11 / 8;
			v13 = v12 / 0x64;
			dword_43C7F0 = v21;
			v20 = v21 * v12;
			v14 = ( * (int (__stdcall *  * )(int, __int16 * ))( * (_DWORD * )dword_4530C0 + 56))(dword_4530C0, &v17);
			dword_453080 = v31[j];
			dword_453088 = v10 - 1;
			dword_453084 = v11;
			dword_453090 = v11 / 8 - 1;
			if (!v14)
			j = 6;
			}
			if (v14)
			return 0;
			// TODO: Transpile: v33[0] = 20;
			if (( * (int (__stdcall *  * )(int, _DWORD * ))( * (_DWORD * )dword_4530C0 + 12))(dword_4530C0, v33))
			return 0;
			dword_45308C = v13;
			dword_43C7F4 = v34;
			v15 = dword_43C7F0 * v13 * dword_453098;
			v16 = dword_43C7F0 * v13;
			dword_43C7C4 = v15;
			if (v34 - v16 < v15)
			dword_43C7C4 = v34 - v16;
			dword_43C7E0 = 0;
			// TODO: Transpile: (*(void (__stdcall **)(int, _DWORD, _DWORD, int))(*(_DWORD *)dword_4530C0 + 48))(dword_4530C0, 0, 0, 1);
			dword_41C844 = 1;
			dword_41C848 = 1;
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
