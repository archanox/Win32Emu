using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x00408750
	/// Original name: sub_408750
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_00408750
	{
		private readonly EmulatorEnvironment _env;

		public Function_00408750(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x00408750
		/// </summary>
		[OriginalAddress(0x00408750)]
		public int Execute()
		{
			// TODO: Transpile: int result; // eax
			// TODO: Transpile: int v1; // esi
			// TODO: Transpile: int v2; // eax
			// TODO: Transpile: int v3; // esi
			// TODO: Transpile: int v4; // edi
			// TODO: Transpile: int v5; // ecx
			// TODO: Transpile: int v6; // edi
			// TODO: Transpile: int v7; // eax
			// TODO: Transpile: int v8; // esi
			// TODO: Transpile: int v9; // ebx
			// TODO: Transpile: int v10; // ecx
			// TODO: Transpile: int v11; // esi
			// TODO: Transpile: int v12; // edi
			// TODO: Transpile: int v13; // ecx
			// TODO: Transpile: int v14; // esi
			// TODO: Transpile: int v15; // ebx
			// TODO: Transpile: int v16; // edi
			// TODO: Transpile: int v17; // ecx
			result = dword_453044;
			if (dword_41CAC0 >= dword_453044)
			{
			result = dword_453058;
			if (dword_41CAB0 <= dword_453058)
			{
			result = dword_453064;
			if (dword_44CEB8 <= dword_453064)
			{
			result = dword_44CED8;
			if (dword_453060 <= dword_44CED8)
			{
			CallFunction(0x00456000);
			CallFunction(0x00407F80, dword_41CAE0, dword_41CADC, dword_44CEA8);
			v1 = CallFunction(0x00408D70, dword_41CAB4, dword_41CAB8, dword_41CABC, dword_41CAC0);
			v2 = CallFunction(0x00408D70, dword_41CAAC, dword_41CAB0, dword_41CAB4, dword_41CAB8);
			dword_44CEE4 = dword_41CAB0;
			dword_44CEE0 = dword_41CAAC;
			dword_44CEE8 = dword_41CAB4;
			dword_44CEEC = dword_41CAB8;
			dword_44CEF0 = dword_41CAC4;
			dword_44CEF4 = dword_41CAC8;
			dword_44CEF8 = dword_41CACC;
			dword_44CEFC = dword_41CAD0;
			if (v1 <= v2)
			{
			CallFunction(0x00456520, dword_44CEBC,  & dword_44CEE0);
			dword_44CEE0 = dword_41CAAC;
			dword_44CEE4 = dword_41CAB0;
			dword_44CEE8 = dword_41CABC;
			dword_44CEEC = dword_41CAC0;
			dword_44CEF0 = dword_41CAC4;
			dword_44CEF4 = dword_41CAC8;
			dword_44CEF8 = dword_41CAD4;
			dword_44CEFC = dword_41CAD8;
			CallFunction(0x00456640, dword_44CEC0,  & dword_44CEE0);
			dword_44CEE0 = dword_41CAB4;
			dword_44CEE4 = dword_41CAB8;
			dword_44CEE8 = dword_41CABC;
			dword_44CEEC = dword_41CAC0;
			dword_44CEF0 = dword_41CACC;
			dword_44CEF4 = dword_41CAD0;
			dword_44CEF8 = dword_41CAD4;
			dword_44CEFC = dword_41CAD8;
			CallFunction(0x00456520, dword_44CEC4,  & dword_44CEE0);
			v11 = 0;
			v12 = 0;
			dword_44CEB4 += dword_45305C *  * dword_44CEC0;
			if ( * (dword_44CEBC + 4) > 0)
			{
			do
			{
			v13 =  * (v11 + dword_44CEC0 + 16);
			if (dword_453054 <= v13)
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: v13 + dword_44CEB4 + 1,
			// TODO: Transpile: *(_DWORD *)(v11 + dword_44CEBC + 16) - v13 - 1,
			// TODO: Transpile: (*(_DWORD *)(v11 + dword_44CEBC + 12) << 24) + (unsigned __int16)*(_DWORD *)(v11 + dword_44CEBC + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v11 + dword_44CEBC + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v11 + dword_44CEC0 + 8));
			else
			// TODO: Transpile: sub_407FC0(
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v11 + dword_44CEBC + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v11 + dword_44CEBC + 12) << 24) + (unsigned __int16)*(_DWORD *)(v11 + dword_44CEBC + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v11 + dword_44CEBC + 12) >> 8));
			v11 += 12;
			++v12;
			dword_44CEB4 += dword_45305C;
			}
			while ( * (dword_44CEBC + 4) > v12)
			}
			result = dword_44CEC4;
			v14 = 0;
			v15 = 0;
			if ( * (dword_44CEC4 + 4) > 0)
			{
			v16 = 12 * v12;
			do
			{
			v17 =  * (v16 + dword_44CEC0 + 16);
			if (dword_453054 <= v17)
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: v17 + dword_44CEB4 + 1,
			// TODO: Transpile: *(_DWORD *)(v14 + dword_44CEC4 + 16) - v17 - 1,
			// TODO: Transpile: (*(_DWORD *)(v14 + dword_44CEC4 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v14 + dword_44CEC4 + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v14 + dword_44CEC4 + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v16 + dword_44CEC0 + 8));
			else
			// TODO: Transpile: sub_407FC0(
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v14 + dword_44CEC4 + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v14 + dword_44CEC4 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v14 + dword_44CEC4 + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v14 + dword_44CEC4 + 12) >> 8));
			v16 += 12;
			dword_44CEB4 += dword_45305C;
			result = dword_44CEC4;
			v14 += 12;
			++v15;
			}
			while ( * (dword_44CEC4 + 4) > v15)
			}
			}
			else
			{
			CallFunction(0x00456640, dword_44CEBC,  & dword_44CEE0);
			dword_44CEE0 = dword_41CAAC;
			dword_44CEE4 = dword_41CAB0;
			dword_44CEE8 = dword_41CABC;
			dword_44CEEC = dword_41CAC0;
			dword_44CEF0 = dword_41CAC4;
			dword_44CEF4 = dword_41CAC8;
			dword_44CEF8 = dword_41CAD4;
			dword_44CEFC = dword_41CAD8;
			CallFunction(0x00456520, dword_44CEC0,  & dword_44CEE0);
			dword_44CEE0 = dword_41CAB4;
			dword_44CEE4 = dword_41CAB8;
			dword_44CEE8 = dword_41CABC;
			dword_44CEEC = dword_41CAC0;
			dword_44CEF0 = dword_41CACC;
			dword_44CEF4 = dword_41CAD0;
			dword_44CEF8 = dword_41CAD4;
			dword_44CEFC = dword_41CAD8;
			CallFunction(0x00456640, dword_44CEC4,  & dword_44CEE0);
			v3 = 0;
			dword_44CEB4 += dword_45305C *  * dword_44CEBC;
			if ( * (dword_44CEBC + 4) > 0)
			{
			v4 = 0;
			do
			{
			v5 =  * (v4 + dword_44CEBC + 16);
			if (dword_453054 <= v5)
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: v5 + dword_44CEB4 + 1,
			// TODO: Transpile: *(_DWORD *)(v4 + dword_44CEC0 + 16) - v5 - 1,
			// TODO: Transpile: (*(_DWORD *)(v4 + dword_44CEC0 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v4 + dword_44CEC0 + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v4 + dword_44CEC0 + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v4 + dword_44CEBC + 8));
			else
			// TODO: Transpile: sub_407FC0(
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v4 + dword_44CEC0 + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v4 + dword_44CEC0 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v4 + dword_44CEC0 + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v4 + dword_44CEC0 + 12) >> 8));
			v4 += 12;
			++v3;
			dword_44CEB4 += dword_45305C;
			}
			while ( * (dword_44CEBC + 4) > v3)
			}
			v6 = 0;
			result = dword_44CEC4;
			if ( * (dword_44CEC4 + 4) > 0)
			{
			v7 = 3 * v3;
			v8 = 0;
			v9 = 4 * v7;
			do
			{
			v10 =  * (v8 + dword_44CEC4 + 16);
			if (dword_453054 <= v10)
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: v10 + dword_44CEB4 + 1,
			// TODO: Transpile: *(_DWORD *)(v9 + dword_44CEC0 + 16) - v10 - 1,
			// TODO: Transpile: (*(_DWORD *)(v9 + dword_44CEC0 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v9 + dword_44CEC0 + 8),
			// TODO: Transpile: (unsigned __int64)*(int *)(v9 + dword_44CEC0 + 12) >> 8,
			// TODO: Transpile: (_BYTE *)(v8 + dword_44CEC4 + 8));
			else
			// TODO: Transpile: sub_407FC0(
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: dword_44CEB4 + dword_453054,
			// TODO: Transpile: *(_DWORD *)(v9 + dword_44CEC0 + 16) - dword_453054,
			// TODO: Transpile: (*(_DWORD *)(v9 + dword_44CEC0 + 12) << 24) + (unsigned __int16)*(_DWORD *)(v9 + dword_44CEC0 + 8),
			// TODO: Transpile: (_BYTE *)((unsigned __int64)*(int *)(v9 + dword_44CEC0 + 12) >> 8));
			v9 += 12;
			dword_44CEB4 += dword_45305C;
			result = dword_44CEC4;
			v8 += 12;
			++v6;
			}
			while ( * (dword_44CEC4 + 4) > v6)
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
