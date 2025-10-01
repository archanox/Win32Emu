using Win32Emu.Cpu;
using Win32Emu.Cpu.IcedImpl;
using Win32Emu.Debugging;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;

namespace Win32Emu;

public class Emulator
{
    private readonly IEmulatorHost? _host;
    private VirtualMemory? _vm;
    private IcedCpu? _cpu;
    private ProcessEnvironment? _env;
    private Win32Dispatcher? _dispatcher;
    private LoadedImage? _image;
    private bool _debugMode;

    public Emulator(IEmulatorHost? host = null)
    {
        _host = host;
    }

    public void LoadExecutable(string path, bool debugMode = false)
    {
        _debugMode = debugMode;

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        LogDebug($"[Loader] Loading PE: {path}");
        _vm = new VirtualMemory();
        var loader = new PeImageLoader(_vm);
        _image = loader.Load(path);
        LogDebug($"[Loader] Image base=0x{_image.BaseAddress:X8} EntryPoint=0x{_image.EntryPointAddress:X8} Size=0x{_image.ImageSize:X}");
        LogDebug($"[Loader] Imports mapped: {_image.ImportAddressMap.Count}");

        _env = new ProcessEnvironment(_vm, 0x01000000, _host);
        _env.InitializeStrings(path, Array.Empty<string>());

        _cpu = new IcedCpu(_vm);
        _cpu.SetEip(_image.EntryPointAddress);
        _cpu.SetRegister("ESP", 0x00200000);

        _dispatcher = new Win32Dispatcher();

        var kernel32Module = new Kernel32Module(_env, _image.BaseAddress, loader);
        kernel32Module.SetDispatcher(_dispatcher);
        _dispatcher.RegisterModule(kernel32Module);

        _dispatcher.RegisterModule(new User32Module(_env, _image.BaseAddress, loader));
        _dispatcher.RegisterModule(new Gdi32Module(_env, _image.BaseAddress, loader));
        _dispatcher.RegisterModule(new DDrawModule(_env, _image.BaseAddress, loader));
        _dispatcher.RegisterModule(new DSoundModule(_env, _image.BaseAddress, loader));
        _dispatcher.RegisterModule(new DInputModule(_env, _image.BaseAddress, loader));
        _dispatcher.RegisterModule(new WinMMModule(_env, _image.BaseAddress, loader));
        _dispatcher.RegisterModule(new Glide2xModule(_env, _image.BaseAddress, loader));
    }

    public void Run()
    {
        if (_cpu == null || _vm == null || _env == null || _dispatcher == null || _image == null)
        {
            throw new InvalidOperationException("Executable not loaded. Call LoadExecutable first.");
        }

        if (_debugMode)
        {
            RunWithEnhancedDebugging();
        }
        else
        {
            RunNormal();
        }

        var exitMessage = _env.ExitRequested ? "[Exit] Process requested exit." : "[Exit] Instruction limit reached.";
        LogDebug(exitMessage);

        LogDebug("=== Unknown Function Summary ===");
        _dispatcher.PrintUnknownFunctionsSummary();
    }

