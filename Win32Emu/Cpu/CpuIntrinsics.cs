using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Aes = System.Runtime.Intrinsics.X86.Aes;

namespace Win32Emu.Cpu;

/// <summary>
/// Provides CPU intrinsics detection and capability information
/// for hardware-accelerated instruction emulation.
/// </summary>
public static class CpuIntrinsics
{
	/// <summary>
	/// Indicates if the host CPU is x86/x64 architecture
	/// </summary>
	public static readonly bool IsX86 = RuntimeInformation.ProcessArchitecture 
		is Architecture.X86 
		or Architecture.X64;

	/// <summary>
	/// Indicates if the host CPU is ARM architecture
	/// </summary>
	public static readonly bool IsArm = RuntimeInformation.ProcessArchitecture 
		is Architecture.Arm 
		or Architecture.Arm64;

	// x86 feature flags
	public static readonly bool HasSse = IsX86 && Sse.IsSupported;
	public static readonly bool HasSse2 = IsX86 && Sse2.IsSupported;
	public static readonly bool HasSse3 = IsX86 && Sse3.IsSupported;
	public static readonly bool HasSsse3 = IsX86 && Ssse3.IsSupported;
	public static readonly bool HasSse41 = IsX86 && Sse41.IsSupported;
	public static readonly bool HasSse42 = IsX86 && Sse42.IsSupported;
	public static readonly bool HasAvx = IsX86 && Avx.IsSupported;
	public static readonly bool HasAvx2 = IsX86 && Avx2.IsSupported;
	public static readonly bool HasAes = IsX86 && Aes.IsSupported;
	public static readonly bool HasPclmulqdq = IsX86 && Pclmulqdq.IsSupported;
	public static readonly bool HasPopcnt = IsX86 && Popcnt.IsSupported;
	public static readonly bool HasLzcnt = IsX86 && Lzcnt.IsSupported;
	public static readonly bool HasBmi1 = IsX86 && Bmi1.IsSupported;
	public static readonly bool HasBmi2 = IsX86 && Bmi2.IsSupported;
	public static readonly bool HasFma = IsX86 && Fma.IsSupported;

	// ARM feature flags
	public static readonly bool HasArmBase = IsArm && ArmBase.IsSupported;
	public static readonly bool HasAdvSimd = IsArm && AdvSimd.IsSupported;
	public static readonly bool HasAesArm = IsArm && System.Runtime.Intrinsics.Arm.Aes.Arm64.IsSupported;
	public static readonly bool HasCrc32Arm = IsArm && Crc32.IsSupported;
	public static readonly bool HasCrc32Arm64 = IsArm && Crc32.Arm64.IsSupported;
	public static readonly bool HasDp = IsArm && Dp.IsSupported;
	public static readonly bool HasRdm = IsArm && Rdm.IsSupported;
	public static readonly bool HasSha1 = IsArm && Sha1.IsSupported;
	public static readonly bool HasSha256 = IsArm && Sha256.IsSupported;

	/// <summary>
	/// Gets CPUID feature flags for ECX register (function 1)
	/// </summary>
	public static uint GetCpuidEcxFeatures()
	{
		uint ecx = 0;

		if (HasSse3)
		{
			ecx |= 1U;        // SSE3
		}

		if (HasPclmulqdq)
		{
			ecx |= 1 << 1;   // PCLMULQDQ
		}

		if (HasSsse3)
		{
			ecx |= 1 << 9;       // SSSE3
		}

		if (HasFma)
		{
			ecx |= 1 << 12;        // FMA
		}

		if (HasSse41)
		{
			ecx |= 1 << 19;      // SSE4.1
		}

		if (HasSse42)
		{
			ecx |= 1 << 20;      // SSE4.2
		}

		if (HasPopcnt)
		{
			ecx |= 1 << 23;     // POPCNT
		}

		if (HasAes)
		{
			ecx |= 1 << 25;        // AES
		}

		if (HasAvx)
		{
			ecx |= 1 << 28;        // AVX
		}

		return ecx;
	}

	/// <summary>
	/// Gets CPUID feature flags for EDX register (function 1)
	/// </summary>
	public static uint GetCpuidEdxFeatures()
	{
		uint edx = 0;

		edx |= 1 << 0;                     // FPU (always supported)
		edx |= 1 << 4;                     // TSC (RDTSC)
		edx |= 1 << 5;                     // MSR (RDMSR/WRMSR)
		edx |= 1 << 8;                     // CMPXCHG8B
		edx |= 1 << 15;                    // CMOV
		if (HasSse)
		{
			edx |= 1 << 25;        // SSE
		}

		if (HasSse2)
		{
			edx |= 1 << 26;       // SSE2
		}

		return edx;
	}

	/// <summary>
	/// Gets extended CPUID feature flags for EBX register (function 7, sub-function 0)
	/// </summary>
	public static uint GetCpuidExtendedEbxFeatures()
	{
		uint ebx = 0;

		if (HasBmi1)
		{
			ebx |= 1 << 3;        // BMI1
		}

		if (HasAvx2)
		{
			ebx |= 1 << 5;        // AVX2
		}

		if (HasBmi2)
		{
			ebx |= 1 << 8;        // BMI2
		}
		// LZCNT is not reported in EBX (function 7); see GetCpuid80000001EcxFeatures()

		return ebx;
	}
	/// <summary>
	/// Gets extended CPUID feature flags for ECX register (function 0x80000001)
	/// LZCNT is bit 5 (per Intel/AMD documentation).
	/// </summary>
	public static uint GetCpuid80000001EcxFeatures()
	{
		uint ecx = 0;
		if (HasLzcnt)
		{
			ecx |= 1 << 5; // LZCNT
		}

		return ecx;
	}
}
