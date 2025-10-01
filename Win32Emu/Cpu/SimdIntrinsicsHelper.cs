using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

namespace Win32Emu.Cpu;

/// <summary>
/// Provides hardware-accelerated implementations of SIMD instructions
/// using .NET intrinsics when available on the host CPU.
/// </summary>
public static class SimdIntrinsicsHelper
{
	/// <summary>
	/// Adds two 128-bit vectors of 4 single-precision floats using hardware acceleration.
	/// Used for implementing ADDPS (SSE) instruction.
	/// </summary>
	/// <param name="a">First operand as 16 bytes</param>
	/// <param name="b">Second operand as 16 bytes</param>
	/// <returns>Result as 16 bytes</returns>
	public static byte[] AddPackedSingle(byte[] a, byte[] b)
	{
		if (CpuIntrinsics.HasSse)
		{
			// Use x86 SSE intrinsics for hardware acceleration
			var vec1 = Vector128.Create(
				BitConverter.ToSingle(a, 0),
				BitConverter.ToSingle(a, 4),
				BitConverter.ToSingle(a, 8),
				BitConverter.ToSingle(a, 12)
			);
			var vec2 = Vector128.Create(
				BitConverter.ToSingle(b, 0),
				BitConverter.ToSingle(b, 4),
				BitConverter.ToSingle(b, 8),
				BitConverter.ToSingle(b, 12)
			);
			
			var result = Sse.Add(vec1, vec2);
			
			var output = new byte[16];
			BitConverter.GetBytes(result.GetElement(0)).CopyTo(output, 0);
			BitConverter.GetBytes(result.GetElement(1)).CopyTo(output, 4);
			BitConverter.GetBytes(result.GetElement(2)).CopyTo(output, 8);
			BitConverter.GetBytes(result.GetElement(3)).CopyTo(output, 12);
			return output;
		}
		else if (CpuIntrinsics.HasAdvSimd)
		{
			// Use ARM NEON intrinsics for hardware acceleration
			var vec1 = Vector128.Create(
				BitConverter.ToSingle(a, 0),
				BitConverter.ToSingle(a, 4),
				BitConverter.ToSingle(a, 8),
				BitConverter.ToSingle(a, 12)
			);
			var vec2 = Vector128.Create(
				BitConverter.ToSingle(b, 0),
				BitConverter.ToSingle(b, 4),
				BitConverter.ToSingle(b, 8),
				BitConverter.ToSingle(b, 12)
			);
			
			var result = AdvSimd.Add(vec1, vec2);
			
			var output = new byte[16];
			BitConverter.GetBytes(result.GetElement(0)).CopyTo(output, 0);
			BitConverter.GetBytes(result.GetElement(1)).CopyTo(output, 4);
			BitConverter.GetBytes(result.GetElement(2)).CopyTo(output, 8);
			BitConverter.GetBytes(result.GetElement(3)).CopyTo(output, 12);
			return output;
		}
		else
		{
			// Software fallback
			var output = new byte[16];
			for (int i = 0; i < 4; i++)
			{
				var f1 = BitConverter.ToSingle(a, i * 4);
				var f2 = BitConverter.ToSingle(b, i * 4);
				BitConverter.GetBytes(f1 + f2).CopyTo(output, i * 4);
			}
			return output;
		}
	}

