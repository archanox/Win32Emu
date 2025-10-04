namespace Win32Emu.Cpu
{
	public readonly record struct CpuStepResult(bool IsCall, uint CallTarget);
}