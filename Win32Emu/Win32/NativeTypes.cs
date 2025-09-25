using System.Diagnostics.CodeAnalysis;

namespace Win32Emu.Win32;

public static class NativeTypes
{
	public readonly struct HModule(uint value)
	{
		public readonly uint Value = value;
		public bool IsNull => Value == 0;
		public static implicit operator uint(HModule h) => h.Value;
	}

	public readonly unsafe struct PVOID(void* v)
	{
		public readonly void* Value = v;
		public static implicit operator void*(PVOID p) => p.Value;
		public static implicit operator PVOID(void* v) => new(v);
	}

	public readonly unsafe struct HANDLE(void* v) : IEquatable<HANDLE>
	{
		public readonly void* Value = v;
		public static implicit operator void*(HANDLE h) => h.Value;
		public static implicit operator HANDLE(void* v) => new(v);

		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return obj is HANDLE other && Equals(other);
		}

		public bool Equals(HANDLE other)
		{
			return Value == other.Value;
		}

		public override int GetHashCode()
		{
			return unchecked((int)(long)Value);
		}

		public static bool operator ==(HANDLE left, HANDLE right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HANDLE left, HANDLE right)
		{
			return !left.Equals(right);
		}
	}

	public readonly unsafe struct HINSTANCE(void* v)
	{
		public readonly void* Value = v;
		public static implicit operator void*(HINSTANCE h) => h.Value;
		public static implicit operator HINSTANCE(void* v) => new(v);
	}

	// DWORD is a 32-bit unsigned integer
	public struct DWORD(uint v)
	{
		public uint Value = v;
		public static implicit operator uint(DWORD d) => d.Value;
		public static implicit operator DWORD(uint v) => new(v);
	}
}