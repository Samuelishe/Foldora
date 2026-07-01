using System.Globalization;

namespace Foldora.App.Services;

/// <summary>
/// Читает язык пользовательского интерфейса через .NET culture API.
/// </summary>
public sealed class SystemLanguageProvider : ISystemLanguageProvider
{
    public string CurrentUiCultureName => CultureInfo.CurrentUICulture.Name;
}
