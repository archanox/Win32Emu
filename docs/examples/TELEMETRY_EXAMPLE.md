# OpenTelemetry Example Usage

This document provides practical examples of using the OpenTelemetry integration in Win32Emu.

## Example 1: Console Output (Local Development)

The simplest way to see telemetry data is to use console output:

```bash
# Build the project
dotnet build

# Run with console telemetry
dotnet run --project Win32Emu/Win32Emu.csproj -- EXEs/CHKCPU32.exe --telemetry-console
```

You'll see metrics and traces printed to the console, including:
- Instructions executed
- API calls made (with DLL and function name)
- Execution duration

## Example 2: OTLP with Jaeger (Full Distributed Tracing)

For a complete observability setup with visualization:

### Step 1: Start Jaeger

```bash
docker run -d --name jaeger \
  -p 16686:16686 \
  -p 4317:4317 \
  jaegertracing/all-in-one:latest
```

### Step 2: Run Win32Emu with OTLP

```bash
dotnet run --project Win32Emu/Win32Emu.csproj -- EXEs/CHKCPU32.exe --telemetry-otlp http://localhost:4317
```

### Step 3: View Traces

Open http://localhost:16686 in your browser and select "Win32Emu" from the service dropdown.

## Example 3: Production Monitoring

For production environments, combine both console and OTLP:

```bash
# Send to your monitoring backend while keeping local logs
dotnet run --project Win32Emu/Win32Emu.csproj -- \
  path/to/game.exe \
  --telemetry-console \
  --telemetry-otlp https://monitoring.example.com:4317
```

## Example 4: Performance Profiling

To identify performance bottlenecks:

```bash
# Enable debug logging + telemetry for detailed performance analysis
dotnet run --project Win32Emu/Win32Emu.csproj -- \
  EXEs/CHKCPU32.exe \
  --debug \
  --telemetry-console
```

This will show:
- Detailed logs of each operation
- Metrics for instruction counts
- API call durations
- Memory allocation patterns

## Example 5: OpenTelemetry Collector

For advanced setups with multiple exporters:

### collector-config.yaml

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317

exporters:
  logging:
    loglevel: debug
  
  prometheus:
    endpoint: 0.0.0.0:8889
  
  jaeger:
    endpoint: jaeger:14250
    tls:
      insecure: true

service:
  pipelines:
    traces:
      receivers: [otlp]
      exporters: [logging, jaeger]
    metrics:
      receivers: [otlp]
      exporters: [logging, prometheus]
```

### Start the collector

```bash
docker run -d --name otel-collector \
  -p 4317:4317 \
  -p 8889:8889 \
  -v $(pwd)/collector-config.yaml:/etc/otel-collector-config.yaml \
  otel/opentelemetry-collector:latest \
  --config=/etc/otel-collector-config.yaml
```

### Run Win32Emu

```bash
dotnet run --project Win32Emu/Win32Emu.csproj -- \
  EXEs/CHKCPU32.exe \
  --telemetry-otlp http://localhost:4317
```

Now your telemetry is exported to both Prometheus (for metrics) and Jaeger (for traces)!

## Key Metrics to Monitor

- **win32emu.instructions.executed**: Track emulator throughput
- **win32emu.api.calls**: Identify most-called Win32 APIs
- **win32emu.api.duration**: Find slow API implementations
- **win32emu.memory.usage**: Monitor memory consumption
- **win32emu.exceptions**: Detect errors and crashes

## Troubleshooting

### No telemetry data appears

1. Verify the OTLP endpoint is reachable:
   ```bash
   curl http://localhost:4317
   ```

2. Enable console exporter to verify telemetry is being generated:
   ```bash
   dotnet run --project Win32Emu/Win32Emu.csproj -- \
     EXEs/CHKCPU32.exe \
     --telemetry-console
   ```

3. Check the collector logs if using one

### Performance impact

OpenTelemetry has minimal overhead, but if you notice performance issues:

- Use OTLP exporter only (disable console exporter)
- Configure sampling in the collector
- Reduce metric collection frequency if needed

The instrumentation is designed to be lightweight and production-ready.
