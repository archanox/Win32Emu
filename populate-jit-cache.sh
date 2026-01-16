#!/bin/bash
# Script to populate JIT cache for ign_teas by running for a limited time
# then sending SIGTERM for graceful shutdown

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Populating JIT Cache for ign_teas ==="
echo "Starting emulator in background..."

# Set up environment for headless operation
export SDL_VIDEODRIVER=dummy

# Run the emulator in background, capturing PID
cd EXEs/ign_teas
dotnet run --project ../../Win32Emu.Gui/Win32Emu.Gui.csproj --configuration Release --no-build -- --nogui --backend Software IGN_TEAS.EXE > "$SCRIPT_DIR/cache-population.log" 2>&1 &
EMU_PID=$!

echo "Emulator started with PID: $EMU_PID"
echo "Waiting 45 seconds for JIT cache to populate..."

# Wait for some time to let JIT cache populate
sleep 45

echo "Sending SIGTERM for graceful shutdown..."
# Send SIGTERM to allow graceful shutdown and cache save
kill -TERM $EMU_PID 2>/dev/null || true

# Wait for process to complete gracefully
echo "Waiting for emulator to save cache and exit..."
wait $EMU_PID 2>/dev/null || true

echo ""
echo "=== Cache Population Complete ==="
echo ""

# Check if cache was created
cd "$SCRIPT_DIR"
if [ -d ".jitcache" ] && [ -n "$(ls -A .jitcache 2>/dev/null)" ]; then
    echo "✓ JIT cache directory created at: $SCRIPT_DIR/.jitcache"
    echo "✓ Cache contents:"
    find .jitcache -type f | while read file; do
        size=$(stat -f%z "$file" 2>/dev/null || stat -c%s "$file" 2>/dev/null || echo "unknown")
        echo "  - $file ($size bytes)"
    done
else
    echo "✗ JIT cache was not created or is empty"
    echo "Check cache-population.log for details"
fi

echo ""
echo "Log saved to: $SCRIPT_DIR/cache-population.log"
