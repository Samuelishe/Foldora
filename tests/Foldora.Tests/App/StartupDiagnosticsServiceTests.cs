using Foldora.App.Services;

namespace Foldora.Tests.App;

public sealed class StartupDiagnosticsServiceTests
{
    [Fact]
    public void WriteStartupError_WritesControlledDiagnosticToTempRoot()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraStartupDiagnostics-");

        try
        {
            var logPath = StartupDiagnosticsService.WriteStartupError(
                new InvalidOperationException("startup failed"),
                root.FullName);

            Assert.True(File.Exists(logPath));
            Assert.Equal(Path.Combine(root.FullName, "Logs", StartupDiagnosticsService.LogFileName), logPath);

            var content = File.ReadAllText(logPath);
            Assert.Contains("startup failed", content);
            Assert.Contains(nameof(InvalidOperationException), content);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
