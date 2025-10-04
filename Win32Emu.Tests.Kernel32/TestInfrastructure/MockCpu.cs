using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Kernel32.TestInfrastructure
{
	/// <summary>
	/// Mock implementation of ICpu for testing Win32 API calls
	/// </summary>
	public class MockCpu : ICpu
	{
		private readonly Dictionary<string, uint> _registers = new(StringComparer.OrdinalIgnoreCase);
		private readonly Stack<uint> _stack = new();
		private uint _esp = 0x00200000; // Default stack pointer
		private uint _eip = 0x00400000; // Default instruction pointer

		public MockCpu()
		{
			// Initialize common registers
			_registers["EAX"] = 0;
			_registers["EBX"] = 0;
			_registers["ECX"] = 0;
			_registers["EDX"] = 0;
			_registers["ESP"] = _esp;
			_registers["EBP"] = 0;
			_registers["ESI"] = 0;
			_registers["EDI"] = 0;
			_registers["EIP"] = _eip;
		}

		public uint GetRegister(string name)
		{
			return _registers.TryGetValue(name, out var value) ? value : 0;
		}

		public void SetRegister(string name, uint value)
		{
			_registers[name] = value;
			if (name.Equals("ESP", StringComparison.OrdinalIgnoreCase))
			{
				_esp = value;
			}
			else if (name.Equals("EIP", StringComparison.OrdinalIgnoreCase))
			{
				_eip = value;
			}
		}

		public uint GetEip() => _eip;
		public void SetEip(uint eip)
		{
			_eip = eip;
			_registers["EIP"] = eip;
		}

		// These methods are not needed for basic API testing but must be implemented
		public CpuStepResult SingleStep(VirtualMemory memory) => throw new NotImplementedException("Mock CPU doesn't support single stepping");

		/// <summary>
		/// Push a value onto the stack (decrements ESP and writes value)
		/// </summary>
		public void PushStack(VirtualMemory memory, uint value)
		{
			_esp -= 4;
			memory.Write32(_esp, value);
			SetRegister("ESP", _esp);
			_stack.Push(value);
		}

		/// <summary>
		/// Pop a value from the stack (reads value and increments ESP)
		/// </summary>
		public uint PopStack(VirtualMemory memory)
		{
			var value = memory.Read32(_esp);
			_esp += 4;
			SetRegister("ESP", _esp);
			if (_stack.Count > 0)
			{
				_stack.Pop();
			}

			return value;
		}

		/// <summary>
		/// Set up stack arguments for API call testing
		/// </summary>
		public void SetupStackArgs(VirtualMemory memory, params uint[] args)
		{
			// Push arguments in reverse order so the first argument ends up at ESP+4
			for (var i = args.Length - 1; i >= 0; i--)
			{
				PushStack(memory, args[i]);
			}
        
			// Finally, push a fake return address (as would happen in a real function call)
			PushStack(memory, 0x12345678);
		}
	}
}