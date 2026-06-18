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
