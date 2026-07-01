namespace Foldora.App.Services;

/// <summary>
/// Выполняет пользовательские действия с путями из окна настроек.
/// </summary>
public interface IPathActionService
{
    void OpenFolder(string path);

    void OpenLocation(string path);

    void CopyPath(string path);
}
