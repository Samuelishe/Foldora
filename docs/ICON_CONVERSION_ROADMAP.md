# Icon Conversion Roadmap

Этот документ фиксирует план для image-to-ICO conversion. IC1 foundation реализует техническую основу ICO container writing, IC2a добавляет Windows-specific decode/PNG encode foundation, IC2b добавляет pure alpha-aware resize/downscale foundation, IC2c связывает эти части в Windows-specific stream-based image-to-ICO conversion service, IC3 добавляет single-file CLI `convert-icon`, IC4a подключает auto-conversion к WPF icon picker, а IC4b добавляет drag/drop replacement на preview иконки. Converter window, batch conversion, pack import/export, generated icon cleanup and repair flows are not implemented yet.

## Current Implementation Status

IC1 implemented:

- `src/Foldora.Imaging` project scaffold targeting `net10.0`.
- Standard square icon frame-size model for `16`, `24`, `32`, `48`, `64`, `128` and `256`.
- Minimal conversion options/result/error models for future pipeline stages.
- `IcoWriter`, which writes a deterministic `.ico` container from already encoded PNG frame payload bytes.
- Binary structure tests for ICONDIR, ICONDIRENTRY fields, 256x256 directory byte encoding, sorted frame order, offsets, payload concatenation, validation and stream ownership.

IC2a implemented:

- `src/Foldora.Imaging.Windows` project scaffold targeting `net10.0-windows`.
- `RgbaImage` tightly packed RGBA buffer model in pure `Foldora.Imaging`.
- Windows/WPF decoding of PNG/JPG/JPEG/BMP streams to `RgbaImage`.
- PNG frame payload encoding from `RgbaImage`.
- Tests for decode/encode behavior, alpha/opaque handling, stream ownership and ICO writer compatibility.

IC2b implemented:

- Pure `RgbaImage` resize/downscale foundation in `Foldora.Imaging`.
- Lanczos3 separable resize from `RgbaImage` to `RgbaImage`.
- Premultiplied-alpha filtering to avoid transparent RGB halo artifacts.
- Tests for dimensions, validation, constant-color normalization, alpha behavior, deterministic output, edge cases and ICO writer compatibility.

IC2c implemented:

- `WindowsImageToIconConverter` service/foundation in `Foldora.Imaging.Windows`.
- Stream-based PNG/JPG/JPEG/BMP to multi-size ICO conversion pipeline.
- Decode source image once, then resize every target frame size from the original decoded `RgbaImage`.
- Encode each resized square frame as PNG payload and write a deterministic ICO container.
- Default standard frame sizes: `16`, `24`, `32`, `48`, `64`, `128`, `256`.
- Custom target frame sizes with sorted deterministic output.
- Contain-fit transparent square policy for non-square source images.
- Tests for standard/custom sizes, stream validation/ownership, source report, alpha, tiny sources and non-square contain-fit behavior.

IC3 implemented:

- `Foldora.Cli.exe convert-icon --input "<image>" --output "<icon.ico>" [--force]`.
- Single-file PNG/JPG/JPEG/BMP to multi-size ICO conversion through `WindowsImageToIconConverter`.
- Default frames: `16`, `24`, `32`, `48`, `64`, `128`, `256`.
- `--force` controls overwrite; without it existing output is rejected.
- Safe temp-file write before moving to the final `.ico` path.
- Parser/help/runner tests, file validation tests and manual CLI smoke.

IC4a implemented:

- WPF icon picker accepts `.ico`, `.png`, `.jpg`, `.jpeg` and `.bmp`.
- Existing `.ico` selection keeps the staged import-on-save workflow.
- PNG/JPG/JPEG/BMP selections are converted immediately to generated multi-size `.ico` files under `%AppData%\Foldora\icons\generated`.
- Generated `.ico` paths are staged, previewed and saved as normal `IconPath` values.
- Conversion failures keep the previous icon unchanged and remove temp/partial output where possible.
- App/service/ViewModel tests cover generated icon storage, picker flow, error handling and project boundaries.

IC4b implemented:

- Dragging one `.ico`, `.png`, `.jpg`, `.jpeg` or `.bmp` file onto an entry icon preview updates the staged icon.
- Existing `.ico` drops reuse the staged import-on-save workflow.
- Raster drops reuse the existing App icon preparation/conversion service and produce generated multi-size `.ico` files.
- Unsupported, multiple-file, directory and failed/corrupt drops leave the current entry icon unchanged and report a user-facing error.

Not implemented yet:

- Drag-and-drop ordering.
- Batch/directory CLI conversion.
- Converter window.
- Generated icon cleanup.
- Pack import/export.
- SVG support.

## Priority

Текущий feature priority для Foldora после IC4b:

1. Converter window / batch conversion.
2. Drag-and-drop ordering.
3. Pack import/export.
4. Diagnostics/repair.
5. Release/install packaging polish.

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

Projects:

```text
src/Foldora.Imaging/          pure net10.0
src/Foldora.Imaging.Windows/  Windows-specific net10.0-windows
```

Purpose:

- pure icon frame metadata, conversion options/result models and tightly packed RGBA buffer model;
- ICO writing, IC1 foundation implemented;
- Windows/WPF image decode and PNG encode bridge, IC2a foundation implemented;
- alpha-aware Lanczos3 resize/downscale, IC2b foundation implemented;
- stream-based image-to-ICO conversion service, IC2c foundation implemented;
- conversion result/reporting.

Suggested future services/classes:

