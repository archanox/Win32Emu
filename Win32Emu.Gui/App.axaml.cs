using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Win32Emu.Gui.Backends.DirectDraw;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Services;
using Win32Emu.Gui.ViewModels;
using Win32Emu.Gui.Views;
using Win32Emu.Telemetry;
using Win32Emu.Win32.DirectDraw;

namespace Win32Emu.Gui;

public class App : Application
{
    private TelemetryService? _telemetryService;
    private LoggingService? _loggingService;
    private McpServerHost? _mcpServerHost;
    
    /// <summary>
    /// Global telemetry service instance for the entire Avalonia session
    /// </summary>
    public static TelemetryService? TelemetryService { get; private set; }
    
    /// <summary>
    /// Global logging service instance for the entire Avalonia session
    /// </summary>
    public static LoggingService? LoggingService { get; private set; }
    
    /// <summary>
    /// Global MCP server host for AI-assisted debugging throughout the application lifecycle
    /// </summary>
    public static McpServerHost? McpServerHost { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Initialize the optimized SIMD blitter for desktop platforms
        InitializeBlitter();
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
            
            // Initialize MCP server for AI-assisted debugging if enabled
            InitializeMcpServer();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            // Clean up services on exit
            desktop.Exit += (s, e) => CleanupServices();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitializeBlitter()
    {
        // Use the optimized SIMD blitter for desktop platforms (SSE2/AVX2/NEON)
        OptimizedBlitter.Current = SimdBlitter.Instance;
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
    
    private void InitializeMcpServer()
    {
        try
        {
            var configService = new ConfigurationService();
            var config = configService.GetEmulatorConfiguration();
            
            // Initialize MCP server if enabled
            if (config.EnableMcpServer || config.AutoStartMcpServer)
            {
                if (LoggingService != null)
                {
                    var logger = LoggingService.CreateLogger<App>();
                    logger.LogInformation("[MCP] Initializing server at application startup for AI-assisted debugging");
                    
                    // Create a temporary EmulatorService to pass to McpServerHost
                    // This allows AI to interact with the app before any emulation session starts
                    var emulatorService = new EmulatorService(config, null, logger);
                    
                    _mcpServerHost = new McpServerHost(config, emulatorService, logger);
                    McpServerHost = _mcpServerHost;
                    
                    // Start the server asynchronously
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _mcpServerHost.StartAsync();
                            var transportType = config.McpUseHttpTransport ? "HTTP" : "STDIO";
                            var endpoint = config.McpUseHttpTransport 
                                ? $"http://127.0.0.1:{config.McpHttpPort}" 
                                : "STDIO";
                            logger.LogInformation("[MCP] Server started successfully using {Transport} transport at {Endpoint} - AI assistants can now connect", 
                                transportType, endpoint);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "[MCP] Failed to start server");
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception to help diagnose MCP initialization failures
            System.Diagnostics.Debug.WriteLine($"MCP server initialization failed: {ex}");
            
            if (LoggingService != null)
            {
                var logger = LoggingService.CreateLogger<App>();
                logger.LogWarning(ex, "[MCP] Server initialization failed");
            }
            
            _mcpServerHost = null;
            McpServerHost = null;
        }
    }
    
    private void CleanupServices()
    {
        _mcpServerHost?.StopAsync().GetAwaiter().GetResult();
        _mcpServerHost?.Dispose();
        _mcpServerHost = null;
        McpServerHost = null;
        
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