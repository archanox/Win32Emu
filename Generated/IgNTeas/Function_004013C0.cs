using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x004013C0
	/// Original name: sub_4013C0
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_004013C0
	{
		private readonly EmulatorEnvironment _env;

		public Function_004013C0(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x004013C0
		/// </summary>
		[OriginalAddress(0x004013C0)]
		public int Execute(uint a1, int a2, int a3, int a4)
		{
			// TODO: Transpile: int v4; // esi
			// TODO: Transpile: int v5; // edi
			// TODO: Transpile: int v6; // eax
			// TODO: Transpile: __int16 *v7; // eax
			// TODO: Transpile: _BYTE *v8; // ecx
			// TODO: Transpile: _BYTE *v9; // ebx
			// TODO: Transpile: __int16 v10; // dx
			// TODO: Transpile: __int16 v11; // dx
			// TODO: Transpile: _BYTE *v12; // ebx
			// TODO: Transpile: unsigned __int16 v13; // dx
			// TODO: Transpile: _BYTE *v14; // ebx
			// TODO: Transpile: unsigned __int16 v15; // dx
			// TODO: Transpile: _BYTE *v16; // ebx
			// TODO: Transpile: __int16 v17; // dx
			// TODO: Transpile: _BYTE *v18; // ebx
			// TODO: Transpile: __int16 v19; // dx
			// TODO: Transpile: char v20; // dl
			// TODO: Transpile: char v21; // dl
			// TODO: Transpile: unsigned __int16 v22; // dx
			// TODO: Transpile: char v23; // dl
			// TODO: Transpile: _BYTE *v24; // ecx
			// TODO: Transpile: __int16 v25; // dx
			// TODO: Transpile: unsigned __int16 v26; // dx
			// TODO: Transpile: int v28; // [esp+Ch] [ebp-4h]
			v4 = a3;
			if (dword_43C3C4 < 44000)
			{
			v6 = a4;
			v5 = v28;
			}
			// TODO: Transpile: else
			{
			v5 = a3 % 2;
			v4 = a3 / 2;
			v6 = a4 / 2;
			}
			v7 = (__int16 * )(a2 + 2 * v6);
			if (dword_43C400 == 16)
			{
			if (dword_43C3C8 == 1 && !dword_43C3D0 && dword_43C3C4 < 44000)
			{
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v10 =  * v7 ^ 0x8000;
			// TODO: Transpile: v9 += 4;
			// TODO: Transpile: *((_WORD *)v9 - 2) = v10;
			// TODO: Transpile: ++v7;
			// TODO: Transpile: *((_WORD *)v9 - 1) = v10;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_101;
			}
			if (dword_43C400 != 16)
			uint LABEL_25;
			if (dword_43C3C8 == 1 && !dword_43C3D0 && dword_43C3C4 > = 44000)
			{
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v11 =  * v7 ^ 0x8000;
			v12 = v9 + 4;
			// TODO: Transpile: *((_WORD *)v12 - 2) = v11;
			// TODO: Transpile: v12 += 2;
			// TODO: Transpile: *((_WORD *)v12 - 2) = v11;
			v9 = v12 + 2;
			// TODO: Transpile: *((_WORD *)v9 - 2) = v11;
			// TODO: Transpile: ++v7;
			// TODO: Transpile: *((_WORD *)v9 - 1) = v11;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_101;
			}
			}
			if (dword_43C400 != 16)
			uint LABEL_32;
			if (dword_43C3C8 == 1 && dword_43C3D0 == 1 && dword_43C3C4 < 44000)
			{
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v13 =  * v7;
			v14 = v9 + 2;
			// TODO: Transpile: *((_WORD *)v14 - 1) = *v7;
			v9 = v14 + 2;
			// TODO: Transpile: *((_WORD *)v9 - 1) = v13;
			// TODO: Transpile: ++v7;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_101;
			}
			// TODO: Transpile: LABEL_25:
			if (dword_43C400 != 16)
			uint LABEL_39;
			if (dword_43C3C8 == 1 && dword_43C3D0 == 1 && dword_43C3C4 > = 44000)
			{
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v15 =  * v7;
			v16 = v9 + 2;
			// TODO: Transpile: *((_WORD *)v16 - 1) = *v7;
			// TODO: Transpile: v16 += 2;
			// TODO: Transpile: *((_WORD *)v16 - 1) = v15;
			// TODO: Transpile: v16 += 2;
			// TODO: Transpile: *((_WORD *)v16 - 1) = v15;
			v9 = v16 + 2;
			// TODO: Transpile: *((_WORD *)v9 - 1) = v15;
			// TODO: Transpile: ++v7;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_101;
			}
			// TODO: Transpile: LABEL_32:
			if (dword_43C400 != 16)
			uint LABEL_46;
			if (!dword_43C3C8 && !dword_43C3D0 && dword_43C3C4 < 44000)
			{
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: v9 += 2;
			v17 =  * v7 +  + ^ 0x8000;
			// TODO: Transpile: *((_WORD *)v9 - 1) = v17;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_101;
			}
			// TODO: Transpile: LABEL_39:
			if (dword_43C400 != 16)
			uint LABEL_53;
			if (!dword_43C3C8 && !dword_43C3D0 && dword_43C3C4 > = 44000)
			{
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: v9 += 4;
			// TODO: Transpile: *((_WORD *)v9 - 2) = *v7++ ^ 0x8000;
			// TODO: Transpile: --v4;
			// TODO: Transpile: *((_WORD *)v9 - 1) = *(v7 - 1) ^ 0x8000;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_101;
			}
			// TODO: Transpile: LABEL_46:
			if (dword_43C400 != 16)
			uint LABEL_60;
			if (!dword_43C3C8 && dword_43C3D0 == 1 && dword_43C3C4 < 44000)
			{
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: v9 += 2;
			// TODO: Transpile: *((_WORD *)v9 - 1) = *v7++;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_101;
			}
			// TODO: Transpile: LABEL_53:
			if (dword_43C400 != 16 || dword_43C3C8 || dword_43C3D0 != 1 || dword_43C3C4 < 44000)
			{
			// TODO: Transpile: LABEL_60:
			if (dword_43C400 == 8)
			{
			if (dword_43C3C8 == 1 && !dword_43C3D0 && dword_43C3C4 < 44000)
			{
			v8 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: v8 += 2;
			v20 = HIBYTE( * v7 +  + ) ^ 0x80;
			// TODO: Transpile: *(v8 - 2) = v20;
			// TODO: Transpile: --v4;
			// TODO: Transpile: *(v8 - 1) = HIBYTE(*(v7 - 1)) ^ 0x80;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_100;
			}
			if (dword_43C400 != 8)
			uint LABEL_81;
			if (dword_43C3C8 == 1 && !dword_43C3D0 && dword_43C3C4 > = 44000)
			{
			v8 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v21 = HIBYTE( * v7) ^ 0x80;
			// TODO: Transpile: v8 += 4;
			// TODO: Transpile: ++v7;
			// TODO: Transpile: --v4;
			// TODO: Transpile: *(v8 - 4) = v21;
			// TODO: Transpile: *(v8 - 3) = v21;
			// TODO: Transpile: *(v8 - 2) = v21;
			// TODO: Transpile: *(v8 - 1) = v21;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_100;
			}
			}
			if (dword_43C400 != 8)
			uint LABEL_88;
			if (dword_43C3C8 == 1 && dword_43C3D0 == 1 && dword_43C3C4 < 44000)
			{
			v8 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v22 =  * v7;
			// TODO: Transpile: v8 += 2;
			// TODO: Transpile: ++v7;
			// TODO: Transpile: --v4;
			// TODO: Transpile: *(v8 - 2) = HIBYTE(v22);
			// TODO: Transpile: *(v8 - 1) = HIBYTE(*(v7 - 1));
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_100;
			}
			// TODO: Transpile: LABEL_81:
			if (dword_43C400 != 8)
			uint LABEL_93;
			if (dword_43C3C8 == 1 && dword_43C3D0 == 1 && dword_43C3C4 > = 44000)
			{
			v8 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v23 =  * ((_BYTE * )v7 +  +  + 1);
			v24 = v8 + 3;
			// TODO: Transpile: *(v24 - 3) = v23;
			// TODO: Transpile: *(v24 - 2) = v23;
			// TODO: Transpile: *(v24 - 1) = v23;
			// TODO: Transpile: *v24 = v23;
			v8 = v24 + 1;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_100;
			}
			// TODO: Transpile: LABEL_88:
			if (dword_43C400 != 8)
			uint LABEL_94;
			if (!dword_43C3C8)
			{
			v8 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: ++v8;
			v25 =  * v7 +  +  >  > 8;
			// TODO: Transpile: --v4;
			// TODO: Transpile: *(v8 - 1) = v25 ^ 0x80;
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_100;
			}
			// TODO: Transpile: LABEL_93:
			if (dword_43C400 != 8)
			{
			// TODO: Transpile: LABEL_99:
			v8 = a1;
			// TODO: Transpile: LABEL_100:
			v9 = (_BYTE * )v28;
			uint LABEL_101;
			}
			// TODO: Transpile: LABEL_94:
			if (dword_43C400 == 8 && !dword_43C3C8)
			{
			v8 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v26 =  * v7;
			// TODO: Transpile: ++v8;
			// TODO: Transpile: ++v7;
			// TODO: Transpile: --v4;
			// TODO: Transpile: *(v8 - 1) = HIBYTE(v26);
			}
			// TODO: Transpile: while ( v4 );
			}
			uint LABEL_100;
			}
			uint LABEL_99;
			}
			v8 = a1;
			v9 = a1;
			if (v4 > 0)
			{
			// TODO: Transpile: do
			{
			v18 = v9 + 2;
			// TODO: Transpile: *((_WORD *)v18 - 1) = *v7;
			v9 = v18 + 2;
			v19 =  * v7 +  + ;
			// TODO: Transpile: *((_WORD *)v9 - 1) = v19;
			// TODO: Transpile: --v4;
			}
			// TODO: Transpile: while ( v4 );
			}
			// TODO: Transpile: LABEL_101:
			if (dword_43C3C4 > = 44000 && v5 == 1)
			{
			if (dword_43C400 == 16)
			v8 = v9;
			if (dword_43C400 == 8)
			{
			// TODO: Transpile: *v8 = *(v8 - 1);
			}
			// TODO: Transpile: else if ( dword_43C400 != 16 || dword_43C3C8 )
			{
			// TODO: Transpile: *(_DWORD *)v8 = *((_DWORD *)v8 - 1);
			}
			// TODO: Transpile: else
			{
			// TODO: Transpile: *(_WORD *)v8 = *((_WORD *)v8 - 1);
			}
			}
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
