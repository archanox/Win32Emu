using System;
using System.Threading.Tasks;
using Win32Emu.Cpu; // For CpuStepResult

namespace Win32Emu.Jit.Generated
{
    /// <summary>
    /// Auto-generated JIT code for block at 0x00401000
    /// Generated from RTL intermediate representation
    /// </summary>
    public class JitBlock_6503f239_00401000
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

            // Block at offset 0x401000
            ESP = ESP - 0x4u; // @0x401000
            mem.Write32(ESP, EBP); // @0x401000
            ESP = ESP - 0x4u; // @0x401001
            mem.Write32(ESP, EBX); // @0x401001
            ESP = ESP - 0x4u; // @0x401002
            mem.Write32(ESP, EDI); // @0x401002
            ESP = ESP - 0x4u; // @0x401003
            mem.Write32(ESP, ESI); // @0x401003
            ESP = ESP - 0x0u; // @0x401004
            EAX = (ESP + 0x90u); // @0x401007
            FLAGS = (EAX - 0x0u); // @0x40100E
            if ((FLAGS == 0x0u)) goto Label_4012B8; // @0x401011

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
