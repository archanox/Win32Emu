using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class Glide2XModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
	{
		public string Name => "GLIDE2X.DLL";

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			//var a = new StackArgs(cpu, memory);

			Console.WriteLine($"[Glide2x] Unimplemented export: {export}");
			return false;
		}

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for Glide2x - currently no implemented exports
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{"_ConvertAndDownloadRle@64", 1}
			};
			return exports;
		}
	}
}