using Win32Emu.Cpu.IcedImpl;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Debugging;
using System.Linq;

namespace Win32Emu
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: Win32Emu <path-to-pe> [--debug]");
				Console.WriteLine("  --debug: Enable enhanced debugging to catch 0xFFFFFFFD memory access errors");
				return;
			}

			// Parse command line arguments
			var debugMode = args.Contains("--debug");
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
			env.InitializeStrings(path, args.Where(a => a != "--debug").ToArray());

			var cpu = new IcedCpu(vm);
			cpu.SetEip(image.EntryPointAddress);
			cpu.SetRegister("ESP", 0x00200000); // crude stack top

			var dispatcher = new Win32Dispatcher();
			dispatcher.RegisterModule(new Kernel32Module(env, image.BaseAddress, loader));

			if (debugMode)
			{
				RunWithEnhancedDebugging(cpu, vm, env, dispatcher, image);
			}
			else
			{
				RunNormal(cpu, vm, env, dispatcher, image);
			}

			Console.WriteLine(
				env.ExitRequested ? "[Exit] Process requested exit." : "[Exit] Instruction limit reached.");
		}

		private static void RunNormal(IcedCpu cpu, VirtualMemory vm, ProcessEnvironment env, Win32Dispatcher dispatcher, LoadedImage image)
		{
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
		}

		private static void RunWithEnhancedDebugging(IcedCpu cpu, VirtualMemory vm, ProcessEnvironment env, Win32Dispatcher dispatcher, LoadedImage image)
		{
			// *** ENHANCED DEBUGGING SETUP ***
			var debugger = cpu.CreateDebugger(vm);
			debugger.EnableSuspiciousRegisterDetection = true;
			debugger.LogToConsole = true;
			debugger.LogAllInstructions = false; // Set to true for full instruction trace
			debugger.SuspiciousThreshold = 0x1000; // Adjust if needed

			Console.WriteLine("[Debug] Enhanced debugging enabled - will catch 0xFFFFFFFD errors");
			Console.WriteLine("[Debug] Monitoring for suspicious register values");
			Console.WriteLine("[Debug] Use --debug argument to enable this mode");
			
			const int maxInstr = 500000;
			for (var i = 0; i < maxInstr && !env.ExitRequested; i++)
			{
				var currentEip = cpu.GetEip();
				
				// *** CHECK FOR SYNTHETIC IMPORT ADDRESS EXECUTION ***
				if (currentEip >= 0x0F000000 && currentEip < 0x10000000)
				{
					Console.WriteLine($"\n[Debug] *** CPU TRYING TO EXECUTE SYNTHETIC IMPORT ADDRESS! ***");
					Console.WriteLine($"[Debug] EIP=0x{currentEip:X8} at instruction {i}");
					
					if (image.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
					{
						Console.WriteLine($"[Debug] This is import: {importInfo.dll}!{importInfo.name}");
					}
					else
					{
						Console.WriteLine($"[Debug] Unknown synthetic address - not in import map");
					}
					
					Console.WriteLine("[Debug] This should now execute an INT3 stub that will be handled as an import call");
					cpu.LogRegisters("[Debug] ");
					
					// Show the actual instruction bytes at this address
					try 
					{
						var instrBytes = vm.GetSpan(currentEip, 4);
						Console.WriteLine($"[Debug] Instruction bytes at synthetic address: {Convert.ToHexString(instrBytes)}");
						
						if (instrBytes[0] == 0xCC)
						{
							Console.WriteLine("[Debug] Good! Found INT3 (0xCC) stub - continuing execution");
							// Don't break, let the INT3 execute and be handled
						}
						else
						{
							Console.WriteLine("[Debug] ERROR: Expected INT3 (0xCC) but found different instruction");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[Debug] Could not read instruction bytes: {ex.Message}");
					}
					
					// Continue execution instead of breaking - let's see if the INT3 handling works
				}
				
				// *** CHECK FOR PROBLEMATIC EIP (keep existing check for other cases) ***
				if (debugger.IsProblematicEip(0x0F000512))
				{
					Console.WriteLine($"\n[Debug] *** FOUND PROBLEMATIC EIP AT INSTRUCTION {i} ***");
					debugger.HandleProblematicEip();
					Console.WriteLine("[Debug] Stopping execution to prevent crash");
					break;
				}
				
				// *** CHECK FOR SUSPICIOUS REGISTERS ***
				if (cpu.HasSuspiciousRegisters() && i > 100) // Skip early initialization
				{
					Console.WriteLine($"[Debug] [Instruction {i}] Suspicious registers detected");
					cpu.LogRegisters("[Debug] ");
				}
				
				try
				{
					// *** ENHANCED SINGLE STEP ***
					var stateBefore = debugger.GetLastRegisterState();
					var wasCall = WillBeCall(cpu, vm); // Check if instruction is a call before execution
					var callTarget = wasCall ? GetCallTarget(cpu, vm) : 0u;
					
					debugger.SafeSingleStep();
					
					// Create step result for compatibility with existing import handling
					var step = new CpuStepResult(wasCall, callTarget);
					
					// *** EXISTING IMPORT HANDLING CODE ***
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
				catch (IndexOutOfRangeException ex) when (ex.Message.Contains("0xFFFFFFFD") || 
														  ex.Message.Contains("0xFFFFFFFF"))
				{
					// *** ENHANCED ERROR HANDLING ***
					Console.WriteLine($"\n[Debug] *** CAUGHT MEMORY ACCESS VIOLATION AT INSTRUCTION {i} ***");
					Console.WriteLine($"[Debug] Exception: {ex.Message}");
					
					// Check if we're trying to execute a synthetic import address
					if (currentEip >= 0x0F000000 && currentEip < 0x10000000)
					{
						Console.WriteLine($"[Debug] ERROR CAUSE: Trying to execute synthetic import address 0x{currentEip:X8}");
						if (image.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
						{
							Console.WriteLine($"[Debug] This is import: {importInfo.dll}!{importInfo.name}");
						}
						Console.WriteLine($"[Debug] SOLUTION: The program should CALL THROUGH the IAT, not execute the import address directly");
					}
					
					// Get execution trace to see what led to this
					var trace = debugger.GetExecutionTrace();
					var suspiciousStates = debugger.FindSuspiciousStates();
					
					Console.WriteLine($"[Debug] Execution trace has {trace.Count} entries");
					Console.WriteLine($"[Debug] Found {suspiciousStates.Count} suspicious register states");
					
					if (suspiciousStates.Count > 0)
					{
						Console.WriteLine("[Debug] First suspicious state occurred at:");
						var first = suspiciousStates.First();
						Console.WriteLine($"[Debug]   EIP=0x{first.EIP:X8} EBP=0x{first.EBP:X8} ESP=0x{first.ESP:X8}");
					}
					
					// Show last few instructions
					if (trace.Count > 5)
					{
						Console.WriteLine("[Debug] Last 5 instructions:");
						foreach (var state in trace.TakeLast(5))
						{
							Console.WriteLine($"[Debug]   0x{state.EIP:X8}: {state.InstructionBytes} (EBP=0x{state.EBP:X8})");
						}
					}
					
					throw; // Re-throw to maintain original behavior
				}
				catch (Exception ex)
				{
					// *** CATCH OTHER EXCEPTIONS ***
					Console.WriteLine($"[Debug] Unexpected exception at instruction {i}: {ex}");
					cpu.LogRegisters("[Debug] Register state: ");
					throw;
				}
			}
			
			// *** FINAL DEBUG SUMMARY ***
			var finalTrace = debugger.GetExecutionTrace();
			var finalSuspicious = debugger.FindSuspiciousStates();
			Console.WriteLine($"[Debug] Final execution summary:");
			Console.WriteLine($"[Debug]   Total traced instructions: {finalTrace.Count}");
			Console.WriteLine($"[Debug]   Suspicious register states: {finalSuspicious.Count}");
		}

		/// <summary>
		/// Check if the current instruction will be a CALL instruction
		/// </summary>
		private static bool WillBeCall(IcedCpu cpu, VirtualMemory vm)
		{
			try
			{
				var eip = cpu.GetEip();
				var opcode = vm.Read8(eip);
				
				// Common CALL opcodes:
				// 0xE8 = CALL rel32
				// 0xFF = CALL r/m32 (with ModR/M byte)
				// 0xCC = INT3 (used for synthetic import stubs)
				if (opcode == 0xE8 || (opcode == 0xFF && IsCallVariant(vm, eip)))
				{
					return true;
				}
				
				// Check for INT3 in synthetic import address range
				if (opcode == 0xCC && eip >= 0x0F000000 && eip < 0x10000000)
				{
					return true;
				}
				
				return false;
			}
			catch
			{
				return false; // If we can't read the instruction, assume it's not a call
			}
		}

		/// <summary>
		/// Check if FF opcode is a CALL variant by examining ModR/M byte
		/// </summary>
		private static bool IsCallVariant(VirtualMemory vm, uint eip)
		{
			try
			{
				var modRm = vm.Read8(eip + 1);
				var reg = (modRm >> 3) & 0x07; // Extract reg field
				return reg == 2; // CALL is when reg field = 010
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Get the target address of a CALL instruction
		/// </summary>
		private static uint GetCallTarget(IcedCpu cpu, VirtualMemory vm)
		{
			try
			{
				var eip = cpu.GetEip();
				var opcode = vm.Read8(eip);
				
				if (opcode == 0xE8) // CALL rel32
				{
					var displacement = vm.Read32(eip + 1);
					return (uint)(eip + 5 + (int)displacement); // EIP + instruction length + displacement
				}
				else if (opcode == 0xFF) // CALL r/m32
				{
					// This is more complex as it depends on ModR/M and potentially SIB bytes
					// For debugging purposes, we'll return 0 and let the normal execution handle it
					return 0;
				}
				else if (opcode == 0xCC && eip >= 0x0F000000 && eip < 0x10000000) // INT3 in synthetic import range
				{
					// For synthetic import stubs, the target is the address itself
					return eip;
				}
			}
			catch
			{
				// If we can't decode, return 0
			}
			
			return 0;
		}
	}
}