using System;
using System.Threading.Tasks;
using Win32Emu.Cpu; // For CpuStepResult

namespace Win32Emu.Jit.Generated
{
    /// <summary>
    /// Auto-generated JIT code for block at 0x00411450
    /// Generated from RTL intermediate representation
    /// </summary>
    public class JitBlock_0f18c821_00411450
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

            // Temporaries
            uint t0 = 0;
            uint t1 = 0;
            uint t2 = 0;

            // Block at offset 0x411450
            ESP = ESP - 0x4u; // @0x411450
            mem.Write32(ESP, EBX); // @0x411450
            ESP = ESP - 0x4u; // @0x411451
            mem.Write32(ESP, ESI); // @0x411451
            ESI = (ESP + 0x10u); // @0x411452
            ESP = ESP - 0x4u; // @0x411456
            mem.Write32(ESP, EDI); // @0x411456
            FLAGS = (ESI - 0x0u); // @0x411457
            EAX = ESI; // @0x41145A

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
