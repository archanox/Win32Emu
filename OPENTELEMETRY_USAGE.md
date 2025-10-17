# OpenTelemetry Integration

Win32Emu now supports OpenTelemetry for comprehensive observability including logging, metrics, and distributed tracing.

## Features

- **Distributed Tracing**: Track execution flow and performance profiling
- **Metrics Collection**: Monitor emulator performance with custom metrics
- **Console Exporter**: View telemetry data directly in the console for debugging
- **OTLP Exporter**: Send telemetry data to any OpenTelemetry-compatible backend (e.g., Jaeger, Grafana, Azure Monitor)

## Command-Line Options

### Console Output

To enable OpenTelemetry with console output:

```bash
Win32Emu game.exe --telemetry-console
```

This will output telemetry data directly to the console, useful for debugging and development.

### OTLP Endpoint (HTTP)

To send telemetry to an OpenTelemetry collector or backend:

```bash
# Using default endpoint (http://localhost:4317)
Win32Emu game.exe --telemetry-otlp

# Using custom endpoint
Win32Emu game.exe --telemetry-otlp http://my-collector:4317
```

### Combining Options

You can use both console and OTLP exporters simultaneously:

```bash
Win32Emu game.exe --telemetry-console --telemetry-otlp http://localhost:4317
```

## Metrics

The following metrics are collected:

### Counters

- `win32emu.instructions.executed` - Total number of x86 instructions executed
- `win32emu.api.calls` - Total number of Win32 API calls (tagged by DLL and function)
- `win32emu.memory.allocations` - Total number of memory allocations
- `win32emu.exceptions` - Total number of exceptions encountered (tagged by type)

### Histograms

- `win32emu.api.duration` - Duration of Win32 API calls in milliseconds (tagged by DLL and function)

### Gauges

- `win32emu.memory.usage` - Current memory usage of the emulator in bytes

## Traces

Activities (traces) are automatically created for:

- `Emulator.Run` - The main emulation loop (tagged with executable path and debug mode)

Additional activities can be added by Win32 API implementations as needed.

## Setting Up an OpenTelemetry Collector

### Using Docker

You can quickly set up an OpenTelemetry collector using Docker:

```bash
docker run -d --name otel-collector \
  -p 4317:4317 \
  -p 4318:4318 \
  otel/opentelemetry-collector:latest
```

### Using Jaeger for Visualization

For visualizing traces with Jaeger:

```bash
docker run -d --name jaeger \
  -p 16686:16686 \
  -p 4317:4317 \
  jaegertracing/all-in-one:latest
```

Then run Win32Emu with:

```bash
Win32Emu game.exe --telemetry-otlp http://localhost:4317
```

Open http://localhost:16686 in your browser to view traces in the Jaeger UI.

## Environment Variables

OpenTelemetry also respects standard environment variables:

- `OTEL_EXPORTER_OTLP_ENDPOINT` - Default OTLP endpoint
- `OTEL_SERVICE_NAME` - Service name (defaults to "Win32Emu")

## Example Usage

### Debugging Performance Issues

```bash
# Enable telemetry with console output to see API call durations
Win32Emu game.exe --telemetry-console --debug
```

### Production Monitoring

```bash
# Send telemetry to your monitoring system
Win32Emu game.exe --telemetry-otlp http://monitoring.example.com:4317
```

### Development with Full Observability

```bash
# Enable all features for comprehensive debugging
Win32Emu game.exe --debug --telemetry-console --telemetry-otlp http://localhost:4317
```

## Integration with Existing Logging

OpenTelemetry integration works alongside the existing Microsoft.Extensions.Logging infrastructure. Both systems operate independently:

- Console logging (`--debug` flag) provides detailed execution logs
- OpenTelemetry (`--telemetry-*` flags) provides structured metrics and traces

You can enable both for maximum observability.
