#!/bin/bash
# generate-missing-functions-report.sh
# Script to generate the missing functions report locally

set -e  # Exit on error

echo "Win32Emu - Missing Functions Report Generator"
echo "=============================================="
echo

# Check if DLLs directory exists
if [ ! -d "DLLs/WinME" ]; then
    echo "Error: DLLs/WinME directory not found!"
    echo "Please ensure you have the Windows ME DLLs in DLLs/WinME/"
    exit 1
fi

# Create output directory
mkdir -p docs/pages

echo "Step 1: Building Win32Emu to generate API metadata..."
dotnet build Win32Emu/Win32Emu.csproj --configuration Release

echo
echo "Step 2: Building API Status Generator..."
dotnet build Win32Emu.Tools.ApiStatusGenerator/Win32Emu.Tools.ApiStatusGenerator.csproj --configuration Release

echo
echo "Step 3: Building Native DLL Analyzer..."
dotnet build Win32Emu.Tools.NativeDllAnalyzer/Win32Emu.Tools.NativeDllAnalyzer.csproj --configuration Release

echo
echo "Step 4: Generating API status JSON..."
dotnet run --project Win32Emu.Tools.ApiStatusGenerator --configuration Release --no-build -- \
    docs/pages/api-status.json

echo
echo "Step 5: Analyzing native DLLs and generating missing functions report..."
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer --configuration Release --no-build -- \
    DLLs/WinME \
    docs/pages/api-status.json \
    docs/pages/missing-functions.json

echo
echo "=============================================="
echo "✅ Report generation complete!"
echo
echo "Generated files:"
echo "  - docs/pages/api-status.json"
echo "  - docs/pages/missing-functions.json"
echo
echo "To view the report:"
echo "  1. Open docs/pages/missing-functions.html in your browser"
echo "  2. Or start a local web server: python3 -m http.server -d docs/pages 8000"
echo "     Then visit: http://localhost:8000/missing-functions.html"
echo
