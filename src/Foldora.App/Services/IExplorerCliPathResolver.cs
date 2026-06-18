namespace Foldora.App.Services;

/// <summary>
/// Находит CLI executable path для legacy Explorer menu commands.
/// </summary>
public interface IExplorerCliPathResolver
{
    string ResolveCliPath();
}
