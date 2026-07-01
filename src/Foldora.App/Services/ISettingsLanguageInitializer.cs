namespace Foldora.App.Services;

/// <summary>
/// Инициализирует persisted language перед загрузкой WPF draft state.
/// </summary>
public interface ISettingsLanguageInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
