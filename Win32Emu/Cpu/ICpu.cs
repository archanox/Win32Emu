using System.Runtime.CompilerServices;
using Win32Emu.Memory;

namespace Win32Emu.Cpu;

public interface ICpu
{
	void SetEip(uint eip);
	uint GetEip();
	uint GetRegister(string name);
	void SetRegister(string name, uint value, [CallerMemberName] string callerName = "");
	CpuStepResult SingleStep(VirtualMemory mem);
}