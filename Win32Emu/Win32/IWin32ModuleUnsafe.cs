using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public interface IWin32ModuleUnsafe
{
	string Name { get; }

	// Optional extension: modules can implement pointer-based stubs for selected exports.
	bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue);
}

public class User32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "USER32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[User32] Unimplemented export: {export}");
		return false;
	}
}

public class Gdi32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "GDI32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[Gdi32] Unimplemented export: {export}");
		return false;
	}
}

public class DDrawModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "DDRAW.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[DDraw] Unimplemented export: {export}");
		return false;
	}
}

public class DSoundModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "DSOUND.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[DSound] Unimplemented export: {export}");
		return false;
	}
}


public class DInputModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "DINPUT.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[DInput] Unimplemented export: {export}");
		return false;
	}
}

public class WinMMModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "WINMM.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[WinMM] Unimplemented export: {export}");
		return false;
	}
}

public class Glide2xModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "GLIDE2X.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[Glide2x] Unimplemented export: {export}");
		return false;
	}
}