using System;
using System.Threading.Tasks;
using Win32Emu.Cpu; // For CpuStepResult

namespace Win32Emu.Jit.Generated
{
    /// <summary>
    /// Auto-generated JIT code for block at 0x00401130
    /// Generated from RTL intermediate representation
    /// </summary>
    public class JitBlock_1d6fffd8_00401130
    {
        public async Task<CpuStepResult> Execute(dynamic cpu, dynamic mem)
        {
            // CPU state
            uint EAX = cpu.GetRegister("EAX");
            uint EBX = cpu.GetRegister("EBX");
            uint ECX = cpu.GetRegister("ECX");
            uint EDX = cpu.GetRegister("EDX");
            uint ESI = cpu.GetRegister("ESI");
            uint EDI = cpu.GetRegister("EDI");
            uint EBP = cpu.GetRegister("EBP");
            uint ESP = cpu.GetRegister("ESP");
            uint FLAGS = 0;

            // Block at offset 0x401130
            EAX = 0x1878u; // @0x401130
            EAX = await CallFunction(0x402820u, new object[] {  }); // @0x401135

            // Save CPU state
            cpu.SetRegister("EAX", EAX);
            cpu.SetRegister("EBX", EBX);
            cpu.SetRegister("ECX", ECX);
            cpu.SetRegister("EDX", EDX);
            cpu.SetRegister("ESI", ESI);
            cpu.SetRegister("EDI", EDI);
            cpu.SetRegister("EBP", EBP);
            cpu.SetRegister("ESP", ESP);

            return await Task.FromResult(new CpuStepResult { IsCall = false, CallTarget = 0 });
        }
    }
}
