namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents a controller preset configuration
/// Based on Windows Server 2003 DirectInput presets
/// </summary>
public class ControllerPreset
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int NumberOfAxes { get; set; }
    public int NumberOfButtons { get; set; }
    public bool HasPointOfView { get; set; }
    public Dictionary<string, string> AxisMappings { get; set; } = new();
    public Dictionary<string, string> ButtonMappings { get; set; } = new();

    /// <summary>
    /// Standard preset configurations based on Windows Server 2003 DirectInput
    /// </summary>
    public static List<ControllerPreset> StandardPresets { get; } = new()
    {
        new ControllerPreset
        {
            Name = "2-axis, 2-button joystick",
            Type = "Joystick",
            NumberOfAxes = 2,
            NumberOfButtons = 2,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "2-axis, 4-button joystick",
            Type = "Joystick",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "2-button flight yoke",
            Type = "Flight yoke/stick",
            NumberOfAxes = 2,
            NumberOfButtons = 2,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "2-button flight yoke w/throttle",
            Type = "Flight yoke/stick",
            NumberOfAxes = 3,
            NumberOfButtons = 2,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "2-button gamepad",
            Type = "Game pad",
            NumberOfAxes = 2,
            NumberOfButtons = 2,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "3-axis, 2-button joystick",
            Type = "Joystick",
            NumberOfAxes = 3,
            NumberOfButtons = 2,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "3-axis, 4-button joystick",
            Type = "Joystick",
            NumberOfAxes = 3,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "4-button flight yoke",
            Type = "Flight yoke/stick",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "4-button flight yoke w/throttle",
            Type = "Flight yoke/stick",
            NumberOfAxes = 3,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "4-button gamepad",
            Type = "Game pad",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "CH Flightstick",
            Type = "Joystick",
            NumberOfAxes = 3,
            NumberOfButtons = 4,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "CH Flightstick Pro",
            Type = "Joystick",
            NumberOfAxes = 3,
            NumberOfButtons = 4,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "CH Virtual Pilot",
            Type = "Joystick",
            NumberOfAxes = 3,
            NumberOfButtons = 8,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "Gravis Analog Joystick",
            Type = "Joystick",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "Gravis Analog Pro Joystick",
            Type = "Joystick",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "Gravis Gamepad",
            Type = "Game pad",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "Logitech ThunderPad",
            Type = "Game pad",
            NumberOfAxes = 2,
            NumberOfButtons = 10,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "Logitech WingMan",
            Type = "Joystick",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "Logitech WingMan Extreme",
            Type = "Joystick",
            NumberOfAxes = 3,
            NumberOfButtons = 4,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "Logitech WingMan Light",
            Type = "Joystick",
            NumberOfAxes = 2,
            NumberOfButtons = 4,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "Microsoft SideWinder Freestyle Pro",
            Type = "Game pad",
            NumberOfAxes = 2,
            NumberOfButtons = 10,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "Microsoft SideWinder game pad",
            Type = "Game pad",
            NumberOfAxes = 2,
            NumberOfButtons = 10,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "Microsoft SideWinder Precision Pro",
            Type = "Joystick",
            NumberOfAxes = 3,
            NumberOfButtons = 8,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "Thrustmaster Flight Control System",
            Type = "Flight yoke/stick",
            NumberOfAxes = 3,
            NumberOfButtons = 4,
            HasPointOfView = true
        },
        new ControllerPreset
        {
            Name = "Thrustmaster Formula T1/T2 with adapter",
            Type = "Race car controller",
            NumberOfAxes = 2,
            NumberOfButtons = 2,
            HasPointOfView = false
        },
        new ControllerPreset
        {
            Name = "Thrustmaster Formula T1/T2 without adapter",
            Type = "Race car controller",
            NumberOfAxes = 2,
            NumberOfButtons = 2,
            HasPointOfView = false
        }
    };
}
