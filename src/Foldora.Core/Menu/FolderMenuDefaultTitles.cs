using Foldora.Core.Settings;

namespace Foldora.Core.Menu;

/// <summary>
/// Compatibility-набор известных product default titles для режима локализованного default title.
/// </summary>
public static class FolderMenuDefaultTitles
{
    public const string Russian = "Создать папку";

    public const string English = "Create folder";

    public const string SimplifiedChinese = "创建文件夹";

    public const string German = "Ordner erstellen";

    public const string Spanish = "Crear carpeta";

    public const string French = "Créer un dossier";

    public const string Japanese = "フォルダーを作成";

    public const string BrazilianPortuguese = "Criar pasta";

    public const string Korean = "폴더 만들기";

    public const string Ukrainian = "Створити папку";

    public const string Polish = "Utwórz folder";

    public const string Turkish = "Klasör oluştur";

    public const string Romanian = "Creează folder";

    public const string Czech = "Vytvořit složku";

    public const string Hungarian = "Mappa létrehozása";

    public const string Bulgarian = "Създай папка";

    public const string Italian = "Crea cartella";

    public const string Dutch = "Map maken";

    public const string Indonesian = "Buat folder";

    public const string Vietnamese = "Tạo thư mục";

    public const string Hindi = "फ़ोल्डर बनाएँ";

    public const string Thai = "สร้างโฟลเดอร์";

    public const string TraditionalChinese = "建立資料夾";

    public const string PortuguesePortugal = "Criar pasta";

    private static readonly string[] KnownDefaults =
    [
        Russian,
        English,
        SimplifiedChinese,
        German,
        Spanish,
        French,
        Japanese,
        BrazilianPortuguese,
        Korean,
        Ukrainian,
        Polish,
        Turkish,
        Romanian,
        Czech,
        Hungarian,
        Bulgarian,
        Italian,
        Dutch,
        Indonesian,
        Vietnamese,
        Hindi,
        Thai,
        TraditionalChinese,
        PortuguesePortugal
    ];

    public static string GetForLanguage(string language)
    {
        return FoldoraLanguage.NormalizeOrDefault(language) switch
        {
            FoldoraLanguage.Russian => Russian,
            FoldoraLanguage.SimplifiedChinese => SimplifiedChinese,
            FoldoraLanguage.German => German,
            FoldoraLanguage.Spanish => Spanish,
            FoldoraLanguage.French => French,
            FoldoraLanguage.Japanese => Japanese,
            FoldoraLanguage.BrazilianPortuguese => BrazilianPortuguese,
            FoldoraLanguage.Korean => Korean,
            FoldoraLanguage.Ukrainian => Ukrainian,
            FoldoraLanguage.Polish => Polish,
            FoldoraLanguage.Turkish => Turkish,
            FoldoraLanguage.Romanian => Romanian,
            FoldoraLanguage.Czech => Czech,
            FoldoraLanguage.Hungarian => Hungarian,
            FoldoraLanguage.Bulgarian => Bulgarian,
            FoldoraLanguage.Italian => Italian,
            FoldoraLanguage.Dutch => Dutch,
            FoldoraLanguage.Indonesian => Indonesian,
            FoldoraLanguage.Vietnamese => Vietnamese,
            FoldoraLanguage.Hindi => Hindi,
            FoldoraLanguage.Thai => Thai,
            FoldoraLanguage.TraditionalChinese => TraditionalChinese,
            FoldoraLanguage.PortuguesePortugal => PortuguesePortugal,
            _ => English
        };
    }

    public static bool IsKnownDefault(string? title)
    {
        var normalized = Normalize(title);
        return string.IsNullOrEmpty(normalized)
               || KnownDefaults.Any(defaultTitle => string.Equals(normalized, defaultTitle, StringComparison.Ordinal));
    }

    public static string Normalize(string? title)
    {
        return string.IsNullOrWhiteSpace(title) ? string.Empty : title.Trim();
    }
}
