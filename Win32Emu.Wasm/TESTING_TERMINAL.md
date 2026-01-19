# Testing the Terminal Emulator Integration

## Manual Testing Steps

### 1. Build and Run the WASM Project

```bash
cd Win32Emu.Wasm
dotnet watch run
```

Open your browser to `https://localhost:5001` (or the port shown in the console).

### 2. Test Standard Output Terminal

#### Test A: Initial Display
- The Standard Output panel should now show an xterm.js terminal with a dark theme
- The terminal should be empty initially (black background with slight blue tint)

#### Test B: Load a Sample Executable
1. Click "Simple DirectDraw" or any sample button
2. Click "Start"
3. Watch the Standard Output terminal for:
   - "Win32Emu Emulator Starting" message
   - "Executable loaded and ready" message
   - Any console output from the running application

#### Test C: Terminal Features
1. Verify scrollback works (if there's enough output)
2. Test the "Clear" button - terminal should clear all content
3. Verify ANSI color codes work (if the application outputs colors)

#### Test D: Console Output
1. Look for messages like:
   - Emulator initialization messages
   - DirectDraw API calls (if debug output is routed to stdout)
   - Any `printf` or `Console.WriteLine` output from the emulated application

### 3. Visual Verification

The terminal should display:
- **Theme**: Dark background (#1e1e1e), light gray text (#d4d4d4)
- **Cursor**: Green blinking cursor
- **Font**: Consolas or Courier New monospace
- **Size**: Auto-fit to the card body (approximately 100 cols x 12 rows)

### 4. Browser Console Check

Open browser DevTools (F12) and check for:
- No JavaScript errors related to xterm.js
- Console logs like:
  - `Created xterm.js terminal: stdout-terminal (100x12)`
  - `Canvas updated successfully: 640x480`

## Expected Behavior

### On Page Load
- Terminal component initializes
- xterm.js loads from CDN
- Terminal is ready but shows no content

### On Executable Load
- "Starting emulation..." appears in terminal
- Initial messages appear with timestamps
- Terminal scrolls as output appears

### On Stop
- "Emulation stopped" message appears
- Terminal retains previous output (scrollback preserved)

### On Clear
- Terminal clears all content
- Cursor returns to top-left
- No scrollback history

## Common Issues

### Terminal Not Visible
- Check browser console for errors loading xterm.js from CDN
- Verify container div has proper height (`height: 200px`)
- Check if `createXtermTerminal` JavaScript function was called

### No Output Appears
- Verify `OnEmulatorStdOutput` event handler is wired up
- Check that `_stdoutTerminal` reference is not null
- Look for errors in browser console about `WriteAsync` calls

### Theme Issues
- Verify xterm.css loaded from CDN
- Check that theme configuration matches in JavaScript

## Future Testing

Consider adding:
1. Automated Playwright tests to verify terminal initialization
2. Unit tests for TerminalComponent (using bUnit for Blazor)
3. Integration tests that verify output appears in terminal after emulator operations
