using Foldora.Core.Validation;

namespace Foldora.App.Services;

/// <summary>
/// Локализует Core validation issues через WPF catalog keys.
/// </summary>
public sealed class ValidationMessageLocalizer : IValidationMessageLocalizer
{
    private const string ValidationKeyPrefix = "Validation.";

    private readonly ILocalizationService localizationService;

    public ValidationMessageLocalizer(ILocalizationService localizationService)
    {
        this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public string Localize(FolderMenuValidationIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        if (string.IsNullOrWhiteSpace(issue.Code))
        {
            return issue.Message;
        }

        var key = ValidationKeyPrefix + issue.Code;
        var template = localizationService.Resources[key];
        if (string.Equals(template, key, StringComparison.Ordinal))
        {
            return issue.Message;
        }

        return ApplyParameters(template, issue.Parameters);
    }

    private static string ApplyParameters(string template, IReadOnlyDictionary<string, string> parameters)
    {
        var value = template;
        foreach (var parameter in parameters)
        {
            value = value.Replace("{" + parameter.Key + "}", parameter.Value, StringComparison.Ordinal);
        }

        return value;
    }
}
