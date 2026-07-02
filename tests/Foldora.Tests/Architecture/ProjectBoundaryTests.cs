namespace Foldora.Tests.Architecture;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void CoreProject_DoesNotReferenceWpf()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.Core", "Foldora.Core.csproj");

        var projectText = File.ReadAllText(projectFile);

        Assert.DoesNotContain("UseWPF", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("net10.0-windows", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PresentationCore", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WindowsBase", projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CoreProject_DoesNotReferenceShellDesktopPositioning()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.Core", "Foldora.Core.csproj");

        var projectText = File.ReadAllText(projectFile);

        Assert.DoesNotContain("Foldora.Shell", projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ImagingProject_RemainsPureNetProject()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.Imaging", "Foldora.Imaging.csproj");

        var projectText = File.ReadAllText(projectFile);

        Assert.Contains("<TargetFramework>net10.0</TargetFramework>", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("net10.0-windows", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UseWPF", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectReference", projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WindowsImagingProject_OnlyReferencesPureImagingProject()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.Imaging.Windows", "Foldora.Imaging.Windows.csproj");

        var projectText = File.ReadAllText(projectFile);

        Assert.Contains("<TargetFramework>net10.0-windows</TargetFramework>", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<UseWPF>true</UseWPF>", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("..\\Foldora.Imaging\\Foldora.Imaging.csproj", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Foldora.App", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Foldora.Core", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Foldora.Cli", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Foldora.MenuHost", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Foldora.Shell", projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CliProject_TargetsWindowsAndMayReferenceWindowsImaging()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.Cli", "Foldora.Cli.csproj");

        var projectText = File.ReadAllText(projectFile);

        Assert.Contains("<TargetFramework>net10.0-windows</TargetFramework>", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("..\\Foldora.Imaging.Windows\\Foldora.Imaging.Windows.csproj", projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AppProject_MayReferenceWindowsImagingForIconPickerConversion()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.App", "Foldora.App.csproj");

        var projectText = File.ReadAllText(projectFile);

        Assert.Contains("<TargetFramework>net10.0-windows</TargetFramework>", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("..\\Foldora.Imaging.Windows\\Foldora.Imaging.Windows.csproj", projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MenuHostProject_DoesNotReferenceImagingProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.MenuHost", "Foldora.MenuHost.csproj");

        var projectText = File.ReadAllText(projectFile);

        Assert.DoesNotContain("Foldora.Imaging", projectText, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Foldora.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
