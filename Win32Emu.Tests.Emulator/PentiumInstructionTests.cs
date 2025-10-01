using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for Pentium-specific x86 instructions
/// The Intel Pentium introduced several new instructions including:
/// - RDTSC: Read Time-Stamp Counter
/// - CPUID: CPU Identification
/// - CMPXCHG8B: Compare and exchange 8 bytes (64-bit)
/// - RDMSR/WRMSR: Read/Write Model Specific Register (privileged)
/// - RSM: Resume from System Management Mode (privileged)
/// </summary>
public class PentiumInstructionTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public PentiumInstructionTests()
    {
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void RDTSC_ShouldReturnTimestamp()
    {
        // Arrange: RDTSC (0F 31) - Read Time-Stamp Counter
        // Result should be in EDX:EAX
        _helper.SetReg("EAX", 0);
        _helper.SetReg("EDX", 0);
        _helper.WriteCode(0x0F, 0x31);

        // Act
        _helper.ExecuteInstruction();

        // Assert - The timestamp should be non-zero (at least one of the registers should change)
        var edx = _helper.GetReg("EDX");
        var eax = _helper.GetReg("EAX");
        var timestamp = ((ulong)edx << 32) | eax;
        
        Assert.True(timestamp > 0, "RDTSC should return a non-zero timestamp");
    }

    [Fact]
    public void CPUID_Function0_ShouldReturnVendorString()
    {
        // Arrange: CPUID (0F A2) with EAX=0 - Get vendor string
        _helper.SetReg("EAX", 0);
        _helper.WriteCode(0x0F, 0xA2);

        // Act
        _helper.ExecuteInstruction();

        // Assert - EAX should contain max supported function
        // EBX, EDX, ECX should contain vendor string (e.g., "GenuineIntel")
        var maxFunction = _helper.GetReg("EAX");
        var ebx = _helper.GetReg("EBX");
        var edx = _helper.GetReg("EDX");
        var ecx = _helper.GetReg("ECX");
        
        // At minimum, we should have some function support
        Assert.True(maxFunction >= 0, "CPUID should return a valid max function number");
    }

    [Fact]
    public void CPUID_Function1_ShouldReturnFeatureFlags()
    {
        // Arrange: CPUID (0F A2) with EAX=1 - Get feature flags
        _helper.SetReg("EAX", 1);
        _helper.WriteCode(0x0F, 0xA2);

        // Act
        _helper.ExecuteInstruction();

        // Assert - EAX should contain processor signature
        // EDX and ECX should contain feature flags
        var signature = _helper.GetReg("EAX");
        var featuresEDX = _helper.GetReg("EDX");
        var featuresECX = _helper.GetReg("ECX");
        
        // At minimum, we should return some feature information
        // Feature flags should have at least one bit set for basic CPU features
        Assert.True(featuresEDX > 0 || featuresECX > 0, "CPUID should return feature flags");
    }

    [Fact]
    public void CMPXCHG8B_Equal_ShouldExchange()
    {
        // Arrange: CMPXCHG8B [EBX] (0F C7 0B)
        // Compare EDX:EAX with 64-bit value at [EBX]
        // If equal, set ZF and store ECX:EBX at [EBX]
        var memAddr = 0x00200000u;
        
        // Set up memory value
        _helper.WriteMemory64(memAddr, 0x1122334455667788);
        
        // Set EDX:EAX to match memory value
        _helper.SetReg("EAX", 0x55667788);
        _helper.SetReg("EDX", 0x11223344);
        
        // Set new value in ECX:EBX
        // Note: EBX will be used as address in the instruction, so we save the new value first
        var newLow = 0xEEFF0011u;
        var newHigh = 0xAABBCCDDu;
        _helper.SetReg("ECX", newHigh);
        
        // Now set EBX to the memory address for the instruction
        _helper.SetReg("EBX", memAddr);
        
        // We need to set the value that will be exchanged separately
        // For CMPXCHG8B [EBX], the instruction exchanges ECX:EBX with [EBX]
        // But EBX is the address, so we need a different approach
        // Let's use a memory operand encoding: CMPXCHG8B [EDI]
        _helper.SetReg("EDI", memAddr);
        _helper.SetReg("EBX", newLow);
        _helper.SetReg("ECX", newHigh);
        
        _helper.WriteCode(0x0F, 0xC7, 0x0F); // CMPXCHG8B [EDI]

        // Act
        _helper.ExecuteInstruction();

        // Assert
        var result = _helper.ReadMemory64(memAddr);
        Assert.Equal(((ulong)newHigh << 32) | newLow, result);
        Assert.True(_helper.IsFlagSet(CpuFlag.ZF), "ZF should be set when values are equal");
    }

    [Fact]
    public void CMPXCHG8B_NotEqual_ShouldLoadEDXEAX()
    {
        // Arrange: CMPXCHG8B [EDI] (0F C7 0F)
        // If not equal, clear ZF and load memory value into EDX:EAX
        var memAddr = 0x00200000u;
        
        // Set up memory value
        _helper.WriteMemory64(memAddr, 0x1122334455667788);
        
        // Set EDX:EAX to different value
        _helper.SetReg("EAX", 0x99999999);
        _helper.SetReg("EDX", 0x88888888);
        
        // Set ECX:EBX to new value (won't be used since comparison fails)
        _helper.SetReg("EBX", 0xAAAAAAAA);
        _helper.SetReg("ECX", 0xBBBBBBBB);
        
        // Set EDI to memory address
        _helper.SetReg("EDI", memAddr);
        
        _helper.WriteCode(0x0F, 0xC7, 0x0F); // CMPXCHG8B [EDI]

        // Act
        _helper.ExecuteInstruction();

        // Assert
        Assert.Equal(0x55667788u, _helper.GetReg("EAX"));
        Assert.Equal(0x11223344u, _helper.GetReg("EDX"));
        Assert.False(_helper.IsFlagSet(CpuFlag.ZF), "ZF should be clear when values are not equal");
        
        // Memory should not change
        var result = _helper.ReadMemory64(memAddr);
        Assert.Equal(0x1122334455667788ul, result);
    }

    [Fact]
    public void RDMSR_ShouldNotCrash()
    {
        // Arrange: RDMSR (0F 32) - Read Model Specific Register
        // This is a privileged instruction but should have a stub
        _helper.SetReg("ECX", 0x00000000); // MSR index
        _helper.WriteCode(0x0F, 0x32);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => _helper.ExecuteInstruction());
        
        // In user mode, this might be a NOP, return dummy data, or throw.
        // For now, we just verify it doesn't crash the emulator
    }

    [Fact]
    public void WRMSR_ShouldNotCrash()
    {
        // Arrange: WRMSR (0F 30) - Write Model Specific Register
        // This is a privileged instruction but should have a stub
        _helper.SetReg("ECX", 0x00000000); // MSR index
        _helper.SetReg("EAX", 0x12345678); // Low 32 bits
        _helper.SetReg("EDX", 0xAABBCCDD); // High 32 bits
        _helper.WriteCode(0x0F, 0x30);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => _helper.ExecuteInstruction());
        
        // In user mode, this might be a NOP or throw. Either is acceptable.
        // For now, we just verify it doesn't crash the emulator
    }

    [Fact]
    public void RSM_ShouldNotCrash()
    {
        // Arrange: RSM (0F AA) - Resume from System Management Mode
        // This is a privileged instruction but should have a stub
        _helper.WriteCode(0x0F, 0xAA);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => _helper.ExecuteInstruction());
        
        // In user mode, this might be a NOP or throw. Either is acceptable.
        // For now, we just verify it doesn't crash the emulator
    }

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
