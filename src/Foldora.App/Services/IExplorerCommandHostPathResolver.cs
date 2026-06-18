namespace Foldora.App.Services;

/// <summary>
/// Находит executable host для legacy Explorer menu commands.
/// </summary>
public interface IExplorerCommandHostPathResolver
{
    string ResolveCommandHostPath();
}
