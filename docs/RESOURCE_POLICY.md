# Resource Policy

Foldora may include visual resources such as icons, fonts, vector assets and small UI illustrations only when their license is clear and compatible with a public GitHub project.

Original Foldora source code, documentation and self-authored project assets are licensed under 0BSD unless explicitly noted otherwise. This does not relicense third-party materials.

## Allowed Sources

Acceptable resource licenses include:

- open source licenses such as MIT, Apache or BSD;
- SIL OFL for fonts;
- public domain or CC0;
- Creative Commons licenses with compatible terms and practical attribution requirements;
- other clearly documented free-to-use licenses that allow redistribution in this repository.

## Not Allowed

Do not add:

- resources with unknown license;
- ripped or proprietary assets;
- assets copied from commercial packs without explicit permission;
- files found on the internet without a source/license page;
- assets whose attribution requirements cannot be satisfied clearly.

## Required Metadata

When adding an external resource to git, record:

- resource name;
- type: dependency, font, icon, image, code or other;
- author or copyright holder;
- source URL;
- exact license name;
- license source URL or bundled license text path;
- local repository path;
- attribution text required by the license;
- whether the file was modified.

Attribution must be added to `THIRD_PARTY_NOTICES.md`. If the resource is user-visible or prominent, also mention it in the root `README.md`.

If a license requires including its full text, add it under a clear folder such as `third-party/licenses/`.

## Required License Check

Before adding any third-party resource, verify:

1. Explicit license text or license metadata exists.
2. Redistribution is allowed.
3. Modification is allowed, if Foldora modifies the resource.
4. Commercial use is allowed, because 0BSD permits commercial reuse of Foldora's original materials.
5. Attribution requirements are clear and practical.
6. Bundling the full license text is either not required or is handled in the repository.
7. The license is compatible with the repository's public GitHub distribution model and does not conflict with 0BSD-covered original materials.

Free download availability alone is not a license. If the license is missing, unclear or incompatible, do not add the resource to git or include it in the build. Ask the author/rightsholder first.

When a resource is added, update in the same change:

- `THIRD_PARTY_NOTICES.md`;
- root `README.md`, if the resource is visible or prominent;
- `docs/FILE_INDEX.md`;
- bundled license files, if required;
- this policy document, if the policy itself changes.

## Current State

No third-party visual assets are currently bundled, except where explicitly listed in the root `THIRD_PARTY_NOTICES.md`.

The Foldora app icon under `src/Foldora.App/Assets/` is a self-authored folded blue/cyan folder mark with a broad light-cyan folded plane. `FoldoraIcon.svg` is the source vector and `Foldora.ico` is the generated Windows application icon; both follow the repository's default 0BSD licensing for self-authored Foldora materials.

For small UI icons, prefer self-authored XAML/vector shapes or documented system UI fonts before adding external icon packs.
