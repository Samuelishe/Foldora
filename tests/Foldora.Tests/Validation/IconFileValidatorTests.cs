using Foldora.Core.Validation;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.Validation;

public sealed class IconFileValidatorTests
{
    [Fact]
    public void Validate_RejectsMissingFile()
    {
        var result = new IconFileValidator().Validate(Path.Combine(Path.GetTempPath(), "missing.ico"));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == FolderMenuValidationIssueCodes.IconMissing
                && issue.Parameters.ContainsKey("filePath"));
    }

    [Fact]
    public async Task Validate_RejectsNonIcoFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraIconValidator-");

        try
        {
            var path = Path.Combine(root.FullName, "icon.png");
            await File.WriteAllTextAsync(path, "png");

            var result = new IconFileValidator().Validate(path);

            Assert.False(result.IsValid);
            Assert.Contains(
                result.Issues,
                issue => issue.Code == FolderMenuValidationIssueCodes.IconExtension
                    && issue.Parameters["extension"] == ".png");
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Validate_RejectsEmptyFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraIconValidator-");

        try
        {
            var path = Path.Combine(root.FullName, "icon.ico");
            await File.WriteAllBytesAsync(path, []);

            var result = new IconFileValidator().Validate(path);

            Assert.False(result.IsValid);
            Assert.Contains(
                result.Issues,
                issue => issue.Code == FolderMenuValidationIssueCodes.IconEmpty
                    && issue.Parameters["filePath"] == path);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Validate_RejectsTooLargeFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraIconValidator-");

        try
        {
            var path = Path.Combine(root.FullName, "icon.ico");
            await using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                stream.SetLength(IconFileValidator.MaxIconFileSizeBytes + 1);
            }

            var result = new IconFileValidator().Validate(path);

            Assert.False(result.IsValid);
            Assert.Contains(
                result.Issues,
                issue => issue.Code == FolderMenuValidationIssueCodes.IconTooLarge
                    && issue.Parameters["maxBytes"] == IconFileValidator.MaxIconFileSizeBytes.ToString()
                    && issue.Parameters["actualBytes"] == (IconFileValidator.MaxIconFileSizeBytes + 1).ToString());
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Validate_AcceptsMinimalValidIco()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraIconValidator-");

        try
        {
            var path = Path.Combine(root.FullName, "icon.ico");
            await IcoTestFile.WriteValidAsync(path);

            var result = new IconFileValidator().Validate(path);

            Assert.True(result.IsValid);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Validate_RejectsImageDataOutsideFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraIconValidator-");

        try
        {
            var path = Path.Combine(root.FullName, "icon.ico");
            await File.WriteAllBytesAsync(path, IcoTestFile.CreateOutOfBoundsBytes());

            var result = new IconFileValidator().Validate(path);

            Assert.False(result.IsValid);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
