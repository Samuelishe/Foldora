namespace Foldora.App.Services;

/// <summary>
/// Возвращает текущую UI culture операционной системы для first-run выбора языка.
/// </summary>
public interface ISystemLanguageProvider
{
    string CurrentUiCultureName { get; }
}
