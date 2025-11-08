namespace Win32Emu.Win32;

/// <summary>
/// Marks a struct to have a corresponding ref struct generated for memory-mapped access.
/// The generator will create a {StructName}Ref type with properties that read/write
/// directly to memory addresses.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Struct)]
public class GenerateMemoryRefAttribute : System.Attribute
{
}
