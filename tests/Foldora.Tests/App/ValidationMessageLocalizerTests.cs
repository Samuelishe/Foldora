using Foldora.App.Services;
using Foldora.Core.Validation;

namespace Foldora.Tests.App;

public sealed class ValidationMessageLocalizerTests
{
    [Fact]
    public void Localize_RendersFolderNameInvalidCharacterInEnglish()
    {
        var localizer = new ValidationMessageLocalizer(new InMemoryLocalizationService("en"));
        var issue = new FolderMenuValidationIssue(
            FolderMenuValidationSeverity.Error,
            "debug fallback",
            FolderMenuValidationIssueCodes.FolderNameInvalidChars,
            parameters: new Dictionary<string, string>
            {
                ["character"] = ":"
            });

        var message = localizer.Localize(issue);

        Assert.Equal("Created folder name contains invalid character \":\".", message);
    }

    [Fact]
    public void Localize_RendersFolderNameInvalidCharacterInRussian()
    {
        var localizer = new ValidationMessageLocalizer(new InMemoryLocalizationService("ru"));
        var issue = new FolderMenuValidationIssue(
            FolderMenuValidationSeverity.Error,
            "debug fallback",
            FolderMenuValidationIssueCodes.FolderNameInvalidChars,
            parameters: new Dictionary<string, string>
            {
                ["character"] = ":"
            });

        var message = localizer.Localize(issue);

        Assert.Equal("Имя создаваемой папки содержит недопустимый символ \":\".", message);
    }

    [Fact]
    public void Localize_RendersMissingIconInEnglishAndRussian()
    {
        var issue = new FolderMenuValidationIssue(
            FolderMenuValidationSeverity.Error,
            "debug fallback",
            FolderMenuValidationIssueCodes.EntryIconPathEmpty);

        Assert.Equal(
            "Choose an .ico for the menu entry before saving.",
            new ValidationMessageLocalizer(new InMemoryLocalizationService("en")).Localize(issue));
        Assert.Equal(
            "Выберите .ico для пункта меню перед сохранением.",
            new ValidationMessageLocalizer(new InMemoryLocalizationService("ru")).Localize(issue));
    }

    [Fact]
    public void Localize_FallsBackToIssueMessageWhenKeyIsMissing()
    {
        var localizer = new ValidationMessageLocalizer(new InMemoryLocalizationService("en"));
        var issue = new FolderMenuValidationIssue(
            FolderMenuValidationSeverity.Error,
            "debug fallback",
            "unknown_code");

        Assert.Equal("debug fallback", localizer.Localize(issue));
    }
}
