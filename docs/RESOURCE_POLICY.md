# Resource Policy

Foldora may include visual resources such as icons, fonts, vector assets and small UI illustrations only when their license is clear and compatible with a public GitHub project.

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
- author;
- source URL;
- license name;
- local repository path;
- attribution text required by the license.

Attribution must be added to the root `README.md`. If bundled third-party resources grow beyond one or two small items, create a dedicated `THIRD_PARTY_NOTICES.md` or `docs/THIRD_PARTY_NOTICES.md`.

If a license requires including its full text, add it under a clear folder such as `licenses/` or `third-party/`.

## Current State

No third-party visual assets are currently bundled, except where explicitly listed in the root `README.md` or a future third-party notices file.

For small UI icons, prefer self-authored XAML/vector shapes or documented system UI fonts before adding external icon packs.
