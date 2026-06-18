# Third-Party Notices

Unless otherwise noted, original Foldora source code, documentation, and self-authored project assets are licensed under the Zero-Clause BSD License (0BSD). See [LICENSE](LICENSE).

Third-party components and assets remain under their respective licenses. Foldora does not relicense third-party materials.

## Bundled Visual Assets

No third-party visual assets are currently bundled in this repository.

Current WPF UI icons and vector shapes are self-authored XAML resources unless a future notice states otherwise.

## NuGet and .NET Dependencies

Foldora uses the .NET SDK/runtime and NuGet packages for building and testing. These dependencies remain under their own licenses.

Current direct NuGet package references are test-only dependencies in `tests/Foldora.Tests/Foldora.Tests.csproj`:

| Package | Purpose | License metadata | Local use |
| --- | --- | --- | --- |
| `coverlet.collector` | Test coverage collector | MIT, from local NuGet `.nuspec` metadata | Test project only |
| `Microsoft.NET.Test.Sdk` | .NET test SDK | MIT, from local NuGet `.nuspec` metadata | Test project only |
| `xunit` | Unit test framework | Apache-2.0, from local NuGet `.nuspec` metadata | Test project only |
| `xunit.runner.visualstudio` | xUnit test runner | Apache-2.0, from local NuGet `.nuspec` metadata | Test project only |

This file is not intended to replace each dependency's own license metadata. Before redistributing a packaged build with bundled third-party components or assets, review the relevant dependency licenses and add any required notices or license texts.

## Future Third-Party Materials

For every future bundled third-party resource, add a notice with:

- name;
- type;
- author or copyright holder;
- source URL;
- exact license;
- license URL or bundled license path;
- local repository path;
- required attribution;
- whether changes were made.
