namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents a controller mapping configuration
/// </summary>
public class ControllerConfiguration
{
    /// <summary>
    /// Physical controller ID from the backend
    /// </summary>
    public uint PhysicalControllerId { get; set; }

    /// <summary>
    /// Name of the physical controller
    /// </summary>
    public string PhysicalControllerName { get; set; } = string.Empty;

    /// <summary>
    /// Selected preset name (or "Custom")
    /// </summary>
    public string SelectedPreset { get; set; } = "Custom";

    /// <summary>
    /// Custom controller configuration (used when SelectedPreset is "Custom")
    /// </summary>
    public ControllerPreset? CustomConfiguration { get; set; }

    /// <summary>
    /// Axis mappings from physical axis to virtual axis
    /// Key: virtual axis index, Value: physical axis index
    /// </summary>
    public Dictionary<int, int> AxisMappings { get; set; } = new();

    /// <summary>
    /// Button mappings from physical button to virtual button
    /// Key: virtual button index, Value: physical button index
    /// </summary>
    public Dictionary<int, int> ButtonMappings { get; set; } = new();

    /// <summary>
    /// POV hat mapping
    /// </summary>
    public int PovHatMapping { get; set; } = -1; // -1 means not mapped

    /// <summary>
    /// Get the effective preset (either from StandardPresets or CustomConfiguration)
    /// </summary>
    public ControllerPreset? GetEffectivePreset()
    {
        if (SelectedPreset == "Custom")
        {
            return CustomConfiguration;
        }

        return ControllerPreset.StandardPresets.FirstOrDefault(p => p.Name == SelectedPreset);
    }
}
