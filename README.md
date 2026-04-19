# autoplugins

Plugins for [Auto](https://github.com/hvards/auto).

## Plugins

| Plugin | Description |
|---|---|
| **CloseWindow** | Close a window by handle or process name. |
| **FocusWindow** | Focus a window by handle or process name. |
| **WindowKeys** | Get window handle by typing label. |

## Building

```powershell
dotnet build
```

Each plugin's build deploys to `~/.config/auto/plugins/<PluginName>/`.
