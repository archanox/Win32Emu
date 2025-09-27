using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Win32Emu.Logging;

/// <summary>
/// Configuration for OpenTelemetry integration in Win32Emu
/// </summary>
public static class OpenTelemetryConfig
{
    public const string ServiceName = "Win32Emu";
    public const string ServiceVersion = "1.0.0";

    /// <summary>
    /// Configure OpenTelemetry services
    /// </summary>
    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, bool enableHttpExport = false, string? otlpEndpoint = null)
    {
        // Configure resource information
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, ServiceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = "development",
                ["service.instance.id"] = Environment.MachineName
            });

        // Configure tracing
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(ServiceName)
                    .SetSampler(new AlwaysOnSampler());

                if (enableHttpExport && !string.IsNullOrEmpty(otlpEndpoint))
                {
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
                else
                {
                    builder.AddConsoleExporter();
                }
            })
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(ServiceName);

                if (enableHttpExport && !string.IsNullOrEmpty(otlpEndpoint))
                {
                    builder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
                else
                {
                    builder.AddConsoleExporter();
                }
            });

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;

                if (enableHttpExport && !string.IsNullOrEmpty(otlpEndpoint))
                {
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(otlpEndpoint);
                    });
                }
                else
                {
                    options.AddConsoleExporter();
                }
            });

            // Also add console logging for immediate visibility
            builder.AddConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
            });
        });

        // Register our Win32Logger
        services.AddSingleton<IWin32Logger, Win32Logger>();

        return services;
    }

    /// <summary>
    /// Get or create the ActivitySource for tracing
    /// </summary>
    public static ActivitySource GetActivitySource()
    {
        return new ActivitySource(ServiceName, ServiceVersion);
    }
}