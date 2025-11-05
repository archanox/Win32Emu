using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify that DirectDraw and DirectInput modules properly write COM interface pointers
/// to the correct address and verify the write succeeded.
/// This addresses the bug identified in FURTHER_INVESTIGATION.md:
/// "DirectDraw/DirectInput API implementation bug - Not writing COM interface pointer to correct 
/// address or writing wrong value due to parameter handling issue"
/// </summary>
public class ComPointerWriteVerificationTests
{
	private readonly ITestOutputHelper _output;

	public ComPointerWriteVerificationTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void DirectDrawCreate_ShouldWriteValidComPointerToOutputParameter()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddraw = new DDrawModule(env, 0x00400000, null, NullLogger.Instance);

		// Allocate output pointer in valid data section range (not stack)
		var outputPtrAddr = 0x00500000u;
		memory.Write32(outputPtrAddr, 0xDEADBEEF); // Initialize with garbage

		// Set up stack for stdcall convention
		cpu.SetRegister("ESP", 0x001FF000);
		var esp = cpu.GetRegister("ESP");
		
		// Push arguments: lpGuid, lplpDD, pUnkOuter
		memory.Write32(esp + 4, 0u); // lpGuid = NULL
		memory.Write32(esp + 8, outputPtrAddr); // lplpDD = address to write COM pointer to
		memory.Write32(esp + 12, 0u); // pUnkOuter = NULL

		// Act
		var result = ddraw.TryInvokeUnsafe("DirectDrawCreate", cpu, memory, out var returnValue);

		// Assert
		Assert.True(result, "DirectDrawCreate should be callable");
		Assert.Equal(0u, returnValue); // DD_OK

		var comPointer = memory.Read32(outputPtrAddr);
		_output.WriteLine($"COM pointer written: 0x{comPointer:X8}");
		
		// Verify a valid COM pointer was written (not the garbage value)
		Assert.NotEqual(0xDEADBEEFu, comPointer);
		Assert.NotEqual(0u, comPointer);
		
