using System.Xml.Linq;

namespace Foldora.Tests.App;

public sealed class DesignResourceTests
{
    [Fact]
    public void AppXaml_MergesDesignResourceDictionaries()
    {
        var appXaml = LoadXml("src", "Foldora.App", "App.xaml");

        var sources = appXaml.Descendants()
            .Attributes("Source")
            .Select(attribute => attribute.Value)
            .ToArray();

        Assert.Contains("Resources/DesignTokens.xaml", sources);
        Assert.Contains("Resources/Typography.xaml", sources);
        Assert.Contains("Resources/Controls.xaml", sources);
    }

    [Fact]
    public void DesignTokens_DefineCoreSemanticBrushes()
    {
        var tokens = LoadXml("src", "Foldora.App", "Resources", "DesignTokens.xaml");
        var keys = GetResourceKeys(tokens);

        Assert.Contains("PageBackgroundBrush", keys);
        Assert.Contains("SurfaceBrush", keys);
        Assert.Contains("TextPrimaryBrush", keys);
        Assert.Contains("AccentBrush", keys);
        Assert.Contains("DangerBrush", keys);
        Assert.Contains("FocusBrush", keys);
    }

    [Fact]
    public void Controls_DefineMainSemanticStyles()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var keys = GetResourceKeys(controls);