	/// <summary>
	/// Multiplies two 128-bit vectors of 4 single-precision floats using hardware acceleration.
	/// Used for implementing MULPS (SSE) instruction.
	/// </summary>
	/// <param name="a">First operand as 16 bytes</param>
	/// <param name="b">Second operand as 16 bytes</param>
	/// <returns>Result as 16 bytes</returns>
	public static byte[] MultiplyPackedSingle(byte[] a, byte[] b)
	{
		if (CpuIntrinsics.HasSse)
		{
			var vec1 = Vector128.Create(
				BitConverter.ToSingle(a, 0),
				BitConverter.ToSingle(a, 4),
				BitConverter.ToSingle(a, 8),
				BitConverter.ToSingle(a, 12)
			);
			var vec2 = Vector128.Create(
				BitConverter.ToSingle(b, 0),
				BitConverter.ToSingle(b, 4),
				BitConverter.ToSingle(b, 8),
				BitConverter.ToSingle(b, 12)
			);
			
			var result = Sse.Multiply(vec1, vec2);
			
			var output = new byte[16];
			BitConverter.GetBytes(result.GetElement(0)).CopyTo(output, 0);
			BitConverter.GetBytes(result.GetElement(1)).CopyTo(output, 4);
			BitConverter.GetBytes(result.GetElement(2)).CopyTo(output, 8);
			BitConverter.GetBytes(result.GetElement(3)).CopyTo(output, 12);
			return output;
		}
		else if (CpuIntrinsics.HasAdvSimd)
		{
			var vec1 = Vector128.Create(
				BitConverter.ToSingle(a, 0),
				BitConverter.ToSingle(a, 4),
				BitConverter.ToSingle(a, 8),
				BitConverter.ToSingle(a, 12)
			);
			var vec2 = Vector128.Create(
				BitConverter.ToSingle(b, 0),
				BitConverter.ToSingle(b, 4),
				BitConverter.ToSingle(b, 8),
				BitConverter.ToSingle(b, 12)
			);
			
			var result = AdvSimd.Multiply(vec1, vec2);
			
			var output = new byte[16];
			BitConverter.GetBytes(result.GetElement(0)).CopyTo(output, 0);
			BitConverter.GetBytes(result.GetElement(1)).CopyTo(output, 4);
			BitConverter.GetBytes(result.GetElement(2)).CopyTo(output, 8);
			BitConverter.GetBytes(result.GetElement(3)).CopyTo(output, 12);
			return output;
		}
		else
		{
			// Software fallback
			var output = new byte[16];
			for (int i = 0; i < 4; i++)
			{
				var f1 = BitConverter.ToSingle(a, i * 4);
				var f2 = BitConverter.ToSingle(b, i * 4);
				BitConverter.GetBytes(f1 * f2).CopyTo(output, i * 4);
			}
			return output;
		}
	}

