using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Services;
using Win32Emu.Gui.ViewModels;
using Win32Emu.Gui.Views;
using Win32Emu.Telemetry;

namespace Win32Emu.Gui;

public class App : Application
{
    private TelemetryService? _telemetryService;
    private LoggingService? _loggingService;
    
    /// <summary>
    /// Global telemetry service instance for the entire Avalonia session
    /// </summary>
    public static TelemetryService? TelemetryService { get; private set; }
    
    /// <summary>
    /// Global logging service instance for the entire Avalonia session
    /// </summary>
    public static LoggingService? LoggingService { get; private set; }

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
            
            // Initialize logging first so it's available for telemetry and other services
            InitializeLogging();
            
            // Initialize OpenTelemetry based on configuration
            InitializeTelemetry();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            // Clean up services on exit
            desktop.Exit += (s, e) => CleanupServices();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeLogging()
    {
        try
        {
            var configService = new ConfigurationService();
            var config = configService.GetEmulatorConfiguration();
            
            _loggingService = new LoggingService(config);
            LoggingService = _loggingService;
            
            var logger = _loggingService.CreateLogger<App>();
            logger.LogInformation("Win32Emu.Gui logging initialized");
        }
        catch (Exception ex)
        {
            // Log the exception to help diagnose logging initialization failures
            System.Diagnostics.Debug.WriteLine($"Logging initialization failed: {ex}");
            Console.WriteLine($"Logging initialization failed: {ex}");
            _loggingService = null;
            LoggingService = null;
        }
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
                
                // Log telemetry initialization using the logging service
                if (LoggingService != null)
                {
                    var logger = LoggingService.CreateLogger<App>();
                    logger.LogInformation("OpenTelemetry initialized - Console: {Console}, OTLP: {Otlp}",
                        config.UseConsoleExporter, config.UseOtlpExporter);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception to help diagnose telemetry initialization failures
            System.Diagnostics.Debug.WriteLine($"Telemetry initialization failed: {ex}");
            
            if (LoggingService != null)
            {
                var logger = LoggingService.CreateLogger<App>();
                logger.LogWarning(ex, "Telemetry initialization failed");
            }
            
            _telemetryService = null;
            TelemetryService = null;
        }
    }
    
    private void CleanupServices()
    {
        _telemetryService?.Dispose();
        _telemetryService = null;
        TelemetryService = null;
        
        _loggingService?.Dispose();
        _loggingService = null;
        LoggingService = null;
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