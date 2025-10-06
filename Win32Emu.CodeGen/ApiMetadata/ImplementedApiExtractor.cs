using System.Reflection;

namespace Win32Emu.CodeGen.ApiMetadata;

/// <summary>
/// Utility to extract implemented API information from the Win32Emu assembly
/// by reading the generated StdCallMeta and DllModuleExportInfo classes
/// </summary>
public class ImplementedApiExtractor
{
    /// <summary>
    /// Extract all implemented APIs from the Win32Emu assembly
    /// </summary>
    /// <param name="assemblyPath">Path to Win32Emu.dll or Win32Emu.exe</param>
    /// <returns>Dictionary of DLL name -> (function name -> arg bytes)</returns>
    public static Dictionary<string, Dictionary<string, int>> ExtractFromAssembly(string assemblyPath)
    {
        var result = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        
        if (!File.Exists(assemblyPath))
        {
            Console.WriteLine($"Warning: Assembly not found: {assemblyPath}");
            return result;
        }
        
        try
        {
            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            // Find the DllModuleExportInfo class
            var exportInfoType = assembly.GetType("Win32Emu.Win32.DllModuleExportInfo");
            if (exportInfoType == null)
            {
                Console.WriteLine("Warning: DllModuleExportInfo class not found in assembly");
                return result;
            }
            
            // Find the GetAllExports method
            var getAllExportsMethod = exportInfoType.GetMethod("GetAllExports", 
                BindingFlags.Public | BindingFlags.Static);
            
            if (getAllExportsMethod == null)
            {
                Console.WriteLine("Warning: GetAllExports method not found");
                return result;
            }
            
            // Find the StdCallMeta class for arg bytes
            var stdCallMetaType = assembly.GetType("Win32Emu.Win32.StdCallMeta");
            var getArgBytesMethod = stdCallMetaType?.GetMethod("GetArgBytes",
                BindingFlags.Public | BindingFlags.Static);
            
            // We need to discover which DLLs are implemented
            // We'll try common DLL names
            var commonDlls = new[]
            {
                "KERNEL32.DLL", "USER32.DLL", "GDI32.DLL", "WINMM.DLL",
                "DPLAYX.DLL", "DSOUND.DLL", "DINPUT.DLL", "DDRAW.DLL",
                "GLIDE2X.DLL", "DPLAY.DLL", "DINPUT8.DLL"
            };
            
            foreach (var dllName in commonDlls)
            {
                try
                {
                    var exports = getAllExportsMethod.Invoke(null, new object[] { dllName });
                    if (exports is Dictionary<string, uint> exportDict && exportDict.Count > 0)
                    {
                        var dllExports = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        
                        foreach (var (funcName, ordinal) in exportDict)
                        {
                            // Try to get arg bytes
                            int argBytes = 0;
                            if (getArgBytesMethod != null)
                            {
                                try
                                {
                                    argBytes = (int)getArgBytesMethod.Invoke(null, new object[] { dllName, funcName })!;
                                }
                                catch
                                {
                                    // If GetArgBytes throws, assume 0
                                    argBytes = 0;
                                }
                            }
                            
                            dllExports[funcName] = argBytes;
                        }
                        
                        result[dllName] = dllExports;
                    }
                }
                catch
                {
                    // DLL not implemented, skip
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading assembly: {ex.Message}");
        }
        
        return result;
    }
}
