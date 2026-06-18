using Foldora.App.Services;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.App;

public sealed class WpfIconPreviewServiceTests
{
    [Fact]
    public async Task LoadPreview_WithValidIconReturnsPreview()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraPreview-");

        try
        {
            var iconPath = Path.Combine(root.FullName, "valid.ico");
            await IcoTestFile.WriteValidPreviewAsync(iconPath);
            var service = new WpfIconPreviewService();

            var result = service.LoadPreview(iconPath);

            Assert.True(result.HasPreview);
            Assert.NotNull(result.Image);
            Assert.Null(result.ErrorMessage);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void LoadPreview_WithEmptyIconPathReturnsNoPreview()
    {
        var service = new WpfIconPreviewService();

        var result = service.LoadPreview(null);

        Assert.False(result.HasPreview);
        Assert.Null(result.Image);
        Assert.Equal("Иконка не выбрана", result.StatusText);
    }

    [Fact]
    public void LoadPreview_WithMissingFileReturnsStructuredError()
    {
        var service = new WpfIconPreviewService();

        var result = service.LoadPreview(@"C:\Foldora\missing.ico");

        Assert.False(result.HasPreview);
        Assert.Null(result.Image);
        Assert.Equal("Иконка не найдена", result.StatusText);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task LoadPreview_WithCorruptIconReturnsStructuredError()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraPreview-");

        try
        {
            var iconPath = Path.Combine(root.FullName, "bad.ico");
            await File.WriteAllTextAsync(iconPath, "not ico");
            var service = new WpfIconPreviewService();

            var result = service.LoadPreview(iconPath);

            Assert.False(result.HasPreview);
            Assert.Null(result.Image);
            Assert.NotNull(result.ErrorMessage);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
