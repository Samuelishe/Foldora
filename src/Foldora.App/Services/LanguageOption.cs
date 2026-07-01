namespace Foldora.App.Services;

/// <summary>
/// Вариант языка для UI настроек.
/// </summary>
public sealed record LanguageOption(string Code, string DisplayName, string EnglishSortName, int SortOrder);
