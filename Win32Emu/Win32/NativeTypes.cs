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

	public readonly unsafe struct Pvoid(void* v)
	{
		public readonly void* Value = v;
		public static implicit operator void*(Pvoid p) => p.Value;
		public static implicit operator Pvoid(void* v) => new(v);
	}

	public readonly unsafe struct Handle(void* v) : IEquatable<Handle>
	{
		public readonly void* Value = v;
		public static implicit operator void*(Handle h) => h.Value;
		public static implicit operator Handle(void* v) => new(v);

		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return obj is Handle other && Equals(other);
		}

		public bool Equals(Handle other)
		{
			return Value == other.Value;
		}

		public override int GetHashCode()
		{
			return unchecked((int)(long)Value);
		}

		public static bool operator ==(Handle left, Handle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Handle left, Handle right)
		{
			return !left.Equals(right);
		}
	}

	public readonly unsafe struct Hinstance(void* v)
	{
		public readonly void* Value = v;
		public static implicit operator void*(Hinstance h) => h.Value;
		public static implicit operator Hinstance(void* v) => new(v);
	}

	// DWORD is a 32-bit unsigned integer
	public struct Dword(uint v)
	{
		public uint Value = v;
		public static implicit operator uint(Dword d) => d.Value;
		public static implicit operator Dword(uint v) => new(v);
	}
}