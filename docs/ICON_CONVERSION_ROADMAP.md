# Icon Conversion Roadmap

Этот документ фиксирует будущий план для image-to-ICO conversion. Это не описание текущей реализации: PNG/JPG/BMP conversion, converter CLI/UI, drag-and-drop icon replacement, pack import/export and repair flows are not implemented yet.

## Priority

Следующий feature priority для Foldora:

1. Image -> ICO conversion foundation.
2. WPF icon picker integration for PNG/JPG/BMP auto-conversion.
3. Drag image onto icon preview.
4. Converter window / batch conversion.
5. Drag-and-drop ordering.
6. Pack import/export.
7. Diagnostics/repair.
8. Release/install packaging polish.

Release readiness, self-contained zip, installer polish, MSI/MSIX, winget and code signing remain important, but they are intentionally below MVP feature work until icon conversion and adjacent menu-editing UX are stronger.

## Image -> ICO Conversion

Goal:

```text
User selects PNG/JPG/BMP instead of ICO
  -> Foldora creates a multi-size ICO
  -> generated ICO is imported/staged like a normal folder icon
  -> saved menu entry still stores a normal IconPath to an .ico file
```

Phase 1 input formats:

- `.png`
- `.jpg`
- `.jpeg`
- `.bmp`

SVG is not part of the first conversion milestone. Treat SVG as phase 2 / research because true SVG rendering without third-party libraries is non-trivial, WPF is not a complete SVG renderer, and implementing full SVG manually would be too large for the first conversion pass. Possible future options:

- limited SVG subset;
- optional dependency after explicit license review;
- external conversion pipeline only if explicitly accepted later.

## Proposed Architecture

Future project:

```text
src/Foldora.Imaging/
```

Purpose:

- image decode orchestration;
- high-quality resizing;
- ICO writing;
- conversion result/reporting.

Suggested future services/classes:

- `ImageToIconConversionService`
- `ImageDecoder`
- `IconEncoder`
- `IconResizeService`
- `IconConversionOptions`
- `IconConversionResult`
- `IconFrameSize`

Dependency direction:

- `Foldora.Core` must not depend on `Foldora.Imaging`.
- `Foldora.Imaging` should avoid App/UI dependencies.
- `Foldora.App` may use `Foldora.Imaging`.
- `Foldora.Cli` may use `Foldora.Imaging`.
- `Foldora.MenuHost` should not need `Foldora.Imaging`.

Rationale: conversion belongs to app/CLI workflows. `Foldora.MenuHost` should stay small and focused on already-saved menu actions, and `Foldora.Core` should remain model/storage/validation oriented without pulling Windows imaging dependencies.

## ICO Output Design

Preferred frame sizes:

- 16x16
- 24x24
- 32x32
- 48x48
- 64x64
- 128x128
- 256x256

Minimum acceptable future set:

- 16x16
- 32x32
- 48x48
- 256x256

Quality rules:

- resize each target frame from the original/source image, not by chaining `256 -> 128 -> 64 -> ...`;
- use high-quality downscale, similar in intent to Photoshop-style resize for reduction;
- consider Lanczos3 or Mitchell-Netravali;
- handle alpha correctly;
- prefer premultiplied-alpha-aware processing;
- preserve transparency;
- write PNG-compressed frames inside ICO if compatible with Windows Explorer.

Implementation preference: avoid third-party conversion libraries if reasonably possible. System Windows/WPF decoders for PNG/JPG/BMP may be acceptable. Resize algorithm and ICO writer should preferably be implemented in project code.

## WPF UX Plan

File picker idea:

```text
Icon/image files (*.ico;*.png;*.jpg;*.jpeg;*.bmp)
```

Behavior:

- if user selects `.ico`, existing import behavior stays;
- if user selects `.png`, `.jpg`, `.jpeg` or `.bmp`, Foldora auto-converts to a generated multi-size `.ico`;
- generated `.ico` is copied into `%AppData%\Foldora\icons` or a future generated/imported subfolder;
- entry preview updates after conversion;
- save persists a normal icon path;
- user should not need to understand ICO internals.

Drag image onto icon preview:

```text
Drop .ico/.png/.jpg/.jpeg/.bmp onto entry icon preview
  -> replace staged/pending icon
  -> auto-convert if needed
  -> update preview
  -> Save persists it
```