		// Verify the pointer is not a stack address (not in range 0x00100000-0x00400000)
		Assert.False(comPointer >= 0x00100000 && comPointer < 0x00400000, 
			$"COM pointer 0x{comPointer:X8} should not be a stack address!");
	}

	[Fact]
	public void DirectDrawCreate_ShouldRejectNullOutputPointer()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddraw = new DDrawModule(env, 0x00400000, null, NullLogger.Instance);

		// Set up stack with NULL output pointer
		cpu.SetRegister("ESP", 0x001FF000);
		var esp = cpu.GetRegister("ESP");
		
		memory.Write32(esp + 4, 0u); // lpGuid = NULL
		memory.Write32(esp + 8, 0u); // lplpDD = NULL (invalid!)
		memory.Write32(esp + 12, 0u); // pUnkOuter = NULL

		// Act
		var result = ddraw.TryInvokeUnsafe("DirectDrawCreate", cpu, memory, out var returnValue);

		// Assert
		Assert.True(result, "DirectDrawCreate should be callable");
		Assert.Equal(0x80070057u, returnValue); // DDERR_INVALIDPARAMS
	}

	[Fact]
	public void DirectInputCreateA_ShouldWriteValidComPointerToOutputParameter()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var dinput = new DInputModule(env, 0x00400000, null, NullLogger.Instance);

		// Allocate output pointer in valid data section range (not stack)
		var outputPtrAddr = 0x00500000u;
		memory.Write32(outputPtrAddr, 0xDEADBEEF); // Initialize with garbage

		// Set up stack for stdcall convention
		cpu.SetRegister("ESP", 0x001FF000);
		var esp = cpu.GetRegister("ESP");
		
		// Push arguments: hinst, dwVersion, lplpDirectInput, pUnkOuter
		memory.Write32(esp + 4, 0x00400000u); // hinst
		memory.Write32(esp + 8, 0x00000300u); // dwVersion = DIRECTINPUT_VERSION
		memory.Write32(esp + 12, outputPtrAddr); // lplpDirectInput = address to write COM pointer to
		memory.Write32(esp + 16, 0u); // pUnkOuter = NULL

		// Act
		var result = dinput.TryInvokeUnsafe("DirectInputCreateA", cpu, memory, out var returnValue);

		// Assert
		Assert.True(result, "DirectInputCreateA should be callable");
		Assert.Equal(0u, returnValue); // DI_OK

		var comPointer = memory.Read32(outputPtrAddr);
		_output.WriteLine($"COM pointer written: 0x{comPointer:X8}");
		
		// Verify a valid COM pointer was written (not the garbage value)
		Assert.NotEqual(0xDEADBEEFu, comPointer);
		Assert.NotEqual(0u, comPointer);
		
		// Verify the pointer is not a stack address
		Assert.False(comPointer >= 0x00100000 && comPointer < 0x00400000, 
			$"COM pointer 0x{comPointer:X8} should not be a stack address!");
	}

	[Fact]
	public void DirectInputCreateA_ShouldRejectNullOutputPointer()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var dinput = new DInputModule(env, 0x00400000, null, NullLogger.Instance);

		// Set up stack with NULL output pointer
		cpu.SetRegister("ESP", 0x001FF000);
		var esp = cpu.GetRegister("ESP");
		
		memory.Write32(esp + 4, 0x00400000u); // hinst
		memory.Write32(esp + 8, 0x00000300u); // dwVersion
		memory.Write32(esp + 12, 0u); // lplpDirectInput = NULL (invalid!)
		memory.Write32(esp + 16, 0u); // pUnkOuter = NULL

		// Act
		var result = dinput.TryInvokeUnsafe("DirectInputCreateA", cpu, memory, out var returnValue);

		// Assert
		Assert.True(result, "DirectInputCreateA should be callable");
		Assert.Equal(0x80004003u, returnValue); // DIERR_INVALIDPARAM
	}

	[Fact]
	public void DirectDrawCreateEx_ShouldWriteValidComPointerToOutputParameter()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var ddraw = new DDrawModule(env, 0x00400000, null, NullLogger.Instance);

		// Allocate output pointer in valid data section range
		var outputPtrAddr = 0x00500000u;
		memory.Write32(outputPtrAddr, 0xCAFEBABEu); // Initialize with garbage

		// Set up stack
		cpu.SetRegister("ESP", 0x001FF000);
		var esp = cpu.GetRegister("ESP");
		
		// Push arguments: lpGuid, lplpDD, iid, pUnkOuter
		memory.Write32(esp + 4, 0u); // lpGuid = NULL
		memory.Write32(esp + 8, outputPtrAddr); // lplpDD
		memory.Write32(esp + 12, 0u); // iid
		memory.Write32(esp + 16, 0u); // pUnkOuter = NULL

		// Act
		var result = ddraw.TryInvokeUnsafe("DirectDrawCreateEx", cpu, memory, out var returnValue);

		// Assert
		Assert.True(result, "DirectDrawCreateEx should be callable");
		Assert.Equal(0u, returnValue); // DD_OK

		var comPointer = memory.Read32(outputPtrAddr);
		_output.WriteLine($"COM pointer written: 0x{comPointer:X8}");
		
		// Verify a valid COM pointer was written
		Assert.NotEqual(0xCAFEBABEu, comPointer);
		Assert.NotEqual(0u, comPointer);
		Assert.False(comPointer >= 0x00100000 && comPointer < 0x00400000, 
			$"COM pointer 0x{comPointer:X8} should not be a stack address!");
	}

	[Fact]
	public void DirectInputCreate_ShouldWriteValidComPointerToOutputParameter()
	{
		// Arrange
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, NullLogger.Instance);
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		var dinput = new DInputModule(env, 0x00400000, null, NullLogger.Instance);

		// Allocate output pointer in valid data section range
		var outputPtrAddr = 0x00500000u;
		memory.Write32(outputPtrAddr, 0xDEADC0DEu); // Initialize with garbage

		// Set up stack
		cpu.SetRegister("ESP", 0x001FF000);
		var esp = cpu.GetRegister("ESP");
		
		// Push arguments: hinst, dwVersion, lplpDirectInput, pUnkOuter
		memory.Write32(esp + 4, 0x00400000u); // hinst
		memory.Write32(esp + 8, 0x00000300u); // dwVersion
		memory.Write32(esp + 12, outputPtrAddr); // lplpDirectInput
		memory.Write32(esp + 16, 0u); // pUnkOuter = NULL

		// Act
		var result = dinput.TryInvokeUnsafe("DirectInputCreate", cpu, memory, out var returnValue);

		// Assert
		Assert.True(result, "DirectInputCreate should be callable");
		Assert.Equal(0u, returnValue); // DI_OK

		var comPointer = memory.Read32(outputPtrAddr);
		_output.WriteLine($"COM pointer written: 0x{comPointer:X8}");
		
		// Verify a valid COM pointer was written
		Assert.NotEqual(0xDEADC0DEu, comPointer);
		Assert.NotEqual(0u, comPointer);
		Assert.False(comPointer >= 0x00100000 && comPointer < 0x00400000, 
			$"COM pointer 0x{comPointer:X8} should not be a stack address!");
	}
}
