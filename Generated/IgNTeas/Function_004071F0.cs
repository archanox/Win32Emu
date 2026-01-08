using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x004071F0
	/// Original name: sub_4071F0
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_004071F0
	{
		private readonly EmulatorEnvironment _env;

		public Function_004071F0(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x004071F0
		/// </summary>
		[OriginalAddress(0x004071F0)]
		public int Execute(uint a1)
		{
			// TODO: Transpile: int v1; // eax
			// TODO: Transpile: int v2; // edx
			// TODO: Transpile: int v3; // ebx
			// TODO: Transpile: int v4; // eax
			// TODO: Transpile: int v5; // eax
			// TODO: Transpile: int v6; // edx
			// TODO: Transpile: int v7; // ebx
			// TODO: Transpile: int v8; // eax
			// TODO: Transpile: int v9; // eax
			// TODO: Transpile: int v10; // edx
			// TODO: Transpile: int v11; // ebx
			// TODO: Transpile: int v12; // eax
			// TODO: Transpile: int result; // eax
			// TODO: Transpile: int v14; // esi
			// TODO: Transpile: int v15; // eax
			// TODO: Transpile: int v16; // esi
			// TODO: Transpile: int v17; // edi
			// TODO: Transpile: _DWORD *v18; // ebx
			// TODO: Transpile: int v19; // ebp
			// TODO: Transpile: int v20; // edi
			// TODO: Transpile: int v21; // eax
			// TODO: Transpile: int v22; // esi
			// TODO: Transpile: int v23; // ebx
			// TODO: Transpile: _DWORD *v24; // ebp
			// TODO: Transpile: int v25; // ecx
			// TODO: Transpile: int v26; // esi
			// TODO: Transpile: int v27; // edi
			// TODO: Transpile: _DWORD *v28; // ebx
			// TODO: Transpile: int v29; // ebp
			// TODO: Transpile: int v30; // esi
			// TODO: Transpile: int v31; // ebx
			// TODO: Transpile: int v32; // edi
			// TODO: Transpile: int v33; // ebp
			// TODO: Transpile: int v34; // edx
			// TODO: Transpile: int v35; // eax
			// TODO: Transpile: _BYTE *v36; // [esp-4h] [ebp-14h]
			dword_41CAAC = a1[1];
			dword_41CAB0 = a1[2];
			dword_41CAB4 = a1[3];
			dword_41CAB8 = a1[4];
			dword_41CABC = a1[5];
			dword_41CAC0 = a1[6];
			dword_41CAC4 = a1[7];
			dword_41CAC8 = a1[8];
			dword_41CACC = a1[9];
			dword_41CAD0 = a1[10];
			dword_41CAD4 = a1[11];
			dword_41CAD8 = a1[12];
			dword_44CE24 = a1[13];
			dword_44CE30 = dword_453068;
			if (dword_41CAAC < = dword_41CAB4)
			{
			dword_44CE58 = dword_41CAB4;
			dword_44CE34 = dword_41CAAC;
			}
			// TODO: Transpile: else
			{
			dword_44CE34 = dword_41CAB4;
			dword_44CE58 = dword_41CAAC;
			}
			if (dword_41CABC < = dword_44CE58)
			{
			if (dword_44CE34 > dword_41CABC)
			dword_44CE34 = dword_41CABC;
			}
			// TODO: Transpile: else
			{
			dword_44CE58 = dword_41CABC;
			}
			v1 = dword_41CAC0;
			if (dword_41CAB0 > dword_41CAC0)
			{
			v2 = dword_41CAD8;
			v3 = dword_41CABC;
			dword_41CAC0 = dword_41CAB0;
			dword_41CAB0 = v1;
			dword_41CABC = dword_41CAAC;
			dword_41CAD8 = dword_41CAC8;
			dword_41CAC8 = v2;
			v4 = dword_41CAD4;
			dword_41CAAC = v3;
			dword_41CAD4 = dword_41CAC4;
			dword_41CAC4 = v4;
			}
			v5 = dword_41CAB8;
			if (dword_41CAB0 < = dword_41CAB8)
			{
			v9 = dword_41CAB8;
			if (dword_41CAC0 < dword_41CAB8)
			{
			v10 = dword_41CAD0;
			v11 = dword_41CAB4;
			dword_41CAB8 = dword_41CAC0;
			dword_41CAC0 = v9;
			dword_41CAB4 = dword_41CABC;
			dword_41CAD0 = dword_41CAD8;
			dword_41CAD8 = v10;
			v12 = dword_41CACC;
			dword_41CABC = v11;
			dword_41CACC = dword_41CAD4;
			dword_41CAD4 = v12;
			}
			}
			// TODO: Transpile: else
			{
			v6 = dword_41CAD0;
			v7 = dword_41CAB4;
			dword_41CAB8 = dword_41CAB0;
			dword_41CAB0 = v5;
			dword_41CAB4 = dword_41CAAC;
			dword_41CAD0 = dword_41CAC8;
			dword_41CAC8 = v6;
			v8 = dword_41CACC;
			dword_41CAAC = v7;
			dword_41CACC = dword_41CAC4;
			dword_41CAC4 = v8;
			}
			// TODO: Transpile: if ( dword_41CAB0 < dword_453044
			// TODO: Transpile: || dword_41CAC0 > dword_453058
			// TODO: Transpile: || dword_44CE58 > dword_453064
			// TODO: Transpile: || dword_44CE34 < dword_453060 )
			{
			return CallFunction(0x00407910, (int)a1);
			}
			CallFunction(0x00456000);
			CallFunction(0x00407130, dword_41CAE0, dword_41CADC, dword_44CE24, a1[14]);
			v14 = CallFunction(0x00407F40, dword_41CAAC, dword_41CAB0, dword_41CABC, dword_41CAC0);
			v15 = CallFunction(0x00407F40, dword_41CAAC, dword_41CAB0, dword_41CAB4, dword_41CAB8);
			dword_44CE04 = dword_41CAB0;
			dword_44CE00 = dword_41CAAC;
			dword_44CE08 = dword_41CAB4;
			dword_44CE0C = dword_41CAB8;
			dword_44CE10 = dword_41CAC4;
			dword_44CE14 = dword_41CAC8;
			dword_44CE18 = dword_41CACC;
			dword_44CE1C = dword_41CAD0;
			if (v14 < = v15)
			{
			CallFunction(0x00456180, (int * )dword_44CE38, &dword_44CE00);
			dword_44CE00 = dword_41CAAC;
			dword_44CE04 = dword_41CAB0;
			dword_44CE08 = dword_41CABC;
			dword_44CE0C = dword_41CAC0;
			dword_44CE10 = dword_41CAC4;
			dword_44CE14 = dword_41CAC8;
			dword_44CE18 = dword_41CAD4;
			dword_44CE1C = dword_41CAD8;
			CallFunction(0x004561C0, (int * )dword_44CE3C, &dword_44CE00);
			dword_44CE00 = dword_41CAB4;
			dword_44CE04 = dword_41CAB8;
			dword_44CE08 = dword_41CABC;
			dword_44CE0C = dword_41CAC0;
			dword_44CE10 = dword_41CACC;
			dword_44CE14 = dword_41CAD0;
			dword_44CE18 = dword_41CAD4;
			dword_44CE1C = dword_41CAD8;
			CallFunction(0x00456180, (int * )dword_44CE40, &dword_44CE00);
			v26 = 0;
			v27 = 0;
			// TODO: Transpile: dword_44CE30 += dword_45305C * *(_DWORD *)dword_44CE3C;
			if ( * (int * )(dword_44CE38 + 4) > 0)
			{
			// TODO: Transpile: do
			{
			v28 = (_DWORD * )(v26 + dword_44CE38);
			v29 = v26 + dword_44CE3C + 8;
			// TODO: Transpile: v26 += 12;
			// TODO: Transpile: ++v27;
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: *(_DWORD *)(v29 + 8) + dword_44CE30 + 1,
			// TODO: Transpile: v28[4] - *(_DWORD *)(v29 + 8) - 1,
			// TODO: Transpile: (v28[3] << 24) + (unsigned __int16)v28[2],
			// TODO: Transpile: (unsigned __int64)(int)v28[3] >> 8,
			// TODO: Transpile: (_BYTE *)v29);
			// TODO: Transpile: dword_44CE30 += dword_45305C;
			}
			// TODO: Transpile: while ( *(_DWORD *)(dword_44CE38 + 4) > v27 );
			}
			result = dword_44CE40;
			v30 = 0;
			v31 = 0;
			if ( * (int * )(dword_44CE40 + 4) > 0)
			{
			v32 = 12 * v27;
			// TODO: Transpile: do
			{
			v33 = v30 + dword_44CE40;
			v34 = v32 + dword_44CE3C + 8;
			// TODO: Transpile: v32 += 12;
			v35 =  * (_DWORD * )(v30 + dword_44CE40 + 12);
			// TODO: Transpile: v30 += 12;
			// TODO: Transpile: ++v31;
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: *(_DWORD *)(v34 + 8) + dword_44CE30 + 1,
			// TODO: Transpile: *(_DWORD *)(v33 + 16) - *(_DWORD *)(v34 + 8) - 1,
			// TODO: Transpile: (v35 << 24) + (unsigned __int16)*(_DWORD *)(v33 + 8),
			// TODO: Transpile: SBYTE1(v35),
			// TODO: Transpile: (_BYTE *)v34);
			result = dword_44CE40;
			// TODO: Transpile: dword_44CE30 += dword_45305C;
			}
			// TODO: Transpile: while ( *(_DWORD *)(dword_44CE40 + 4) > v31 );
			}
			}
			// TODO: Transpile: else
			{
			CallFunction(0x004561C0, (int * )dword_44CE38, &dword_44CE00);
			dword_44CE00 = dword_41CAAC;
			dword_44CE04 = dword_41CAB0;
			dword_44CE08 = dword_41CABC;
			dword_44CE0C = dword_41CAC0;
			dword_44CE10 = dword_41CAC4;
			dword_44CE14 = dword_41CAC8;
			dword_44CE18 = dword_41CAD4;
			dword_44CE1C = dword_41CAD8;
			CallFunction(0x00456180, (int * )dword_44CE3C, &dword_44CE00);
			dword_44CE00 = dword_41CAB4;
			dword_44CE04 = dword_41CAB8;
			dword_44CE08 = dword_41CABC;
			dword_44CE0C = dword_41CAC0;
			dword_44CE10 = dword_41CACC;
			dword_44CE14 = dword_41CAD0;
			dword_44CE18 = dword_41CAD4;
			dword_44CE1C = dword_41CAD8;
			CallFunction(0x004561C0, (int * )dword_44CE40, &dword_44CE00);
			v16 = 0;
			// TODO: Transpile: dword_44CE30 += dword_45305C * *(_DWORD *)dword_44CE38;
			if ( * (int * )(dword_44CE38 + 4) > 0)
			{
			v17 = 0;
			// TODO: Transpile: do
			{
			v18 = (_DWORD * )(v17 + dword_44CE3C);
			v19 = v17 + dword_44CE38 + 8;
			// TODO: Transpile: v17 += 12;
			// TODO: Transpile: ++v16;
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: *(_DWORD *)(v19 + 8) + dword_44CE30 + 1,
			// TODO: Transpile: v18[4] - *(_DWORD *)(v19 + 8) - 1,
			// TODO: Transpile: (v18[3] << 24) + (unsigned __int16)v18[2],
			// TODO: Transpile: (unsigned __int64)(int)v18[3] >> 8,
			// TODO: Transpile: (_BYTE *)v19);
			// TODO: Transpile: dword_44CE30 += dword_45305C;
			}
			// TODO: Transpile: while ( *(_DWORD *)(dword_44CE38 + 4) > v16 );
			}
			v20 = 0;
			result = dword_44CE40;
			if ( * (int * )(dword_44CE40 + 4) > 0)
			{
			v21 = 3 * v16;
			v22 = 0;
			v23 = 4 * v21;
			// TODO: Transpile: do
			{
			v24 = (_DWORD * )(v23 + dword_44CE3C);
			// TODO: Transpile: v23 += 12;
			v36 = (_BYTE * )(v22 + dword_44CE40 + 8);
			v25 =  * (_DWORD * )(v22 + dword_44CE40 + 16);
			// TODO: Transpile: v22 += 12;
			// TODO: Transpile: ++v20;
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: v25 + dword_44CE30 + 1,
			// TODO: Transpile: v24[4] - v25 - 1,
			// TODO: Transpile: (v24[3] << 24) + (unsigned __int16)v24[2],
			// TODO: Transpile: BYTE1(v24[3]),
			// TODO: Transpile: v36);
			result = dword_44CE40;
			// TODO: Transpile: dword_44CE30 += dword_45305C;
			}
			// TODO: Transpile: while ( *(_DWORD *)(dword_44CE40 + 4) > v20 );
			}
			}
			return result;
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
