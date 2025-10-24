using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UnicornEngine;
using UnicornEngine.Const;
using Win32Emu.Memory;

namespace Win32Emu.Cpu.Unicorn;

/// <summary>
/// Unicorn Engine-based CPU emulator wrapper that implements the IAsyncCpu interface.
/// This provides a reference implementation for testing and validation purposes.
/// </summary>
public class UnicornCpu : IAsyncCpu
{
	private readonly VirtualMemory _mem;
	private readonly ILogger _logger;
	private readonly UnicornEngine.Unicorn _unicorn;
	
	// Track current register state
	private uint _eip;

	public UnicornCpu(VirtualMemory mem, ILogger? logger = null)
	{
		_mem = mem;
		_logger = logger ?? NullLogger.Instance;
		
		// Initialize Unicorn emulator for x86 32-bit
		_unicorn = new UnicornEngine.Unicorn(Common.UC_ARCH_X86, Common.UC_MODE_32);
		
		// Map the entire virtual memory space that Win32Emu uses
		// We'll map in 1GB chunks to cover the typical address space
		const long mapSize = 0x40000000; // 1GB
		_unicorn.MemMap(0, mapSize, Common.UC_PROT_ALL);
		
		_logger.LogInformation("[UnicornCpu] Initialized Unicorn CPU backend");
	}

	public void SetEip(uint eip)
	{
		_eip = eip;
		_unicorn.RegWrite(X86.UC_X86_REG_EIP, (int)eip);
	}

	public uint GetEip()
	{
		_eip = (uint)_unicorn.RegRead(X86.UC_X86_REG_EIP);
		return _eip;
	}

	public uint GetRegister(string name)
	{
		var regId = GetUnicornRegister(name);
		if (regId == -1)
			return 0;
		
		return (uint)_unicorn.RegRead(regId);
	}

	public void SetRegister(string name, uint value)
	{
		var regId = GetUnicornRegister(name);
		if (regId == -1)
			return;
		
		_unicorn.RegWrite(regId, (int)value);
	}

	public CpuStepResult SingleStep(VirtualMemory mem)
	{
		// Sync memory from Win32Emu to Unicorn before execution
		SyncMemoryToUnicorn();
		
		var currentEip = GetEip();
		
		try
		{
			// Execute single instruction
			_unicorn.EmuStart(currentEip, 0xFFFFFFFF, 0, 1);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[UnicornCpu] Emulation error at EIP=0x{Eip:X8}", currentEip);
		}
		
		// Sync memory from Unicorn back to Win32Emu
		SyncMemoryFromUnicorn();
		
		// Check if this was a call instruction
		// For simplicity, we'll return false for isCall - full implementation would decode the instruction
		return new CpuStepResult(false, 0);
	}

	public Task<CpuStepResult> SingleStepAsync(VirtualMemory mem)
	{
		var result = SingleStep(mem);
		return Task.FromResult(result);
	}

	public Task<CpuStepResult> ExecuteBlockAsync(VirtualMemory mem)
	{
		// For Unicorn, we'll just execute single steps in a loop until we hit a call
		// This is a simplified implementation
		return SingleStepAsync(mem);
	}

	public bool SupportsJit => false;

	public CpuState SaveState()
	{
		return new CpuState
		{
			Eax = GetRegister("EAX"),
			Ebx = GetRegister("EBX"),
			Ecx = GetRegister("ECX"),
			Edx = GetRegister("EDX"),
			Esi = GetRegister("ESI"),
			Edi = GetRegister("EDI"),
			Ebp = GetRegister("EBP"),
			Esp = GetRegister("ESP"),
			Eip = GetEip(),
			Eflags = GetRegister("EFLAGS")
		};
	}

	public void RestoreState(CpuState state)
	{
		SetRegister("EAX", state.Eax);
		SetRegister("EBX", state.Ebx);
		SetRegister("ECX", state.Ecx);
		SetRegister("EDX", state.Edx);
		SetRegister("ESI", state.Esi);
		SetRegister("EDI", state.Edi);
		SetRegister("EBP", state.Ebp);
		SetRegister("ESP", state.Esp);
		SetEip(state.Eip);
		SetRegister("EFLAGS", state.Eflags);
	}

	private void SyncMemoryToUnicorn()
	{
		// Sync critical memory regions from Win32Emu to Unicorn
		// This is a simplified version - a full implementation would track dirty pages
		
		// Sync stack region (typical stack area)
		SyncMemoryRegion(0x00100000, 0x00200000);
		
		// Sync heap/data region
		SyncMemoryRegion(0x00200000, 0x01000000);
		
		// Sync code region
		SyncMemoryRegion(0x00400000, 0x10000000);
	}

	private void SyncMemoryFromUnicorn()
	{
		// Sync memory changes from Unicorn back to Win32Emu
		// This is a simplified version
		
		// Sync stack region
		SyncMemoryRegionFromUnicorn(0x00100000, 0x00200000);
		
		// Sync heap/data region  
		SyncMemoryRegionFromUnicorn(0x00200000, 0x01000000);
	}

	private void SyncMemoryRegion(uint startAddr, uint endAddr)
	{
		try
		{
			var size = Math.Min((int)(endAddr - startAddr), 0x100000); // Limit to 1MB chunks
			var buffer = new byte[size];
			
			// Read from Win32Emu memory
			for (var i = 0; i < size; i++)
			{
				buffer[i] = _mem.Read8(startAddr + (uint)i);
			}
			
			// Write to Unicorn memory
			_unicorn.MemWrite(startAddr, buffer);
		}
		catch
		{
			// Ignore memory sync errors - regions may not be mapped
		}
	}

	private void SyncMemoryRegionFromUnicorn(uint startAddr, uint endAddr)
	{
		try
		{
			var size = Math.Min((int)(endAddr - startAddr), 0x100000); // Limit to 1MB chunks
			var buffer = new byte[size];
			_unicorn.MemRead(startAddr, buffer);
			
			// Write to Win32Emu memory
			for (var i = 0; i < buffer.Length; i++)
			{
				_mem.Write8(startAddr + (uint)i, buffer[i]);
			}
		}
		catch
		{
			// Ignore memory sync errors
		}
	}

	private static int GetUnicornRegister(string name)
	{
		return name.ToUpperInvariant() switch
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
			_ => -1
		};
	}
}
