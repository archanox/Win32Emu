using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x00401E30
	/// Original name: sub_401E30
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_00401E30
	{
		private readonly EmulatorEnvironment _env;

		public Function_00401E30(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x00401E30
		/// </summary>
		[OriginalAddress(0x00401E30)]
		public int Execute(int a1, int a2)
		{
			// TODO: Transpile: int v2; // esi
			// TODO: Transpile: int v3; // ecx
			// TODO: Transpile: _BYTE *v4; // eax
			// TODO: Transpile: __int16 *v5; // edx
			// TODO: Transpile: int v6; // edi
			// TODO: Transpile: int v7; // ecx
			// TODO: Transpile: __int16 *v8; // edx
			// TODO: Transpile: int v9; // esi
			// TODO: Transpile: int v10; // ecx
			// TODO: Transpile: _DWORD *v11; // esi
			// TODO: Transpile: _DWORD *v12; // edi
			// TODO: Transpile: int v13; // ecx
			// TODO: Transpile: int v14; // edx
			// TODO: Transpile: _BYTE *v15; // eax
			// TODO: Transpile: int v16; // ebx
			// TODO: Transpile: int v17; // ebx
			// TODO: Transpile: _BYTE *v18; // eax
			// TODO: Transpile: int v19; // ebx
			// TODO: Transpile: int v20; // ebx
			// TODO: Transpile: int v21; // ebx
			// TODO: Transpile: _DWORD *v22; // edi
			// TODO: Transpile: int v23; // ebx
			// TODO: Transpile: int v24; // ebx
			// TODO: Transpile: _DWORD *v25; // edi
			// TODO: Transpile: int v26; // ebx
			// TODO: Transpile: int v27; // ebx
			// TODO: Transpile: int v28; // ebx
			// TODO: Transpile: _DWORD *v29; // edi
			// TODO: Transpile: int v30; // ebx
			// TODO: Transpile: int v31; // ebx
			// TODO: Transpile: int v32; // ebx
			// TODO: Transpile: int v33; // ebx
			// TODO: Transpile: _DWORD *v34; // edi
			// TODO: Transpile: int v35; // ebx
			// TODO: Transpile: int v36; // ebx
			// TODO: Transpile: int v37; // ebx
			// TODO: Transpile: int v38; // ebx
			// TODO: Transpile: _DWORD *v39; // edi
			// TODO: Transpile: int v40; // ebx
			// TODO: Transpile: int v41; // ebx
			v2 = dword_43C418;
			v3 = dword_43C408 + 12 * a1;
			v4 =  * (_BYTE *  * )v3;
			v5 =  * (__int16 *  * )(v3 + 4);
			v6 =  * (_DWORD * )(v3 + 8);
			if (a2)
			{
			if (a2 == 1 && v6 > 0)
			{
			// TODO: Transpile: do
			{
			v8 = v5 + 1;
			v9 = v2 + 4;
			// TODO: Transpile: *(_DWORD *)(v9 - 4) = 32 * *(v8 - 1);
			v10 =  * v8;
			v5 = v8 + 1;
			v2 = v9 + 4;
			// TODO: Transpile: --v6;
			// TODO: Transpile: *(_DWORD *)(v2 - 4) = 38 * v10;
			}
			// TODO: Transpile: while ( v6 );
			}
			}
			// TODO: Transpile: else
			{
			v7 = 2 * v6;
			if (2 * v6 > 0)
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: ++v5;
			// TODO: Transpile: v2 += 4;
			// TODO: Transpile: --v7;
			// TODO: Transpile: *(_DWORD *)(v2 - 4) = 16 * *(v5 - 1);
			}
			// TODO: Transpile: while ( v7 );
			}
			}
			v11 = (_DWORD * )dword_43C414;
			v12 = (_DWORD * )dword_43C464;
			if ( * v4 == 0xFF)
			{
			// TODO: Transpile: LABEL_36:
			// TODO: Transpile: *v11 = 0;
			return dword_43C414;
			}
			// TODO: Transpile: else
			{
			// TODO: Transpile: while ( 1 )
			{
			// TODO: Transpile: v4 += 2;
			v13 = (unsigned __int8) * (v4 - 2);
			v14 = (unsigned __int8) * (v4 - 1);
			if ((unsigned int)(v13 - 7) > 0xC)
			return 0;
			// TODO: Transpile: switch ( *(v4 - 2) )
			{
			// TODO: Transpile: case 7:
			if (a2)
			{
			if (a2 == 1 &&  * (v4 - 1))
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: *v11++ = v12;
			// TODO: Transpile: *v12 = v13;
			v18 = v4 + 6;
			// TODO: Transpile: v12 += 9;
			v19 = 32 *  * ((__int16 * )v18 - 3);
			v4 = v18 + 2;
			// TODO: Transpile: *(v12 - 8) = dword_43C410 + v19;
			// TODO: Transpile: *(v12 - 7) = v12 - 4;
			// TODO: Transpile: *(v12 - 4) = *((unsigned __int16 *)v4 - 3) << 9;
			// TODO: Transpile: *(v12 - 3) = 0;
			// TODO: Transpile: *(v12 - 2) = 0;
			// TODO: Transpile: --v14;
			// TODO: Transpile: *(v12 - 1) = 614 * *((unsigned __int16 *)v4 - 2);
			v20 =  * ((__int16 * )v4 - 1);
			// TODO: Transpile: *(v12 - 6) = *(_DWORD *)(dword_43C418 + 8 * v20);
			// TODO: Transpile: *(v12 - 5) = *(_DWORD *)(dword_43C418 + 8 * v20 + 4);
			}
			// TODO: Transpile: while ( v14 );
			}
			}
			// TODO: Transpile: else if ( *(v4 - 1) )
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: *v11++ = v12;
			// TODO: Transpile: *v12 = v13;
			v15 = v4 + 6;
			// TODO: Transpile: v12 += 9;
			v16 = 32 *  * ((__int16 * )v15 - 3);
			v4 = v15 + 2;
			// TODO: Transpile: *(v12 - 8) = dword_43C410 + v16;
			// TODO: Transpile: *(v12 - 7) = v12 - 4;
			// TODO: Transpile: *(v12 - 4) = *((unsigned __int16 *)v4 - 3) << 8;
			// TODO: Transpile: *(v12 - 3) = 0;
			// TODO: Transpile: *(v12 - 2) = 0;
			// TODO: Transpile: --v14;
			// TODO: Transpile: *(v12 - 1) = *((unsigned __int16 *)v4 - 2) << 8;
			v17 =  * ((__int16 * )v4 - 1);
			// TODO: Transpile: *(v12 - 6) = *(_DWORD *)(dword_43C418 + 8 * v17);
			// TODO: Transpile: *(v12 - 5) = *(_DWORD *)(dword_43C418 + 8 * v17 + 4);
			}
			// TODO: Transpile: while ( v14 );
			}
			// TODO: Transpile: break;
			// TODO: Transpile: case 8:
			// TODO: Transpile: case 9:
			// TODO: Transpile: case 0xA:
			// TODO: Transpile: case 0xC:
			// TODO: Transpile: case 0xE:
			// TODO: Transpile: case 0x10:
			return 0;
			// TODO: Transpile: case 0xB:
			if ( * (v4 - 1))
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: *v11++ = v12;
			// TODO: Transpile: *v12 = v13;
			// TODO: Transpile: v4 += 5;
			v21 =  * (__int16 * )(v4 - 5);
			v22 = v12 + 8;
			// TODO: Transpile: *(v22 - 7) = *(_DWORD *)(dword_43C418 + 8 * v21);
			v12 = v22 + 1;
			// TODO: Transpile: *(v12 - 7) = *(_DWORD *)(dword_43C418 + 8 * v21 + 4);
			v23 =  * (__int16 * )(v4 - 3);
			// TODO: Transpile: *(v12 - 5) = *(_DWORD *)(dword_43C418 + 8 * v23);
			// TODO: Transpile: *(v12 - 4) = *(_DWORD *)(dword_43C418 + 8 * v23 + 4);
			// TODO: Transpile: --v14;
			// TODO: Transpile: *(v12 - 2) = (unsigned __int8)*(v4 - 1);
			// TODO: Transpile: *(v12 - 1) = 0;
			}
			// TODO: Transpile: while ( v14 );
			}
			// TODO: Transpile: break;
			// TODO: Transpile: case 0xD:
			if ( * (v4 - 1))
			v4 = &v4[8 * v14 - v14];
			// TODO: Transpile: break;
			// TODO: Transpile: case 0xF:
			if ( * (v4 - 1))
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: *v11++ = v12;
			// TODO: Transpile: *v12 = v13;
			// TODO: Transpile: v4 += 7;
			v24 =  * (__int16 * )(v4 - 7);
			v25 = v12 + 11;
			// TODO: Transpile: *(v25 - 10) = *(_DWORD *)(dword_43C418 + 8 * v24);
			v12 = v25 + 1;
			// TODO: Transpile: *(v12 - 10) = *(_DWORD *)(dword_43C418 + 8 * v24 + 4);
			v26 =  * (__int16 * )(v4 - 5);
			// TODO: Transpile: *(v12 - 8) = *(_DWORD *)(dword_43C418 + 8 * v26);
			// TODO: Transpile: *(v12 - 7) = *(_DWORD *)(dword_43C418 + 8 * v26 + 4);
			v27 =  * (__int16 * )(v4 - 3);
			// TODO: Transpile: *(v12 - 5) = *(_DWORD *)(dword_43C418 + 8 * v27);
			// TODO: Transpile: *(v12 - 4) = *(_DWORD *)(dword_43C418 + 8 * v27 + 4);
			// TODO: Transpile: --v14;
			// TODO: Transpile: *(v12 - 2) = (unsigned __int8)*(v4 - 1);
			// TODO: Transpile: *(v12 - 1) = 0;
			}
			// TODO: Transpile: while ( v14 );
			}
			// TODO: Transpile: break;
			// TODO: Transpile: case 0x11:
			if ( * (v4 - 1))
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: *v11++ = v12;
			// TODO: Transpile: *v12 = v13;
			// TODO: Transpile: v4 += 8;
			v28 =  * ((__int16 * )v4 - 4);
			v29 = v12 + 8;
			// TODO: Transpile: *(v29 - 7) = *(_DWORD *)(dword_43C418 + 8 * v28);
			v12 = v29 + 1;
			// TODO: Transpile: *(v12 - 7) = *(_DWORD *)(dword_43C418 + 8 * v28 + 4);
			v30 =  * ((__int16 * )v4 - 3);
			// TODO: Transpile: *(v12 - 6) = *(_DWORD *)(dword_43C418 + 8 * v30);
			// TODO: Transpile: *(v12 - 5) = *(_DWORD *)(dword_43C418 + 8 * v30 + 4);
			v31 =  * ((__int16 * )v4 - 2);
			// TODO: Transpile: *(v12 - 4) = *(_DWORD *)(dword_43C418 + 8 * v31);
			// TODO: Transpile: *(v12 - 3) = *(_DWORD *)(dword_43C418 + 8 * v31 + 4);
			v32 = 28 *  * ((__int16 * )v4 - 1);
			// TODO: Transpile: --v14;
			// TODO: Transpile: *(v12 - 2) = v32 + dword_43C41C;
			// TODO: Transpile: *(v12 - 1) = *(_DWORD *)(dword_43C41C + v32 + 24);
			}
			// TODO: Transpile: while ( v14 );
			}
			// TODO: Transpile: break;
			// TODO: Transpile: case 0x12:
			if ( * (v4 - 1))
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: *v11++ = v12;
			// TODO: Transpile: *v12 = v13;
			// TODO: Transpile: v4 += 9;
			v33 =  * (__int16 * )(v4 - 9);
			v34 = v12 + 9;
			// TODO: Transpile: *(v34 - 8) = *(_DWORD *)(dword_43C418 + 8 * v33);
			v12 = v34 + 1;
			// TODO: Transpile: *(v12 - 8) = *(_DWORD *)(dword_43C418 + 8 * v33 + 4);
			v35 =  * (__int16 * )(v4 - 7);
			// TODO: Transpile: *(v12 - 7) = *(_DWORD *)(dword_43C418 + 8 * v35);
			// TODO: Transpile: *(v12 - 6) = *(_DWORD *)(dword_43C418 + 8 * v35 + 4);
			v36 =  * (__int16 * )(v4 - 5);
			// TODO: Transpile: *(v12 - 5) = *(_DWORD *)(dword_43C418 + 8 * v36);
			// TODO: Transpile: *(v12 - 4) = *(_DWORD *)(dword_43C418 + 8 * v36 + 4);
			v37 = 28 *  * (__int16 * )(v4 - 3);
			// TODO: Transpile: *(v12 - 3) = v37 + dword_43C41C;
			// TODO: Transpile: *(v12 - 2) = *(_DWORD *)(dword_43C41C + v37 + 24);
			// TODO: Transpile: --v14;
			// TODO: Transpile: *(v12 - 1) = dword_43C420[(unsigned __int8)*(v4 - 1)];
			}
			// TODO: Transpile: while ( v14 );
			}
			// TODO: Transpile: break;
			// TODO: Transpile: case 0x13:
			if ( * (v4 - 1))
			{
			// TODO: Transpile: do
			{
			// TODO: Transpile: *v11++ = v12;
			// TODO: Transpile: *v12 = v13;
			// TODO: Transpile: v4 += 8;
			v38 =  * ((__int16 * )v4 - 4);
			v39 = v12 + 8;
			// TODO: Transpile: *(v39 - 7) = *(_DWORD *)(dword_43C418 + 8 * v38);
			v12 = v39 + 1;
			// TODO: Transpile: *(v12 - 7) = *(_DWORD *)(dword_43C418 + 8 * v38 + 4);
			v40 =  * ((__int16 * )v4 - 3);
			// TODO: Transpile: *(v12 - 6) = *(_DWORD *)(dword_43C418 + 8 * v40);
			// TODO: Transpile: *(v12 - 5) = *(_DWORD *)(dword_43C418 + 8 * v40 + 4);
			v41 =  * ((__int16 * )v4 - 2);
			// TODO: Transpile: *(v12 - 4) = *(_DWORD *)(dword_43C418 + 8 * v41);
			// TODO: Transpile: *(v12 - 3) = *(_DWORD *)(dword_43C418 + 8 * v41 + 4);
			// TODO: Transpile: *(v12 - 2) = (unsigned __int8)*(v4 - 2);
			// TODO: Transpile: --v14;
			// TODO: Transpile: *(v12 - 1) = dword_43C420[(unsigned __int8)*(v4 - 1)];
			}
			// TODO: Transpile: while ( v14 );
			}
			// TODO: Transpile: break;
			}
			if ( * v4 == 0xFF)
			uint LABEL_36;
			}
			}
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