This should come after the conversion foundation so the drag/drop path can reuse the same validation and conversion code.

Converter window:

```text
Tools -> Icon converter
```

Planned capabilities:

- single-file conversion;
- batch conversion;
- choose output folder;
- preview generated sizes;
- overwrite policy;
- conversion report/errors.

Do not build the converter window first. The first milestone should be engine + CLI + picker integration.

## CLI Plan

Planned command, not implemented:

```powershell
Foldora.Cli.exe convert-icon --input ".\image.png" --output ".\folder.ico"
```

Possible later flags:

```powershell
Foldora.Cli.exe convert-icon --input ".\image.png" --output ".\folder.ico" --force
Foldora.Cli.exe convert-icon --input ".\images" --output ".\icons" --recursive
```

Rationale:

- test conversion without WPF;
- useful for power users;
- supports batch workflows later;
- stays PowerShell-friendly.

## Generated Icon Storage And Cleanup

Current manual workflow often has users manually place icons in `%AppData%\Foldora\icons` once. That does not automatically create junk by itself.

Potential orphan/generated icon cases become more important later:

- UI imports/copies icons;
- user replaces an entry icon;
- user deletes an entry;
- image converter generates ICO files;
- pack import copies icons;
- failed/aborted conversions leave temp/generated files.

Conclusion: orphan icon cleanup is deferred. It becomes more relevant after auto-conversion and pack import/export exist.

Future storage idea, not current behavior:

```text
%AppData%\Foldora\icons\imported\
%AppData%\Foldora\icons\generated\
%AppData%\Foldora\icons\packs\
```

Do not change actual storage until the converter/import flow needs it.

## Pack Import / Export

Pack import/export is useful, but lower priority than converter and drag/drop icon UX.

Possible future format:

```text
*.foldorapack
```

Container idea:

```text
zip container:
  manifest.json
  icons/
  preview/
    optional-preview.png
```

Manifest draft:

```json
{
  "formatVersion": 1,
  "title": "Developer Pack",
  "entries": [
    {
      "displayName": "Rider",
      "folderName": "Rider",
      "groupName": "Development",
      "icon": "icons/rider.ico",
      "enabled": true,
      "sortOrder": 10
    }
  ]
}
```

Import flow:

- unpack to temp;
- validate manifest;
- validate icon paths;
- validate file count/size limits;
- copy selected icons into AppData storage;
- generate new entry ids;
- support merge/replace strategy later;
- never blindly overwrite user settings without confirmation.

Export flow:

- collect selected/current entries;
- copy used icons;
- write manifest;
- zip into `.foldorapack`.

Security/resource policy:

- packs are user-provided content;
- bundled official packs, if ever added, require license review;
- third-party icons in packs need explicit rights/notices;
- Foldora should not imply ownership over imported third-party icons.

## Drag-And-Drop Ordering

Important UX feature after converter work:

- reorder entries within a group;
- move entry between groups;
- reorder groups;
- reuse existing `SortOrder` if the current flat model remains sufficient;
- preserve staged edit model;
- keep Save/Discard behavior clear.

Future group polish ideas:

- reorder groups;
- drag entries between groups;
- collapse/expand groups;
- optional empty group as first-class concept;
- duplicate entry;
- duplicate group;
- group context menu if useful.

Do not describe rename/delete groups as missing; the current app already has group rename/delete presentation for non-empty groups.

## Diagnostics / Repair

Useful later, not the next priority.

Possible Settings -> Diagnostics / Repair scenarios:

1. Registry points to missing `Foldora.MenuHost.exe`.
2. Explorer menu enabled but host path invalid.
3. User moved/deleted install folder.
4. `settings.json` corrupted or cannot be parsed.
5. `IconPath` points to missing `.ico`.
6. Imported/generated icon file missing.
7. Menu has enabled entries but registry roots are missing.
8. Registry roots exist but settings says integration disabled.
9. `desktop.ini` references missing AppData icon.
10. MenuHost log exists with recent failures.

Possible actions:

- Check integration.
- Repair Explorer menu.
- Disable stale menu.
- Validate icons.
- Open logs folder.
- Open settings file location.

Priority: do after core MVP feature work.

