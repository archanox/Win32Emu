# Game Info Window - UI Mockup

## Window Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│  Game Information                                            [_][□][X]│
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ Game Title                                                    │  │
│  │ ┌───────────────────────────────────────────────────────────┐│  │
│  │ │ [Enter game title]                                        ││  │
│  │ └───────────────────────────────────────────────────────────┘│  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                       │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ File Information                                              │  │
│  │                                                               │  │
│  │  File Name:          ign_win.exe                             │  │
│  │  File Size:          2.45 MB                                 │  │
│  │  Compiled:           1998-11-15 14:23:41 UTC                 │  │
│  │  Machine Type:       Intel 386 or later processors           │  │
│  │  Minimum OS:         Windows 95 / NT 4.0                     │  │
│  │  OS Version:         4.00                                    │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                       │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ DLL Imports                                                   │  │
│  │ ┌───────────────────────────────────────────────────────────┐│  │
│  │ │ KERNEL32.DLL    CreateFileA                  ✓ Yes (green)││  │
│  │ │ KERNEL32.DLL    ReadFile                     ✓ Yes (green)││  │
│  │ │ KERNEL32.DLL    WriteFile                    ✓ Yes (green)││  │
│  │ │ USER32.DLL      CreateWindowExA              ✓ Yes (green)││  │
│  │ │ USER32.DLL      ShowWindow                   ✓ Yes (green)││  │
│  │ │ GDI32.DLL       CreateCompatibleDC           ✓ Yes (green)││  │
│  │ │ DDRAW.DLL       DirectDrawCreate             ✗ No (red)   ││  │
│  │ │ DSOUND.DLL      DirectSoundCreate            ✗ No (red)   ││  │
│  │ │ WINMM.DLL       timeGetTime                  ✓ Yes (green)││  │
│  │ │ ...                                                        ││  │
│  │ └───────────────────────────────────────────────────────────┘│  │
│  │                                                               │  │
│  │  [● Implemented]  [● Not Implemented]                        │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                       │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ Environment Variables                                         │  │
│  │ ┌───────────────────────────────────────────────────────────┐│  │
│  │ │ [KEY=value (one per line)]                                ││  │
│  │ │                                                            ││  │
│  │ └───────────────────────────────────────────────────────────┘│  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                       │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ Program Arguments                                             │  │
│  │ ┌───────────────────────────────────────────────────────────┐│  │
│  │ │ [Command line arguments]                                   ││  │
│  │ └───────────────────────────────────────────────────────────┘│  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                       │
│              [Copy GameDB Stub] [Open in VirusTotal]                 │
│                                [Save Changes] [Close]                 │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

## Context Menu Access

From the Game Library view:

```
┌─────────────────────────────────────────────────────────────────────┐
│  Game Library                           [Add Folder] [Add Game]      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                          │
│  │   🎮     │  │   🎮     │  │   🎮     │◄─ Right-click            │
│  │          │  │          │  │          │                            │
│  │ Ignition │  │ Game 2   │  │ Game 3   │   ┌──────────────────┐  │
│  │          │  │          │  │          │   │ View Info        │  │
│  │ Played:  │  │ Played:  │  │ Played:  │   ├──────────────────┤  │
│  │ 5 times  │  │ 2 times  │  │ 0 times  │   │ Launch           │  │
│  │          │  │          │  │          │   ├──────────────────┤  │
│  │[Launch]  │  │[Launch]  │  │[Launch]  │   │ Remove           │  │
│  │[Remove]  │  │[Remove]  │  │[Remove]  │   └──────────────────┘  │
│  └──────────┘  └──────────┘  └──────────┘                          │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

## Color Scheme

- **Implemented Imports**: Green (#28A745) with checkmark "✓ Yes"
- **Not Implemented Imports**: Red (#DC3545) with X mark "✗ No"
- **Section Backgrounds**: System-themed alternating backgrounds
- **Text**: System-themed foreground colors

## Interaction Flow

1. **Opening the Window**
   ```
   User Action: Right-click on game → Select "View Info"
   Result: Game Info Window opens with all data pre-loaded
   ```

2. **Copying GameDB Stub**
   ```
   User Action: Click "Copy GameDB Stub"
   Feedback: Button changes to "✓ Copied!" for 2 seconds
   Result: JSON copied to clipboard
   ```

3. **Opening VirusTotal**
   ```
   User Action: Click "Open in VirusTotal"
   Result: Browser opens to VirusTotal page with SHA256 hash
   ```

4. **Saving Changes**
   ```
   User Action: Edit title → Click "Save Changes"
   Result: Changes persisted to library.json
   ```

## Example GameDB Stub Output

When clicking "Copy GameDB Stub", this JSON is copied to clipboard:

```json
{
  "id": "a7f8e3b2-4c9d-4e1f-8a3b-2c9d4e1f8a3b",
  "title": "Ignition",
  "description": "Racing game from 1998",
  "executables": [
    {
      "name": "ign_win.exe",
      "md5": "42aeaf49af6191400fa18ba3e3c47e48",
      "sha1": "eda557a84013bcf42100c3dd43e40263cb8d3353",
      "sha256": "52b0c3a95cc70eb909b46d5f872a6779eb228b1925274c9da072463934ff2099"
    }
  ]
}
```

## Responsive Design

The window is:
- **Width**: 800px
- **Height**: 700px
- **Scrollable**: Content scrolls vertically if needed
- **Modal**: Can be multiple instances open at once
- **Centered**: Opens centered on the owner window

## Accessibility Features

1. **Keyboard Navigation**: Full keyboard support for all controls
2. **Screen Reader Support**: Proper ARIA labels and descriptions
3. **High Contrast**: Uses system theme colors for better visibility
4. **Tooltips**: (Future) Tooltips on technical fields explaining what they mean

## Performance

- **Load Time**: < 100ms for typical executables
- **PE Parsing**: Lazy-loaded on window open
- **Hash Calculation**: Computed once, cached in memory
- **Import List**: Scrollable for games with many imports (100+)
