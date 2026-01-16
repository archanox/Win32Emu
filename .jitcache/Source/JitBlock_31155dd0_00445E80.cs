using System;
using System.Threading.Tasks;
using Win32Emu.Cpu; // For CpuStepResult

namespace Win32Emu.Jit.Generated
{
    /// <summary>
    /// Auto-generated JIT code for block at 0x00445E80
    /// Generated from RTL intermediate representation
    /// </summary>
    public class JitBlock_31155dd0_00445E80
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
            uint t3 = 0;

            // Block at offset 0x445E80
            ESP = ESP - 0x4u; // @0x445E80
            mem.Write32(ESP, EBX); // @0x445E80
            EBX = (ESP + 0x8u); // @0x445E81
            ESP = ESP - 0x4u; // @0x445E85
            mem.Write32(ESP, EBP); // @0x445E85
            EBP = (ESP + 0x14u); // @0x445E86
            ESP = ESP - 0x4u; // @0x445E8A
            mem.Write32(ESP, ESI); // @0x445E8A
            ESP = ESP - 0x4u; // @0x445E8B
            mem.Write32(ESP, EDI); // @0x445E8B
            EDI = (ESP + 0x18u); // @0x445E8C
            FLAGS = (EDI - 0x0u); // @0x445E90

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
