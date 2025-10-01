# Win32Emu GUI - Visual Layout

## Main Window Structure

```
┌────────────────────────────────────────────────────────────────────────┐
│ Win32Emu - Windows 32-bit Emulator                          [_][□][X] │
├──────────┬─────────────────────────────────────────────────────────────┤
│          │                                                             │
│ Win32Emu │  Game Library                              [Add Game]      │
│          │                                                             │
│ ┌──────┐ │  ┌─────────┐  ┌─────────┐  ┌─────────┐                    │
│ │ 📚   │ │  │   🎮    │  │   🎮    │  │   🎮    │                    │
│ │Libr- │ │  │         │  │         │  │         │                    │
│ │ary   │ │  │Ignition │  │  Game2  │  │  Game3  │                    │
│ └──────┘ │  │         │  │         │  │         │                    │
│          │  │Classic  │  │Racing   │  │Action   │                    │
│ ┌──────┐ │  │racing   │  │game     │  │game     │                    │
│ │ ⚙️   │ │  │Played:5 │  │Played:0 │  │Played:2 │                    │
│ │Sett- │ │  │         │  │         │  │         │                    │
│ │ings  │ │  │[Launch] │  │[Launch] │  │[Launch] │                    │
│ └──────┘ │  │[Remove] │  │[Remove] │  │[Remove] │                    │
│          │  └─────────┘  └─────────┘  └─────────┘                    │
│          │                                                             │
│          │                                                             │
└──────────┴─────────────────────────────────────────────────────────────┘
```

## Settings View

```
┌────────────────────────────────────────────────────────────────────────┐
│ Win32Emu - Windows 32-bit Emulator                          [_][□][X] │
├──────────┬─────────────────────────────────────────────────────────────┤
│          │                                                             │
│ Win32Emu │  Emulator Settings                                         │
│          │                                                             │
│ ┌──────┐ │  Rendering Backend                                         │
│ │ 📚   │ │  ┌────────────────────────────────────────┐                │
│ │Libr- │ │  │ Software                        ▼      │                │
│ │ary   │ │  └────────────────────────────────────────┘                │
│ └──────┘ │  Select the graphics rendering backend for games           │
│          │                                                             │
│ ┌──────┐ │  Resolution Scale Factor                                   │
│ │ ⚙️   │ │  ┌────────────────────────────────────────┐                │
│ │Sett- │ │  │ 1                               ▼      │                │
│ │ings  │ │  └────────────────────────────────────────┘                │
│ └──────┘ │  Upscale the game resolution by this factor                │
│          │                                                             │
│          │  Reserved Memory (MB)                                       │
│          │  ┌────────────────────────────────────────┐                │
│          │  │ 256                    [▲][▼]          │                │
│          │  └────────────────────────────────────────┘                │
│          │  Amount of memory to allocate for the emulated environment │
│          │                                                             │
│          │  Windows Version                                            │
│          │  ┌────────────────────────────────────────┐                │
│          │  │ Windows 95                      ▼      │                │
│          │  └────────────────────────────────────────┘                │
│          │  The Windows version to emulate for compatibility          │
│          │                                                             │
│          │  ☐ Enable Debug Mode                                       │
│          │  Enable enhanced debugging to catch memory access errors   │
│          │                                                             │
└──────────┴─────────────────────────────────────────────────────────────┘
```

## Key Features Illustrated

1. **Navigation Sidebar**: Clean, icon-based navigation between Library and Settings
2. **Game Cards**: Visual cards showing game information, thumbnails, and action buttons
3. **Settings Panel**: Form-based interface with dropdowns and numeric inputs
4. **Responsive Layout**: Designed to work on desktop platforms (Windows, macOS, Linux)

## Color Scheme
- Uses Avalonia's Fluent theme with system-aware light/dark mode
- Consistent with modern desktop application aesthetics
- Clean, minimalist design focusing on functionality
