using UnicornEngine;
using UnicornEngine.Const;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator.TestInfrastructure;

/// <summary>
/// Helper class for testing CPU instruction execution by comparing Win32Emu against Unicorn Engine
/// </summary>
public class UnicornTestHelper : IDisposable
{
    private readonly Unicorn _unicorn;
    private readonly IcedCpu _win32EmuCpu;
    private readonly VirtualMemory _win32EmuMemory;
    
    private const long CodeBaseAddress = 0x00400000;
    private const long StackBaseAddress = 0x00100000;
    private const long DataBaseAddress = 0x00200000;
    private const long MemorySize = 0x100000; // 1MB

    public UnicornTestHelper()
    {
        // Initialize Unicorn emulator for x86 32-bit
        _unicorn = new Unicorn(Common.UC_ARCH_X86, Common.UC_MODE_32);
        
        // Map memory regions in Unicorn
        _unicorn.MemMap(CodeBaseAddress, MemorySize, Common.UC_PROT_ALL);
        _unicorn.MemMap(StackBaseAddress, MemorySize, Common.UC_PROT_ALL);
        _unicorn.MemMap(DataBaseAddress, MemorySize, Common.UC_PROT_ALL);
        
        // Initialize Win32Emu
        _win32EmuMemory = new VirtualMemory();
        _win32EmuCpu = new IcedCpu(_win32EmuMemory);
        
        // Initialize stack pointers for both
        var initialEsp = (uint)(StackBaseAddress + 0x8000);
        _unicorn.RegWrite(X86.UC_X86_REG_ESP, (int)initialEsp);
        _win32EmuCpu.SetRegister("ESP", initialEsp);
        _win32EmuCpu.SetRegister("EBP", initialEsp);
        
        // Initialize instruction pointers for both
        _unicorn.RegWrite(X86.UC_X86_REG_EIP, (int)CodeBaseAddress);
        _win32EmuCpu.SetEip((uint)CodeBaseAddress);
    }

    /// <summary>
    /// Write machine code bytes at the current EIP in both emulators
    /// </summary>
    public void WriteCode(params byte[] code)
    {
        var eip = CodeBaseAddress;
        
        // Write to Unicorn
        _unicorn.MemWrite(eip, code);
        
        // Write to Win32Emu
        for (var i = 0; i < code.Length; i++)
        {
            _win32EmuMemory.Write8((uint)eip + (uint)i, code[i]);
        }
    }

    /// <summary>
    /// Set a register value in both emulators
    /// </summary>
    public void SetReg(string name, uint value)
    {
        // Set in Win32Emu
        _win32EmuCpu.SetRegister(name, value);
        
        // Set in Unicorn
        var regId = name.ToUpperInvariant() switch
        {
            "EAX" => X86.UC_X86_REG_EAX,
            "EBX" => X86.UC_X86_REG_EBX,
            "ECX" => X86.UC_X86_REG_ECX,
            "EDX" => X86.UC_X86_REG_EDX,
            "ESI" => X86.UC_X86_REG_ESI,
            "EDI" => X86.UC_X86_REG_EDI,
            "EBP" => X86.UC_X86_REG_EBP,
            "ESP" => X86.UC_X86_REG_ESP,
            "EIP" => X86.UC_X86_REG_EIP,
            "EFLAGS" => X86.UC_X86_REG_EFLAGS,
            _ => 0
        };
        
        if (regId == 0)
        {
            throw new ArgumentException($"Unsupported register name: {name}", nameof(name));
        }
        _unicorn.RegWrite(regId, (int)value);
    }

    /// <summary>
    /// Execute a single instruction in both emulators
    /// </summary>
    public void ExecuteInstruction()
    {
        // Execute in Unicorn - execute 1 instruction starting from current EIP
        var startEip = (long)(int)_unicorn.RegRead(X86.UC_X86_REG_EIP);
        _unicorn.EmuStart(startEip, startEip + 15, 0, 1); // up to 15 bytes, 1 instruction
        
        // Execute in Win32Emu
        _win32EmuCpu.SingleStep(_win32EmuMemory);
    }

    /// <summary>
    /// Get a register value from Win32Emu
    /// </summary>
    public uint GetWin32EmuReg(string name)
    {
        return _win32EmuCpu.GetRegister(name);
    }

