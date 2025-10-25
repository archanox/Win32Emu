using UnicornEngine;
using UnicornEngine.Const;
using Win32Emu.Cpu.Iced;
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator.TestInfrastructure;

/// <summary>
/// Helper class for three-way testing: comparing Unicorn, IcedCpu, and JitCpu implementations
/// Ensures all three emulators behave identically for the Pentium instruction set
/// </summary>
public class ThreeWayTestHelper : IDisposable
{
	private readonly Unicorn _unicorn;
	private readonly IcedCpu _icedCpu;
	private readonly JitCpu _jitCpu;
	private readonly VirtualMemory _icedMemory;
	private readonly VirtualMemory _jitMemory;
	
	private const long CodeBaseAddress = 0x00400000;
	private const long StackBaseAddress = 0x00100000;
	private const long DataBaseAddress = 0x00200000;
	private const long MemorySize = 0x100000; // 1MB
	
	// Public accessors for debugging
	public IcedCpu GetIcedCpu() => _icedCpu;
	public JitCpu GetJitCpu() => _jitCpu;
	public Unicorn GetUnicorn() => _unicorn;

	public ThreeWayTestHelper()
	{
		// Initialize Unicorn emulator for x86 32-bit
		_unicorn = new Unicorn(Common.UC_ARCH_X86, Common.UC_MODE_32);
		
		// Map memory regions in Unicorn
		_unicorn.MemMap(CodeBaseAddress, MemorySize, Common.UC_PROT_ALL);
		_unicorn.MemMap(StackBaseAddress, MemorySize, Common.UC_PROT_ALL);
		_unicorn.MemMap(DataBaseAddress, MemorySize, Common.UC_PROT_ALL);
		
		// Initialize IcedCpu
		_icedMemory = new VirtualMemory();
		_icedCpu = new IcedCpu(_icedMemory);
		
		// Initialize JitCpu
		_jitMemory = new VirtualMemory();
		_jitCpu = new JitCpu(_jitMemory);
		
		// Initialize stack pointers for all three
		var initialEsp = (uint)(StackBaseAddress + 0x8000);
		_unicorn.RegWrite(X86.UC_X86_REG_ESP, (int)initialEsp);
		_icedCpu.SetRegister("ESP", initialEsp);
		_icedCpu.SetRegister("EBP", initialEsp);
		_jitCpu.SetRegister("ESP", initialEsp);
		_jitCpu.SetRegister("EBP", initialEsp);
		
		// Initialize instruction pointers for all three
		_unicorn.RegWrite(X86.UC_X86_REG_EIP, (int)CodeBaseAddress);
		_icedCpu.SetEip((uint)CodeBaseAddress);
		_jitCpu.SetEip((uint)CodeBaseAddress);
	}

	/// <summary>
	/// Write machine code bytes at the current EIP in all three emulators
	/// </summary>
	public void WriteCode(params byte[] code)
	{
		// Get current EIP for Unicorn
		var unicornEip = (uint)_unicorn.RegRead(X86.UC_X86_REG_EIP);
		
		// Write to Unicorn
		_unicorn.MemWrite(unicornEip, code);
		
		// Get current EIP for IcedCpu
		var icedEip = _icedCpu.GetEip();
		
		// Write to IcedCpu
		for (var i = 0; i < code.Length; i++)
		{
			_icedMemory.Write8(icedEip + (uint)i, code[i]);
		}
		
		// Get current EIP for JitCpu
		var jitEip = _jitCpu.GetEip();
		
		// Write to JitCpu
		for (var i = 0; i < code.Length; i++)
		{
			_jitMemory.Write8(jitEip + (uint)i, code[i]);
		}
	}

	/// <summary>
	/// Set a register value in all three emulators
	/// </summary>
	public void SetReg(string name, uint value)
	{
		// Set in IcedCpu
		_icedCpu.SetRegister(name, value);
		
		// Set in JitCpu
		_jitCpu.SetRegister(name, value);
		
		// Set in Unicorn
		var regId = name.ToUpperInvariant() switch
		{
			"EAX" => X86.UC_X86_REG_EAX,
			"EBX" => X86.UC_X86_REG_EBX,
			"ECX" => X86.UC_X86_REG_ECX,
			"EDX" => X86.UC_X86_REG_EDX,
			"ESI" => X86.UC_X86_REG_ESI,
			"EDI" => X86.UC_X86_REG_EDI,
			"EBP" => X86.UC_X86_REG_EBP,
			"ESP" => X86.UC_X86_REG_ESP,
			"EIP" => X86.UC_X86_REG_EIP,
			"EFLAGS" => X86.UC_X86_REG_EFLAGS,
			_ => throw new ArgumentException($"Unknown register: {name}")
		};
		
		_unicorn.RegWrite(regId, (int)value);
	}

	/// <summary>
	/// Write data to memory in all three emulators
	/// </summary>
	public void WriteMemory(uint address, params byte[] data)
	{
		_unicorn.MemWrite(address, data);
		foreach (var (b, i) in data.Select((b, i) => (b, i)))
		{
			_icedMemory.Write8(address + (uint)i, b);
			_jitMemory.Write8(address + (uint)i, b);
		}
	}

