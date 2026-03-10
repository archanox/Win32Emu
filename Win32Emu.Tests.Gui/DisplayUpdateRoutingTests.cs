using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Win32Emu.Gui.ViewModels;
using Win32Emu.Gui.Views;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Gui;

public class DisplayUpdateRoutingTests
{
	[AvaloniaFact]
	public async Task OnDisplayUpdate_TargetedDialog_UpdatesDialogInsteadOfMainDisplay()
	{
		var viewModel = new EmulatorWindowViewModel();
		var dialogHandle = 0x00012000u;

		await viewModel.OnDialogCreate(new DialogCreateInfo
		{
			Handle = dialogHandle,
			Template = new DialogTemplate
			{
				Title = "DirectDraw Target",
				Width = 64,
				Height = 48,
				Items = []
			}
		});
		await FlushUiThreadAsync();

		var dialogs = GetTrackedWindows<DialogWindow>(viewModel, "_createdDialogs");
		var dialogWindow = Assert.IsType<DialogWindow>(dialogs[dialogHandle]);

		viewModel.OnDisplayUpdate(new DisplayUpdateInfo
		{
			FrameBuffer = CreateFrameBuffer(4, 3),
			Width = 4,
			Height = 3,
			Stride = 16,
			TargetWindowHandle = (IntPtr)(long)dialogHandle
		});
		await FlushUiThreadAsync();

		var contentPanel = dialogWindow.FindControl<Panel>("DialogContentPanel");
		Assert.NotNull(contentPanel);

		var displayImage = contentPanel.Children.OfType<Image>().FirstOrDefault();
		Assert.NotNull(displayImage);
		Assert.NotNull(displayImage.Source);
		Assert.True(displayImage.IsVisible);
		Assert.Equal(4, displayImage.Width);
		Assert.Equal(3, displayImage.Height);
		Assert.Null(viewModel.DisplayBitmap);
		Assert.False(viewModel.HasDisplay);

		dialogWindow.Close();
		await FlushUiThreadAsync();
	}

	[AvaloniaFact]
	public async Task OnDisplayUpdate_UnknownTarget_DoesNotFallbackToMainDisplay()
	{
		var viewModel = new EmulatorWindowViewModel();

		viewModel.OnDisplayUpdate(new DisplayUpdateInfo
		{
			FrameBuffer = CreateFrameBuffer(2, 2),
			Width = 2,
			Height = 2,
			Stride = 8,
			TargetWindowHandle = (IntPtr)0x00ABCDEF
		});
		await FlushUiThreadAsync();

		Assert.Null(viewModel.DisplayBitmap);
		Assert.False(viewModel.HasDisplay);
	}

	private static Dictionary<uint, TWindow> GetTrackedWindows<TWindow>(EmulatorWindowViewModel viewModel, string fieldName)
		where TWindow : Window
	{
		var field = typeof(EmulatorWindowViewModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(field);

		return Assert.IsType<Dictionary<uint, TWindow>>(field.GetValue(viewModel));
	}

	private static byte[] CreateFrameBuffer(int width, int height)
	{
		var bytes = new byte[width * height * 4];
		for (var i = 0; i < bytes.Length; i += 4)
		{
			bytes[i] = 0x11;
			bytes[i + 1] = 0x22;
			bytes[i + 2] = 0x33;
			bytes[i + 3] = 0xFF;
		}

		return bytes;
	}

	private static async Task FlushUiThreadAsync()
	{
		await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
	}
}
