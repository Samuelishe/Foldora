namespace Foldora.App.Services;

/// <summary>
/// Загружает preview для .ico без генерации preview-файлов.
/// </summary>
public interface IIconPreviewService
{
    IconPreviewResult LoadPreview(string? iconPath);
}
