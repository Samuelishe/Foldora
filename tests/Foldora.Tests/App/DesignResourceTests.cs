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
    public void SettingsWindow_IsResizableAndKeepsScrollableContentWithFixedFooter()
    {
        var settingsWindow = LoadXml("src", "Foldora.App", "SettingsWindow.xaml");
        var root = settingsWindow.Root ?? throw new InvalidOperationException("SettingsWindow root was not found.");

        Assert.Equal("CanResize", root.Attribute("ResizeMode")?.Value);
        Assert.Equal("Manual", root.Attribute("SizeToContent")?.Value);

        var scrollViewer = settingsWindow.Descendants()
            .SingleOrDefault(element => element.Name.LocalName == "ScrollViewer");
        Assert.NotNull(scrollViewer);
        Assert.Equal("1", scrollViewer!.Attribute("Grid.Row")?.Value);
        Assert.Equal("Auto", scrollViewer.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("Disabled", scrollViewer.Attribute("HorizontalScrollBarVisibility")?.Value);

        var footer = settingsWindow.Descendants()
            .Where(element => element.Name.LocalName == "StackPanel")
            .SingleOrDefault(element => element.Attribute("Grid.Row")?.Value == "2");

        Assert.NotNull(footer);
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
