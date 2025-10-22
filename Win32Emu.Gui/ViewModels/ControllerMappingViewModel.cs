using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.ViewModels;

public partial class ControllerMappingViewModel : ViewModelBase
{
    private readonly ConfigurationService? _configService;
    private readonly EmulatorSettings? _settings;
    private AxisMapping? _currentAxisMapping;
    private ButtonMapping? _currentButtonMapping;

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

    public ControllerMappingViewModel() : this(null, null)
    {
    }

    public ControllerMappingViewModel(ConfigurationService? configService, EmulatorSettings? settings)
    {
        _configService = configService;
        _settings = settings;
        
        // Initialize with default mappings
        UpdateMappings();
        
        // Load physical controllers (placeholder for now - would need input backend access)
        LoadPhysicalControllers();
    }

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

    private void LoadPhysicalControllers()
    {
        // Placeholder: In a full implementation, this would enumerate controllers from the input backend
        // For now, add example controllers
        PhysicalControllers.Clear();
        PhysicalControllers.Add("Xbox Controller (Example)");
        PhysicalControllers.Add("PlayStation Controller (Example)");
        PhysicalControllers.Add("Generic Gamepad (Example)");
    }

    [RelayCommand]
    private void StartMapping(object? parameter)
    {
        if (parameter is AxisMapping axisMapping)
        {
            _currentAxisMapping = axisMapping;
            _currentButtonMapping = null;
            IsMappingMode = true;
            MappingInstructions = $"Move the axis you want to map to '{axisMapping.VirtualAxisName}'";
            
            // In a full implementation, this would:
            // 1. Start polling the input backend for axis changes
            // 2. Detect which physical axis moved the most
            // 3. Assign that axis to the virtual axis
            // 4. Update the mapping display
            // 5. Exit mapping mode
        }
        else if (parameter is ButtonMapping buttonMapping)
        {
            _currentAxisMapping = null;
            _currentButtonMapping = buttonMapping;
            IsMappingMode = true;
            MappingInstructions = $"Press the button you want to map to '{buttonMapping.VirtualButtonName}'";
            
            // In a full implementation, this would:
            // 1. Start polling the input backend for button presses
            // 2. Detect which physical button was pressed
            // 3. Assign that button to the virtual button
            // 4. Update the mapping display
            // 5. Exit mapping mode
        }
    }

    [RelayCommand]
    private void CancelMapping()
    {
        IsMappingMode = false;
        _currentAxisMapping = null;
        _currentButtonMapping = null;
        MappingInstructions = "Select a virtual control and press the corresponding physical input";
    }

    [RelayCommand]
    private void TestController()
    {
        // Open a controller test window to verify mappings
        // In a full implementation, this would:
        // 1. Create a new window showing a visual representation of the virtual controller
        // 2. Poll the physical controller via the input backend
        // 3. Apply the current mappings
        // 4. Display the virtual controller state in real-time
        // 5. Show which buttons are pressed and axis positions
        
        // For now, this is a placeholder that could show a message
        // TODO: Implement controller test window when window management is available
    }

    [RelayCommand]
    private void LoadConfiguration()
    {
        if (_configService == null || _settings == null || string.IsNullOrEmpty(SelectedPhysicalController))
        {
            return;
        }

        // Load the configuration for the selected physical controller
        if (_settings.ControllerConfigurations.TryGetValue(SelectedPhysicalController, out var config))
        {
            // Load preset or custom configuration
            SelectedControllerPreset = config.SelectedPreset;
            
            if (config.SelectedPreset == "Custom" && config.CustomConfiguration != null)
            {
                // Load custom controller settings
                ControllerType = config.CustomConfiguration.Type;
                NumberOfAxes = config.CustomConfiguration.NumberOfAxes;
                NumberOfButtons = config.CustomConfiguration.NumberOfButtons;
                HasPointOfView = config.CustomConfiguration.HasPointOfView;
                CustomControllerName = config.CustomConfiguration.Name;
            }
            
            // Load axis mappings
            foreach (var mapping in AxisMappings)
            {
                if (config.AxisMappings.TryGetValue(mapping.VirtualAxisIndex, out var physicalAxis))
                {
                    mapping.PhysicalAxisIndex = physicalAxis;
                }
            }
            
            // Load button mappings
            foreach (var mapping in ButtonMappings)
            {
                if (config.ButtonMappings.TryGetValue(mapping.VirtualButtonIndex, out var physicalButton))
                {
                    mapping.PhysicalButtonIndex = physicalButton;
                }
            }
        }
    }

    [RelayCommand]
    private void SaveConfiguration()
    {
        if (_configService == null || _settings == null || string.IsNullOrEmpty(SelectedPhysicalController))
        {
            return;
        }

        // Create or update the configuration for the selected physical controller
        var config = new ControllerConfiguration
        {
            PhysicalControllerName = SelectedPhysicalController,
            SelectedPreset = SelectedControllerPreset
        };

        // Save custom configuration if "Custom" is selected
        if (SelectedControllerPreset == "Custom")
        {
            config.CustomConfiguration = new ControllerPreset
            {
                Name = CustomControllerName,
                Type = ControllerType,
                NumberOfAxes = NumberOfAxes,
                NumberOfButtons = NumberOfButtons,
                HasPointOfView = HasPointOfView
            };
        }

        // Save axis mappings
        config.AxisMappings.Clear();
        foreach (var mapping in AxisMappings)
        {
            if (mapping.PhysicalAxisIndex >= 0)
            {
                config.AxisMappings[mapping.VirtualAxisIndex] = mapping.PhysicalAxisIndex;
            }
        }

        // Save button mappings
        config.ButtonMappings.Clear();
        foreach (var mapping in ButtonMappings)
        {
            if (mapping.PhysicalButtonIndex >= 0)
            {
                config.ButtonMappings[mapping.VirtualButtonIndex] = mapping.PhysicalButtonIndex;
            }
        }

        // Update the settings
        _settings.ControllerConfigurations[SelectedPhysicalController] = config;
        
        // Persist to disk
        _configService.SaveEmulatorSettings();
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