- `WindowsImageToIconConverter`;
- `WindowsImageDecoder`;
- `WindowsPngFrameEncoder`;
- `IconEncoder` / `IcoWriter`;
- `RgbaImageResizer`;
- `ImageResizeOptions`;
- `ImageResizeFilter`;
- `IconConversionOptions`
- `IconConversionResult`
- `IconFrameSize`
- `RgbaImage`
- `IconImageFitMode`

Dependency direction:

- `Foldora.Core` must not depend on `Foldora.Imaging`.
- `Foldora.Imaging` must stay pure `net10.0` without WPF/Windows imaging dependencies.
- `Foldora.Imaging.Windows` may depend on `Foldora.Imaging` and Windows/WPF imaging APIs.
- `Foldora.App` may use `Foldora.Imaging.Windows` for picker/drop auto-conversion.
- `Foldora.Cli` may use `Foldora.Imaging.Windows` for the Windows-only `convert-icon` command.
- `Foldora.MenuHost` should not need `Foldora.Imaging` or `Foldora.Imaging.Windows`.

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
- use the IC2b `RgbaImageResizer` Lanczos3 foundation for high-quality downscale;
- handle alpha through premultiplied-alpha filtering;
- preserve transparency;
- write PNG-compressed frames inside ICO if compatible with Windows Explorer.

IC2c source aspect-ratio policy:

- ICO frames are always square.
- Square sources are resized directly to each target square size.
- Non-square sources use contain-fit into a transparent square canvas.
- The source aspect ratio is preserved.
- The resized content is centered.
- Padding pixels are transparent RGBA `0,0,0,0`.
- Cropping/fill modes are not implemented yet.

Implementation preference: avoid third-party conversion libraries if reasonably possible. System Windows/WPF decoders for PNG/JPG/BMP may be acceptable. Resize algorithm and ICO writer should preferably be implemented in project code.

Gamma-correct resizing remains deferred/research. IC2b performs alpha-aware Lanczos-style resizing in byte/sRGB value space; this is materially better than nearest/bilinear for the MVP converter foundation, but it does not claim physically perfect color-space handling.

## WPF Picker UX

Implemented file picker filter:

```text
Icon/image files (*.ico;*.png;*.jpg;*.jpeg;*.bmp)
```

Behavior:

- if user selects `.ico`, existing import behavior stays;
- if user selects `.png`, `.jpg`, `.jpeg` or `.bmp`, Foldora auto-converts to a generated multi-size `.ico`;
- generated `.ico` is written under `%AppData%\Foldora\icons\generated`;
- generated filenames use a sanitized source base name plus a short content hash;
- entry preview updates after conversion;
- save persists a normal icon path;
- discard reverts the staged entry, but may leave an unused generated `.ico`;
- conversion failure keeps the previous icon unchanged;
- user should not need to understand ICO internals.

Drag image onto icon preview implemented in IC4b:

```text
Drop .ico/.png/.jpg/.jpeg/.bmp onto entry icon preview
  -> replace staged/pending icon
  -> auto-convert if needed
  -> update preview
  -> Save persists it
```

Only one dropped file is accepted. Unsupported files, directories, multiple files and failed conversion/decode leave the previous staged icon unchanged. This is a UX layer over the IC4a `IconAssetPreparationService`, not a second conversion pipeline.

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

Do not mix converter-window work with drag/drop ordering or SVG. The next converter-window milestone should stay on single/batch PNG/JPG/BMP-to-ICO workflows over the existing conversion service.

## CLI Conversion

Implemented single-file command:

```powershell
Foldora.Cli.exe convert-icon --input ".\image.png" --output ".\folder.ico"
Foldora.Cli.exe convert-icon --input ".\image.png" --output ".\folder.ico" --force
```

Current behavior:

- supported input formats: `.png`, `.jpg`, `.jpeg`, `.bmp`;
- output format: multi-size `.ico`;
- default frame sizes: `16`, `24`, `32`, `48`, `64`, `128`, `256`;
- non-square images use contain-fit transparent square frames;
- output is not overwritten unless `--force` is provided.

Possible later flags/features:

```powershell
Foldora.Cli.exe convert-icon --input ".\images" --output ".\icons" --recursive
```

Rationale:

- test conversion without WPF;
- useful for power users;
- supports batch workflows later;
- stays PowerShell-friendly.

## Generated Icon Storage And Cleanup

Current manual workflow can still have users manually place icons in `%AppData%\Foldora\icons` once. That does not automatically create junk by itself.

IC4a/IC4b generated icon workflow:

```text
%AppData%\Foldora\icons\generated\
```

The WPF picker and preview drop flow write generated multi-size `.ico` files there when the user selects or drops `.png`, `.jpg`, `.jpeg` or `.bmp`. The settings file stores the generated `.ico` path as a normal `IconPath`; it does not store the original source image path or conversion options.

Potential orphan/generated icon cases become more important later:

- UI imports/copies icons;
- user replaces an entry icon;
- user deletes an entry;
- image converter generates ICO files;
- pack import copies icons;
- failed/aborted conversions leave temp/generated files.

Conclusion: orphan icon cleanup is deferred. It is now relevant for generated picker/drop icons and becomes more important after converter/batch work and pack import/export.

Future storage separation idea:

```text
%AppData%\Foldora\icons\imported\
%AppData%\Foldora\icons\generated\
%AppData%\Foldora\icons\packs\
```

Do not move existing imported icons until a cleanup/import migration is explicitly designed.

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