	/// <summary>
	/// Adds two 128-bit vectors of 2 double-precision floats using hardware acceleration.
	/// Used for implementing ADDPD (SSE2) instruction.
	/// </summary>
	/// <param name="a">First operand as 16 bytes</param>
	/// <param name="b">Second operand as 16 bytes</param>
	/// <returns>Result as 16 bytes</returns>
	public static byte[] AddPackedDouble(byte[] a, byte[] b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			var vec1 = Vector128.Create(
				BitConverter.ToDouble(a, 0),
				BitConverter.ToDouble(a, 8)
			);
			var vec2 = Vector128.Create(
				BitConverter.ToDouble(b, 0),
				BitConverter.ToDouble(b, 8)
			);
			
			var result = Sse2.Add(vec1, vec2);
			
			var output = new byte[16];
			BitConverter.GetBytes(result.GetElement(0)).CopyTo(output, 0);
			BitConverter.GetBytes(result.GetElement(1)).CopyTo(output, 8);
			return output;
		}
		else if (CpuIntrinsics.HasAdvSimd)
		{
			var vec1 = Vector128.Create(
				BitConverter.ToDouble(a, 0),
				BitConverter.ToDouble(a, 8)
			);
			var vec2 = Vector128.Create(
				BitConverter.ToDouble(b, 0),
				BitConverter.ToDouble(b, 8)
			);
			
			var result = AdvSimd.Arm64.Add(vec1, vec2);
			
			var output = new byte[16];
			BitConverter.GetBytes(result.GetElement(0)).CopyTo(output, 0);
			BitConverter.GetBytes(result.GetElement(1)).CopyTo(output, 8);
			return output;
		}
		else
		{
			// Software fallback
			var output = new byte[16];
			for (int i = 0; i < 2; i++)
			{
				var d1 = BitConverter.ToDouble(a, i * 8);
				var d2 = BitConverter.ToDouble(b, i * 8);
				BitConverter.GetBytes(d1 + d2).CopyTo(output, i * 8);
			}
			return output;
		}
	}

	/// <summary>
	/// Adds 16 bytes as integers using hardware acceleration.
	/// Used for implementing PADDB (SSE2) instruction.
	/// </summary>
	/// <param name="a">First operand as 16 bytes</param>
	/// <param name="b">Second operand as 16 bytes</param>
	/// <returns>Result as 16 bytes</returns>
	public static byte[] AddPackedBytes(byte[] a, byte[] b)
	{
		if (CpuIntrinsics.HasSse2)
		{
			var vec1 = Vector128.Create(a);
			var vec2 = Vector128.Create(b);
			
			var result = Sse2.Add(vec1, vec2);
			
			var output = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				output[i] = result.GetElement(i);
			}
			return output;
		}
		else if (CpuIntrinsics.HasAdvSimd)
		{
			var vec1 = Vector128.Create(a);
			var vec2 = Vector128.Create(b);
			
			var result = AdvSimd.Add(vec1, vec2);
			
			var output = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				output[i] = result.GetElement(i);
			}
			return output;
		}
		else
		{
			// Software fallback
			var output = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				output[i] = (byte)(a[i] + b[i]);
			}
			return output;
		}
	}

	/// <summary>
	/// Population count (count set bits) using hardware acceleration.
	/// Used for implementing POPCNT instruction.
	/// </summary>
	/// <param name="value">Value to count bits in</param>
	/// <returns>Number of set bits</returns>
	public static uint PopCount(uint value)
	{
		if (CpuIntrinsics.HasPopcnt)
		{
			return Popcnt.PopCount(value);
		}
		else if (CpuIntrinsics.HasAdvSimd)
		{
			// ARM has population count in AdvSimd
			// Software fallback for ARM since getting the scalar result is complex
			uint count = 0;
			while (value != 0)
			{
				value &= value - 1;
				count++;
			}
			return count;
		}
		else
		{
			// Software fallback using Brian Kernighan's algorithm
			uint count = 0;
			while (value != 0)
			{
				value &= value - 1;
				count++;
			}
			return count;
		}
	}

	/// <summary>
	/// Leading zero count using hardware acceleration.
	/// Used for implementing LZCNT instruction.
	/// </summary>
	/// <param name="value">Value to count leading zeros in</param>
	/// <returns>Number of leading zero bits</returns>
	public static uint LeadingZeroCount(uint value)
	{
		if (CpuIntrinsics.HasLzcnt)
		{
			return Lzcnt.LeadingZeroCount(value);
		}
		else if (CpuIntrinsics.HasArmBase)
		{
			return (uint)ArmBase.Arm64.LeadingZeroCount(value);
		}
		else
		{
			// Software fallback
			if (value == 0) return 32;
			uint count = 0;
			if ((value & 0xFFFF0000) == 0) { count += 16; value <<= 16; }
			if ((value & 0xFF000000) == 0) { count += 8; value <<= 8; }
			if ((value & 0xF0000000) == 0) { count += 4; value <<= 4; }
			if ((value & 0xC0000000) == 0) { count += 2; value <<= 2; }
			if ((value & 0x80000000) == 0) { count += 1; }
			return count;
		}
	}

	/// <summary>
	/// CRC32 calculation using hardware acceleration.
	/// Used for implementing CRC32 instruction (SSE4.2).
	/// </summary>
	/// <param name="crc">Previous CRC value</param>
	/// <param name="data">Data byte to process</param>
	/// <returns>Updated CRC value</returns>
	public static uint Crc32C(uint crc, byte data)
	{
		if (CpuIntrinsics.HasSse42)
		{
			return Sse42.Crc32(crc, data);
		}
		else if (CpuIntrinsics.HasCrc32_Arm || CpuIntrinsics.HasCrc32_Arm64)
		{
			return Crc32.ComputeCrc32(crc, data);
		}
		else
		{
			// Software fallback using standard CRC32 algorithm
			crc ^= data;
			for (int i = 0; i < 8; i++)
			{
				if ((crc & 1) != 0)
					crc = (crc >> 1) ^ 0xEDB88320;
				else
					crc >>= 1;
			}
			return crc;
		}
	}
}
