using Win32Emu.Memory;

namespace Win32Emu.Win32;

public class ProcessEnvironment(VirtualMemory vm, uint heapBase = 0x01000000)
{
	private uint _allocPtr = heapBase;
	private bool _exitRequested;

	public uint CommandLinePtr { get; private set; }
	public uint ModuleFileNamePtr { get; private set; }
	public uint ModuleFileNameLength { get; private set; }
	public bool ExitRequested => _exitRequested;

	// Default standard handles (pseudo values)
	public uint StdInputHandle { get; set; } = 0x00000001;
	public uint StdOutputHandle { get; set; } = 0x00000002;
	public uint StdErrorHandle { get; set; } = 0x00000003;

	// Simple handle table for host resources (files etc.)
	private readonly Dictionary<uint, object> _handles = new();
	private uint _nextHandle = 0x00001000; // avoid low values used as sentinels

	public void InitializeStrings(string exePath, string[] args)
	{
		var cmdLine = string.Join(" ", new[] { exePath }.Concat(args.Skip(1)));
		CommandLinePtr = WriteAnsiString(cmdLine + '\0');
		ModuleFileNamePtr = WriteAnsiString(exePath + '\0');
		ModuleFileNameLength = (uint)exePath.Length;
	}

	public uint SimpleAlloc(uint size)
	{
		if (size == 0) size = 1;
		var addr = _allocPtr;
		_allocPtr = AlignUp(_allocPtr + size, 16);
		return addr;
	}

	public void RequestExit() => _exitRequested = true;

	// Guest memory helpers
	public uint WriteAnsiString(string s)
	{
		var bytes = System.Text.Encoding.ASCII.GetBytes(s);
		var addr = SimpleAlloc((uint)bytes.Length);
		vm.WriteBytes(addr, bytes);
		return addr;
	}

	public void WriteAnsiStringAt(uint addr, string s, bool nullTerminate = true)
	{
		var bytes = System.Text.Encoding.ASCII.GetBytes(nullTerminate ? s + "\0" : s);
		vm.WriteBytes(addr, bytes);
	}

	public string ReadAnsiString(uint addr)
	{
		var buf = new List<byte>();
		var p = addr;
		for (;;)
		{
			var b = vm.Read8(p++);
			if (b == 0) break;
			buf.Add(b);
		}

		return System.Text.Encoding.ASCII.GetString(buf.ToArray());
	}

	public byte[] MemReadBytes(uint addr, int count) => vm.GetSpan(addr, count);
	public byte MemRead8(uint addr) => vm.Read8(addr);
	public void MemWriteBytes(uint addr, ReadOnlySpan<byte> data) => vm.WriteBytes(addr, data);
	public void MemWrite32(uint addr, uint value) => vm.Write32(addr, value);
	public void MemWrite16(uint addr, ushort value) => vm.Write16(addr, value);
	public void MemZero(uint addr, uint size) => vm.WriteBytes(addr, new byte[size]);

	// Handle table ops
	public uint RegisterHandle(object obj)
	{
		var h = _nextHandle;
		_nextHandle += 4;
		_handles[h] = obj;
		return h;
	}

	public bool TryGetHandle<T>(uint handle, out T? value) where T : class
	{
		if (_handles.TryGetValue(handle, out var obj) && obj is T t)
		{
			value = t;
			return true;
		}

		value = null;
		return false;
	}

	public bool CloseHandle(uint handle) => _handles.Remove(handle);

	// Heaps
	private readonly Dictionary<uint, HeapState> _heaps = new();

	public uint HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize)
	{
		var init = AlignUp(dwInitialSize == 0 ? 0x10000u : dwInitialSize, 0x1000);
		var max = dwMaximumSize == 0 ? init : AlignUp(dwMaximumSize, 0x1000);
		var baseAddr = SimpleAlloc(init);
		_heaps[baseAddr] = new HeapState(baseAddr, baseAddr, baseAddr + max);
		return baseAddr;
	}

	public uint HeapAlloc(uint hHeap, uint dwBytes)
	{
		if (!_heaps.TryGetValue(hHeap, out var hs)) return SimpleAlloc(dwBytes);
		var size = AlignUp(dwBytes == 0 ? 1u : dwBytes, 16);
		if (hs.Current + size <= hs.Limit)
		{
			var ptr = hs.Current;
			hs = hs with { Current = hs.Current + size };
			_heaps[hHeap] = hs;
			return ptr;
		}

		return SimpleAlloc(dwBytes);
	}

	public uint HeapFree(uint hHeap, uint lpMem) => 1;

	// VirtualAlloc
	public uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
	{
		var size = AlignUp(dwSize == 0 ? 1u : dwSize, 0x1000);
		if (lpAddress != 0)
		{
			if (lpAddress + size <= vm.Size) vm.WriteBytes(lpAddress, new byte[size]);
			return lpAddress;
		}

		var addr = AlignUp(_allocPtr, 0x1000);
		_allocPtr = addr + size;
		vm.WriteBytes(addr, new byte[size]);
		return addr;
	}

	private static uint AlignUp(uint value, uint align) => (value + (align - 1)) & ~(align - 1);

	private record struct HeapState(uint Base, uint Current, uint Limit);
}