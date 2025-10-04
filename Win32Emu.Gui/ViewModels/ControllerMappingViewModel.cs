using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Win32Emu.Gui.ViewModels
{
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
		}
	}
}
