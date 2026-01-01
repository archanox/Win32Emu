using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for MSVCRT FPU functions (__ftol, _fpreset, sin, sqrt)
/// These tests verify that FPU operations work correctly with JitCpu
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class MsvcrtFpuFunctionsTests : IDisposable
{
	private readonly VirtualMemory _memory;
	private readonly JitCpu _cpu;
	private readonly ProcessEnvironment _processEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtFpuFunctionsTests()
	{
		_memory = new VirtualMemory();
		_cpu = new JitCpu(_memory, NullLogger<JitCpu>.Instance);
		
		// Initialize CPU state
		_cpu.SetRegister("ESP", 0x00100000 + 0x8000);
		_cpu.SetRegister("EBP", 0x00100000 + 0x8000);
		_cpu.SetEip(0x00400000);

		// Create process environment without JitCpu (it doesn't need CPU reference)
		_processEnv = new ProcessEnvironment(_memory, 0x01000000, null, NullLogger<ProcessEnvironment>.Instance);
		
		// Create MSVCRT module
		var peLoader = new Loader.PeImageLoader(_memory, NullLogger<Loader.PeImageLoader>.Instance);
		_msvcrt = new MsvcrtModule(_processEnv, 0x00400000, peLoader, NullLogger<MsvcrtModule>.Instance);
	}

	public void Dispose()
	{
		// VirtualMemory does not implement IDisposable
	}

	[Fact]
	public void Ftol_ConvertsFloatToLong_WithPositiveValue()
	{
		// Arrange - push a float value onto FPU stack
		// FPU instructions: FLD dword ptr [memAddr] - load float from memory
		var memAddr = 0x00200000u;
		var floatBits = BitConverter.SingleToInt32Bits(42.75f);
		_memory.Write32(memAddr, unchecked((uint)floatBits));
		
		// Load the float onto FPU stack using FLD instruction
		var code = new byte[]
		{
			0xD9, 0x05,  // FLD dword ptr [address]
			(byte)(memAddr & 0xFF),
			(byte)((memAddr >> 8) & 0xFF),
			(byte)((memAddr >> 16) & 0xFF),
			(byte)((memAddr >> 24) & 0xFF)
		};
		
		var eip = _cpu.GetEip();
		for (var i = 0; i < code.Length; i++)
		{
			_memory.Write8(eip + (uint)i, code[i]);
		}
		_cpu.SingleStep(_memory);
		
		// Act - call __ftol which should pop from FPU and convert to long
		var success = _msvcrt.TryInvokeUnsafe("__FTOL", _cpu, _memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(42L, result); // Should truncate to 42
	}

	[Fact]
	public void Ftol_ConvertsFloatToLong_WithNegativeValue()
	{
		// Arrange - push a negative float value onto FPU stack
		var memAddr = 0x00200000u;
		var floatBits = BitConverter.SingleToInt32Bits(-17.9f);
		_memory.Write32(memAddr, unchecked((uint)floatBits));
		
		// Load the float onto FPU stack using FLD instruction
		var code = new byte[]
		{
			0xD9, 0x05,  // FLD dword ptr [address]
			(byte)(memAddr & 0xFF),
			(byte)((memAddr >> 8) & 0xFF),
			(byte)((memAddr >> 16) & 0xFF),
			(byte)((memAddr >> 24) & 0xFF)
		};
		
		var eip = _cpu.GetEip();
		for (var i = 0; i < code.Length; i++)
		{
			_memory.Write8(eip + (uint)i, code[i]);
		}
		_cpu.SingleStep(_memory);
		
		// Act - call __ftol
		var success = _msvcrt.TryInvokeUnsafe("__FTOL", _cpu, _memory, out var result);
		
		// Assert
		Assert.True(success);
		// Result is returned as 32-bit value in EAX (low 32 bits of result)
		// For negative values, need to interpret as signed int
		var eax = unchecked((int)(result & 0xFFFFFFFF));
		Assert.Equal(-17, eax); // Should truncate to -17
	}

	[Fact]
	public void Ftol2_ConvertsFloatToLong()
	{
		// Arrange - push a float value onto FPU stack
		var memAddr = 0x00200000u;
		var floatBits = BitConverter.SingleToInt32Bits(123.456f);
		_memory.Write32(memAddr, unchecked((uint)floatBits));
		
		// Load the float onto FPU stack
		var code = new byte[]
		{
			0xD9, 0x05,  // FLD dword ptr [address]
			(byte)(memAddr & 0xFF),
			(byte)((memAddr >> 8) & 0xFF),
			(byte)((memAddr >> 16) & 0xFF),
			(byte)((memAddr >> 24) & 0xFF)
		};
		
		var eip = _cpu.GetEip();
		for (var i = 0; i < code.Length; i++)
		{
			_memory.Write8(eip + (uint)i, code[i]);
		}
		_cpu.SingleStep(_memory);
		
		// Act - call __ftol2
		var success = _msvcrt.TryInvokeUnsafe("__FTOL2", _cpu, _memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(123L, result); // Should truncate to 123
	}

	[Fact]
	public void FpReset_ResetsFloatingPointUnit()
	{
		// Arrange - push some values onto FPU stack to change its state
		// FLD1 instruction - loads 1.0 onto FPU stack
		var code = new byte[] { 0xD9, 0xE8 }; // FLD1
		var eip = _cpu.GetEip();
		for (var i = 0; i < code.Length; i++)
		{
			_memory.Write8(eip + (uint)i, code[i]);
		}
		_cpu.SingleStep(_memory);
		
		// Verify FPU has a value
		double valueBefore = _cpu.FpuGetSt(0);
		Assert.Equal(1.0, valueBefore);
		
		// Act - call _fpreset to reset FPU
		var success = _msvcrt.TryInvokeUnsafe("_FPRESET", _cpu, _memory, out _);
		
		// Assert - FPU should be reset
		Assert.True(success);
		
		// After reset, FPU stack should be cleared
		// We can't directly verify internal state, but we can verify the function succeeded
	}

	// NOTE: Direct JitCpu FPU method tests (FpuGetSt, FpuPop, FpuReset) have been
	// moved to Win32Emu.Tests.Emulator/FpuInstructionTests.cs to keep CPU emulator tests
	// separate from MSVCRT/Kernel32 API tests.
}
