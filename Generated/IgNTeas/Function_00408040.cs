using System;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// Function at 0x00408040
	/// Original name: sub_408040
	/// Decompiled from C++ and transpiled to C#
	/// </summary>
	public class Function_00408040
	{
		private readonly EmulatorEnvironment _env;

		public Function_00408040(EmulatorEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// Execute function at 0x00408040
		/// </summary>
		[OriginalAddress(0x00408040)]
		public int Execute(uint a1)
		{
			// TODO: Transpile: int v1; // edx
			// TODO: Transpile: int v2; // eax
			// TODO: Transpile: int v3; // ebx
			// TODO: Transpile: int v4; // ebp
			// TODO: Transpile: int v5; // eax
			// TODO: Transpile: int v6; // eax
			// TODO: Transpile: int v7; // ebx
			// TODO: Transpile: int v8; // ebp
			// TODO: Transpile: int v9; // eax
			// TODO: Transpile: int v10; // eax
			// TODO: Transpile: int v11; // ebx
			// TODO: Transpile: int v12; // ebp
			// TODO: Transpile: int v13; // eax
			// TODO: Transpile: int result; // eax
			// TODO: Transpile: int v15; // esi
			// TODO: Transpile: int v16; // eax
			// TODO: Transpile: int v17; // esi
			// TODO: Transpile: int v18; // edi
			// TODO: Transpile: _DWORD *v19; // ebx
			// TODO: Transpile: int v20; // ebp
			// TODO: Transpile: int v21; // edi
			// TODO: Transpile: int v22; // eax
			// TODO: Transpile: int v23; // esi
			// TODO: Transpile: int v24; // ebx
			// TODO: Transpile: _DWORD *v25; // ebp
			// TODO: Transpile: int v26; // ecx
			// TODO: Transpile: int v27; // esi
			// TODO: Transpile: int v28; // edi
			// TODO: Transpile: _DWORD *v29; // ebx
			// TODO: Transpile: int v30; // ebp
			// TODO: Transpile: int v31; // esi
			// TODO: Transpile: int v32; // ebx
			// TODO: Transpile: int v33; // edi
			// TODO: Transpile: int v34; // ebp
			// TODO: Transpile: int v35; // edx
			// TODO: Transpile: int v36; // eax
			// TODO: Transpile: _BYTE *v37; // [esp-4h] [ebp-14h]
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
			dword_44CEA8 = a1[13];
			dword_44CEB4 = dword_453068;
			if (dword_41CAB4 >= dword_41CAAC)
			{
			v1 = dword_41CAAC;
			dword_44CED8 = dword_41CAB4;
			}
			else
			{
			v1 = dword_41CAB4;
			dword_44CED8 = dword_41CAAC;
			}
			dword_44CEB8 = v1;
			if (dword_41CABC <= dword_44CED8)
			{
			if (dword_44CEB8 > dword_41CABC)
			dword_44CEB8 = dword_41CABC;
			}
			else
			{
			dword_44CED8 = dword_41CABC;
			}
			v2 = dword_41CAC0;
			if (dword_41CAB0 > dword_41CAC0)
			{
			v3 = dword_41CAD8;
			v4 = dword_41CABC;
			dword_41CAC0 = dword_41CAB0;
			dword_41CAB0 = v2;
			dword_41CABC = dword_41CAAC;
			dword_41CAD8 = dword_41CAC8;
			dword_41CAC8 = v3;
			v5 = dword_41CAD4;
			dword_41CAAC = v4;
			dword_41CAD4 = dword_41CAC4;
			dword_41CAC4 = v5;
			}
			v6 = dword_41CAB8;
			if (dword_41CAB0 <= dword_41CAB8)
			{
			v10 = dword_41CAB8;
			if (dword_41CAC0 < dword_41CAB8)
			{
			v11 = dword_41CAD0;
			v12 = dword_41CAB4;
			dword_41CAB8 = dword_41CAC0;
			dword_41CAC0 = v10;
			dword_41CAB4 = dword_41CABC;
			dword_41CAD0 = dword_41CAD8;
			dword_41CAD8 = v11;
			v13 = dword_41CACC;
			dword_41CABC = v12;
			dword_41CACC = dword_41CAD4;
			dword_41CAD4 = v13;
			}
			}
			else
			{
			v7 = dword_41CAD0;
			v8 = dword_41CAB4;
			dword_41CAB8 = dword_41CAB0;
			dword_41CAB0 = v6;
			dword_41CAB4 = dword_41CAAC;
			dword_41CAD0 = dword_41CAC8;
			dword_41CAC8 = v7;
			v9 = dword_41CACC;
			dword_41CAAC = v8;
			dword_41CACC = dword_41CAC4;
			dword_41CAC4 = v9;
			}
			// TODO: Transpile: if ( dword_41CAB0 < dword_453044
			// TODO: Transpile: || dword_41CAC0 > dword_453058
			// TODO: Transpile: || dword_44CED8 > dword_453064
			// TODO: Transpile: || dword_453060 > dword_44CEB8 )
			{
			return CallFunction(0x00408750);
			}
			CallFunction(0x00456000);
			CallFunction(0x00407F80, dword_41CAE0, dword_41CADC, dword_44CEA8);
			v15 = CallFunction(0x00408D70, dword_41CAAC, dword_41CAB0, dword_41CABC, dword_41CAC0);
			v16 = CallFunction(0x00408D70, dword_41CAAC, dword_41CAB0, dword_41CAB4, dword_41CAB8);
			dword_44CE8C = dword_41CAB0;
			dword_44CE88 = dword_41CAAC;
			dword_44CE90 = dword_41CAB4;
			dword_44CE94 = dword_41CAB8;
			dword_44CE98 = dword_41CAC4;
			dword_44CE9C = dword_41CAC8;
			dword_44CEA0 = dword_41CACC;
			dword_44CEA4 = dword_41CAD0;
			if (v15 <= v16)
			{
			CallFunction(0x00456180, dword_44CEBC,  & dword_44CE88);
			dword_44CE88 = dword_41CAAC;
			dword_44CE8C = dword_41CAB0;
			dword_44CE90 = dword_41CABC;
			dword_44CE94 = dword_41CAC0;
			dword_44CE98 = dword_41CAC4;
			dword_44CE9C = dword_41CAC8;
			dword_44CEA0 = dword_41CAD4;
			dword_44CEA4 = dword_41CAD8;
			CallFunction(0x004561C0, dword_44CEC0,  & dword_44CE88);
			dword_44CE88 = dword_41CAB4;
			dword_44CE8C = dword_41CAB8;
			dword_44CE90 = dword_41CABC;
			dword_44CE94 = dword_41CAC0;
			dword_44CE98 = dword_41CACC;
			dword_44CE9C = dword_41CAD0;
			dword_44CEA0 = dword_41CAD4;
			dword_44CEA4 = dword_41CAD8;
			CallFunction(0x00456180, dword_44CEC4,  & dword_44CE88);
			v27 = 0;
			v28 = 0;
			dword_44CEB4 += dword_45305C *  * dword_44CEC0;
			if ( * (dword_44CEBC + 4) > 0)
			{
			do
			{
			v29 = (v27 + dword_44CEBC);
			v30 = v27 + dword_44CEC0 + 8;
			v27 += 12;
			++v28;
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: *(_DWORD *)(v30 + 8) + dword_44CEB4 + 1,
			// TODO: Transpile: v29[4] - *(_DWORD *)(v30 + 8) - 1,
			// TODO: Transpile: (v29[3] << 24) + (unsigned __int16)v29[2],
			// TODO: Transpile: (unsigned __int64)(int)v29[3] >> 8,
			// TODO: Transpile: (_BYTE *)v30);
			dword_44CEB4 += dword_45305C;
			}
			while ( * (dword_44CEBC + 4) > v28)
			}
			result = dword_44CEC4;
			v31 = 0;
			v32 = 0;
			if ( * (dword_44CEC4 + 4) > 0)
			{
			v33 = 12 * v28;
			do
			{
			v34 = v31 + dword_44CEC4;
			v35 = v33 + dword_44CEC0 + 8;
			v33 += 12;
			v36 =  * (v31 + dword_44CEC4 + 12);
			v31 += 12;
			++v32;
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: *(_DWORD *)(v35 + 8) + dword_44CEB4 + 1,
			// TODO: Transpile: *(_DWORD *)(v34 + 16) - *(_DWORD *)(v35 + 8) - 1,
			// TODO: Transpile: (v36 << 24) + (unsigned __int16)*(_DWORD *)(v34 + 8),
			// TODO: Transpile: SBYTE1(v36),
			// TODO: Transpile: (_BYTE *)v35);
			result = dword_44CEC4;
			dword_44CEB4 += dword_45305C;
			}
			while ( * (dword_44CEC4 + 4) > v32)
			}
			}
			else
			{
			CallFunction(0x004561C0, dword_44CEBC,  & dword_44CE88);
			dword_44CE88 = dword_41CAAC;
			dword_44CE8C = dword_41CAB0;
			dword_44CE90 = dword_41CABC;
			dword_44CE94 = dword_41CAC0;
			dword_44CE98 = dword_41CAC4;
			dword_44CE9C = dword_41CAC8;
			dword_44CEA0 = dword_41CAD4;
			dword_44CEA4 = dword_41CAD8;
			CallFunction(0x00456180, dword_44CEC0,  & dword_44CE88);
			dword_44CE88 = dword_41CAB4;
			dword_44CE8C = dword_41CAB8;
			dword_44CE90 = dword_41CABC;
			dword_44CE94 = dword_41CAC0;
			dword_44CE98 = dword_41CACC;
			dword_44CE9C = dword_41CAD0;
			dword_44CEA0 = dword_41CAD4;
			dword_44CEA4 = dword_41CAD8;
			CallFunction(0x004561C0, dword_44CEC4,  & dword_44CE88);
			v17 = 0;
			dword_44CEB4 += dword_45305C *  * dword_44CEBC;
			if ( * (dword_44CEBC + 4) > 0)
			{
			v18 = 0;
			do
			{
			v19 = (v18 + dword_44CEC0);
			v20 = v18 + dword_44CEBC + 8;
			v18 += 12;
			++v17;
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: *(_DWORD *)(v20 + 8) + dword_44CEB4 + 1,
			// TODO: Transpile: v19[4] - *(_DWORD *)(v20 + 8) - 1,
			// TODO: Transpile: (v19[3] << 24) + (unsigned __int16)v19[2],
			// TODO: Transpile: (unsigned __int64)(int)v19[3] >> 8,
			// TODO: Transpile: (_BYTE *)v20);
			dword_44CEB4 += dword_45305C;
			}
			while ( * (dword_44CEBC + 4) > v17)
			}
			v21 = 0;
			result = dword_44CEC4;
			if ( * (dword_44CEC4 + 4) > 0)
			{
			v22 = 3 * v17;
			v23 = 0;
			v24 = 4 * v22;
			do
			{
			v25 = (v24 + dword_44CEC0);
			v24 += 12;
			v37 = (v23 + dword_44CEC4 + 8);
			v26 =  * (v23 + dword_44CEC4 + 16);
			v23 += 12;
			++v21;
			// TODO: Transpile: sub_407FA0(
			// TODO: Transpile: v26 + dword_44CEB4 + 1,
			// TODO: Transpile: v25[4] - v26 - 1,
			// TODO: Transpile: (v25[3] << 24) + (unsigned __int16)v25[2],
			// TODO: Transpile: BYTE1(v25[3]),
			// TODO: Transpile: v37);
			result = dword_44CEC4;
			dword_44CEB4 += dword_45305C;
			}
			while ( * (dword_44CEC4 + 4) > v21)
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
