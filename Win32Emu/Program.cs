using Win32Emu.Cpu.IcedImpl;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Debugging;
using Win32Emu.Logging;
using System.Linq;

namespace Win32Emu
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: Win32Emu <path-to-pe> [--debug] [--otel-endpoint <endpoint>]");
				Console.WriteLine("  --debug: Enable enhanced debugging to catch 0xFFFFFFFD memory access errors");
				Console.WriteLine("  --otel-endpoint <endpoint>: Enable HTTP OpenTelemetry export to specified endpoint");
				return;
			}

			// Parse command line arguments
			var debugMode = args.Contains("--debug");
			var path = args[0];
			
			// Parse OpenTelemetry endpoint
			string? otlpEndpoint = null;
			var otlpIndex = Array.IndexOf(args, "--otel-endpoint");
			if (otlpIndex >= 0 && otlpIndex + 1 < args.Length)
			{
				otlpEndpoint = args[otlpIndex + 1];
			}
			
			// Initialize logging system
			Logging.LoggerFactory.Initialize(enableHttpExport: !string.IsNullOrEmpty(otlpEndpoint), otlpEndpoint);
			var logger = Logging.LoggerFactory.CreateScopedLogger("Win32Emu");
			
			try
			{
				if (!File.Exists(path))
				{
					logger.LogError("File not found: {FilePath}", path);
					return;
				}

				using var loaderScope = logger.BeginScope("PELoader");
				logger.LogInformation("Loading PE: {FilePath}", path);
				var vm = new VirtualMemory();
				var loader = new PeImageLoader(vm);
				var image = loader.Load(path);
				logger.LogInformation("Image loaded - Base=0x{BaseAddress:X8} EntryPoint=0x{EntryPointAddress:X8} Size=0x{ImageSize:X}", 
					image.BaseAddress, image.EntryPointAddress, image.ImageSize);
				logger.LogInformation("Imports mapped: {ImportCount}", image.ImportAddressMap.Count);

				var env = new ProcessEnvironment(vm);
				env.InitializeStrings(path, args.Where(a => a != "--debug" && a != "--otel-endpoint" && a != otlpEndpoint).ToArray());

				var cpu = new IcedCpu(vm);
				cpu.SetEip(image.EntryPointAddress);
				cpu.SetRegister("ESP", 0x00200000); // crude stack top

				var dispatcher = new Win32Dispatcher();
				var kernel32Logger = Logging.LoggerFactory.CreateScopedLogger("Kernel32Module");
				dispatcher.RegisterModule(new Kernel32Module(env, image.BaseAddress, kernel32Logger));

				if (debugMode)
				{
					RunWithEnhancedDebugging(cpu, vm, env, dispatcher, image, logger);
				}
				else
				{
					RunNormal(cpu, vm, env, dispatcher, image, logger);
				}

				logger.LogInformation(env.ExitRequested ? "Process requested exit." : "Instruction limit reached.");
			}
			finally
			{
				Logging.LoggerFactory.Dispose();
			}
		}

		private static void RunNormal(IcedCpu cpu, VirtualMemory vm, ProcessEnvironment env, Win32Dispatcher dispatcher, LoadedImage image, IScopedLogger logger)
		{
			using var executionScope = logger.BeginScope("Execution");
			const int maxInstr = 500000;
			for (var i = 0; i < maxInstr && !env.ExitRequested; i++)
			{
				var step = cpu.SingleStep(vm);
				if (step.IsCall && image.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
				{
					var dll = imp.dll.ToUpperInvariant();
					var name = imp.name;
					using var importScope = logger.BeginScope($"Import.{dll}.{name}");
					logger.LogInformation("Calling import: {Dll}!{Function}", dll, name);
					if (dispatcher.TryInvoke(dll, name, cpu, vm, out var ret, out var argBytes))
					{
						logger.LogDebug("Import returned: 0x{ReturnValue:X8}", ret);
						var esp = cpu.GetRegister("ESP");
						var retEip = vm.Read32(esp);
						esp += 4 + (uint)argBytes;
						cpu.SetRegister("ESP", esp);
						cpu.SetEip(retEip);
					}
				}
			}
		}

		private static void RunWithEnhancedDebugging(IcedCpu cpu, VirtualMemory vm, ProcessEnvironment env, Win32Dispatcher dispatcher, LoadedImage image, IScopedLogger logger)
		{
			using var debugScope = logger.BeginScope("EnhancedDebugging");
			// *** ENHANCED DEBUGGING SETUP ***
			var debugger = cpu.CreateDebugger(vm);
			debugger.EnableSuspiciousRegisterDetection = true;
			debugger.LogToConsole = true;
			debugger.LogAllInstructions = false; // Set to true for full instruction trace
			debugger.SuspiciousThreshold = 0x1000; // Adjust if needed

			logger.LogInformation("Enhanced debugging enabled - will catch 0xFFFFFFFD errors");
			logger.LogInformation("Monitoring for suspicious register values");
			logger.LogDebug("Use --debug argument to enable this mode");
			
			const int maxInstr = 500000;
			for (var i = 0; i < maxInstr && !env.ExitRequested; i++)
			{
				var currentEip = cpu.GetEip();
				
				// *** CHECK FOR SYNTHETIC IMPORT ADDRESS EXECUTION ***
				if (currentEip >= 0x0F000000 && currentEip < 0x10000000)
				{
					using var synthScope = logger.BeginScope("SyntheticImportAddress");
					logger.LogWarning("CPU trying to execute synthetic import address!");
					logger.LogDebug("EIP=0x{CurrentEip:X8} at instruction {InstructionIndex}", currentEip, i);
					
					if (image.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
					{
						logger.LogDebug("This is import: {Dll}!{Function}", importInfo.dll, importInfo.name);
					}
					else
					{
						logger.LogWarning("Unknown synthetic address - not in import map");
					}
					
					logger.LogDebug("This should now execute an INT3 stub that will be handled as an import call");
					cpu.LogRegisters("[Debug] ");
					
					// Show the actual instruction bytes at this address
					try 
					{
						var instrBytes = vm.GetSpan(currentEip, 4);
						logger.LogDebug("Instruction bytes at synthetic address: {InstructionBytes}", Convert.ToHexString(instrBytes));
						
						if (instrBytes[0] == 0xCC)
						{
							logger.LogDebug("Good! Found INT3 (0xCC) stub - continuing execution");
							// Don't break, let the INT3 execute and be handled
						}
						else
						{
							logger.LogError("ERROR: Expected INT3 (0xCC) but found different instruction");
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Could not read instruction bytes");
					}
					
					// Continue execution instead of breaking - let's see if the INT3 handling works
				}
				
				// *** CHECK FOR PROBLEMATIC EIP (keep existing check for other cases) ***
				if (debugger.IsProblematicEip(0x0F000512))
				{
					using var problematicScope = logger.BeginScope("ProblematicEIP");
					logger.LogError("Found problematic EIP at instruction {InstructionIndex}", i);
					debugger.HandleProblematicEip();
					logger.LogWarning("Stopping execution to prevent crash");
					break;
				}
				
				// *** CHECK FOR SUSPICIOUS REGISTERS ***
				if (cpu.HasSuspiciousRegisters() && i > 100) // Skip early initialization
				{
					using var suspiciousScope = logger.BeginScope("SuspiciousRegisters");
					logger.LogWarning("Suspicious registers detected at instruction {InstructionIndex}", i);
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
						using var importScope = logger.BeginScope($"Import.{dll}.{name}");
						logger.LogInformation("Calling debug import: {Dll}!{Function}", dll, name);
						if (dispatcher.TryInvoke(dll, name, cpu, vm, out var ret, out var argBytes))
						{
							logger.LogDebug("Debug import returned: 0x{ReturnValue:X8}", ret);
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
					using var errorScope = logger.BeginScope("MemoryAccessViolation");
					logger.LogError(ex, "Caught memory access violation at instruction {InstructionIndex}", i);
					
					// Check if we're trying to execute a synthetic import address
					if (currentEip >= 0x0F000000 && currentEip < 0x10000000)
					{
						logger.LogError("ERROR CAUSE: Trying to execute synthetic import address 0x{CurrentEip:X8}", currentEip);
						if (image.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
						{
							logger.LogDebug("This is import: {Dll}!{Function}", importInfo.dll, importInfo.name);
						}
						logger.LogError("SOLUTION: The program should CALL THROUGH the IAT, not execute the import address directly");
					}
					
					// Get execution trace to see what led to this
					var trace = debugger.GetExecutionTrace();
					var suspiciousStates = debugger.FindSuspiciousStates();
					
					logger.LogDebug("Execution trace has {TraceCount} entries", trace.Count);
					logger.LogDebug("Found {SuspiciousCount} suspicious register states", suspiciousStates.Count);
					
					if (suspiciousStates.Count > 0)
					{
						logger.LogDebug("First suspicious state occurred at:");
						var first = suspiciousStates.First();
						logger.LogDebug("  EIP=0x{EIP:X8} EBP=0x{EBP:X8} ESP=0x{ESP:X8}", first.EIP, first.EBP, first.ESP);
					}
					
					// Show last few instructions
					if (trace.Count > 5)
					{
						logger.LogDebug("Last 5 instructions:");
						foreach (var state in trace.TakeLast(5))
						{
							logger.LogDebug("  0x{EIP:X8}: {InstructionBytes} (EBP=0x{EBP:X8})", state.EIP, state.InstructionBytes, state.EBP);
						}
					}
					
					throw; // Re-throw to maintain original behavior
				}
				catch (Exception ex)
				{
					// *** CATCH OTHER EXCEPTIONS ***
					using var unexpectedScope = logger.BeginScope("UnexpectedException");
					logger.LogError(ex, "Unexpected exception at instruction {InstructionIndex}", i);
					cpu.LogRegisters("[Debug] Register state: ");
					throw;
				}
			}
			
			// *** FINAL DEBUG SUMMARY ***
			using var summaryScope = logger.BeginScope("DebugSummary");
			var finalTrace = debugger.GetExecutionTrace();
			var finalSuspicious = debugger.FindSuspiciousStates();
			logger.LogInformation("Final execution summary:");
			logger.LogInformation("  Total traced instructions: {TracedInstructions}", finalTrace.Count);
			logger.LogInformation("  Suspicious register states: {SuspiciousStates}", finalSuspicious.Count);
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
				return opcode == 0xE8 || (opcode == 0xFF && IsCallVariant(vm, eip));
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
			}
			catch
			{
				// If we can't decode, return 0
			}
			
			return 0;
		}
	}
}