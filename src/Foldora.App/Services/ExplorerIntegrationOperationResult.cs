namespace Foldora.App.Services;

/// <summary>
/// Результат WPF operation для Explorer integration.
/// </summary>
public sealed class ExplorerIntegrationOperationResult
{
    public ExplorerIntegrationOperationResult(
        bool success,
        string message,
        bool explorerIntegrationEnabled,
        IEnumerable<string>? details = null)
    {
        Success = success;
        Message = message;
        ExplorerIntegrationEnabled = explorerIntegrationEnabled;
        Details = (details ?? []).ToArray();
    }

    public bool Success { get; }

    public string Message { get; }

    public bool ExplorerIntegrationEnabled { get; }

    public IReadOnlyList<string> Details { get; }
}
