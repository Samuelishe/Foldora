using Foldora.Core.Validation;

namespace Foldora.App.Services;

/// <summary>
/// Рендерит invariant validation issue в user-facing строку текущей локали.
/// </summary>
public interface IValidationMessageLocalizer
{
    string Localize(FolderMenuValidationIssue issue);
}
