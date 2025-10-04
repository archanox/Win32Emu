using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator.TestInfrastructure
{
	/// <summary>
	/// Helper class for testing CPU instruction execution
	/// </summary>
	public class CpuTestHelper : IDisposable
	{
		public IcedCpu Cpu { get; }
		public VirtualMemory Memory { get; }
    
		private const uint CodeBaseAddress = 0x00400000;
		private const uint StackBaseAddress = 0x00100000;
		private const uint DataBaseAddress = 0x00200000;

		public CpuTestHelper()
		{
			Memory = new VirtualMemory();
			Cpu = new IcedCpu(Memory);
        
			// Initialize stack pointer
			Cpu.SetRegister("ESP", StackBaseAddress + 0x8000);
			Cpu.SetRegister("EBP", StackBaseAddress + 0x8000);
        
			// Initialize instruction pointer
			Cpu.SetEip(CodeBaseAddress);
		}

		/// <summary>
		/// Write machine code bytes at the current EIP
		/// </summary>
		public void WriteCode(params byte[] code)
		{
			var eip = Cpu.GetEip();
			for (var i = 0; i < code.Length; i++)
			{
				Memory.Write8(eip + (uint)i, code[i]);
			}
		}

		/// <summary>
		/// Execute a single instruction
		/// </summary>
		public void ExecuteInstruction()
		{
			Cpu.SingleStep(Memory);
		}

		/// <summary>
		/// Execute multiple instructions
		/// </summary>
		public void ExecuteInstructions(int count)
		{
			for (var i = 0; i < count; i++)
			{
				Cpu.SingleStep(Memory);
			}
		}

		/// <summary>
		/// Set a register value
		/// </summary>
		public void SetReg(string name, uint value)
		{
			Cpu.SetRegister(name, value);
		}

		/// <summary>
		/// Get a register value
		/// </summary>
		public uint GetReg(string name)
		{
			return Cpu.GetRegister(name);
		}

		/// <summary>
		/// Set the EFLAGS register
		/// </summary>
		public void SetFlags(uint value)
		{
			Cpu.SetRegister("EFLAGS", value);
		}

		/// <summary>
		/// Get the EFLAGS register
		/// </summary>
		public uint GetFlags()
		{
			return Cpu.GetRegister("EFLAGS");
		}

		/// <summary>
		/// Check if a specific flag is set
		/// </summary>
		public bool IsFlagSet(CpuFlag flag)
		{
			return (GetFlags() & (1u << (int)flag)) != 0;
		}

		/// <summary>
		/// Write a 32-bit value to memory
		/// </summary>
		public void WriteMemory32(uint address, uint value)
		{
			Memory.Write32(address, value);
		}

		/// <summary>
		/// Read a 32-bit value from memory
		/// </summary>
		public uint ReadMemory32(uint address)
		{
			return Memory.Read32(address);
		}

		/// <summary>
		/// Write a 64-bit value to memory
		/// </summary>
		public void WriteMemory64(uint address, ulong value)
		{
			Memory.Write64(address, value);
		}

		/// <summary>
		/// Read a 64-bit value from memory
		/// </summary>
		public ulong ReadMemory64(uint address)
		{
			return Memory.Read64(address);
		}

		public void Dispose()
		{
			if (Cpu is IDisposable cpuDisposable)
			{
				cpuDisposable.Dispose();
			}
			if (Memory is IDisposable memoryDisposable)
			{
				memoryDisposable.Dispose();
			}
		}
	}

	/// <summary>
	/// CPU flags enumeration
	/// </summary>
	public enum CpuFlag
	{
		Cf = 0,  // Carry Flag
		Pf = 2,  // Parity Flag
		Af = 4,  // Auxiliary Carry Flag
		Zf = 6,  // Zero Flag
		Sf = 7,  // Sign Flag
		Tf = 8,  // Trap Flag
		If = 9,  // Interrupt Enable Flag
		Df = 10, // Direction Flag
		Of = 11  // Overflow Flag
	}
}
