#!/bin/bash

# Test script for DirectDraw samples in headless mode with frame dumping
# This script runs all DirectDraw samples and captures frames to disk

set -e

echo "🧪 DirectDraw Headless Mode Test"
echo "=================================="
echo ""

# Configuration
PROJECT_ROOT="$(cd "$(dirname "$0")" && pwd)"
FRAMES_DIR="$PROJECT_ROOT/test-frames"
GUI_PROJECT="$PROJECT_ROOT/Win32Emu.Gui/Win32Emu.Gui.csproj"
SAMPLES_DIR="$PROJECT_ROOT/Win32Emu.Wasm/wwwroot/samples"

# DirectDraw samples to test
declare -a SAMPLES=(
    "$SAMPLES_DIR/simple_ddraw.exe"
    "$SAMPLES_DIR/hugi.exe"
    "$PROJECT_ROOT/retrowin32/exe/cpp/ddraw.exe"
)

# ign_teas with DATA folder
IGNTEAS_EXE="$SAMPLES_DIR/ign_teas/IGN_TEAS.EXE"

# Create frames directory
mkdir -p "$FRAMES_DIR"

echo "📁 Frame dump directory: $FRAMES_DIR"
echo "🔧 Samples to test: ${#SAMPLES[@]} + ign_teas"
echo ""

# Test each sample
for SAMPLE in "${SAMPLES[@]}"; do
    if [ ! -f "$SAMPLE" ]; then
        echo "⚠️  Sample not found: $SAMPLE"
        continue
    fi
    
    SAMPLE_NAME=$(basename "$SAMPLE")
    SAMPLE_FRAMES_DIR="$FRAMES_DIR/$SAMPLE_NAME"
    
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "▶️  Testing: $SAMPLE_NAME"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    # Create sample-specific frames directory
    mkdir -p "$SAMPLE_FRAMES_DIR"
    
    # Set environment variables for frame dumping and headless mode
    export WIN32EMU_FRAME_DUMP_PATH="$SAMPLE_FRAMES_DIR"
    export SDL_VIDEODRIVER=dummy
    
    # Run sample for 5 seconds (timeout) in headless mode
    echo "🚀 Running $SAMPLE_NAME..."
    timeout 5 dotnet run --project "$GUI_PROJECT" --configuration Release --no-build -- \
        --nogui \
        --backend Software \
        "$SAMPLE" \
        2>&1 | grep -E "\[Software\]|\[DDraw\]|DirectDraw|Backend|Frame" || true
    
    # Check if frames were generated
    FRAME_COUNT=$(find "$SAMPLE_FRAMES_DIR" -name "frame_*.png" 2>/dev/null | wc -l)
    
    if [ $FRAME_COUNT -gt 0 ]; then
        echo "✅ Generated $FRAME_COUNT frames in $SAMPLE_FRAMES_DIR"
        echo "📸 First frame: $(ls -1 "$SAMPLE_FRAMES_DIR"/frame_*.png 2>/dev/null | head -1)"
        echo "📸 Last frame: $(ls -1 "$SAMPLE_FRAMES_DIR"/frame_*.png 2>/dev/null | tail -1)"
    else
        echo "❌ No frames generated for $SAMPLE_NAME"
    fi
    
    echo ""
done

# Test ign_teas separately (requires DATA folder)
if [ -f "$IGNTEAS_EXE" ]; then
    SAMPLE_NAME="IGN_TEAS.EXE"
    SAMPLE_FRAMES_DIR="$FRAMES_DIR/$SAMPLE_NAME"
    
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "▶️  Testing: $SAMPLE_NAME (with DATA folder)"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    mkdir -p "$SAMPLE_FRAMES_DIR"
    
    export WIN32EMU_FRAME_DUMP_PATH="$SAMPLE_FRAMES_DIR"
    export SDL_VIDEODRIVER=dummy
    
    # Change to ign_teas directory so it can find DATA folder
    cd "$(dirname "$IGNTEAS_EXE")"
    
    echo "🚀 Running $SAMPLE_NAME..."
    timeout 5 dotnet run --project "$GUI_PROJECT" --configuration Release --no-build -- \
        --nogui \
        --backend Software \
        "$IGNTEAS_EXE" \
        2>&1 | grep -E "\[Software\]|\[DDraw\]|DirectDraw|Backend|Frame" || true
    
    cd "$PROJECT_ROOT"
    
    FRAME_COUNT=$(find "$SAMPLE_FRAMES_DIR" -name "frame_*.png" 2>/dev/null | wc -l)
    
    if [ $FRAME_COUNT -gt 0 ]; then
        echo "✅ Generated $FRAME_COUNT frames in $SAMPLE_FRAMES_DIR"
        echo "📸 First frame: $(ls -1 "$SAMPLE_FRAMES_DIR"/frame_*.png 2>/dev/null | head -1)"
        echo "📸 Last frame: $(ls -1 "$SAMPLE_FRAMES_DIR"/frame_*.png 2>/dev/null | tail -1)"
    else
        echo "❌ No frames generated for $SAMPLE_NAME"
    fi
    
    echo ""
fi

# Summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📊 Test Summary"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

TOTAL_FRAMES=$(find "$FRAMES_DIR" -name "frame_*.png" 2>/dev/null | wc -l)
SAMPLES_WITH_FRAMES=$(find "$FRAMES_DIR" -mindepth 1 -maxdepth 1 -type d -exec sh -c 'test $(find "$1" -name "frame_*.png" 2>/dev/null | wc -l) -gt 0' _ {} \; -print | wc -l)

echo "Total frames generated: $TOTAL_FRAMES"
echo "Samples with frames: $SAMPLES_WITH_FRAMES"
echo "Frames directory: $FRAMES_DIR"
echo ""

if [ $TOTAL_FRAMES -gt 0 ]; then
    echo "✅ Headless mode frame dumping is working!"
    echo ""
    echo "To view frames, open: $FRAMES_DIR"
    exit 0
else
    echo "❌ No frames were generated - DirectDraw initialization may be failing"
    exit 1
fi
