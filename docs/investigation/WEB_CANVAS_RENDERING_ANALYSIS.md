# Web Canvas Rendering Investigation - BasicDD.exe

## Investigation Summary

Investigated the web frontend canvas rendering system to determine why BasicDD.exe might not be displaying on the deployed WASM frontend despite successful DirectDraw API calls.

## Test Results

### JavaScript `updateCanvas` Function - ✅ WORKING

**Location**: `Win32Emu.Wasm/wwwroot/index.html` lines 381-422

**Test Method**: Created standalone HTML test page with identical JavaScript code

**Results**:
- ✅ Successfully renders RGBA data to canvas
- ✅ Correctly handles base64 decoding
- ✅ Properly resizes canvas when dimensions change
- ✅ Error handling detects and logs data length mismatches
- ✅ Validates expected data size (width × height × 4 bytes)

**Test Screenshots**:
- Initial state: Black canvas with green "Ready" text
- After update: Successfully displays color gradient test pattern (320×240)

### Canvas Element - ✅ EXISTS

**Location**: `Win32Emu.Wasm/Pages/Home.razor` line 36

```html
<canvas id="emulatorCanvas" 
        width="640" 
        height="480"
        style="max-width: 100%; height: auto; image-rendering: pixelated;">
</canvas>
```

**Configuration**:
- Default dimensions: 640×480
- ID: `emulatorCanvas` (matches JavaScript calls)
- Rendering: Uses `image-rendering: pixelated` for crisp pixel art
- Styling: Responsive with max-width 100%

### C# to JavaScript Interop - ⚠️ POTENTIAL ISSUE

**Location**: `Win32Emu.Wasm/Backend/WasmRenderingBackend.cs` lines 194-203

**Current Implementation**:
```csharp
var base64Data = Convert.ToBase64String(_frameBuffer);
_jsRuntime.InvokeVoidAsync("updateCanvas", _canvasId, base64Data, _width, _height)
    .AsTask()
    .ContinueWith(t =>
    {
        if (t.IsFaulted)
        {
            _logger.LogError(t.Exception?.GetBaseException(), 
                "[WASM] Failed to invoke updateCanvas JavaScript function");
        }
    });
```

## Identified Issues

### 1. Fire-and-Forget Error Handling

**Problem**: The `ContinueWith` pattern doesn't guarantee error logging
- Errors might not be logged until after the method returns `true`
- The continuation might not execute if the calling context is disposed
- No backpressure mechanism for rapid frame generation

**Impact**: Silent failures - code reports success but JavaScript errors are swallowed

### 2. No Frame Drop Detection

**Problem**: If JavaScript canvas updates are slower than C# frame generation
- Frames queue up in the JavaScript interop pipeline
- No way to detect or handle dropped/skipped frames
- Could lead to increasing lag over time

### 3. Missing Success Logging

**Problem**: No confirmation when canvas updates succeed
- Difficult to debug whether frames are reaching JavaScript
- Can't distinguish between "not calling" vs "calling but failing"

## Root Cause Analysis

Based on testing, the **JavaScript code works correctly**. The issue is in the **C# → JavaScript interop layer**:

1. **Error Suppression**: Fire-and-forget pattern hides JavaScript errors
2. **No Diagnostics**: Missing logs for successful updates makes debugging impossible
3. **Race Conditions**: No guarantee that canvas element exists when first frame is drawn

## Recommendations

### Immediate Debugging Steps (for deployed site)

1. **Open browser developer console** on `https://archanox.github.io/Win32Emu/emulator/`
2. **Check for JavaScript errors**:
   - "Canvas not found: emulatorCanvas"
   - "Data length mismatch: expected X bytes, got Y bytes"
   - "Could not get 2D context"
3. **Verify function calls**: Add temporary logging in `updateCanvas` function
4. **Check data integrity**: Log `base64Data.length` in JavaScript to verify data is received

### Code Improvements

#### Option 1: Proper Async/Await (Recommended)
```csharp
public async Task<bool> UpdateFrameBuffer(byte[] data, int pitch)
{
    if (!_initialized || _frameBuffer == null)
    {
        _logger.LogWarning("[WASM] UpdateFrameBuffer called but backend not initialized");
        return false;
    }

    try
    {
        // ... existing copy logic ...
        
        var base64Data = Convert.ToBase64String(_frameBuffer);
        
        // Proper await with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await _jsRuntime.InvokeVoidAsync("updateCanvas", cts.Token, _canvasId, base64Data, _width, _height);
        
        _logger.LogTrace("[WASM] Canvas updated successfully: {Width}x{Height}", _width, _height);
        return true;
    }
    catch (JSException jsEx)
    {
        _logger.LogError(jsEx, "[WASM] JavaScript error updating canvas: {Message}", jsEx.Message);
        return false;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "[WASM] Failed to update frame buffer");
        return false;
    }
}
```

#### Option 2: Add Debug Logging (Minimal Change)
```csharp
// Add before the InvokeVoidAsync call
_logger.LogDebug("[WASM] Calling updateCanvas: canvasId={CanvasId}, width={Width}, height={Height}, dataSize={DataSize}",
    _canvasId, _width, _height, base64Data.Length);

// Modify ContinueWith to log success
.ContinueWith(t =>
{
    if (t.IsFaulted)
    {
        _logger.LogError(t.Exception?.GetBaseException(), 
            "[WASM] Failed to invoke updateCanvas JavaScript function");
    }
    else
    {
        _logger.LogTrace("[WASM] Canvas update call completed successfully");
    }
});
```

#### Option 3: Add JavaScript Logging (Quick Fix)
Modify `updateCanvas` in `index.html` to always log:
```javascript
window.updateCanvas = (canvasId, base64Data, width, height) => {
    console.log(`[updateCanvas] Called: ${canvasId}, ${width}x${height}, data length: ${base64Data.length}`);
    
    // ... rest of function ...
    
    console.log(`[updateCanvas] SUCCESS: Canvas updated`);
};
```

## Testing Evidence

### Test Environment
- Created standalone HTML test page with identical JavaScript code
- Used Playwright browser automation to verify functionality
- Tested both success and error scenarios

### Test Case 1: Valid Data ✅
- Input: 320×240 RGBA gradient (307,200 bytes)
- Base64 length: 409,600 characters
- Result: Canvas successfully updated with gradient
- Console: "SUCCESS: Canvas updated successfully: 320x240"

### Test Case 2: Invalid Data ✅
- Input: 307,100 bytes (100 bytes short)
- Expected: 307,200 bytes
- Result: Error correctly detected and logged
- Console: "ERROR: Data length mismatch: expected 307200 bytes, got 307100 bytes"

## Conclusion

The JavaScript `updateCanvas` function is **working correctly**. The canvas element **exists and is properly configured**. 

The issue is in the **C# to JavaScript interop layer** which uses a fire-and-forget pattern that can suppress errors. The most likely causes for a blank canvas are:

1. **Silent JavaScript errors** not being logged due to fire-and-forget pattern
2. **Timing issues** where frames are generated before the canvas element is ready
3. **Data format issues** (wrong dimensions, corrupted base64) not being caught
4. **Blazor lifecycle issues** where the backend is disposed before frames complete

**Next Step**: Check the browser console on the deployed site for actual JavaScript errors, or add diagnostic logging to the C# interop code to trace frame updates.

---

**Date**: 2024-12-11  
**Tested By**: GitHub Copilot  
**Test Method**: Playwright browser automation with isolated JavaScript test  
**Result**: JavaScript code ✅ Working | C# interop ⚠️ Needs error handling improvements
