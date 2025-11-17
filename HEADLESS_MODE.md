# Headless Mode Launcher

This script (`run-headless.sh`) provides an easy way to run Win32Emu in headless environments where no display is available (e.g., CI/CD, Docker, SSH sessions).

## What it does

The script automatically sets the `SDL_VIDEODRIVER=dummy` environment variable before launching Win32Emu, which tells SDL to use its dummy video driver instead of trying to connect to a display server.

## Usage

```bash
# Basic usage
./run-headless.sh --nogui <path-to-exe> --backend Software

# Example: Run ign_teas in headless mode
./run-headless.sh --nogui ./EXEs/ign_teas/IGN_TEAS.EXE --backend Software

# With debugging enabled
./run-headless.sh --nogui <path-to-exe> --backend Software --debug

# With file logging
./run-headless.sh --nogui <path-to-exe> --backend Software --log-file
```

## Why is this needed?

SDL (Simple DirectMedia Layer) reads the `SDL_VIDEODRIVER` environment variable when its native library is first loaded, which happens during .NET CLR initialization - before any managed code (including Module Initializers) can run. This means the environment variable must be set **before** the .NET process starts.

## Alternative approaches

If you don't want to use the launcher script, you can:

1. **Set the environment variable manually:**
   ```bash
   SDL_VIDEODRIVER=dummy dotnet run --project Win32Emu.Gui --configuration Release -- --nogui <path-to-exe> --backend Software
   ```

2. **Export for your entire session:**
   ```bash
   export SDL_VIDEODRIVER=dummy
   dotnet run --project Win32Emu.Gui --configuration Release -- --nogui <path-to-exe> --backend Software
   ```

3. **Add to your shell profile** (for permanent headless operation):
   ```bash
   echo 'export SDL_VIDEODRIVER=dummy' >> ~/.bashrc
   source ~/.bashrc
   ```

## Requirements

- The Software backend must be explicitly specified with `--backend Software`
- The `--nogui` flag must be used to run in CLI mode
- Linux or Unix-like environment (for the bash script)

## Supported platforms

- Linux (Ubuntu, Debian, Fedora, etc.)
- macOS
- WSL (Windows Subsystem for Linux)
- CI/CD environments (GitHub Actions, GitLab CI, Jenkins, etc.)
- Docker containers
- SSH sessions

## Testing

To verify headless mode is working:

```bash
# Should see "Created software renderer" in the output
./run-headless.sh --nogui ./EXEs/ign_teas/IGN_TEAS.EXE --backend Software 2>&1 | grep "Created software"
```

If you see "Failed to initialize SDL video: No available video device", the SDL_VIDEODRIVER variable was not set early enough.
