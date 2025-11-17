#!/bin/bash
# Launcher script for running Win32Emu in headless mode
# This script automatically configures SDL for headless operation

# Set SDL to use dummy video driver for headless environments
export SDL_VIDEODRIVER=dummy

# Run Win32Emu.Gui with all passed arguments
exec dotnet run --project "$(dirname "$0")/Win32Emu.Gui" --configuration Release -- "$@"
