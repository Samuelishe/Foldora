namespace Foldora.App.Services;

/// <summary>
/// Результат подготовки выбранного пользователем icon/image-файла для staged editor.
/// </summary>
public sealed record IconAssetPreparationResult(string IconPath, bool ImportOnSave, bool WasConverted)
{
    public static IconAssetPreparationResult ExistingIcon(string iconPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iconPath);
        return new IconAssetPreparationResult(iconPath, ImportOnSave: true, WasConverted: false);
    }

    public static IconAssetPreparationResult GeneratedIcon(string iconPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iconPath);
        return new IconAssetPreparationResult(iconPath, ImportOnSave: false, WasConverted: true);
    }
}
