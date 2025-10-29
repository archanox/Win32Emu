# JetBrains Rider OpenTelemetry Integration

Win32Emu now supports automatic integration with JetBrains Rider's built-in OpenTelemetry features through the standard `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable.

## What's New

- Automatic detection of `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable
- No command-line flags required when using the environment variable
- Command-line arguments still work and override environment variables

## Setting Up with JetBrains Rider

### Step 1: Enable OpenTelemetry in Rider

1. Open **Settings** (Ctrl+Alt+S / Cmd+,)
2. Navigate to **Tools → OpenTelemetry**
3. Check **Enable OpenTelemetry**
4. Note the OTLP endpoint (default is usually `http://localhost:4317`)

### Step 2: Configure Run Configuration

1. Go to **Run → Edit Configurations**
2. Select your Win32Emu run configuration
3. In the **Environment Variables** section, add:
   ```
   OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
   ```
   (Use the endpoint from Rider's OpenTelemetry settings)

### Step 3: Run and Monitor

1. Run your configuration
2. In Rider, open the **OpenTelemetry** tool window
3. You should see traces and metrics from Win32Emu appearing automatically

## What Gets Sent to Rider

Win32Emu will send the following telemetry data:

### Traces
- **Emulator.Run** activity with tags for:
  - Executable path
  - Debug mode status
  - Execution time

### Metrics
- **win32emu.instructions.executed** - Total x86 instructions executed
- **win32emu.api.calls** - Win32 API calls (tagged by DLL and function)
- **win32emu.memory.allocations** - Memory allocation events
- **win32emu.api.duration** - API call durations
- **win32emu.memory.usage** - Current memory usage
- **win32emu.exceptions** - Exception counts

Plus standard .NET runtime metrics:
- GC statistics
- Thread pool metrics
- JIT compilation metrics

## Troubleshooting

### No Data Appearing in Rider

1. **Check the endpoint**: Verify that `OTEL_EXPORTER_OTLP_ENDPOINT` matches Rider's settings
2. **Check Rider's OpenTelemetry tool window**: Make sure it's enabled and listening
3. **Check the console output**: Win32Emu logs when OpenTelemetry is initialized:
   ```
   OpenTelemetry initialized - Console: False, OTLP: True (http://localhost:4317)
   ```

### Environment Variable Not Working

Make sure you're setting it in the Run Configuration's environment variables, not your system environment variables (though system variables work too).

### Port Conflicts

If port 4317 is already in use, you can:
1. Change Rider's OpenTelemetry port in Settings
2. Update the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable to match

## Alternative: Command-Line Usage

If you prefer not to use environment variables, you can still use command-line flags:

```bash
Win32Emu game.exe --telemetry-otlp http://localhost:4317
```

Command-line flags will override environment variables if both are set.

## Example Run Configuration

```xml
<component name="ProjectRunConfigurationManager">
  <configuration default="false" name="Win32Emu" type="DotNetProject" factoryName=".NET Project">
    <option name="EXE_PATH" value="$PROJECT_DIR$/Win32Emu/bin/Debug/net9.0/Win32Emu.dll" />
    <option name="PROGRAM_PARAMETERS" value="game.exe" />
    <option name="WORKING_DIRECTORY" value="$PROJECT_DIR$" />
    <envs>
      <env name="OTEL_EXPORTER_OTLP_ENDPOINT" value="http://localhost:4317" />
    </envs>
  </configuration>
</component>
```

## See Also

- [OPENTELEMETRY_USAGE.md](OPENTELEMETRY_USAGE.md) - Complete OpenTelemetry documentation
- [TELEMETRY_EXAMPLE.md](../examples/TELEMETRY_EXAMPLE.md) - Practical examples
- [JetBrains Rider OpenTelemetry Documentation](https://www.jetbrains.com/help/rider/2025.2/OpenTelemetry.html)
