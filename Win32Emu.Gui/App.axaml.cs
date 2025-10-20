using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.ViewModels;
using Win32Emu.Gui.Views;
using Win32Emu.Telemetry;

namespace Win32Emu.Gui;

public class App : Application
{
    private TelemetryService? _telemetryService;
    
    /// <summary>
    /// Global telemetry service instance for the entire Avalonia session
    /// </summary>
    public static TelemetryService? TelemetryService { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Initialize OpenTelemetry based on configuration
            InitializeTelemetry();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            // Clean up telemetry on exit
            desktop.Exit += (s, e) => CleanupTelemetry();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeTelemetry()
    {
        try
        {
            var configService = new ConfigurationService();
            var config = configService.GetEmulatorConfiguration();
            
            // Initialize OpenTelemetry if enabled
            if (config.EnableOpenTelemetry)
            {
                var telemetryConfig = new TelemetryConfig
                {
                    EnableTracing = true,
                    EnableMetrics = true,
                    UseConsoleExporter = config.UseConsoleExporter,
                    UseOtlpExporter = config.UseOtlpExporter,
                    OtlpEndpoint = config.OtlpEndpoint
                };
                
                _telemetryService = new TelemetryService(telemetryConfig);
                TelemetryService = _telemetryService;
            }
        }
        catch (Exception ex)
        {
            // Log the exception to help diagnose telemetry initialization failures
            System.Diagnostics.Debug.WriteLine($"Telemetry initialization failed: {ex}");
            _telemetryService = null;
            TelemetryService = null;
        }
    }
    
    private void CleanupTelemetry()
    {
        _telemetryService?.Dispose();
        _telemetryService = null;
        TelemetryService = null;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}