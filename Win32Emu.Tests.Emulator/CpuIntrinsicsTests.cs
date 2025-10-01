using Win32Emu.Cpu;
using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for CPU intrinsics detection and CPUID feature reporting
/// </summary>
public class CpuIntrinsicsTests : IDisposable
{
    private readonly CpuTestHelper _helper;

    public CpuIntrinsicsTests()
    {
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void CpuIntrinsics_ShouldDetectArchitecture()
    {
        // Assert - At least one architecture should be detected
        Assert.True(CpuIntrinsics.IsX86 || CpuIntrinsics.IsArm, 
            "Should detect either x86 or ARM architecture");
    }

    [Fact]
    public void CPUID_Function0_ShouldReturnMaxFunction()
    {
        // Arrange: CPUID (0F A2) with EAX=0 - Get vendor string
        _helper.SetReg("EAX", 0);
        _helper.WriteCode(0x0F, 0xA2);

        // Act
        _helper.ExecuteInstruction();

        // Assert - EAX should contain max supported function (at least 1, now extended to 7)
        var maxFunction = _helper.GetReg("EAX");
        Assert.True(maxFunction >= 1, "CPUID should support at least function 1");
    }

    [Fact]
    public void CPUID_Function1_ShouldReturnHostBasedFeatures()
    {
        // Arrange: CPUID (0F A2) with EAX=1 - Get feature flags
        _helper.SetReg("EAX", 1);
        _helper.WriteCode(0x0F, 0xA2);

        // Act
        _helper.ExecuteInstruction();

        // Assert - EDX should contain basic features
        var featuresEDX = _helper.GetReg("EDX");
        var featuresECX = _helper.GetReg("ECX");

        // FPU (bit 0) should always be set
        Assert.True((featuresEDX & (1 << 0)) != 0, "FPU feature should be set");
        
        // TSC (bit 4) should always be set
        Assert.True((featuresEDX & (1 << 4)) != 0, "TSC feature should be set");
        
        // CMOV (bit 15) should always be set
        Assert.True((featuresEDX & (1 << 15)) != 0, "CMOV feature should be set");
        
        // CMPXCHG8B (bit 8) should always be set
        Assert.True((featuresEDX & (1 << 8)) != 0, "CMPXCHG8B feature should be set");

        // If running on x86 host with SSE support, SSE flags should be set
        if (CpuIntrinsics.HasSse)
        {
            Assert.True((featuresEDX & (1 << 25)) != 0, "SSE feature should be set on x86 hosts with SSE");
        }

        if (CpuIntrinsics.HasSse2)
        {
            Assert.True((featuresEDX & (1 << 26)) != 0, "SSE2 feature should be set on x86 hosts with SSE2");
        }

        if (CpuIntrinsics.HasSse3)
        {
            Assert.True((featuresECX & (1 << 0)) != 0, "SSE3 feature should be set on x86 hosts with SSE3");
        }

        if (CpuIntrinsics.HasSsse3)
        {
            Assert.True((featuresECX & (1 << 9)) != 0, "SSSE3 feature should be set on x86 hosts with SSSE3");
        }
    }

    [Fact]
    public void CPUID_Function7_SubFunction0_ShouldReturnExtendedFeatures()
    {
        // Arrange: CPUID (0F A2) with EAX=7, ECX=0 - Get extended features
        _helper.SetReg("EAX", 7);
        _helper.SetReg("ECX", 0);
        _helper.WriteCode(0x0F, 0xA2);

        // Act
        _helper.ExecuteInstruction();

        // Assert - EAX should contain max sub-function (0 for now)
        var maxSubFunction = _helper.GetReg("EAX");
        var extendedFeaturesEBX = _helper.GetReg("EBX");

        Assert.Equal(0u, maxSubFunction);

        // If running on x86 host with AVX2 support, AVX2 flag should be set
        if (CpuIntrinsics.HasAvx2)
        {
            Assert.True((extendedFeaturesEBX & (1 << 5)) != 0, 
                "AVX2 feature should be set on x86 hosts with AVX2");
        }

        if (CpuIntrinsics.HasBmi1)
        {
            Assert.True((extendedFeaturesEBX & (1 << 3)) != 0, 
                "BMI1 feature should be set on x86 hosts with BMI1");
        }

        if (CpuIntrinsics.HasBmi2)
        {
            Assert.True((extendedFeaturesEBX & (1 << 8)) != 0, 
                "BMI2 feature should be set on x86 hosts with BMI2");
        }
    }

    [Fact]
    public void CPUID_UnsupportedFunction_ShouldReturnZeros()
    {
        // Arrange: CPUID (0F A2) with unsupported function
        _helper.SetReg("EAX", 0xFFFFFFFF);
        _helper.SetReg("EBX", 0x12345678);
        _helper.SetReg("ECX", 0xABCDEF01);
        _helper.SetReg("EDX", 0x98765432);
        _helper.WriteCode(0x0F, 0xA2);

        // Act
        _helper.ExecuteInstruction();

        // Assert - All registers should be zero for unsupported functions
        Assert.Equal(0u, _helper.GetReg("EAX"));
        Assert.Equal(0u, _helper.GetReg("EBX"));
        Assert.Equal(0u, _helper.GetReg("ECX"));
        Assert.Equal(0u, _helper.GetReg("EDX"));
    }

    [Fact]
    public void CpuIntrinsics_GetCpuidFeatures_ShouldNotThrow()
    {
        // Act & Assert - Should not throw when querying features
        var ecxFeatures = CpuIntrinsics.GetCpuidEcxFeatures();
        var edxFeatures = CpuIntrinsics.GetCpuidEdxFeatures();
        var extendedFeatures = CpuIntrinsics.GetCpuidExtendedEbxFeatures();

        // Basic validation - EDX should always have some features set
        Assert.True(edxFeatures > 0, "EDX features should include at least basic CPU features");
    }

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
