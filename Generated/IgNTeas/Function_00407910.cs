using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x00407910
	/// Original name: sub_407910
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_00407910
	{
		private readonly EmulatorEnvironment _env;

		public Function_00407910(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x00407910
		/// </summary>
		[OriginalAddress(0x00407910)]
		public int Execute(int a1)
		{
			// TODO: Transpile: int result; // eax
			// TODO: Transpile: int v2; // esi
			// TODO: Transpile: int v3; // eax
			// TODO: Transpile: int v4; // esi
			// TODO: Transpile: int v5; // edi
			// TODO: Transpile: int v6; // ecx
			// TODO: Transpile: int v7; // edi
			// TODO: Transpile: int v8; // eax
			// TODO: Transpile: int v9; // esi
			// TODO: Transpile: int v10; // ebx
			// TODO: Transpile: int v11; // ecx
			// TODO: Transpile: int v12; // esi
			// TODO: Transpile: int v13; // edi
			// TODO: Transpile: int v14; // ecx
			// TODO: Transpile: int v15; // esi
			// TODO: Transpile: int v16; // ebx
			// TODO: Transpile: int v17; // edi
			// TODO: Transpile: int v18; // ecx
			result = dword_453044;
			if (dword_41CAC0 >= dword_453044)
			{
			result = dword_453058;
			if (dword_41CAB0 <= dword_453058)
			{
			result = dword_453064;
			if (dword_44CE34 <= dword_453064)
			{
			result = dword_453060;
			if (dword_44CE58 >= dword_453060)
			{
			CallFunction(0x00456000);
			CallFunction(0x00407130, dword_41CAE0, dword_41CADC, dword_44CE24,  * (a1 + 56));
			v2 = CallFunction(0x00407F40, dword_41CAB4, dword_41CAB8, dword_41CABC, dword_41CAC0);
			v3 = CallFunction(0x00407F40, dword_41CAAC, dword_41CAB0, dword_41CAB4, dword_41CAB8);
			dword_44CE64 = dword_41CAB0;
			dword_44CE60 = dword_41CAAC;
			dword_44CE68 = dword_41CAB4;
			dword_44CE6C = dword_41CAB8;
			dword_44CE70 = dword_41CAC4;
			dword_44CE74 = dword_41CAC8;
			dword_44CE78 = dword_41CACC;
			dword_44CE7C = dword_41CAD0;
			if (v2 <= v3)
			{
			CallFunction(0x00456520, dword_44CE38,  & dword_44CE60);
			dword_44CE60 = dword_41CAAC;
			dword_44CE64 = dword_41CAB0;
			dword_44CE68 = dword_41CABC;
			dword_44CE6C = dword_41CAC0;
			dword_44CE70 = dword_41CAC4;
			dword_44CE74 = dword_41CAC8;
			dword_44CE78 = dword_41CAD4;
			dword_44CE7C = dword_41CAD8;
			CallFunction(0x00456640, dword_44CE3C,  & dword_44CE60);
			dword_44CE60 = dword_41CAB4;
			dword_44CE64 = dword_41CAB8;
			dword_44CE68 = dword_41CABC;
			dword_44CE6C = dword_41CAC0;
			dword_44CE70 = dword_41CACC;
			dword_44CE74 = dword_41CAD0;
			dword_44CE78 = dword_41CAD4;
			dword_44CE7C = dword_41CAD8;
			CallFunction(0x00456520, dword_44CE40,  & dword_44CE60);
			v12 = 0;
			v13 = 0;
			dword_44CE30 += dword_45305C *  * dword_44CE3C;
			if ( * (dword_44CE38 + 4) > 0)
			{
			do
			{
			v14 =  * (v12 + dword_44CE3C + 16);
			if (dword_453054 <= v14)
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: v14 + dword_44CE30 + 1,
			// TODO: Transpile: *(_DWORD *)(v12 + dword_44CE38 + 16) - v14 - 1,
			// TODO: Transpile: (*(_DWORD *)(v12 + dword_44CE38 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v12 + dword_44CE38 + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v12 + dword_44CE38 + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v12 + dword_44CE3C + 8));
			else
			// TODO: Transpile: sub_407170(
			// TODO: Transpile: dword_44CE30 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v12 + dword_44CE38 + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v12 + dword_44CE38 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v12 + dword_44CE38 + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v12 + dword_44CE38 + 12) >> 8));
			v12 += 12;
			++v13;
			dword_44CE30 += dword_45305C;
			}
			while ( * (dword_44CE38 + 4) > v13)
			}
			result = dword_44CE40;
			v15 = 0;
			v16 = 0;
			if ( * (dword_44CE40 + 4) > 0)
			{
			v17 = 12 * v13;
			do
			{
			v18 =  * (v17 + dword_44CE3C + 16);
			if (dword_453054 <= v18)
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: v18 + dword_44CE30 + 1,
			// TODO: Transpile: *(_DWORD *)(v15 + dword_44CE40 + 16) - v18 - 1,
			// TODO: Transpile: (*(_DWORD *)(v15 + dword_44CE40 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v15 + dword_44CE40 + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v15 + dword_44CE40 + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v17 + dword_44CE3C + 8));
			else
			// TODO: Transpile: sub_407170(
			// TODO: Transpile: dword_44CE30 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v15 + dword_44CE40 + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v15 + dword_44CE40 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v15 + dword_44CE40 + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v15 + dword_44CE40 + 12) >> 8));
			v17 += 12;
			dword_44CE30 += dword_45305C;
			result = dword_44CE40;
			v15 += 12;
			++v16;
			}
			while ( * (dword_44CE40 + 4) > v16)
			}
			}
			else
			{
			CallFunction(0x00456640, dword_44CE38,  & dword_44CE60);
			dword_44CE60 = dword_41CAAC;
			dword_44CE64 = dword_41CAB0;
			dword_44CE68 = dword_41CABC;
			dword_44CE6C = dword_41CAC0;
			dword_44CE70 = dword_41CAC4;
			dword_44CE74 = dword_41CAC8;
			dword_44CE78 = dword_41CAD4;
			dword_44CE7C = dword_41CAD8;
			CallFunction(0x00456520, dword_44CE3C,  & dword_44CE60);
			dword_44CE60 = dword_41CAB4;
			dword_44CE64 = dword_41CAB8;
			dword_44CE68 = dword_41CABC;
			dword_44CE6C = dword_41CAC0;
			dword_44CE70 = dword_41CACC;
			dword_44CE74 = dword_41CAD0;
			dword_44CE78 = dword_41CAD4;
			dword_44CE7C = dword_41CAD8;
			CallFunction(0x00456640, dword_44CE40,  & dword_44CE60);
			v4 = 0;
			dword_44CE30 += dword_45305C *  * dword_44CE38;
			if ( * (dword_44CE38 + 4) > 0)
			{
			v5 = 0;
			do
			{
			v6 =  * (v5 + dword_44CE38 + 16);
			if (dword_453054 <= v6)
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: v6 + dword_44CE30 + 1,
			// TODO: Transpile: *(_DWORD *)(v5 + dword_44CE3C + 16) - v6 - 1,
			// TODO: Transpile: (*(_DWORD *)(v5 + dword_44CE3C + 12) << 24) + (unsigned __int16)*(_DWORD *)(v5 + dword_44CE3C + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v5 + dword_44CE3C + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v5 + dword_44CE38 + 8));
			else
			// TODO: Transpile: sub_407170(
			// TODO: Transpile: dword_44CE30 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v5 + dword_44CE3C + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v5 + dword_44CE3C + 12) << 24) + (unsigned __int16)*(_DWORD *)(v5 + dword_44CE3C + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v5 + dword_44CE3C + 12) >> 8));
			v5 += 12;
			++v4;
			dword_44CE30 += dword_45305C;
			}
			while ( * (dword_44CE38 + 4) > v4)
			}
			v7 = 0;
			result = dword_44CE40;
			if ( * (dword_44CE40 + 4) > 0)
			{
			v8 = 3 * v4;
			v9 = 0;
			v10 = 4 * v8;
			do
			{
			v11 =  * (v9 + dword_44CE40 + 16);
			if (dword_453054 <= v11)
			// TODO: Transpile: sub_407150(
			// TODO: Transpile: v11 + dword_44CE30 + 1,
			// TODO: Transpile: *(_DWORD *)(v10 + dword_44CE3C + 16) - v11 - 1,
			// TODO: Transpile: (*(_DWORD *)(v10 + dword_44CE3C + 12) << 24) + (unsigned __int16)*(_DWORD *)(v10 + dword_44CE3C + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v10 + dword_44CE3C + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v9 + dword_44CE40 + 8));
			else
			// TODO: Transpile: sub_407170(
			// TODO: Transpile: dword_44CE30 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v10 + dword_44CE3C + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v10 + dword_44CE3C + 12) << 24) + (unsigned __int16)*(_DWORD *)(v10 + dword_44CE3C + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v10 + dword_44CE3C + 12) >> 8));
			v10 += 12;
			dword_44CE30 += dword_45305C;
			result = dword_44CE40;
			v9 += 12;
			++v7;
			}
			while ( * (dword_44CE40 + 4) > v7)
			}
			}
			}
			}
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
