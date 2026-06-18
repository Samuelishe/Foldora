# Foldora

Foldora is a working name for a lightweight Windows 11 utility for creating folders with custom icons from a user-defined Explorer menu.

The project is in an early MVP / experimental stage. It is usable for local testing, but it does not have an installer, modern Windows 11 context menu integration, or a stable public release flow yet.

## Idea

Foldora is not an icon pack manager first. The main object is a user-created menu entry:

- choose any `.ico` file;
- write the menu label;
- write the folder name that should be created;
- decide whether the entry is enabled.

Foldora imports the icon into AppData, stores settings as JSON, and generates a safe HKCU legacy Explorer context menu from saved entries.

Example visible legacy menu:

```text
Создать папку
  Череп
  Музыка
```

Selecting an entry creates a folder and applies its icon through `desktop.ini`.

## Current Capabilities

- WPF editor for the user menu.
- Add/remove menu entries.
- Staged save: changes are not written until `Save`.
- `.ico` import into `%AppData%\Foldora\icons\`.
- Direct `.ico` preview in the WPF editor.
- Safe HKCU legacy context menu registration under Foldora-owned registry roots.
- No-console `Foldora.MenuHost.exe` for Explorer menu commands.
- Small icons in the legacy Explorer menu.
- Folder creation with a custom icon through `desktop.ini`.
- Unregister flow that disables Explorer integration without deleting entries.
- Reset flow that clears the user menu and disables Explorer integration.

## Platform and Stack

- Windows 11.
- C#.
- .NET 10.
- WPF.
- xUnit tests.

## Data Location

Foldora stores user data under:

```text
%AppData%\Foldora\
%AppData%\Foldora\settings.json
%AppData%\Foldora\icons\
```

Imported icons are copied into the `icons` directory. The original source icon file is not used as the permanent menu icon path.

## Build and Run

```text
dotnet build Foldora.sln
dotnet test Foldora.sln
dotnet run --project src/Foldora.App/Foldora.App.csproj
```

The project currently targets .NET 10. Do not retarget it to .NET 8 for this repository state.

## Basic CLI Example

After building/publishing, use the CLI executable for manual commands:

```text
foldora menu add --icon "<path-to-icon.ico>" --name "Череп" --folder-name "Череп"
foldora register-menu --host-path "<path-to-Foldora.MenuHost.exe>"
foldora unregister-menu
```

`Foldora.MenuHost.exe` is the no-console executable intended for Explorer context menu commands. `Foldora.Cli.exe` remains a console tool for manual commands and diagnostics.

## Limitations

- Modern Windows 11 context menu integration is not implemented.
- The current Explorer integration uses the legacy context menu, so the menu may appear under `Show more options`.
- Creating a desktop folder exactly under the mouse cursor is not supported in the current legacy-menu MVP; Explorer chooses the new desktop icon position.
- No installer/MSIX yet.
- No icon pack import/export yet.
- No PNG-to-ICO conversion yet.
- No Explorer restart or icon cache reset flow.

## Documentation

Detailed project documentation starts at [docs/README.md](docs/README.md).
