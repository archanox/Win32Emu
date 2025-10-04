using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class DPlayXModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
	{
		public string Name => "DPLAYX.DLL";

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTPLAYCREATE":
					returnValue = DirectPlayCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "DIRECTPLAYENUMERATEA":
					returnValue = DirectPlayEnumerateA(a.UInt32(0), a.UInt32(1));
					return true;
				default:
					Console.WriteLine($"[DPlayX] Unimplemented export: {export}");
					return false;
			}
		}

		[DllModuleExport(2)]
		private unsafe uint DirectPlayEnumerateA(uint pCallback, uint pContext)
		{
			// TODO: Implement DirectPlayEnumerateA
			Console.WriteLine($"[DPlayX] DirectPlayEnumerateA({nameof(pCallback)}=0x{pCallback:X8}, {nameof(pContext)}=0x{pContext:X8})");
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint DirectPlayCreate(uint lpGUID, uint lplpDP, uint pUnkOuter)
		{
			// TODO: Implement DirectPlayCreate
			Console.WriteLine($"[DPlayX] DirectPlayCreate({nameof(lpGUID)}=0x{lpGUID:X8}, {nameof(lplpDP)}=0x{lplpDP:X8}, {nameof(pUnkOuter)}=0x{pUnkOuter:X8})");
			return 0;
		}

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Auto-generated from [DllModuleExport] attributes
			return DllModuleExportInfo.GetAllExports("DPLAYX.DLL");
		}
	}
}