	/// <summary>
	/// Execute one instruction in all three emulators
	/// </summary>
	public void ExecuteInstruction()
	{
		// Get current EIP from each emulator
		var unicornEip = (long)_unicorn.RegRead(X86.UC_X86_REG_EIP);
		var icedEip = _icedCpu.GetEip();
		var jitEip = _jitCpu.GetEip();
		
		// Read instruction length from Unicorn by trying to execute
		var codeSize = 15; // Max x86 instruction length
		var code = new byte[codeSize];
		_unicorn.MemRead(unicornEip, code);
		
		// Execute in Unicorn (just one instruction)
		_unicorn.EmuStart(unicornEip, unicornEip + codeSize, 0, 1);
		
		// Execute in IcedCpu
		_icedCpu.SingleStep(_icedMemory);
		
		// Execute in JitCpu
		_jitCpu.SingleStep(_jitMemory);
	}

	/// <summary>
	/// Assert that a register matches across all three emulators
	/// </summary>
	public void AssertRegistersMatch(params string[] registerNames)
	{
		foreach (var name in registerNames)
		{
			var unicornValue = GetUnicornReg(name);
			var icedValue = _icedCpu.GetRegister(name);
			var jitValue = _jitCpu.GetRegister(name);
			
			// Debug output to help identify which emulator is failing
			if (unicornValue != icedValue || unicornValue != jitValue || icedValue != jitValue)
			{
				Console.WriteLine($"Register {name} mismatch:");
				Console.WriteLine($"  Unicorn: 0x{unicornValue:X8}");
				Console.WriteLine($"  IcedCpu: 0x{icedValue:X8}");
				Console.WriteLine($"  JitCpu:  0x{jitValue:X8}");
				
				// Also print EFLAGS for debugging
				if (name == "EAX" || name == "EBX" || name == "ECX" || name == "EDX")
				{
					var unicornEflags = (uint)_unicorn.RegRead(X86.UC_X86_REG_EFLAGS);
					var icedEflags = _icedCpu.GetRegister("EFLAGS");
					var jitEflags = _jitCpu.GetRegister("EFLAGS");
					Console.WriteLine($"EFLAGS:");
					Console.WriteLine($"  Unicorn: 0x{unicornEflags:X8}");
					Console.WriteLine($"  IcedCpu: 0x{icedEflags:X8}");
					Console.WriteLine($"  JitCpu:  0x{jitEflags:X8}");
				}
			}
			
			Assert.Equal(unicornValue, icedValue);
			Assert.Equal(unicornValue, jitValue);
			Assert.Equal(icedValue, jitValue);
		}
	}

	/// <summary>
	/// Assert that specific flags match across all three emulators
	/// </summary>
	public void AssertFlagsMatch(params CpuFlag[] flags)
	{
		var unicornEflags = (uint)_unicorn.RegRead(X86.UC_X86_REG_EFLAGS);
		var icedEflags = _icedCpu.GetRegister("EFLAGS");
		var jitEflags = _jitCpu.GetRegister("EFLAGS");
		
		foreach (var flag in flags)
		{
			var mask = 1u << (int)flag;
			var unicornFlag = (unicornEflags & mask) != 0;
			var icedFlag = (icedEflags & mask) != 0;
			var jitFlag = (jitEflags & mask) != 0;
			
			Assert.Equal(unicornFlag, icedFlag);
			Assert.Equal(unicornFlag, jitFlag);
			Assert.Equal(icedFlag, jitFlag);
		}
	}

	/// <summary>
	/// Assert that memory matches across all three emulators
	/// </summary>
	public void AssertMemoryMatch(uint address, int length)
	{
		var unicornMem = new byte[length];
		_unicorn.MemRead((long)address, unicornMem);
		
		for (int i = 0; i < length; i++)
		{
			var icedByte = _icedMemory.Read8(address + (uint)i);
			var jitByte = _jitMemory.Read8(address + (uint)i);
			
			Assert.Equal(unicornMem[i], icedByte);
			Assert.Equal(unicornMem[i], jitByte);
			Assert.Equal(icedByte, jitByte);
		}
	}

	public uint GetUnicornReg(string name)
	{
		var regId = name.ToUpperInvariant() switch
		{
			"EAX" => X86.UC_X86_REG_EAX,
			"EBX" => X86.UC_X86_REG_EBX,
			"ECX" => X86.UC_X86_REG_ECX,
			"EDX" => X86.UC_X86_REG_EDX,
			"ESI" => X86.UC_X86_REG_ESI,
			"EDI" => X86.UC_X86_REG_EDI,
			"EBP" => X86.UC_X86_REG_EBP,
			"ESP" => X86.UC_X86_REG_ESP,
			"EIP" => X86.UC_X86_REG_EIP,
			"EFLAGS" => X86.UC_X86_REG_EFLAGS,
			_ => throw new ArgumentException($"Unknown register: {name}")
		};
		
		return (uint)_unicorn.RegRead(regId);
	}

	public void Dispose()
	{
		_unicorn?.Dispose();
	}
}
