using Foldora.App.ViewModels;
using Foldora.Core.Settings;

namespace Foldora.App.Services;

/// <summary>
/// Минимальный in-memory слой локализации для WPF.
/// </summary>
public sealed class InMemoryLocalizationService : ILocalizationService
{
    public InMemoryLocalizationService(string language = FoldoraLanguage.Russian)
    {
        Resources = new LocalizationResources();
        SetLanguage(language);
    }

    public string CurrentLanguage { get; private set; } = FoldoraLanguage.Russian;

    public LocalizationResources Resources { get; }

    public void SetLanguage(string language)
    {
        CurrentLanguage = FoldoraLanguage.NormalizeOrDefault(language);
        Resources.Apply(CreateStrings(CurrentLanguage));
    }

    private static IReadOnlyDictionary<string, string> CreateStrings(string language)
    {
        return language == FoldoraLanguage.English ? English : Russian;
    }

    private static readonly IReadOnlyDictionary<string, string> Russian = new Dictionary<string, string>
    {
        ["AppTitle"] = "Foldora",
        ["MenuSettings"] = "Настройка меню",
        ["MenuTitle"] = "Название меню:",
        ["MenuEntries"] = "Пункты меню",
        ["AddEntry"] = "+ Добавить пункт",
        ["EmptyTitle"] = "Пока нет пунктов меню.",
        ["EmptyDescription"] = "Добавьте первый пункт: выберите .ico, задайте название в меню и имя создаваемой папки.",
        ["EntryDisplayName"] = "Название в меню:",
        ["EntryFolderName"] = "Имя создаваемой папки:",
        ["EntryGroupName"] = "Группа:",
        ["EntryGroupHelp"] = "Оставьте пустым, чтобы пункт был в корне меню.",
        ["EntryEnabled"] = "Показывать в меню",
        ["EntryIcon"] = "Иконка:",
        ["ChooseIcon"] = "Выбрать .ico",
        ["Delete"] = "Удалить",
        ["ExplorerIntegration"] = "Интеграция с Проводником",
        ["StatusLabel"] = "Статус: {0}",
        ["DryRun"] = "Проверить план",
        ["RegisterExplorer"] = "Включить меню Проводника",
        ["UnregisterExplorer"] = "Отключить меню Проводника",
        ["TechnicalDetails"] = "Показать технические детали",
        ["DangerZone"] = "Опасная зона",
        ["ResetDescription"] = "Сброс очищает список пунктов меню и отключает интеграцию с Проводником.",
        ["ResetIconNote"] = "Импортированные .ico пока не удаляются.",
        ["ResetConfirm"] = "Я понимаю, что список пунктов будет очищен",
        ["ResetMenu"] = "Сбросить меню",
        ["UnsavedChanges"] = "Несохранённые изменения: {0}",
        ["Reload"] = "Отменить изменения",
        ["Save"] = "Сохранить",
        ["Settings"] = "Настройки",
        ["Minimize"] = "Свернуть",
        ["Maximize"] = "Развернуть",
        ["Close"] = "Закрыть"
    };

    private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
    {
        ["AppTitle"] = "Foldora",
        ["MenuSettings"] = "Menu settings",
        ["MenuTitle"] = "Menu title:",
        ["MenuEntries"] = "Menu entries",
        ["AddEntry"] = "+ Add entry",
        ["EmptyTitle"] = "No menu entries yet.",
        ["EmptyDescription"] = "Add the first entry: choose an .ico, set the menu label, and set the folder name.",
        ["EntryDisplayName"] = "Menu label:",
        ["EntryFolderName"] = "Created folder name:",
        ["EntryGroupName"] = "Group:",
        ["EntryGroupHelp"] = "Leave empty to place the entry in the root menu.",
        ["EntryEnabled"] = "Show in menu",
        ["EntryIcon"] = "Icon:",
        ["ChooseIcon"] = "Choose .ico",
        ["Delete"] = "Delete",
        ["ExplorerIntegration"] = "Explorer integration",
        ["StatusLabel"] = "Status: {0}",
        ["DryRun"] = "Dry run",
        ["RegisterExplorer"] = "Enable Explorer menu",
        ["UnregisterExplorer"] = "Disable Explorer menu",
        ["TechnicalDetails"] = "Show technical details",
        ["DangerZone"] = "Danger zone",
        ["ResetDescription"] = "Reset clears the menu entries and disables Explorer integration.",
        ["ResetIconNote"] = "Imported .ico files are not deleted yet.",
        ["ResetConfirm"] = "I understand that the entry list will be cleared",
        ["ResetMenu"] = "Reset menu",
        ["UnsavedChanges"] = "Unsaved changes: {0}",
        ["Reload"] = "Discard changes",
        ["Save"] = "Save",
        ["Settings"] = "Settings",
        ["Minimize"] = "Minimize",
        ["Maximize"] = "Maximize",
        ["Close"] = "Close"
    };
}
