using Win32Emu.Cpu.IcedImpl;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;

namespace Win32Emu
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: Win32Emu <path-to-pe>");
				return;
			}

			var path = args[0];
			if (!File.Exists(path))
			{
				Console.WriteLine($"File not found: {path}");
				return;
			}

			Console.WriteLine($"[Loader] Loading PE: {path}");
			var vm = new VirtualMemory();
			var loader = new PeImageLoader(vm);
			var image = loader.Load(path);
			Console.WriteLine(
				$"[Loader] Image base=0x{image.BaseAddress:X8} EntryPoint=0x{image.EntryPointAddress:X8} Size=0x{image.ImageSize:X}");
			Console.WriteLine($"[Loader] Imports mapped: {image.ImportAddressMap.Count}");

			var env = new ProcessEnvironment(vm);
			env.InitializeStrings(path, args);

			var cpu = new IcedCpu(vm);
			cpu.SetEip(image.EntryPointAddress);
			cpu.SetRegister("ESP", 0x00200000); // crude stack top

			var dispatcher = new Win32Dispatcher();
			dispatcher.RegisterModule(new Kernel32Module(env, image.BaseAddress));

			const int maxInstr = 500000;
			for (var i = 0; i < maxInstr && !env.ExitRequested; i++)
			{
				var step = cpu.SingleStep(vm);
				if (step.IsCall && image.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
				{
					var dll = imp.dll.ToUpperInvariant();
					var name = imp.name;
					Console.WriteLine($"[Import] {dll}!{name}");
					if (dispatcher.TryInvoke(dll, name, cpu, vm, out var ret, out var argBytes))
					{
						Console.WriteLine($"[Import] Returned 0x{ret:X8}");
						var esp = cpu.GetRegister("ESP");
						var retEip = vm.Read32(esp);
						esp += 4 + (uint)argBytes;
						cpu.SetRegister("ESP", esp);
						cpu.SetEip(retEip);
					}
				}
			}

			Console.WriteLine(
				env.ExitRequested ? "[Exit] Process requested exit." : "[Exit] Instruction limit reached.");
		}
	}
}