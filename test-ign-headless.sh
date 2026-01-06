#!/usr/bin/env bash
# Comprehensive headless test script for ign_teas and ign_demo
# This script runs the applications headlessly and captures detailed logs

set -euo pipefail

# Configuration
TIMEOUT_SECONDS=10
PATTERN_FAIL="fail:"
PATTERN_WARN="warn:"
PATTERN_HEAP_EXEC="heap memory range"
PATTERN_MEMORY_ERROR="Memory access out of range"

PROJECT_ROOT="$(cd "$(dirname "$0")" && pwd)"
LOGS_DIR="$PROJECT_ROOT/diagnostic_logs"
GUI_PROJECT="$PROJECT_ROOT/Win32Emu.Gui/Win32Emu.Gui.csproj"

# Applications to test
declare -A APPS=(
    ["ign_teas"]="$PROJECT_ROOT/EXEs/ign_teas/IGN_TEAS.EXE"
    ["ign_demo"]="$PROJECT_ROOT/EXEs/ign_demo/IGN_DEMO.EXE"
)

# Create logs directory
mkdir -p "$LOGS_DIR"

echo "🧪 IGN Applications Headless Diagnostic Test"
echo "============================================="
echo ""
echo "📁 Logs directory: $LOGS_DIR"
echo "🔧 Applications to test: ${#APPS[@]}"
echo ""

# Test each application
for app_name in "${!APPS[@]}"; do
    app_path="${APPS[$app_name]}"
    
    if [ ! -f "$app_path" ]; then
        echo "⚠️  Application not found: $app_path"
        continue
    fi
    
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "▶️  Testing: $app_name"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "   Path: $app_path"
    
    # Generate timestamp for logs
    timestamp=$(date +%Y%m%d_%H%M%S)
    log_file="$LOGS_DIR/${app_name}_${timestamp}.log"
    
    echo "   Log file: $log_file"
    
    # Set environment variables for headless mode
    export SDL_VIDEODRIVER=dummy
    
    # Change to application directory if it has a DATA folder
    app_dir=$(dirname "$app_path")
    if [ -d "$app_dir/DATA" ]; then
        echo "   Found DATA directory, running from: $app_dir"
        cd "$app_dir"
    fi
    
    # Run application with timeout
    echo "   🚀 Starting emulation..."
    set +e  # Don't exit on error
    timeout $TIMEOUT_SECONDS dotnet run --project "$GUI_PROJECT" --configuration Release --no-build -- \
        --nogui \
        --log-file "$log_file" \
        --backend Software \
        --debug \
        "$app_path" 2>&1 | tee -a "$log_file.stdout"
    
    exit_code=$?
    set -e
    
    # Return to project root
    cd "$PROJECT_ROOT"
    
    # Analyze result
    if [ $exit_code -eq 124 ]; then
        echo "   ⏱️  Application timed out (expected behavior for running apps)"
    elif [ $exit_code -eq 0 ]; then
        echo "   ✅ Application exited cleanly"
    else
        echo "   ❌ Application crashed with exit code: $exit_code"
    fi
    
    # Analyze log for common issues
    echo "   📊 Log analysis:"
    
    if [ -f "$log_file" ]; then
        # Count errors using defined patterns
        error_count=$(grep -c "$PATTERN_FAIL" "$log_file" 2>/dev/null || echo "0")
        warn_count=$(grep -c "$PATTERN_WARN" "$log_file" 2>/dev/null || echo "0")
        heap_exec_count=$(grep -c "$PATTERN_HEAP_EXEC" "$log_file" 2>/dev/null || echo "0")
        memory_error_count=$(grep -c "$PATTERN_MEMORY_ERROR" "$log_file" 2>/dev/null || echo "0")
        
        echo "      Errors: $error_count"
        echo "      Warnings: $warn_count"
        echo "      Heap execution warnings: $heap_exec_count"
        echo "      Memory access errors: $memory_error_count"
        
        # Extract last API call before crash
        last_api=$(grep "Dispatching" "$log_file" | tail -1 || echo "None found")
        echo "      Last API call: $last_api"
        
        # Extract crash location if present
        if grep -q "Exception during SingleStep" "$log_file"; then
            crash_location=$(grep "Exception during SingleStep" "$log_file" | head -1)
            echo "      Crash location: $crash_location"
        fi
        
        # Check for heap execution
        if [ $heap_exec_count -gt 0 ]; then
            first_heap_exec=$(grep "$PATTERN_HEAP_EXEC" "$log_file" | head -1)
            echo "      First heap execution: $first_heap_exec"
        fi
    else
        echo "      ⚠️  Log file not found"
    fi
    
    echo ""
done

# Summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📊 Test Summary"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

log_count=$(find "$LOGS_DIR" -name "*.log" -type f | wc -l)
echo "Total log files generated: $log_count"
echo "Logs directory: $LOGS_DIR"
echo ""
echo "📝 To analyze logs:"
echo "   cat $LOGS_DIR/ign_teas_*.log | less"
echo "   cat $LOGS_DIR/ign_demo_*.log | less"
echo ""
echo "🔍 To find crash patterns:"
echo "   grep -i 'exception\|error\|crash' $LOGS_DIR/*.log"
echo ""
echo "✅ Diagnostic test complete"
