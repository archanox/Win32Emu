#!/usr/bin/env bash
# Test script for running ign_teas with framebuffer dumping in headless mode
# This captures rendered frames as PNG files for visual verification

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "$0")" && pwd)"
IGN_TEAS_EXE="$PROJECT_ROOT/EXEs/ign_teas/IGN_TEAS.EXE"
FRAME_DUMP_DIR="$PROJECT_ROOT/test-screenshots/ign_teas_frames"
LOG_FILE="$PROJECT_ROOT/diagnostic_logs/ign_teas_framedump_$(date +%Y%m%d_%H%M%S).log"
TIMEOUT_SECONDS=15

echo "🎮 IGN_TEAS Framebuffer Dump Test"
echo "================================="
echo ""

# Check if executable exists
if [ ! -f "$IGN_TEAS_EXE" ]; then
    echo "❌ Error: IGN_TEAS.EXE not found at: $IGN_TEAS_EXE"
    exit 1
fi

# Create directories
mkdir -p "$FRAME_DUMP_DIR"
mkdir -p "$(dirname "$LOG_FILE")"

echo "📁 Frame dump directory: $FRAME_DUMP_DIR"
echo "📝 Log file: $LOG_FILE"
echo ""

# Set environment variables for headless mode and frame dumping
export SDL_VIDEODRIVER=dummy
export WIN32EMU_FRAME_DUMP_PATH="$FRAME_DUMP_DIR"

echo "🔧 Configuration:"
echo "   - Backend: Software (CPU rendering)"
echo "   - Display: Headless (SDL_VIDEODRIVER=dummy)"
echo "   - Frame dumping: Enabled"
echo "   - Timeout: ${TIMEOUT_SECONDS}s"
echo ""

# Change to ign_teas directory (it has a DATA folder)
cd "$(dirname "$IGN_TEAS_EXE")"

echo "🚀 Starting emulation..."
echo ""

# Run with timeout
set +e
timeout $TIMEOUT_SECONDS dotnet run \
    --project "$PROJECT_ROOT/Win32Emu.Gui/Win32Emu.Gui.csproj" \
    --configuration Release \
    --no-build \
    -- \
    --nogui \
    --backend Software \
    --log-file "$LOG_FILE" \
    "$(basename "$IGN_TEAS_EXE")" 2>&1 | tee -a "$LOG_FILE.stdout"

exit_code=$?
set -e

cd "$PROJECT_ROOT"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📊 Results"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Analyze exit code
if [ $exit_code -eq 124 ]; then
    echo "⏱️  Application timed out after ${TIMEOUT_SECONDS}s (expected)"
elif [ $exit_code -eq 0 ]; then
    echo "✅ Application exited cleanly"
else
    echo "❌ Application crashed with exit code: $exit_code"
fi

echo ""

# Count captured frames
frame_count=$(find "$FRAME_DUMP_DIR" -name "frame_*.png" -type f 2>/dev/null | wc -l)
echo "🖼️  Frames captured: $frame_count"

if [ $frame_count -gt 0 ]; then
    echo ""
    echo "📷 Captured frames:"
    ls -lh "$FRAME_DUMP_DIR"/frame_*.png | head -10
    
    if [ $frame_count -gt 10 ]; then
        echo "   ... and $((frame_count - 10)) more frames"
    fi
    
    # Show first and last frames
    first_frame=$(ls -1 "$FRAME_DUMP_DIR"/frame_*.png | head -1)
    last_frame=$(ls -1 "$FRAME_DUMP_DIR"/frame_*.png | tail -1)
    
    echo ""
    echo "First frame: $first_frame"
    echo "Last frame:  $last_frame"
    
    # Get frame dimensions if possible
    if command -v identify >/dev/null 2>&1; then
        echo ""
        echo "Frame info (using ImageMagick identify):"
        identify "$first_frame" || true
    fi
else
    echo "⚠️  No frames were captured!"
    echo ""
    echo "Possible issues:"
    echo "  - Emulator didn't reach frame rendering code"
    echo "  - DirectDraw not initialized properly"
    echo "  - Check log file for errors"
fi

echo ""
echo "📝 Log files:"
echo "   Main log: $LOG_FILE"
echo "   Stdout:   $LOG_FILE.stdout"

# Check for errors in log
if [ -f "$LOG_FILE" ]; then
    error_count=$(grep -ci "error\|exception\|fail" "$LOG_FILE" 2>/dev/null || echo "0")
    echo ""
    echo "Log analysis:"
    echo "   Errors/exceptions: $error_count"
    
    if [ $error_count -gt 0 ]; then
        echo ""
        echo "Last 10 errors:"
        grep -i "error\|exception\|fail" "$LOG_FILE" | tail -10
    fi
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Test complete!"
echo ""
echo "To view captured frames:"
echo "   ls $FRAME_DUMP_DIR/"
echo ""
echo "To create a video from frames (requires ffmpeg):"
echo "   ffmpeg -framerate 30 -pattern_type glob -i '$FRAME_DUMP_DIR/frame_*.png' -c:v libx264 -pix_fmt yuv420p $PROJECT_ROOT/ign_teas_output.mp4"
