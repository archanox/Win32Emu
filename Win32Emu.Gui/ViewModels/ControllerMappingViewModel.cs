using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.ViewModels;

public partial class ControllerMappingViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _selectedControllerPreset = "Custom";

    [ObservableProperty]
    private bool _isCustomSelected = true;

    [ObservableProperty]
    private string _controllerType = "Joystick";

    [ObservableProperty]
    private int _numberOfAxes = 2;

    [ObservableProperty]
    private int _numberOfButtons = 2;

    [ObservableProperty]
    private bool _hasPointOfView;

    [ObservableProperty]
    private string _customControllerName = "My Controller";

    [ObservableProperty]
    private string? _selectedPhysicalController;

    [ObservableProperty]
    private bool _isMappingMode;

    [ObservableProperty]
    private string _mappingInstructions = "Select a virtual control and press the corresponding physical input";

    public ObservableCollection<string> PhysicalControllers { get; } = new();
    public ObservableCollection<AxisMapping> AxisMappings { get; } = new();
    public ObservableCollection<ButtonMapping> ButtonMappings { get; } = new();

    public ObservableCollection<string> ControllerPresets { get; } = new()
    {
        "2-axis, 2-button joystick",
        "2-axis, 4-button joystick",
        "2-button flight yoke",
        "2-button flight yoke w/throttle",
        "2-button gamepad",
        "3-axis, 2-button joystick",
        "3-axis, 4-button joystick",
        "4-button flight yoke",
        "4-button flight yoke w/throttle",
        "4-button gamepad",
        "CH Flightstick",
        "CH Flightstick Pro",
        "CH Virtual Pilot",
        "Gravis Analog Joystick",
        "Gravis Analog Pro Joystick",
        "Gravis Gamepad",
        "Logitech ThunderPad",
        "Logitech WingMan",
        "Logitech WingMan Extreme",
        "Logitech WingMan Light",
        "Microsoft SideWinder Freestyle Pro",
        "Microsoft SideWinder game pad",
        "Microsoft SideWinder Precision Pro",
        "Thrustmaster Flight Control System",
        "Thrustmaster Formula T1/T2 with adapter",
        "Thrustmaster Formula T1/T2 without adapter",
        "Custom"
    };

    public ObservableCollection<string> ControllerTypes { get; } = new()
    {
        "Joystick",
        "Flight yoke/stick",
        "Game pad",
        "Race car controller"
    };

    public ObservableCollection<int> AxisOptions { get; } = new() { 2, 3, 4 };
    public ObservableCollection<int> ButtonOptions { get; } = new() { 0, 1, 2, 3, 4 };

    partial void OnSelectedControllerPresetChanged(string value)
    {
        // Update custom settings visibility based on preset
        IsCustomSelected = value == "Custom";

        // Load preset configuration
        if (value != "Custom")
        {
            var preset = ControllerPreset.StandardPresets.FirstOrDefault(p => p.Name == value);
            if (preset != null)
            {
                ControllerType = preset.Type;
                NumberOfAxes = preset.NumberOfAxes;
                NumberOfButtons = preset.NumberOfButtons;
                HasPointOfView = preset.HasPointOfView;
                UpdateMappings();
            }
        }
    }

    partial void OnNumberOfAxesChanged(int value)
    {
        if (IsCustomSelected)
        {
            UpdateMappings();
        }
    }

    partial void OnNumberOfButtonsChanged(int value)
    {
        if (IsCustomSelected)
        {
            UpdateMappings();
        }
    }

    private void UpdateMappings()
    {
        AxisMappings.Clear();
        ButtonMappings.Clear();

        for (var i = 0; i < NumberOfAxes; i++)
        {
            AxisMappings.Add(new AxisMapping
            {
                VirtualAxisIndex = i,
                VirtualAxisName = GetAxisName(i),
                PhysicalAxisIndex = -1
            });
        }

        for (var i = 0; i < NumberOfButtons; i++)
        {
            ButtonMappings.Add(new ButtonMapping
            {
                VirtualButtonIndex = i,
                VirtualButtonName = $"Button {i + 1}",
                PhysicalButtonIndex = -1
            });
        }
    }

    private static string GetAxisName(int index)
    {
        return index switch
        {
            0 => "X Axis",
            1 => "Y Axis",
            2 => "Z Axis (Throttle)",
            3 => "Rx Axis (Rudder)",
            _ => $"Axis {index}"
        };
    }

    [RelayCommand]
    private void StartMapping(object? parameter)
    {
        if (parameter is AxisMapping axisMapping)
        {
            IsMappingMode = true;
            MappingInstructions = $"Move the axis you want to map to '{axisMapping.VirtualAxisName}'";
            // TODO: Start listening for axis input
        }
        else if (parameter is ButtonMapping buttonMapping)
        {
            IsMappingMode = true;
            MappingInstructions = $"Press the button you want to map to '{buttonMapping.VirtualButtonName}'";
            // TODO: Start listening for button input
        }
    }

    [RelayCommand]
    private void CancelMapping()
    {
        IsMappingMode = false;
        MappingInstructions = "Select a virtual control and press the corresponding physical input";
    }

    [RelayCommand]
    private void TestController()
    {
        // TODO: Open controller test window
    }

    [RelayCommand]
    private void LoadConfiguration()
    {
        // TODO: Load configuration from settings
    }

    [RelayCommand]
    private void SaveConfiguration()
    {
        // TODO: Save configuration to settings
    }
}

/// <summary>
/// Represents an axis mapping
/// </summary>
public class AxisMapping
{
    public int VirtualAxisIndex { get; set; }
    public string VirtualAxisName { get; set; } = string.Empty;
    public int PhysicalAxisIndex { get; set; }
}

/// <summary>
/// Represents a button mapping
/// </summary>
public class ButtonMapping
{
    public int VirtualButtonIndex { get; set; }
    public string VirtualButtonName { get; set; } = string.Empty;
    public int PhysicalButtonIndex { get; set; }
}
