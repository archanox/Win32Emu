using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;

namespace Win32Emu.Cpu;

/// <summary>
/// Provides hardware-accelerated implementations of MMX (64-bit SIMD) instructions
/// using .NET intrinsics when available on the host CPU.
/// </summary>
public static class MmxIntrinsicsHelper
{
	/// <summary>
	/// Adds 8 packed unsigned bytes (PADDB).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AddPackedBytes(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 to add packed bytes
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var hwResult = vec1 + vec2;
			return hwResult.AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON for 64-bit packed byte addition
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var hwResult = AdvSimd.Add(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			result |= (ulong)((byte)(va + vb)) << (i * 8);
		}
		return result;
	}

	/// <summary>
	/// Adds 4 packed unsigned words (PADDW).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AddPackedWords(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 to add packed words
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var hwResult = vec1 + vec2;
			return hwResult.AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON for 64-bit packed word addition
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var hwResult = AdvSimd.Add(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			result |= (ulong)((ushort)(va + vb)) << (i * 16);
		}
		return result;
	}

	/// <summary>
	/// Adds 2 packed unsigned dwords (PADDD).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AddPackedDwords(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 to add packed dwords
			var vec1 = Vector64.Create(a).AsUInt32();
			var vec2 = Vector64.Create(b).AsUInt32();
			var hwResult = vec1 + vec2;
			return hwResult.AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON for 64-bit packed dword addition
			var vec1 = Vector64.Create(a).AsUInt32();
			var vec2 = Vector64.Create(b).AsUInt32();
			var hwResult = AdvSimd.Add(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		uint lo_a = (uint)(a & 0xFFFFFFFF);
		uint lo_b = (uint)(b & 0xFFFFFFFF);
		uint hi_a = (uint)((a >> 32) & 0xFFFFFFFF);
		uint hi_b = (uint)((b >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi_a + hi_b) << 32) | (lo_a + lo_b);
	}

	/// <summary>
	/// Subtracts 8 packed unsigned bytes (PSUBB).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong SubtractPackedBytes(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 to subtract packed bytes
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var hwResult = vec1 - vec2;
			return hwResult.AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON for 64-bit packed byte subtraction
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var hwResult = AdvSimd.Subtract(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			result |= (ulong)((byte)(va - vb)) << (i * 8);
		}
		return result;
	}

	/// <summary>
	/// Subtracts 4 packed unsigned words (PSUBW).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong SubtractPackedWords(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 to subtract packed words
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var hwResult = vec1 - vec2;
			return hwResult.AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON for 64-bit packed word subtraction
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var hwResult = AdvSimd.Subtract(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			result |= (ulong)((ushort)(va - vb)) << (i * 16);
		}
		return result;
	}

	/// <summary>
	/// Subtracts 2 packed unsigned dwords (PSUBD).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong SubtractPackedDwords(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 to subtract packed dwords
			var vec1 = Vector64.Create(a).AsUInt32();
			var vec2 = Vector64.Create(b).AsUInt32();
			var hwResult = vec1 - vec2;
			return hwResult.AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON for 64-bit packed dword subtraction
			var vec1 = Vector64.Create(a).AsUInt32();
			var vec2 = Vector64.Create(b).AsUInt32();
			var hwResult = AdvSimd.Subtract(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		uint lo_a = (uint)(a & 0xFFFFFFFF);
		uint lo_b = (uint)(b & 0xFFFFFFFF);
		uint hi_a = (uint)((a >> 32) & 0xFFFFFFFF);
		uint hi_b = (uint)((b >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi_a - hi_b) << 32) | (lo_a - lo_b);
	}

	/// <summary>
	/// Adds 8 packed signed bytes with saturation (PADDSB).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AddPackedSignedBytesWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating add
			var vec1 = Vector64.Create(a).AsSByte();
			var vec2 = Vector64.Create(b).AsSByte();
			// SSE2 doesn't have direct 64-bit saturating ops, need to use 128-bit
			var vec128_1 = Vector128.Create(vec1, Vector64<sbyte>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<sbyte>.Zero);
			var result128 = Sse2.AddSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating add
			var vec1 = Vector64.Create(a).AsSByte();
			var vec2 = Vector64.Create(b).AsSByte();
			var hwResult = AdvSimd.AddSaturate(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			sbyte va = (sbyte)((a >> (i * 8)) & 0xFF);
			sbyte vb = (sbyte)((b >> (i * 8)) & 0xFF);
			int sum = va + vb;
			if (sum > 127) sum = 127;
			if (sum < -128) sum = -128;
			result |= (ulong)((byte)sum) << (i * 8);
		}
		return result;
	}

	/// <summary>
	/// Adds 4 packed signed words with saturation (PADDSW).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AddPackedSignedWordsWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating add
			var vec1 = Vector64.Create(a).AsInt16();
			var vec2 = Vector64.Create(b).AsInt16();
			var vec128_1 = Vector128.Create(vec1, Vector64<short>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<short>.Zero);
			var result128 = Sse2.AddSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating add
			var vec1 = Vector64.Create(a).AsInt16();
			var vec2 = Vector64.Create(b).AsInt16();
			var hwResult = AdvSimd.AddSaturate(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short va = (short)((a >> (i * 16)) & 0xFFFF);
			short vb = (short)((b >> (i * 16)) & 0xFFFF);
			int sum = va + vb;
			if (sum > 32767) sum = 32767;
			if (sum < -32768) sum = -32768;
			result |= (ulong)((ushort)sum) << (i * 16);
		}
		return result;
	}

	/// <summary>
	/// Adds 8 packed unsigned bytes with saturation (PADDUSB).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AddPackedUnsignedBytesWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating add
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var vec128_1 = Vector128.Create(vec1, Vector64<byte>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<byte>.Zero);
			var result128 = Sse2.AddSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating add
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var hwResult = AdvSimd.AddSaturate(vec1, vec2);
			return hwResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			int sum = va + vb;
			if (sum > 255) sum = 255;
			result |= (ulong)((byte)sum) << (i * 8);
		}
		return result;
	}

	/// <summary>
	/// Adds 4 packed unsigned words with saturation (PADDUSW).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AddPackedUnsignedWordsWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating add
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var vec128_1 = Vector128.Create(vec1, Vector64<ushort>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<ushort>.Zero);
			var result128 = Sse2.AddSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating add
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var neonResult = AdvSimd.AddSaturate(vec1, vec2);
			return neonResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong fallbackResult = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			int sum = va + vb;
			if (sum > 65535) sum = 65535;
			fallbackResult |= (ulong)((ushort)sum) << (i * 16);
		}
		return fallbackResult;
	}

	/// <summary>
	/// Subtracts 8 packed signed bytes with saturation (PSUBSB).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong SubtractPackedSignedBytesWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating subtract
			var vec1 = Vector64.Create(a).AsSByte();
			var vec2 = Vector64.Create(b).AsSByte();
			var vec128_1 = Vector128.Create(vec1, Vector64<sbyte>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<sbyte>.Zero);
			var result128 = Sse2.SubtractSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating subtract
			var vec1 = Vector64.Create(a).AsSByte();
			var vec2 = Vector64.Create(b).AsSByte();
			var neonResult = AdvSimd.SubtractSaturate(vec1, vec2);
			return neonResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong fallbackResult = 0;
		for (int i = 0; i < 8; i++)
		{
			sbyte va = (sbyte)((a >> (i * 8)) & 0xFF);
			sbyte vb = (sbyte)((b >> (i * 8)) & 0xFF);
			int diff = va - vb;
			if (diff > 127) diff = 127;
			if (diff < -128) diff = -128;
			fallbackResult |= (ulong)((byte)diff) << (i * 8);
		}
		return fallbackResult;
	}

	/// <summary>
	/// Subtracts 4 packed signed words with saturation (PSUBSW).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong SubtractPackedSignedWordsWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating subtract
			var vec1 = Vector64.Create(a).AsInt16();
			var vec2 = Vector64.Create(b).AsInt16();
			var vec128_1 = Vector128.Create(vec1, Vector64<short>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<short>.Zero);
			var result128 = Sse2.SubtractSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating subtract
			var vec1 = Vector64.Create(a).AsInt16();
			var vec2 = Vector64.Create(b).AsInt16();
			var neonResult = AdvSimd.SubtractSaturate(vec1, vec2);
			return neonResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong fallbackResult = 0;
		for (int i = 0; i < 4; i++)
		{
			short va = (short)((a >> (i * 16)) & 0xFFFF);
			short vb = (short)((b >> (i * 16)) & 0xFFFF);
			int diff = va - vb;
			if (diff > 32767) diff = 32767;
			if (diff < -32768) diff = -32768;
			fallbackResult |= (ulong)((ushort)diff) << (i * 16);
		}
		return fallbackResult;
	}

	/// <summary>
	/// Subtracts 8 packed unsigned bytes with saturation (PSUBUSB).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong SubtractPackedUnsignedBytesWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating subtract
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var vec128_1 = Vector128.Create(vec1, Vector64<byte>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<byte>.Zero);
			var result128 = Sse2.SubtractSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating subtract
			var vec1 = Vector64.Create(a).AsByte();
			var vec2 = Vector64.Create(b).AsByte();
			var neonResult = AdvSimd.SubtractSaturate(vec1, vec2);
			return neonResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong fallbackResult = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			int diff = va - vb;
			if (diff < 0) diff = 0;
			fallbackResult |= (ulong)((byte)diff) << (i * 8);
		}
		return fallbackResult;
	}

	/// <summary>
	/// Subtracts 4 packed unsigned words with saturation (PSUBUSW).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong SubtractPackedUnsignedWordsWithSaturation(ulong a, ulong b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			// Use SSE2 saturating subtract
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var vec128_1 = Vector128.Create(vec1, Vector64<ushort>.Zero);
			var vec128_2 = Vector128.Create(vec2, Vector64<ushort>.Zero);
			var result128 = Sse2.SubtractSaturate(vec128_1, vec128_2);
			return result128.GetLower().AsUInt64().ToScalar();
		}

		if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON saturating subtract
			var vec1 = Vector64.Create(a).AsUInt16();
			var vec2 = Vector64.Create(b).AsUInt16();
			var neonResult = AdvSimd.SubtractSaturate(vec1, vec2);
			return neonResult.AsUInt64().ToScalar();
		}

		// Software fallback
		ulong fallbackResult = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			int diff = va - vb;
			if (diff < 0) diff = 0;
			fallbackResult |= (ulong)((ushort)diff) << (i * 16);
		}
		return fallbackResult;
	}

	/// <summary>
	/// Bitwise AND (PAND).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong And(ulong a, ulong b)
	{
		return a & b;
	}

	/// <summary>
	/// Bitwise OR (POR).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong Or(ulong a, ulong b)
	{
		return a | b;
	}

	/// <summary>
	/// Bitwise XOR (PXOR).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong Xor(ulong a, ulong b)
	{
		return a ^ b;
	}

	/// <summary>
	/// Bitwise AND NOT (PANDN) - computes (~a) & b.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong AndNot(ulong a, ulong b)
	{
		return (~a) & b;
	}
}
