# MessageBox Visual Examples

## Error MessageBox (MB_OK | MB_ICONERROR)
```
┌─────────────────────────────────────────────┐
│ Application Error                      [X]  │
├─────────────────────────────────────────────┤
│                                             │
│  ❌   Backbuffer couldn't be obtained      │
│                                             │
├─────────────────────────────────────────────┤
│                            ┌────────┐       │
│                            │   OK   │       │
│                            └────────┘       │
└─────────────────────────────────────────────┘
```

## Warning MessageBox (MB_YESNO | MB_ICONWARNING)
```
┌─────────────────────────────────────────────┐
│ Warning                                [X]  │
├─────────────────────────────────────────────┤
│                                             │
│  ⚠️   Do you want to continue?             │
│                                             │
├─────────────────────────────────────────────┤
│            ┌────────┐  ┌────────┐          │
│            │  Yes   │  │   No   │          │
│            └────────┘  └────────┘          │
└─────────────────────────────────────────────┘
```

## Information MessageBox (MB_OKCANCEL | MB_ICONINFORMATION)
```
┌─────────────────────────────────────────────┐
│ Information                            [X]  │
├─────────────────────────────────────────────┤
│                                             │
│  ℹ️   Operation completed successfully    │
│                                             │
├─────────────────────────────────────────────┤
│            ┌────────┐  ┌────────┐          │
│            │   OK   │  │ Cancel │          │
│            └────────┘  └────────┘          │
└─────────────────────────────────────────────┘
```

## Question MessageBox (MB_YESNOCANCEL | MB_ICONQUESTION)
```
┌─────────────────────────────────────────────┐
│ Confirm                                [X]  │
├─────────────────────────────────────────────┤
│                                             │
│  ❓   Save changes before closing?        │
│                                             │
├─────────────────────────────────────────────┤
│     ┌────────┐  ┌────────┐  ┌────────┐    │
│     │  Yes   │  │   No   │  │ Cancel │    │
│     └────────┘  └────────┘  └────────┘    │
└─────────────────────────────────────────────┘
```

## Features
- **Unicode Emoji Icons**: ❌ (Error), ⚠️ (Warning), ℹ️ (Info), ❓ (Question)
- **Standard Button Layouts**: Matches Win32 MessageBox button ordering
- **Modal Behavior**: Blocks emulator execution until user responds
- **Keyboard Navigation**: Default and Cancel buttons for Enter/Esc keys
- **Center-Owner Positioning**: Appears centered over the main emulator window

## Technical Details
- Built with Avalonia UI framework
- Uses ShowDialog for modal behavior
- Returns proper Win32 button IDs (IDOK=1, IDCANCEL=2, IDYES=6, IDNO=7, etc.)
- Thread-safe using Dispatcher.UIThread
