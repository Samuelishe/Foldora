# Foldora

License: 0BSD

## About

Foldora is a working name for a lightweight Windows 11 utility for creating folders with custom icons from a user-defined Explorer menu.

The project is in an early MVP / experimental stage. It is usable for local testing through a per-user install script, but it does not have an MSI/MSIX installer, modern Windows 11 context menu integration, or a stable public release flow yet.

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
  Цветные
    Синяя
    Красная
  Готические
    Череп
  Музыка
```

Selecting an entry creates a folder and applies its icon through `desktop.ini`.

## Current Capabilities

- WPF editor for the user menu.
- Compact/edit entry cards for menu entries.
- Add/remove menu entries.
- One-level grouping for menu entries.
- `DisplayName`, `DefaultFolderName`, `GroupName`, and enabled/disabled entry state.
- Staged save: changes are not written until `Save`.
- `.ico` import into `%AppData%\Foldora\icons\`.
- Direct `.ico` preview in the WPF editor.
- Safe HKCU legacy context menu registration under Foldora-owned registry roots.
- No-console `Foldora.MenuHost.exe` for Explorer menu commands.
- Small icons in the legacy Explorer menu.
- Folder creation with a custom icon through `desktop.ini`.
- Best-effort desktop icon placement for folders created from the desktop background legacy menu.
- WPF localization for Russian, English, Simplified Chinese, German, Spanish, French, Japanese, Brazilian Portuguese, Korean, Ukrainian, Polish, Turkish, Romanian, Czech, Hungarian and Bulgarian; new entries use localized defaults for the current UI language.
- Deletion-friendly folder icon attributes: folder `ReadOnly`, `desktop.ini` `Hidden`.
- Unregister flow that disables Explorer integration without deleting entries.
- Reset flow that clears the user menu and disables Explorer integration.

## Platform and Stack

- Windows 11.
- C#.
- .NET 10.
- WPF.
- xUnit tests.

## Minimum Requirements

To build from source:

- Windows 11.
- .NET SDK 10.x.
- PowerShell 7 is recommended for the documented local workflow, but it is not a runtime dependency.
- Git is needed only for working with the repository.

To run with `dotnet run` or a framework-dependent build:

- Windows 11.
- .NET 10 Windows Desktop Runtime, unless the app is published self-contained.
- Explorer legacy context menu support.

Foldora uses HKCU registry keys only for Explorer integration and does not require administrator rights.

Windows 10 and non-Windows platforms are not claimed as supported for the current MVP.

## Data Location

Foldora stores user data under:

```text
%AppData%\Foldora\
%AppData%\Foldora\settings.json
%AppData%\Foldora\icons\
%AppData%\Foldora\previews\
%AppData%\Foldora\packs\
```

Imported icons are copied into the `icons` directory. The original source icon file is not used as the permanent menu icon path.

Per-user installed binaries live separately under:

```text
%LocalAppData%\Programs\Foldora\Foldora.App.exe
%LocalAppData%\Programs\Foldora\Foldora.Cli.exe
%LocalAppData%\Programs\Foldora\Foldora.MenuHost.exe
```

Uninstall keeps `%AppData%\Foldora` by default because existing styled folders can reference imported icons from `%AppData%\Foldora\icons`.

## Localization

Complete enabled UI languages are Russian (`ru`), English (`en`), Simplified Chinese (`zh-Hans`), German (`de`), Spanish (`es`), French (`fr`), Japanese (`ja`), Brazilian Portuguese (`pt-BR`), Korean (`ko`), Ukrainian (`uk`), Polish (`pl`), Turkish (`tr`), Romanian (`ro`), Czech (`cs`), Hungarian (`hu`) and Bulgarian (`bg`). On first WPF launch, Foldora chooses the system UI language only if it is complete and enabled; unsupported system languages fall back to English. The selected language is saved and is not re-detected on later launches. Changing the application language changes UI labels, status text, the untouched default menu title, and defaults for newly created entries. It does not rewrite a custom menu title, entry names, folder names, or group names.

## Development Run

```text
dotnet restore Foldora.sln
dotnet build Foldora.sln
dotnet test Foldora.sln
dotnet run --project src/Foldora.App/Foldora.App.csproj
```

The project currently targets .NET 10. Do not retarget it to .NET 8 for this repository state.

## Dev Publish

For repeatable manual Explorer testing without an installer, create the dev publish layout:

```text
pwsh scripts/publish-dev.ps1
```

The script publishes framework-dependent Release builds into:

```text
artifacts/publish/Foldora/
artifacts/publish/Foldora/Foldora.App.exe
artifacts/publish/Foldora/Foldora.Cli.exe
artifacts/publish/Foldora/Foldora.MenuHost.exe
```

The script does not register the Explorer menu and does not start the app. A published build requires the .NET 10 Windows Desktop Runtime unless a future self-contained publish mode is added.

## Per-User Install

To install the current framework-dependent build for the current user:

```text
pwsh scripts/install-user.ps1
```

The script refreshes the dev publish output and copies it to:

```text
%LocalAppData%\Programs\Foldora\
%LocalAppData%\Programs\Foldora\Foldora.App.exe
%LocalAppData%\Programs\Foldora\Foldora.Cli.exe
%LocalAppData%\Programs\Foldora\Foldora.MenuHost.exe
```

It does not require admin rights, does not register Explorer integration, and does not start the app. After install, run `%LocalAppData%\Programs\Foldora\Foldora.App.exe` and enable Explorer integration from the UI.

When the installed app enables Explorer integration, the registry command should point to the installed sibling `Foldora.MenuHost.exe`:

```text
%LocalAppData%\Programs\Foldora\Foldora.MenuHost.exe
```

`Foldora.MenuHost.exe` is not a service, tray app, background helper, or autostart process. It is a short-lived no-console executable launched by Explorer only when the user clicks a Foldora context-menu command.

## Uninstall

To unregister the menu and remove installed binaries:

```text
pwsh scripts/uninstall-user.ps1
```

By default this keeps:

```text
%AppData%\Foldora\
```

That preserves settings, imported icons and logs. This matters because already styled folders can have `desktop.ini` entries that reference imported `.ico` files under `%AppData%\Foldora\icons`.

Optional full user-data removal:

```text
pwsh scripts/uninstall-user.ps1 -RemoveUserData
```

Use `-RemoveUserData` only when you intentionally want to delete settings, imported icons and logs; existing styled folders can lose their custom icons.

## Basic CLI Example

After building/publishing, use the CLI executable for manual commands:

```text
foldora menu add --icon "<path-to-icon.ico>" --name "Череп" --folder-name "Череп"
foldora menu add --icon "<path-to-blue.ico>" --name "Синяя" --folder-name "Синяя" --group "Цветные"
foldora register-menu --host-path "<path-to-Foldora.MenuHost.exe>"
foldora unregister-menu
```

`Foldora.MenuHost.exe` is the short-lived no-console executable intended for Explorer context menu commands. `Foldora.Cli.exe` remains a console tool for manual commands and diagnostics.

When testing the manual publish layout, Explorer integration should point to the published sibling MenuHost:

```text
artifacts/publish/Foldora/Foldora.Cli.exe register-menu --host-path "<repo>\artifacts\publish\Foldora\Foldora.MenuHost.exe"
```

If you enable Explorer integration from `artifacts/publish/Foldora/Foldora.App.exe`, the WPF app resolves the sibling `Foldora.MenuHost.exe` from the same publish folder.

## Limitations

- Modern Windows 11 context menu integration is not implemented.
- The current Explorer integration uses the legacy context menu, so the menu may appear under `Show more options`.
- Exact original right-click desktop placement is not available from the current legacy-menu MVP. Foldora does a best-effort move of newly created desktop folder icons near the cursor/menu selection position, and Explorer may snap or shift icons according to its grid/layout rules.
- No MSI/MSIX installer yet.
- Per-user install script is available under `%LocalAppData%\Programs\Foldora`, but there is no Program Files layout, code signing, winget package or MSIX package yet.
- No icon pack import/export yet.
- No PNG-to-ICO conversion yet.
- No full nested tree storage beyond the current one-level `GroupName`.
- No drag-and-drop ordering or group icons yet.
- No orphan icon cleanup yet.
- No user-facing diagnostics if `Foldora.MenuHost.exe` fails when invoked by Explorer.
- Localization debt remains for CLI diagnostics/validation output, startup fatal errors and external translation review; WPF catalogs are complete for the enabled locales.
- No Explorer restart or icon cache reset flow.

## Safety Disclaimer

Foldora is experimental early MVP software. It is provided as-is, without warranty. The author is not liable for loss, damage, configuration issues, Explorer behavior, shell behavior, or other problems caused by use or modification of the software.

Foldora modifies user-level HKCU registry keys only under Foldora-owned paths and creates/edits `desktop.ini` inside folders selected or created by the user. Test on non-critical folders first.

## AI Assistance Note

Parts of Foldora were developed with assistance from OpenAI Codex and other AI tools. Product decisions, architecture decisions, manual verification and commits are reviewed or performed by the maintainer.

AI-assisted development does not change ownership or license requirements for third-party materials. Third-party resources still require explicit license review before they can be added to the repository.

## License

Unless otherwise noted, original Foldora source code, documentation and self-authored project assets are licensed under the Zero-Clause BSD License (0BSD). See [LICENSE](LICENSE).

0BSD applies only to materials whose rights belong to the Foldora author/contributors. Third-party components and assets are not relicensed by Foldora; they remain under their respective licenses and attribution requirements. If a README statement conflicts with a bundled third-party license, the third-party material's own license controls that material.

Русское пояснение: если явно не указано иное, оригинальный код Foldora, документация и созданные автором ресурсы распространяются под 0BSD. Сторонние материалы не перелицензируются автором Foldora; для них действуют их собственные лицензии.

## Third-Party Resources

No third-party visual assets are currently bundled. Third-party materials, if added later, are listed in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). Resource rules are documented in [docs/RESOURCE_POLICY.md](docs/RESOURCE_POLICY.md).

Free download availability is not enough to include an asset in this repository. Every bundled third-party resource must have an explicit license that allows Foldora's actual use, redistribution and attribution model.

## Documentation

Detailed project documentation starts at [docs/README.md](docs/README.md).
