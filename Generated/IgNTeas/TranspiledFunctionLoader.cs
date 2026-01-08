using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Win32Emu;

namespace IgNTeas.Generated
{
	/// <summary>
	/// JIT integration loader for transpiled functions
	/// </summary>
	public class TranspiledFunctionLoader
	{
		private readonly Dictionary<uint, Func<EmulatorEnvironment, object[], object>> _functions = new();
		private readonly ILogger? _logger;

		public TranspiledFunctionLoader(ILogger? logger = null)
		{
			_logger = logger;
			LoadFunctions();
		}

		private void LoadFunctions()
		{
			// Register function at 0x00401000 (sub_401000)
			_functions[0x00401000u] = (env, args) =>
			{
				var instance = new Function_00401000(env);
				return instance.Execute();
			};

			// Register function at 0x004011A0 (sub_4011A0)
			_functions[0x004011A0u] = (env, args) =>
			{
				var instance = new Function_004011A0(env);
				return instance.Execute();
			};

			// Register function at 0x004012A0 (sub_4012A0)
			_functions[0x004012A0u] = (env, args) =>
			{
				var instance = new Function_004012A0(env);
				return instance.Execute();
			};

			// Register function at 0x004013A0 (sub_4013A0)
			_functions[0x004013A0u] = (env, args) =>
			{
				var instance = new Function_004013A0(env);
				return instance.Execute();
			};

			// Register function at 0x004013C0 (sub_4013C0)
			_functions[0x004013C0u] = (env, args) =>
			{
				var instance = new Function_004013C0(env);
				return instance.Execute();
			};

			// Register function at 0x004019D0 (sub_4019D0)
			_functions[0x004019D0u] = (env, args) =>
			{
				var instance = new Function_004019D0(env);
				return instance.Execute();
			};

			// Register function at 0x00401B60 (sub_401B60)
			_functions[0x00401B60u] = (env, args) =>
			{
				var instance = new Function_00401B60(env);
				return instance.Execute();
			};

			// Register function at 0x00401BE0 (sub_401BE0)
			_functions[0x00401BE0u] = (env, args) =>
			{
				var instance = new Function_00401BE0(env);
				return instance.Execute();
			};

			// Register function at 0x00401C00 (sub_401C00)
			_functions[0x00401C00u] = (env, args) =>
			{
				var instance = new Function_00401C00(env);
				return instance.Execute();
			};

			// Register function at 0x00401C20 (sub_401C20)
			_functions[0x00401C20u] = (env, args) =>
			{
				var instance = new Function_00401C20(env);
				return instance.Execute();
			};

			// Register function at 0x00401D20 (sub_401D20)
			_functions[0x00401D20u] = (env, args) =>
			{
				var instance = new Function_00401D20(env);
				return instance.Execute();
			};

			// Register function at 0x00401E30 (sub_401E30)
			_functions[0x00401E30u] = (env, args) =>
			{
				var instance = new Function_00401E30(env);
				return instance.Execute();
			};

			// Register function at 0x004023F0 (sub_4023F0)
			_functions[0x004023F0u] = (env, args) =>
			{
				var instance = new Function_004023F0(env);
				return instance.Execute();
			};

			// Register function at 0x00402410 (sub_402410)
			_functions[0x00402410u] = (env, args) =>
			{
				var instance = new Function_00402410(env);
				return instance.Execute();
			};

			// Register function at 0x00402520 (sub_402520)
			_functions[0x00402520u] = (env, args) =>
			{
				var instance = new Function_00402520(env);
				return instance.Execute();
			};

			// Register function at 0x004025D0 (sub_4025D0)
			_functions[0x004025D0u] = (env, args) =>
			{
				var instance = new Function_004025D0(env);
				return instance.Execute();
			};

			// Register function at 0x004027D0 (sub_4027D0)
			_functions[0x004027D0u] = (env, args) =>
			{
				var instance = new Function_004027D0(env);
				return instance.Execute();
			};

			// Register function at 0x00402840 (sub_402840)
			_functions[0x00402840u] = (env, args) =>
			{
				var instance = new Function_00402840(env);
				return instance.Execute();
			};

			// Register function at 0x00402A80 (sub_402A80)
			_functions[0x00402A80u] = (env, args) =>
			{
				var instance = new Function_00402A80(env);
				return instance.Execute();
			};

			// Register function at 0x00402AA0 (sub_402AA0)
			_functions[0x00402AA0u] = (env, args) =>
			{
				var instance = new Function_00402AA0(env);
				return instance.Execute();
			};

			// Register function at 0x00402E10 (sub_402E10)
			_functions[0x00402E10u] = (env, args) =>
			{
				var instance = new Function_00402E10(env);
				return instance.Execute();
			};

			// Register function at 0x00402E30 (sub_402E30)
			_functions[0x00402E30u] = (env, args) =>
			{
				var instance = new Function_00402E30(env);
				return instance.Execute();
			};

			// Register function at 0x00402F00 (sub_402F00)
			_functions[0x00402F00u] = (env, args) =>
			{
				var instance = new Function_00402F00(env);
				return instance.Execute();
			};

			// Register function at 0x00402F70 (sub_402F70)
			_functions[0x00402F70u] = (env, args) =>
			{
				var instance = new Function_00402F70(env);
				return instance.Execute();
			};

			// Register function at 0x004030C0 (sub_4030C0)
			_functions[0x004030C0u] = (env, args) =>
			{
				var instance = new Function_004030C0(env);
				return instance.Execute();
			};

			// Register function at 0x00403140 (WinMain)
			_functions[0x00403140u] = (env, args) =>
			{
				var instance = new Function_00403140(env);
				return instance.Execute();
			};

			// Register function at 0x004032A0 (sub_4032A0)
			_functions[0x004032A0u] = (env, args) =>
			{
				var instance = new Function_004032A0(env);
				return instance.Execute();
			};

			// Register function at 0x00403340 (sub_403340)
			_functions[0x00403340u] = (env, args) =>
			{
				var instance = new Function_00403340(env);
				return instance.Execute();
			};

			// Register function at 0x004034D0 (sub_4034D0)
			_functions[0x004034D0u] = (env, args) =>
			{
				var instance = new Function_004034D0(env);
				return instance.Execute();
			};

			// Register function at 0x00403560 (sub_403560)
			_functions[0x00403560u] = (env, args) =>
			{
				var instance = new Function_00403560(env);
				return instance.Execute();
			};

			// Register function at 0x00403570 (sub_403570)
			_functions[0x00403570u] = (env, args) =>
			{
				var instance = new Function_00403570(env);
				return instance.Execute();
			};

			// Register function at 0x004035A0 (sub_4035A0)
			_functions[0x004035A0u] = (env, args) =>
			{
				var instance = new Function_004035A0(env);
				return instance.Execute();
			};

			// Register function at 0x00403820 (sub_403820)
			_functions[0x00403820u] = (env, args) =>
			{
				var instance = new Function_00403820(env);
				return instance.Execute();
			};

			// Register function at 0x004038E0 (sub_4038E0)
			_functions[0x004038E0u] = (env, args) =>
			{
				var instance = new Function_004038E0(env);
				return instance.Execute();
			};

			// Register function at 0x00403BF0 (sub_403BF0)
			_functions[0x00403BF0u] = (env, args) =>
			{
				var instance = new Function_00403BF0(env);
				return instance.Execute();
			};

			// Register function at 0x00403CB0 (sub_403CB0)
			_functions[0x00403CB0u] = (env, args) =>
			{
				var instance = new Function_00403CB0(env);
				return instance.Execute();
			};

			// Register function at 0x00403D20 (sub_403D20)
			_functions[0x00403D20u] = (env, args) =>
			{
				var instance = new Function_00403D20(env);
				return instance.Execute();
			};

			// Register function at 0x00404320 (sub_404320)
			_functions[0x00404320u] = (env, args) =>
			{
				var instance = new Function_00404320(env);
				return instance.Execute();
			};

			// Register function at 0x00404490 (sub_404490)
			_functions[0x00404490u] = (env, args) =>
			{
				var instance = new Function_00404490(env);
				return instance.Execute();
			};

			// Register function at 0x004044D0 (sub_4044D0)
			_functions[0x004044D0u] = (env, args) =>
			{
				var instance = new Function_004044D0(env);
				return instance.Execute();
			};

			// Register function at 0x00404500 (sub_404500)
			_functions[0x00404500u] = (env, args) =>
			{
				var instance = new Function_00404500(env);
				return instance.Execute();
			};

			// Register function at 0x00404530 (sub_404530)
			_functions[0x00404530u] = (env, args) =>
			{
				var instance = new Function_00404530(env);
				return instance.Execute();
			};

			// Register function at 0x004045E0 (sub_4045E0)
			_functions[0x004045E0u] = (env, args) =>
			{
				var instance = new Function_004045E0(env);
				return instance.Execute();
			};

			// Register function at 0x00404600 (sub_404600)
			_functions[0x00404600u] = (env, args) =>
			{
				var instance = new Function_00404600(env);
				return instance.Execute();
			};

			// Register function at 0x00404640 (sub_404640)
			_functions[0x00404640u] = (env, args) =>
			{
				var instance = new Function_00404640(env);
				return instance.Execute();
			};

			// Register function at 0x00404670 (sub_404670)
			_functions[0x00404670u] = (env, args) =>
			{
				var instance = new Function_00404670(env);
				return instance.Execute();
			};

			// Register function at 0x004046B0 (sub_4046B0)
			_functions[0x004046B0u] = (env, args) =>
			{
				var instance = new Function_004046B0(env);
				return instance.Execute();
			};

			// Register function at 0x004046E0 (sub_4046E0)
			_functions[0x004046E0u] = (env, args) =>
			{
				var instance = new Function_004046E0(env);
				return instance.Execute();
			};

			// Register function at 0x004046F0 (sub_4046F0)
			_functions[0x004046F0u] = (env, args) =>
			{
				var instance = new Function_004046F0(env);
				return instance.Execute();
			};

			// Register function at 0x00404890 (sub_404890)
			_functions[0x00404890u] = (env, args) =>
			{
				var instance = new Function_00404890(env);
				return instance.Execute();
			};

			// Register function at 0x00404910 (sub_404910)
			_functions[0x00404910u] = (env, args) =>
			{
				var instance = new Function_00404910(env);
				return instance.Execute();
			};

			// Register function at 0x00404A90 (sub_404A90)
			_functions[0x00404A90u] = (env, args) =>
			{
				var instance = new Function_00404A90(env);
				return instance.Execute();
			};

			// Register function at 0x00404AA0 (sub_404AA0)
			_functions[0x00404AA0u] = (env, args) =>
			{
				var instance = new Function_00404AA0(env);
				return instance.Execute();
			};

			// Register function at 0x00404AC0 (sub_404AC0)
			_functions[0x00404AC0u] = (env, args) =>
			{
				var instance = new Function_00404AC0(env);
				return instance.Execute();
			};

			// Register function at 0x00404B00 (sub_404B00)
			_functions[0x00404B00u] = (env, args) =>
			{
				var instance = new Function_00404B00(env);
				return instance.Execute();
			};

			// Register function at 0x00404B30 (sub_404B30)
			_functions[0x00404B30u] = (env, args) =>
			{
				var instance = new Function_00404B30(env);
				return instance.Execute();
			};

			// Register function at 0x00404B40 (sub_404B40)
			_functions[0x00404B40u] = (env, args) =>
			{
				var instance = new Function_00404B40(env);
				return instance.Execute();
			};

			// Register function at 0x00404B90 (sub_404B90)
			_functions[0x00404B90u] = (env, args) =>
			{
				var instance = new Function_00404B90(env);
				return instance.Execute();
			};

			// Register function at 0x00404C30 (sub_404C30)
			_functions[0x00404C30u] = (env, args) =>
			{
				var instance = new Function_00404C30(env);
				return instance.Execute();
			};

			// Register function at 0x00404D20 (sub_404D20)
			_functions[0x00404D20u] = (env, args) =>
			{
				var instance = new Function_00404D20(env);
				return instance.Execute();
			};

			// Register function at 0x00404D30 (sub_404D30)
			_functions[0x00404D30u] = (env, args) =>
			{
				var instance = new Function_00404D30(env);
				return instance.Execute();
			};

			// Register function at 0x00404D40 (sub_404D40)
			_functions[0x00404D40u] = (env, args) =>
			{
				var instance = new Function_00404D40(env);
				return instance.Execute();
			};

			// Register function at 0x00404D50 (sub_404D50)
			_functions[0x00404D50u] = (env, args) =>
			{
				var instance = new Function_00404D50(env);
				return instance.Execute();
			};

			// Register function at 0x00404D60 (sub_404D60)
			_functions[0x00404D60u] = (env, args) =>
			{
				var instance = new Function_00404D60(env);
				return instance.Execute();
			};

			// Register function at 0x00404E20 (sub_404E20)
			_functions[0x00404E20u] = (env, args) =>
			{
				var instance = new Function_00404E20(env);
				return instance.Execute();
			};

			// Register function at 0x00404F10 (sub_404F10)
			_functions[0x00404F10u] = (env, args) =>
			{
				var instance = new Function_00404F10(env);
				return instance.Execute();
			};

			// Register function at 0x00404FB0 (sub_404FB0)
			_functions[0x00404FB0u] = (env, args) =>
			{
				var instance = new Function_00404FB0(env);
				return instance.Execute();
			};

			// Register function at 0x00405050 (sub_405050)
			_functions[0x00405050u] = (env, args) =>
			{
				var instance = new Function_00405050(env);
				return instance.Execute();
			};

			// Register function at 0x004050C0 (sub_4050C0)
			_functions[0x004050C0u] = (env, args) =>
			{
				var instance = new Function_004050C0(env);
				return instance.Execute();
			};

			// Register function at 0x00405170 (sub_405170)
			_functions[0x00405170u] = (env, args) =>
			{
				var instance = new Function_00405170(env);
				return instance.Execute();
			};

			// Register function at 0x004052C0 (sub_4052C0)
			_functions[0x004052C0u] = (env, args) =>
			{
				var instance = new Function_004052C0(env);
				return instance.Execute();
			};

			// Register function at 0x004052D0 (sub_4052D0)
			_functions[0x004052D0u] = (env, args) =>
			{
				var instance = new Function_004052D0(env);
				return instance.Execute();
			};

			// Register function at 0x00405900 (sub_405900)
			_functions[0x00405900u] = (env, args) =>
			{
				var instance = new Function_00405900(env);
				return instance.Execute();
			};

			// Register function at 0x00405BF0 (sub_405BF0)
			_functions[0x00405BF0u] = (env, args) =>
			{
				var instance = new Function_00405BF0(env);
				return instance.Execute();
			};

			// Register function at 0x00405CE0 (sub_405CE0)
			_functions[0x00405CE0u] = (env, args) =>
			{
				var instance = new Function_00405CE0(env);
				return instance.Execute();
			};

			// Register function at 0x00405CF0 (sub_405CF0)
			_functions[0x00405CF0u] = (env, args) =>
			{
				var instance = new Function_00405CF0(env);
				return instance.Execute();
			};

			// Register function at 0x00405D60 (sub_405D60)
			_functions[0x00405D60u] = (env, args) =>
			{
				var instance = new Function_00405D60(env);
				return instance.Execute();
			};

			// Register function at 0x00405EF0 (sub_405EF0)
			_functions[0x00405EF0u] = (env, args) =>
			{
				var instance = new Function_00405EF0(env);
				return instance.Execute();
			};

			// Register function at 0x00405FD0 (sub_405FD0)
			_functions[0x00405FD0u] = (env, args) =>
			{
				var instance = new Function_00405FD0(env);
				return instance.Execute();
			};

			// Register function at 0x00406040 (sub_406040)
			_functions[0x00406040u] = (env, args) =>
			{
				var instance = new Function_00406040(env);
				return instance.Execute();
			};

			// Register function at 0x00406050 (sub_406050)
			_functions[0x00406050u] = (env, args) =>
			{
				var instance = new Function_00406050(env);
				return instance.Execute();
			};

			// Register function at 0x004061A0 (sub_4061A0)
			_functions[0x004061A0u] = (env, args) =>
			{
				var instance = new Function_004061A0(env);
				return instance.Execute();
			};

			// Register function at 0x004061F0 (sub_4061F0)
			_functions[0x004061F0u] = (env, args) =>
			{
				var instance = new Function_004061F0(env);
				return instance.Execute();
			};

			// Register function at 0x00406250 (sub_406250)
			_functions[0x00406250u] = (env, args) =>
			{
				var instance = new Function_00406250(env);
				return instance.Execute();
			};

			// Register function at 0x00406460 (sub_406460)
			_functions[0x00406460u] = (env, args) =>
			{
				var instance = new Function_00406460(env);
				return instance.Execute();
			};

			// Register function at 0x00406490 (sub_406490)
			_functions[0x00406490u] = (env, args) =>
			{
				var instance = new Function_00406490(env);
				return instance.Execute();
			};

			// Register function at 0x00406520 (sub_406520)
			_functions[0x00406520u] = (env, args) =>
			{
				var instance = new Function_00406520(env);
				return instance.Execute();
			};

			// Register function at 0x00406570 (sub_406570)
			_functions[0x00406570u] = (env, args) =>
			{
				var instance = new Function_00406570(env);
				return instance.Execute();
			};

			// Register function at 0x00406590 (sub_406590)
			_functions[0x00406590u] = (env, args) =>
			{
				var instance = new Function_00406590(env);
				return instance.Execute();
			};

			// Register function at 0x004065A0 (sub_4065A0)
			_functions[0x004065A0u] = (env, args) =>
			{
				var instance = new Function_004065A0(env);
				return instance.Execute();
			};

			// Register function at 0x004065F0 (sub_4065F0)
			_functions[0x004065F0u] = (env, args) =>
			{
				var instance = new Function_004065F0(env);
				return instance.Execute();
			};

			// Register function at 0x00406630 (sub_406630)
			_functions[0x00406630u] = (env, args) =>
			{
				var instance = new Function_00406630(env);
				return instance.Execute();
			};

			// Register function at 0x004066D0 (sub_4066D0)
			_functions[0x004066D0u] = (env, args) =>
			{
				var instance = new Function_004066D0(env);
				return instance.Execute();
			};

			// Register function at 0x004067E0 (sub_4067E0)
			_functions[0x004067E0u] = (env, args) =>
			{
				var instance = new Function_004067E0(env);
				return instance.Execute();
			};

			// Register function at 0x00406860 (sub_406860)
			_functions[0x00406860u] = (env, args) =>
			{
				var instance = new Function_00406860(env);
				return instance.Execute();
			};

			// Register function at 0x004069B0 (sub_4069B0)
			_functions[0x004069B0u] = (env, args) =>
			{
				var instance = new Function_004069B0(env);
				return instance.Execute();
			};

			// Register function at 0x004069D0 (sub_4069D0)
			_functions[0x004069D0u] = (env, args) =>
			{
				var instance = new Function_004069D0(env);
				return instance.Execute();
			};

			// Register function at 0x00406A10 (sub_406A10)
			_functions[0x00406A10u] = (env, args) =>
			{
				var instance = new Function_00406A10(env);
				return instance.Execute();
			};

			// Register function at 0x00406B70 (sub_406B70)
			_functions[0x00406B70u] = (env, args) =>
			{
				var instance = new Function_00406B70(env);
				return instance.Execute();
			};

			// Register function at 0x00406D20 (sub_406D20)
			_functions[0x00406D20u] = (env, args) =>
			{
				var instance = new Function_00406D20(env);
				return instance.Execute();
			};

			// Register function at 0x00406D50 (sub_406D50)
			_functions[0x00406D50u] = (env, args) =>
			{
				var instance = new Function_00406D50(env);
				return instance.Execute();
			};

			// Register function at 0x00406D90 (sub_406D90)
			_functions[0x00406D90u] = (env, args) =>
			{
				var instance = new Function_00406D90(env);
				return instance.Execute();
			};

			// Register function at 0x00406F40 (sub_406F40)
			_functions[0x00406F40u] = (env, args) =>
			{
				var instance = new Function_00406F40(env);
				return instance.Execute();
			};

			// Register function at 0x00406F60 (sub_406F60)
			_functions[0x00406F60u] = (env, args) =>
			{
				var instance = new Function_00406F60(env);
				return instance.Execute();
			};

			// Register function at 0x00406F70 (sub_406F70)
			_functions[0x00406F70u] = (env, args) =>
			{
				var instance = new Function_00406F70(env);
				return instance.Execute();
			};

			// Register function at 0x00406FC0 (sub_406FC0)
			_functions[0x00406FC0u] = (env, args) =>
			{
				var instance = new Function_00406FC0(env);
				return instance.Execute();
			};

			// Register function at 0x004070F0 (sub_4070F0)
			_functions[0x004070F0u] = (env, args) =>
			{
				var instance = new Function_004070F0(env);
				return instance.Execute();
			};

			// Register function at 0x00407110 (sub_407110)
			_functions[0x00407110u] = (env, args) =>
			{
				var instance = new Function_00407110(env);
				return instance.Execute();
			};

			// Register function at 0x00407150 (sub_407150)
			_functions[0x00407150u] = (env, args) =>
			{
				var instance = new Function_00407150(env);
				return instance.Execute();
			};

			// Register function at 0x00407170 (sub_407170)
			_functions[0x00407170u] = (env, args) =>
			{
				var instance = new Function_00407170(env);
				return instance.Execute();
			};

			// Register function at 0x00407190 (sub_407190)
			_functions[0x00407190u] = (env, args) =>
			{
				var instance = new Function_00407190(env);
				return instance.Execute();
			};

			// Register function at 0x004071F0 (sub_4071F0)
			_functions[0x004071F0u] = (env, args) =>
			{
				var instance = new Function_004071F0(env);
				return instance.Execute();
			};

			// Register function at 0x00407910 (sub_407910)
			_functions[0x00407910u] = (env, args) =>
			{
				var instance = new Function_00407910(env);
				return instance.Execute();
			};

			// Register function at 0x00407F40 (sub_407F40)
			_functions[0x00407F40u] = (env, args) =>
			{
				var instance = new Function_00407F40(env);
				return instance.Execute();
			};

			// Register function at 0x00407FA0 (sub_407FA0)
			_functions[0x00407FA0u] = (env, args) =>
			{
				var instance = new Function_00407FA0(env);
				return instance.Execute();
			};

			// Register function at 0x00407FE0 (sub_407FE0)
			_functions[0x00407FE0u] = (env, args) =>
			{
				var instance = new Function_00407FE0(env);
				return instance.Execute();
			};

			// Register function at 0x00408040 (sub_408040)
			_functions[0x00408040u] = (env, args) =>
			{
				var instance = new Function_00408040(env);
				return instance.Execute();
			};

			// Register function at 0x00408750 (sub_408750)
			_functions[0x00408750u] = (env, args) =>
			{
				var instance = new Function_00408750(env);
				return instance.Execute();
			};

			// Register function at 0x00408D70 (sub_408D70)
			_functions[0x00408D70u] = (env, args) =>
			{
				var instance = new Function_00408D70(env);
				return instance.Execute();
			};

			// Register function at 0x0040FBA0 (sub_40FBA0)
			_functions[0x0040FBA0u] = (env, args) =>
			{
				var instance = new Function_0040FBA0(env);
				return instance.Execute();
			};

			// Register function at 0x00410E20 (sub_410E20)
			_functions[0x00410E20u] = (env, args) =>
			{
				var instance = new Function_00410E20(env);
				return instance.Execute();
			};

			// Register function at 0x00415E90 (sub_415E90)
			_functions[0x00415E90u] = (env, args) =>
			{
				var instance = new Function_00415E90(env);
				return instance.Execute();
			};

			// Register function at 0x00415EB0 (sub_415EB0)
			_functions[0x00415EB0u] = (env, args) =>
			{
				var instance = new Function_00415EB0(env);
				return instance.Execute();
			};

			// Register function at 0x00415ED0 (sub_415ED0)
			_functions[0x00415ED0u] = (env, args) =>
			{
				var instance = new Function_00415ED0(env);
				return instance.Execute();
			};

			// Register function at 0x00415F10 (sub_415F10)
			_functions[0x00415F10u] = (env, args) =>
			{
				var instance = new Function_00415F10(env);
				return instance.Execute();
			};

			// Register function at 0x00419EA0 (sub_419EA0)
			_functions[0x00419EA0u] = (env, args) =>
			{
				var instance = new Function_00419EA0(env);
				return instance.Execute();
			};

			// Register function at 0x00456000 (sub_456000)
			_functions[0x00456000u] = (env, args) =>
			{
				var instance = new Function_00456000(env);
				return instance.Execute();
			};

			// Register function at 0x00456B20 (sub_456B20)
			_functions[0x00456B20u] = (env, args) =>
			{
				var instance = new Function_00456B20(env);
				return instance.Execute();
			};

			_logger?.LogInformation("Loaded {Count} transpiled functions", _functions.Count);
		}

		/// <summary>
		/// Try to execute a transpiled function at the given address
		/// </summary>
		public bool TryExecuteFunction(uint address, EmulatorEnvironment env, object[] args, out object? result)
		{
			if (_functions.TryGetValue(address, out var func))
			{
				try
				{
					result = func(env, args);
					return true;
				}
				catch (Exception ex)
				{
					_logger?.LogError(ex, "Error executing transpiled function at 0x{Address:X8}", address);
					result = null;
					return false;
				}
			}
			result = null;
			return false;
		}

		/// <summary>
		/// Check if a transpiled function exists at the given address
		/// </summary>
		public bool HasFunction(uint address)
		{
			return _functions.ContainsKey(address);
		}

		/// <summary>
		/// Get all registered function addresses
		/// </summary>
		public IEnumerable<uint> GetFunctionAddresses()
		{
			return _functions.Keys;
		}
	}
}
