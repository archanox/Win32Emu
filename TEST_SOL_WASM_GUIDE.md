# Testing sol.exe on WASM with Playwright

This guide explains how to test sol.exe in the WASM environment using the automated Playwright test script.

## Prerequisites

1. **Node.js and npm** (already available in this environment)
2. **Playwright** (install with: `npm install`)
3. **WASM Build** (must be built first)

## Steps to Run the Test

### 1. Build the WASM Application

```bash
cd /home/runner/work/Win32Emu/Win32Emu
dotnet publish Win32Emu.Wasm/Win32Emu.Wasm.csproj -c Release
```

This will create the WASM build at: `Win32Emu.Wasm/bin/Release/net10.0/publish/wwwroot/`

### 2. Install Playwright Dependencies

```bash
npm install
npx playwright install chromium
```

### 3. Run the sol.exe Test

```bash
node test-sol-wasm.js
```

## What the Test Does

The `test-sol-wasm.js` script:

1. ✅ Starts a local web server serving the WASM app
2. ✅ Launches a headless Chromium browser with Playwright
3. ✅ Navigates to the Win32Emu WASM frontend
4. ✅ Loads sol.exe (EXEs/WinME/sol.exe)
5. ✅ Starts the emulator
6. ✅ Monitors console output for Win16 module registration logs:
   - "Registering Win16 thunking modules"
   - "Looking up KERNEL32.DLL"
   - "Looking up USER32.DLL"
   - "Looking up GDI32.DLL"
   - "Looking up WINMM.DLL"
   - "Creating Win16 KERNEL module"
   - "Creating Win16 USER module"
   - "Win16 thunking modules registered successfully"
7. ✅ Captures errors and exceptions
8. ✅ Takes screenshots at key points
9. ✅ Saves debug output and console messages

## Expected Results

### ✅ Success (Fix Working)
- Console shows all Win16 module registration debug logs
- No errors during module registration
- "Win16 thunking modules registered successfully" message appears
- sol.exe loads without crashing

### ❌ Failure (Bug Still Present)
- Exception or error during Win16 module registration
- Truncated error messages in console
- Missing "Win16 thunking modules registered successfully" message
- Browser console shows crash

## Output Files

All output is saved to `test-screenshots/`:
- `sol-01-initial-load.png` - Blazor app loaded
- `sol-02-file-loaded.png` - After sol.exe uploaded
- `sol-03-emulation-started.png` - After clicking Start
- `sol-04-final-state.png` - Final state
- `sol-debug-output.txt` - Debug panel content
- `sol-console-messages.json` - All browser console messages
- `sol-error.png` - Error screenshot (if test fails)

## Test Validation

The test will report:
- ✅ **PASSED** if Win16 registration logs are found and no errors occur
- ❌ **FAILED** if errors are detected during loading
- ⚠️ **INCONCLUSIVE** if unable to determine success/failure

## Troubleshooting

### WASM Build Not Found
```
❌ WASM build not found at: Win32Emu.Wasm/bin/Release/net10.0/publish/wwwroot
   Please run: dotnet publish Win32Emu.Wasm/Win32Emu.Wasm.csproj -c Release
```
**Solution**: Run the publish command to build the WASM app.

### sol.exe Not Found
```
❌ sol.exe not found at: EXEs/WinME/sol.exe
   Please ensure EXEs/WinME/sol.exe exists
```
**Solution**: Verify sol.exe exists in the repository at `EXEs/WinME/sol.exe`.

### Playwright Not Installed
```
Error: Cannot find module 'playwright'
```
**Solution**: Run `npm install` to install dependencies.

## Manual Testing Alternative

If automated testing is not feasible, you can manually test:

1. Build and publish the WASM app
2. Serve it locally: `npx http-server Win32Emu.Wasm/bin/Release/net10.0/publish/wwwroot -p 8080`
3. Open browser to `http://localhost:8080`
4. Open browser DevTools Console (F12)
5. Load sol.exe
6. Click Start
7. Watch console for Win16 module registration logs

## Why This Test Matters

The original issue reported that sol.exe crashed in WASM during Win16 module registration with truncated error messages ("[Emulato..."). 

This test validates that:
1. The detailed debug logging added in the fix is visible in WASM
2. All Win16 modules (KERNEL32, USER32, GDI32, WINMM) can be found
3. All Win16 thunking modules can be created successfully
4. Complete error messages are logged (not truncated)
5. sol.exe loads without crashing

The fix added:
- Debug logs before each module lookup
- Try-catch with detailed error logging
- Explicit null checks with clear error messages
- Step-by-step Win16 module creation logging

This test ensures these improvements work in the WASM environment where the bug originally occurred.