    /// <summary>
    /// Get a register value from Unicorn
    /// </summary>
    public uint GetUnicornReg(string name)
    {
        var regId = name.ToUpperInvariant() switch
        {
            "EAX" => X86.UC_X86_REG_EAX,
            "EBX" => X86.UC_X86_REG_EBX,
            "ECX" => X86.UC_X86_REG_ECX,
            "EDX" => X86.UC_X86_REG_EDX,
            "ESI" => X86.UC_X86_REG_ESI,
            "EDI" => X86.UC_X86_REG_EDI,
            "EBP" => X86.UC_X86_REG_EBP,
            "ESP" => X86.UC_X86_REG_ESP,
            "EIP" => X86.UC_X86_REG_EIP,
            "EFLAGS" => X86.UC_X86_REG_EFLAGS,
            _ => 0
        };
        
        if (regId == 0)
            return 0;
            
        return (uint)(int)_unicorn.RegRead(regId);
    }

    /// <summary>
    /// Get EFLAGS from Win32Emu
    /// </summary>
    public uint GetWin32EmuFlags()
    {
        return _win32EmuCpu.GetRegister("EFLAGS");
    }

    /// <summary>
    /// Get EFLAGS from Unicorn
    /// </summary>
    public uint GetUnicornFlags()
    {
        return (uint)(int)_unicorn.RegRead(X86.UC_X86_REG_EFLAGS);
    }

    /// <summary>
    /// Check if a specific flag is set in Win32Emu
    /// </summary>
    public bool IsWin32EmuFlagSet(CpuFlag flag)
    {
        return (GetWin32EmuFlags() & (1u << (int)flag)) != 0;
    }

    /// <summary>
    /// Check if a specific flag is set in Unicorn
    /// </summary>
    public bool IsUnicornFlagSet(CpuFlag flag)
    {
        return (GetUnicornFlags() & (1u << (int)flag)) != 0;
    }

    /// <summary>
    /// Write a 32-bit value to memory in both emulators
    /// </summary>
    public void WriteMemory32(uint address, uint value)
    {
        // Write to Win32Emu
        _win32EmuMemory.Write32(address, value);
        
        // Write to Unicorn
        var bytes = BitConverter.GetBytes(value);
        _unicorn.MemWrite((long)address, bytes);
    }

    /// <summary>
    /// Read a 32-bit value from Win32Emu memory
    /// </summary>
    public uint ReadWin32EmuMemory32(uint address)
    {
        return _win32EmuMemory.Read32(address);
    }

    /// <summary>
    /// Read a 32-bit value from Unicorn memory
    /// </summary>
    public uint ReadUnicornMemory32(uint address)
    {
        var bytes = new byte[4];
        _unicorn.MemRead((long)address, bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>
    /// Assert that register values match between Win32Emu and Unicorn
    /// </summary>
    public void AssertRegistersMatch(string registerName)
    {
        var win32EmuValue = GetWin32EmuReg(registerName);
        var unicornValue = GetUnicornReg(registerName);
        
        if (win32EmuValue != unicornValue)
        {
            throw new Exception($"{registerName} mismatch: Win32Emu=0x{win32EmuValue:X8}, Unicorn=0x{unicornValue:X8}");
        }
    }

    /// <summary>
    /// Assert that specific flags match between Win32Emu and Unicorn
    /// </summary>
    public void AssertFlagsMatch(params CpuFlag[] flags)
    {
        foreach (var flag in flags)
        {
            var win32EmuSet = IsWin32EmuFlagSet(flag);
            var unicornSet = IsUnicornFlagSet(flag);
            
            if (win32EmuSet != unicornSet)
            {
                throw new Exception($"{flag} mismatch: Win32Emu={win32EmuSet}, Unicorn={unicornSet}");
            }
        }
    }

    public void Dispose()
    {
        _unicorn?.Close();
        
        if (_win32EmuCpu is IDisposable cpuDisposable)
        {
            cpuDisposable.Dispose();
        }
        if (_win32EmuMemory is IDisposable memoryDisposable)
        {
            memoryDisposable.Dispose();
        }
    }
}