    private void RunNormal()
    {
        const int maxInstr = 500000;
        for (var i = 0; i < maxInstr && !_env!.ExitRequested; i++)
        {
            var step = _cpu!.SingleStep(_vm!);
            if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
            {
                var dll = imp.dll.ToUpperInvariant();
                var name = imp.name;
                LogDebug($"[Import] {dll}!{name}");
                if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm, out var ret, out var argBytes))
                {
                    LogDebug($"[Import] Returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm.Read32(esp);
                    esp += 4 + (uint)argBytes;
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetEip(retEip);
                }
            }
        }
    }

    private void RunWithEnhancedDebugging()
    {
        var debugger = _cpu!.CreateDebugger(_vm!);
        debugger.EnableSuspiciousRegisterDetection = true;
        debugger.LogToConsole = true;
        debugger.LogAllInstructions = false;
        debugger.SuspiciousThreshold = 0x1000;

        LogDebug("[Debug] Enhanced debugging enabled - will catch 0xFFFFFFFD errors");
        LogDebug("[Debug] Monitoring for suspicious register values");

        const int maxInstr = 500000;
        for (var i = 0; i < maxInstr && !_env!.ExitRequested; i++)
        {
            var currentEip = _cpu.GetEip();

            if (currentEip >= 0x0F000000 && currentEip < 0x10000000)
            {
                LogDebug($"\n[Debug] *** CPU TRYING TO EXECUTE SYNTHETIC IMPORT ADDRESS! ***");
                LogDebug($"[Debug] EIP=0x{currentEip:X8} at instruction {i}");

                if (_image!.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
                {
                    LogDebug($"[Debug] This is import: {importInfo.dll}!{importInfo.name}");
                }
                else
                {
                    LogDebug($"[Debug] Unknown synthetic address - not in import map");
                }

                LogDebug("[Debug] This should now execute an INT3 stub that will be handled as an import call");
            }

            if (debugger.IsProblematicEip(0x0F000512))
            {
                LogDebug($"\n[Debug] *** FOUND PROBLEMATIC EIP AT INSTRUCTION {i} ***");
                debugger.HandleProblematicEip();
                LogDebug("[Debug] Stopping execution to prevent crash");
                break;
            }

            if (_cpu.HasSuspiciousRegisters() && i > 100)
            {
                LogDebug($"[Debug] [Instruction {i}] Suspicious registers detected");
            }

            try
            {
                var wasCall = WillBeCall(_cpu, _vm!);
                var callTarget = wasCall ? GetCallTarget(_cpu, _vm!) : 0u;

                debugger.SafeSingleStep();

                var step = new CpuStepResult(wasCall, callTarget);

                if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
                {
                    var dll = imp.dll.ToUpperInvariant();
                    var name = imp.name;
                    LogDebug($"[Import] {dll}!{name}");
                    if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm, out var ret, out var argBytes))
                    {
                        LogDebug($"[Import] Returned 0x{ret:X8}");
                        var esp = _cpu.GetRegister("ESP");
                        var retEip = _vm.Read32(esp);
                        esp += 4 + (uint)argBytes;
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetEip(retEip);
                    }
                }
            }
            catch (IndexOutOfRangeException ex) when (ex.Message.Contains("0xFFFFFFFD") || ex.Message.Contains("0xFFFFFFFF"))
            {
                LogDebug($"\n[Debug] *** CAUGHT MEMORY ACCESS VIOLATION AT INSTRUCTION {i} ***");
                LogDebug($"[Debug] Exception: {ex.Message}");

                if (currentEip >= 0x0F000000 && currentEip < 0x10000000)
                {
                    LogDebug($"[Debug] ERROR CAUSE: Trying to execute synthetic import address 0x{currentEip:X8}");
                    if (_image!.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
                    {
                        LogDebug($"[Debug] This is import: {importInfo.dll}!{importInfo.name}");
                    }
                    LogDebug($"[Debug] SOLUTION: The program should CALL THROUGH the IAT, not execute the import address directly");
                }

                var trace = debugger.GetExecutionTrace();
                var suspiciousStates = debugger.FindSuspiciousStates();

                LogDebug($"[Debug] Execution trace has {trace.Count} entries");
                LogDebug($"[Debug] Found {suspiciousStates.Count} suspicious register states");

                if (suspiciousStates.Count > 0)
                {
                    var first = suspiciousStates.First();
                    LogDebug("[Debug] First suspicious state occurred at:");
                    LogDebug($"[Debug]   EIP=0x{first.EIP:X8} EBP=0x{first.EBP:X8} ESP=0x{first.ESP:X8}");
                }

                throw;
            }
            catch (Exception ex)
            {
                LogDebug($"[Debug] Unexpected exception at instruction {i}: {ex}");
                throw;
            }
        }

        var finalTrace = debugger.GetExecutionTrace();
        var finalSuspicious = debugger.FindSuspiciousStates();
        LogDebug($"[Debug] Final execution summary:");
        LogDebug($"[Debug]   Total traced instructions: {finalTrace.Count}");
        LogDebug($"[Debug]   Suspicious register states: {finalSuspicious.Count}");
    }

    private static bool WillBeCall(IcedCpu cpu, VirtualMemory vm)
    {
        try
        {
            var eip = cpu.GetEip();
            var opcode = vm.Read8(eip);

            if (opcode == 0xE8 || (opcode == 0xFF && IsCallVariant(vm, eip)))
            {
                return true;
            }

            if (opcode == 0xCC && eip >= 0x0F000000 && eip < 0x10000000)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsCallVariant(VirtualMemory vm, uint eip)
    {
        try
        {
            var modRm = vm.Read8(eip + 1);
            var reg = (modRm >> 3) & 0x07;
            return reg == 2;
        }
        catch
        {
            return false;
        }
    }

    private static uint GetCallTarget(IcedCpu cpu, VirtualMemory vm)
    {
        try
        {
            var eip = cpu.GetEip();
            var opcode = vm.Read8(eip);

            if (opcode == 0xE8)
            {
                var displacement = vm.Read32(eip + 1);
                return (uint)(eip + 5 + (int)displacement);
            }
            else if (opcode == 0xCC && eip >= 0x0F000000 && eip < 0x10000000)
            {
                return eip;
            }
        }
        catch
        {
            // If we can't decode, return 0
        }

        return 0;
    }

    private void LogDebug(string message)
    {
        if (_host != null)
        {
            _host.OnDebugOutput(message, Win32Emu.DebugLevel.Debug);
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}

public interface IEmulatorHost
{
    void OnDebugOutput(string message, DebugLevel level);
    void OnStdOutput(string output);
    void OnWindowCreate(WindowCreateInfo info);
}

public enum DebugLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error
}

public class WindowCreateInfo
{
    public required uint Handle { get; init; }
    public required string Title { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public required string ClassName { get; init; }
    public uint Style { get; init; }
    public uint ExStyle { get; init; }
    public uint Parent { get; init; }
}