        Assert.Contains("PrimaryButtonStyle", keys);
        Assert.Contains("SecondaryButtonStyle", keys);
        Assert.Contains("DangerButtonStyle", keys);
        Assert.Contains("ActionButtonStyle", keys);
        Assert.Contains("IconButtonStyle", keys);
        Assert.Contains("TextBoxStyle", keys);
        Assert.Contains("CheckBoxStyle", keys);
        Assert.Contains("CardContainerStyle", keys);
        Assert.Contains("GroupContainerStyle", keys);
        Assert.Contains("StatusBannerStyle", keys);
        Assert.Contains("HelpIconButtonStyle", keys);
        Assert.Contains("HelpInfoGlyphStyle", keys);
        Assert.Contains("HelpTooltipTextStyle", keys);
        Assert.Contains("InlineActionButtonStyle", keys);
    }

    [Fact]
    public void PrimarySecondaryAndDangerButtons_UseSharedActionButtonGeometry()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        XName keyName = XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml");

        var styleBases = controls.Descendants()
            .Where(element => element.Name.LocalName == "Style")
            .Where(element => element.Attribute(keyName)?.Value is "PrimaryButtonStyle" or "SecondaryButtonStyle" or "DangerButtonStyle")
            .ToDictionary(
                element => element.Attribute(keyName)!.Value,
                element => element.Attribute("BasedOn")?.Value,
                StringComparer.Ordinal);

        Assert.Equal("{StaticResource ActionButtonStyle}", styleBases["PrimaryButtonStyle"]);
        Assert.Equal("{StaticResource ActionButtonStyle}", styleBases["SecondaryButtonStyle"]);
        Assert.Equal("{StaticResource ActionButtonStyle}", styleBases["DangerButtonStyle"]);
    }

    [Fact]
    public void ActionButtonStyle_DefinesLocalizedLabelFriendlyGeometry()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var actionButtonStyle = FindStyle(controls, "ActionButtonStyle");

        Assert.Equal("36", GetSetterValue(actionButtonStyle, "MinHeight"));
        Assert.Equal("18,7", GetSetterValue(actionButtonStyle, "Padding"));
        Assert.Equal("120", GetSetterValue(actionButtonStyle, "MinWidth"));
    }

    [Fact]
    public void InlineActionButtonStyle_DefinesCompactSettingsGeometry()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var inlineButtonStyle = FindStyle(controls, "InlineActionButtonStyle");

        Assert.Equal("{StaticResource SecondaryButtonStyle}", inlineButtonStyle.Attribute("BasedOn")?.Value);
        Assert.Equal("34", GetSetterValue(inlineButtonStyle, "MinHeight"));
        Assert.Equal("16,6", GetSetterValue(inlineButtonStyle, "Padding"));
        Assert.Equal("88", GetSetterValue(inlineButtonStyle, "MinWidth"));
    }

    [Fact]
    public void HelpTooltipTextStyle_WrapsLongHelpText()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var helpTooltipTextStyle = FindStyle(controls, "HelpTooltipTextStyle");

        Assert.Equal("420", GetSetterValue(helpTooltipTextStyle, "MaxWidth"));
        Assert.Equal("Wrap", GetSetterValue(helpTooltipTextStyle, "TextWrapping"));
    }

    [Fact]
    public void SettingsWindow_IsResizableAndKeepsScrollableContentWithFixedFooter()
    {
        var settingsWindow = LoadXml("src", "Foldora.App", "SettingsWindow.xaml");
        var root = settingsWindow.Root ?? throw new InvalidOperationException("SettingsWindow root was not found.");

        Assert.Equal("CanResize", root.Attribute("ResizeMode")?.Value);
        Assert.Equal("Manual", root.Attribute("SizeToContent")?.Value);
        Assert.Equal("720", root.Attribute("MinWidth")?.Value);

        var scrollViewer = settingsWindow.Descendants()
            .SingleOrDefault(element => element.Name.LocalName == "ScrollViewer");
        Assert.NotNull(scrollViewer);
        Assert.Equal("1", scrollViewer!.Attribute("Grid.Row")?.Value);
        Assert.Equal("Auto", scrollViewer.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("Disabled", scrollViewer.Attribute("HorizontalScrollBarVisibility")?.Value);

        var scrollContent = scrollViewer.Elements()
            .SingleOrDefault(element => element.Name.LocalName == "StackPanel");
        Assert.NotNull(scrollContent);
        Assert.Equal("0,0,14,0", scrollContent!.Attribute("Margin")?.Value);

        var footer = settingsWindow.Descendants()
            .Where(element => element.Name.LocalName == "StackPanel")
            .SingleOrDefault(element => element.Attribute("Grid.Row")?.Value == "2");

        Assert.NotNull(footer);
    }

    [Fact]
    public void MainWindow_DefinesEditorFriendlyMinimumWidth()
    {
        var mainWindow = LoadXml("src", "Foldora.App", "MainWindow.xaml");
        var root = mainWindow.Root ?? throw new InvalidOperationException("MainWindow root was not found.");

        Assert.Equal("940", root.Attribute("MinWidth")?.Value);
    }

    [Fact]
    public void MainWindow_DoesNotExposeExplorerAdminActionsAsMainContent()
    {
        var mainWindowText = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "Foldora.App", "MainWindow.xaml"));

        Assert.DoesNotContain("DryRunCommand", mainWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterExplorerCommand", mainWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("ResetMenuCommand", mainWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("ManageInSettings", mainWindowText, StringComparison.Ordinal);
        Assert.Contains("ExplorerIntegrationStatusLabel", mainWindowText, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsWindow_ContainsExplorerInstallationAndDangerSections()
    {
        var settingsWindowText = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "Foldora.App", "SettingsWindow.xaml"));

        Assert.Contains("ExplorerMenuSection", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("InstallationSection", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("DangerZone", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("PreviewChanges", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("PreviewChangesTooltip", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpInfoGlyphStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpTooltipTextStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("InlineActionButtonStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("OpenInstalledAppPathCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("CopyCommandHostPathCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("DryRunCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("RegisterExplorerCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("ResetMenuCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Style=\"{StaticResource HelpIconButtonStyle}\"", settingsWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Проверить план", settingsWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Check plan", settingsWindowText, StringComparison.OrdinalIgnoreCase);
    }

    private static XDocument LoadXml(params string[] pathParts)
    {
        return XDocument.Load(Path.Combine(GetRepositoryRoot(), Path.Combine(pathParts)));
    }

    private static HashSet<string> GetResourceKeys(XDocument document)
    {
        XName keyName = XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml");
        return document.Descendants()
            .Select(element => element.Attribute(keyName)?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static XElement FindStyle(XDocument document, string key)
    {
        XName keyName = XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml");
        return document.Descendants()
            .Single(element => element.Name.LocalName == "Style" && element.Attribute(keyName)?.Value == key);
    }

    private static string? GetSetterValue(XElement style, string property)
    {
        return style.Elements()
            .Where(element => element.Name.LocalName == "Setter")
            .Single(element => element.Attribute("Property")?.Value == property)
            .Attribute("Value")?.Value;
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Foldora.sln")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        return directory.FullName;
    }
}
