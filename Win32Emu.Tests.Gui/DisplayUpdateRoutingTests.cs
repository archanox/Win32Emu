using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
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

	[AvaloniaFact]
	public async Task OnDisplayUpdate_MainDisplay_RerendersWhenUpdatingExistingBitmap()
	{
		var viewModel = new EmulatorWindowViewModel
		{
			ShowDebugPanel = false,
			ShowStdOutputPanel = false
		};
		var window = new EmulatorWindow
		{
			DataContext = viewModel,
			Width = 800,
			Height = 600
		};
		viewModel.SetOwnerWindow(window);

		window.Show();
		await FlushUiThreadAsync();
		var renderer = GetRenderer(window);
		var sceneInvalidations = 0;
		var invalidationHandler = SubscribeToSceneInvalidated(renderer, () => sceneInvalidations++);

		try
		{
			viewModel.OnDisplayUpdate(new DisplayUpdateInfo
			{
				FrameBuffer = CreateSolidFrameBuffer(320, 240, 0x10, 0x40, 0x80),
				Width = 320,
				Height = 240,
				Stride = 320 * 4
			});
			await FlushUiThreadAsync();
			await ForceRenderAsync();
			Assert.NotNull(viewModel.DisplayBitmap);
			var displayBitmap = viewModel.DisplayBitmap;

			sceneInvalidations = 0;
			viewModel.OnDisplayUpdate(new DisplayUpdateInfo
			{
				FrameBuffer = CreateSolidFrameBuffer(320, 240, 0xD0, 0x20, 0x40),
				Width = 320,
				Height = 240,
				Stride = 320 * 4
			});
			await FlushUiThreadAsync();
			await ForceRenderAsync();

			Assert.Same(displayBitmap, viewModel.DisplayBitmap);
			Assert.True(sceneInvalidations > 0);
		}
		finally
		{
			UnsubscribeFromSceneInvalidated(renderer, invalidationHandler);
		}

		window.Close();
		await FlushUiThreadAsync();
	}

	[AvaloniaFact]
	public async Task OnDisplayUpdate_TargetedWindow_RerendersWhenUpdatingExistingBitmap()
	{
		var viewModel = new EmulatorWindowViewModel();
		var windowHandle = 0x00014000u;

		viewModel.OnWindowCreate(new WindowCreateInfo
		{
			Handle = windowHandle,
			Title = "DirectDraw Window",
			Width = 320,
			Height = 240,
			ClassName = "DirectDrawWindow",
			X = 0,
			Y = 0
		});
		await FlushUiThreadAsync();

		var windows = GetTrackedWindows<Window>(viewModel, "_createdWindows");
		var window = windows[windowHandle];
		var renderer = GetRenderer(window);
		var sceneInvalidations = 0;
		var invalidationHandler = SubscribeToSceneInvalidated(renderer, () => sceneInvalidations++);

		try
		{
			viewModel.OnDisplayUpdate(new DisplayUpdateInfo
			{
				FrameBuffer = CreateSolidFrameBuffer(320, 240, 0x10, 0x40, 0x80),
				Width = 320,
				Height = 240,
				Stride = 320 * 4,
				TargetWindowHandle = (IntPtr)(long)windowHandle
			});
			await FlushUiThreadAsync();
			await ForceRenderAsync();

			var bitmaps = GetTrackedBitmaps(viewModel);
			Assert.True(bitmaps.TryGetValue(windowHandle, out var displayBitmap));

			sceneInvalidations = 0;
			viewModel.OnDisplayUpdate(new DisplayUpdateInfo
			{
				FrameBuffer = CreateSolidFrameBuffer(320, 240, 0xD0, 0x20, 0x40),
				Width = 320,
				Height = 240,
				Stride = 320 * 4,
				TargetWindowHandle = (IntPtr)(long)windowHandle
			});
			await FlushUiThreadAsync();
			await ForceRenderAsync();

			Assert.Same(displayBitmap, bitmaps[windowHandle]);
			Assert.True(sceneInvalidations > 0);
		}
		finally
		{
			UnsubscribeFromSceneInvalidated(renderer, invalidationHandler);
			window.Close();
			await FlushUiThreadAsync();
		}
	}

	[AvaloniaFact]
	public async Task OnDialogEnd_RemovesTrackedDialogBitmap()
	{
		var viewModel = new EmulatorWindowViewModel();
		var dialogHandle = 0x00013000u;

		await viewModel.OnDialogCreate(new DialogCreateInfo
		{
			Handle = dialogHandle,
			Template = new DialogTemplate
			{
				Title = "Cleanup Dialog",
				Width = 64,
				Height = 48,
				Items = []
			}
		});
		await FlushUiThreadAsync();

		viewModel.OnDisplayUpdate(new DisplayUpdateInfo
		{
			FrameBuffer = CreateFrameBuffer(4, 3),
			Width = 4,
			Height = 3,
			Stride = 16,
			TargetWindowHandle = (IntPtr)(long)dialogHandle
		});
		await FlushUiThreadAsync();

		var dialogs = GetTrackedWindows<DialogWindow>(viewModel, "_createdDialogs");
		var bitmaps = GetTrackedBitmaps(viewModel);

		Assert.Contains(dialogHandle, dialogs.Keys);
		Assert.True(bitmaps.ContainsKey(dialogHandle));

		viewModel.OnDialogEnd(dialogHandle, 1);
		await FlushUiThreadAsync();
		// EndDialog posts Close(), and the dialog Closing handler performs the bitmap cleanup on the UI thread.
		await FlushUiThreadAsync();

		Assert.DoesNotContain(dialogHandle, dialogs.Keys);
		Assert.False(bitmaps.ContainsKey(dialogHandle));
	}

	private static Dictionary<uint, TWindow> GetTrackedWindows<TWindow>(EmulatorWindowViewModel viewModel, string fieldName)
		where TWindow : Window
	{
		var field = typeof(EmulatorWindowViewModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(field);

		return Assert.IsType<Dictionary<uint, TWindow>>(field.GetValue(viewModel));
	}

	private static Dictionary<uint, WriteableBitmap> GetTrackedBitmaps(EmulatorWindowViewModel viewModel)
	{
		var field = typeof(EmulatorWindowViewModel).GetField("_windowBitmaps", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(field);

		return Assert.IsType<Dictionary<uint, WriteableBitmap>>(field.GetValue(viewModel));
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

	private static byte[] CreateSolidFrameBuffer(int width, int height, byte redByte, byte greenByte, byte blueByte)
	{
		var bytes = new byte[width * height * 4];
		for (var i = 0; i < bytes.Length; i += 4)
		{
			bytes[i] = redByte;
			bytes[i + 1] = greenByte;
			bytes[i + 2] = blueByte;
			bytes[i + 3] = 0xFF;
		}

		return bytes;
	}

	private static IRenderer GetRenderer(Window window)
	{
		return Assert.IsAssignableFrom<IRenderer>(((IRenderRoot)window).Renderer);
	}

	private static EventHandler<SceneInvalidatedEventArgs> SubscribeToSceneInvalidated(IRenderer renderer, Action onSceneInvalidated)
	{
		EventHandler<SceneInvalidatedEventArgs> handler = (_, _) => onSceneInvalidated();
		GetSceneInvalidatedEvent().AddEventHandler(renderer, handler);
		return handler;
	}

	private static void UnsubscribeFromSceneInvalidated(IRenderer renderer, EventHandler<SceneInvalidatedEventArgs> handler)
	{
		GetSceneInvalidatedEvent().RemoveEventHandler(renderer, handler);
	}

	private static EventInfo GetSceneInvalidatedEvent()
	{
		// Avalonia's renderer event isn't directly subscribable in this test environment, so use EventInfo.
		var sceneInvalidatedEvent = typeof(IRenderer).GetEvent("SceneInvalidated");
		Assert.NotNull(sceneInvalidatedEvent);
		return sceneInvalidatedEvent;
	}

	private static async Task FlushUiThreadAsync()
	{
		await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
	}

	private static async Task ForceRenderAsync()
	{
		await Dispatcher.UIThread.InvokeAsync(() => AvaloniaHeadlessPlatform.ForceRenderTimerTick(1));
	}
}
