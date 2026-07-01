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
    public void FoldoraAppProject_ConfiguresSelfAuthoredApplicationIcon()
    {
        var repositoryRoot = GetRepositoryRoot();
        var appProject = LoadXml("src", "Foldora.App", "Foldora.App.csproj");

        var applicationIcon = appProject.Descendants()
            .Single(element => element.Name.LocalName == "ApplicationIcon")
            .Value;
        var iconResource = appProject.Descendants()
            .SingleOrDefault(element => element.Name.LocalName == "Resource" && element.Attribute("Include")?.Value == @"Assets\Foldora.ico");

        var iconPath = Path.Combine(repositoryRoot, "src", "Foldora.App", "Assets", "Foldora.ico");
        var sourcePath = Path.Combine(repositoryRoot, "src", "Foldora.App", "Assets", "FoldoraIcon.svg");

        Assert.Equal(@"Assets\Foldora.ico", applicationIcon);
        Assert.NotNull(iconResource);
        Assert.True(File.Exists(iconPath));
        Assert.True(new FileInfo(iconPath).Length > 0);
        Assert.True(File.Exists(sourcePath));

        var sourceText = File.ReadAllText(sourcePath);
        Assert.Contains("folded blue/cyan folder mark", sourceText, StringComparison.Ordinal);
        Assert.Contains("broad light-cyan folded plane", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("small menu customization badge", sourceText, StringComparison.Ordinal);
    }

    [Fact]
    public void FoldoraIcon_ContainsCoreWindowsIconSizes()
    {
        var iconPath = Path.Combine(GetRepositoryRoot(), "src", "Foldora.App", "Assets", "Foldora.ico");
        using var stream = File.OpenRead(iconPath);
        using var reader = new BinaryReader(stream);

        Assert.Equal((ushort)0, reader.ReadUInt16());
        Assert.Equal((ushort)1, reader.ReadUInt16());
        var count = reader.ReadUInt16();
        Assert.Equal((ushort)4, count);

        var sizes = new List<int>();
        for (var i = 0; i < count; i++)
        {
            var width = reader.ReadByte();
            var height = reader.ReadByte();
            _ = reader.ReadByte();
            _ = reader.ReadByte();
            Assert.Equal((ushort)1, reader.ReadUInt16());
            Assert.Equal((ushort)32, reader.ReadUInt16());
            Assert.True(reader.ReadUInt32() > 0);
            Assert.True(reader.ReadUInt32() > 0);

            var normalizedWidth = width == 0 ? 256 : width;
            var normalizedHeight = height == 0 ? 256 : height;
            Assert.Equal(normalizedWidth, normalizedHeight);
            sizes.Add(normalizedWidth);
        }

        Assert.Equal([16, 32, 48, 256], sizes.Order().ToArray());
    }

    [Fact]
    public void FoldoraWindows_ReferenceSharedWindowIcon()
    {
        var windows = new[]
        {
            "MainWindow.xaml",
            "SettingsWindow.xaml",
            "HelpWindow.xaml"
        };

        foreach (var window in windows)
        {
            var document = LoadXml("src", "Foldora.App", window);
            var root = document.Root ?? throw new InvalidOperationException($"{window} root was not found.");

            Assert.Equal("Assets/Foldora.ico", root.Attribute("Icon")?.Value);
        }
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
        Assert.Contains("AccentSoftBrush", keys);
        Assert.Contains("DangerBrush", keys);
        Assert.Contains("DangerBorderBrush", keys);
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
        Assert.Contains("PageHeaderContainerStyle", keys);
        Assert.Contains("EmptyStateContainerStyle", keys);
        Assert.Contains("StatusPillStyle", keys);
        Assert.Contains("FooterBarStyle", keys);
        Assert.Contains("PathRowContainerStyle", keys);
        Assert.Contains("HelpStepContainerStyle", keys);
        Assert.Contains("SettingsTabControlStyle", keys);
        Assert.Contains("SettingsTabItemStyle", keys);
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
        Assert.Equal("20,7", GetSetterValue(actionButtonStyle, "Padding"));
        Assert.Equal("128", GetSetterValue(actionButtonStyle, "MinWidth"));
        Assert.Equal("Center", GetSetterValue(actionButtonStyle, "HorizontalContentAlignment"));
        Assert.Equal("Center", GetSetterValue(actionButtonStyle, "VerticalContentAlignment"));
    }

    [Fact]
    public void BaseButtonStyle_TemplateAppliesPaddingAndContentAlignment()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var baseButtonStyle = FindStyle(controls, "BaseButtonStyle");
        XName xName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");

        var buttonChrome = baseButtonStyle.Descendants()
            .Single(element => element.Name.LocalName == "Border" && element.Attribute(xName)?.Value == "ButtonChrome");
        var contentPresenter = buttonChrome.Elements()
            .Single(element => element.Name.LocalName == "ContentPresenter");

        Assert.Equal("{TemplateBinding Padding}", buttonChrome.Attribute("Padding")?.Value);
        Assert.Equal("{TemplateBinding HorizontalContentAlignment}", contentPresenter.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("{TemplateBinding VerticalContentAlignment}", contentPresenter.Attribute("VerticalAlignment")?.Value);
    }

    [Fact]
    public void InlineActionButtonStyle_DefinesCompactSettingsGeometry()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var inlineButtonStyle = FindStyle(controls, "InlineActionButtonStyle");

        Assert.Equal("{StaticResource SecondaryButtonStyle}", inlineButtonStyle.Attribute("BasedOn")?.Value);
        Assert.Equal("36", GetSetterValue(inlineButtonStyle, "MinHeight"));
        Assert.Equal("20,6", GetSetterValue(inlineButtonStyle, "Padding"));
        Assert.Equal("104", GetSetterValue(inlineButtonStyle, "MinWidth"));
    }

    [Fact]
    public void SettingsTabControlStyle_UsesWrappingHeaderHost()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var settingsTabControlStyle = FindStyle(controls, "SettingsTabControlStyle");
        XName xName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");

        var headerHost = settingsTabControlStyle.Descendants()
            .Single(element => element.Attribute("IsItemsHost")?.Value == "True");

        Assert.Equal("WrapPanel", headerHost.Name.LocalName);
        Assert.Equal("Top", headerHost.Attribute("DockPanel.Dock")?.Value);
        Assert.Null(headerHost.Attribute("Width"));
        Assert.Null(headerHost.Attribute("MaxWidth"));
        Assert.Null(headerHost.Attribute("ClipToBounds"));

        var selectedContentHost = settingsTabControlStyle.Descendants()
            .Single(element => element.Attribute(xName)?.Value == "PART_SelectedContentHost");

        Assert.Equal("{TemplateBinding HorizontalContentAlignment}", selectedContentHost.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("{TemplateBinding VerticalContentAlignment}", selectedContentHost.Attribute("VerticalAlignment")?.Value);
    }

    [Fact]
    public void SettingsTabItemStyle_StretchesSelectedContentAndUsesContentSizedHeaders()
    {
        var controls = LoadXml("src", "Foldora.App", "Resources", "Controls.xaml");
        var settingsTabItemStyle = FindStyle(controls, "SettingsTabItemStyle");

        Assert.Equal("{DynamicResource SettingsTabHeaderPadding}", GetSetterValue(settingsTabItemStyle, "Padding"));
        Assert.Equal("32", GetSetterValue(settingsTabItemStyle, "MinHeight"));
        Assert.Equal("Stretch", GetSetterValue(settingsTabItemStyle, "HorizontalContentAlignment"));
        Assert.Equal("Stretch", GetSetterValue(settingsTabItemStyle, "VerticalContentAlignment"));
        Assert.Null(GetOptionalSetterValue(settingsTabItemStyle, "Width"));
        Assert.Null(GetOptionalSetterValue(settingsTabItemStyle, "MaxWidth"));
        Assert.Null(GetOptionalSetterValue(settingsTabItemStyle, "TextTrimming"));
        Assert.Null(GetOptionalSetterValue(settingsTabItemStyle, "ClipToBounds"));

        var contentPresenter = settingsTabItemStyle.Descendants()
            .Single(element => element.Name.LocalName == "ContentPresenter" && element.Attribute("ContentSource")?.Value == "Header");
        Assert.Equal("Center", contentPresenter.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Center", contentPresenter.Attribute("VerticalAlignment")?.Value);
        Assert.Null(contentPresenter.Attribute("Width"));
        Assert.Null(contentPresenter.Attribute("MaxWidth"));
    }

    [Fact]
    public void Typography_DefinesHelpStepTextStyle()
    {
        var typography = LoadXml("src", "Foldora.App", "Resources", "Typography.xaml");
        var keys = GetResourceKeys(typography);

        Assert.Contains("HelpStepTextStyle", keys);
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
    public void SettingsWindow_IsResizableAndUsesTabbedCategoriesWithFixedFooter()
    {
        var settingsWindow = LoadXml("src", "Foldora.App", "SettingsWindow.xaml");
        var root = settingsWindow.Root ?? throw new InvalidOperationException("SettingsWindow root was not found.");

        Assert.Equal("CanResize", root.Attribute("ResizeMode")?.Value);
        Assert.Equal("Manual", root.Attribute("SizeToContent")?.Value);
        Assert.Equal("840", root.Attribute("MinWidth")?.Value);

        var tabControl = settingsWindow.Descendants()
            .SingleOrDefault(element => element.Name.LocalName == "TabControl" && element.Attribute("Grid.Row")?.Value == "1");
        Assert.NotNull(tabControl);
        Assert.Equal("{StaticResource SettingsTabControlStyle}", tabControl!.Attribute("Style")?.Value);
        Assert.Equal("{StaticResource SettingsTabItemStyle}", tabControl.Attribute("ItemContainerStyle")?.Value);

        var tabHeaders = tabControl.Elements()
            .Where(element => element.Name.LocalName == "TabItem")
            .Select(element => element.Attribute("Header")?.Value ?? string.Empty)
            .ToArray();

        Assert.Equal(
            [
                "{Binding L.SettingsTabApplication}",
                "{Binding L.SettingsTabExplorerMenu}",
                "{Binding L.SettingsTabInstallation}",
                "{Binding L.SettingsTabHelpAbout}",
                "{Binding L.SettingsTabDangerZone}"
            ],
            tabHeaders);
        Assert.DoesNotContain(settingsWindow.Descendants().Where(element => element.Name.LocalName == "ScrollViewer"), element => element.Attribute("Grid.Row")?.Value == "1");

        var footer = settingsWindow.Descendants()
            .Where(element => element.Name.LocalName == "StackPanel")
            .SingleOrDefault(element => element.Attribute("Grid.Row")?.Value == "2");

        Assert.NotNull(footer);
    }

    [Fact]
    public void SettingsWindow_ExplorerActionsUseWrappingRowWithoutFixedButtonWidths()
    {
        var settingsWindow = LoadXml("src", "Foldora.App", "SettingsWindow.xaml");

        var actionCommands = new[]
        {
            "DryRunCommand",
            "RegisterExplorerCommand",
            "UnregisterExplorerCommand"
        };

        foreach (var command in actionCommands)
        {
            var button = settingsWindow.Descendants()
                .Single(element => element.Name.LocalName == "Button" && element.Attribute("Command")?.Value == $"{{Binding {command}}}");

            Assert.Equal("{StaticResource InlineActionButtonStyle}", button.Attribute("Style")?.Value);
            Assert.Null(button.Attribute("Width"));
            Assert.Null(button.Attribute("MaxWidth"));
            Assert.Equal("WrapPanel", button.Parent?.Name.LocalName);
        }
    }

    [Fact]
    public void SettingsWindow_PathRowsKeepStarContentAndAutoActions()
    {
        var settingsWindow = LoadXml("src", "Foldora.App", "SettingsWindow.xaml");

        var pathCommands = new[]
        {
            "OpenInstalledAppPathCommand",
            "CopyInstalledAppPathCommand",
            "OpenUserDataPathCommand",
            "CopyUserDataPathCommand",
            "OpenCommandHostPathCommand",
            "CopyCommandHostPathCommand"
        };

        foreach (var command in pathCommands)
        {
            var button = settingsWindow.Descendants()
                .Single(element => element.Name.LocalName == "Button" && element.Attribute("Command")?.Value == $"{{Binding {command}}}");

            Assert.Equal("{StaticResource InlineActionButtonStyle}", button.Attribute("Style")?.Value);
            Assert.Null(button.Attribute("Width"));
            Assert.Null(button.Attribute("MaxWidth"));

            var actions = button.Ancestors().First(element => element.Name.LocalName == "StackPanel");
            Assert.Equal("1", actions.Attribute("Grid.Column")?.Value);
            Assert.Equal("Horizontal", actions.Attribute("Orientation")?.Value);

            var grid = actions.Ancestors().First(element => element.Name.LocalName == "Grid");
            var columnWidths = grid.Descendants()
                .Where(element => element.Name.LocalName == "ColumnDefinition")
                .Select(element => element.Attribute("Width")?.Value ?? string.Empty)
                .ToArray();
            Assert.Equal(["*", "Auto"], columnWidths);
        }
    }

    [Fact]
    public void SettingsWindow_TabBodiesStretchAndContentStartsLeftTop()
    {
        var settingsWindow = LoadXml("src", "Foldora.App", "SettingsWindow.xaml");

        var applicationRoot = FindTabContentRoot(settingsWindow, "{Binding L.SettingsTabApplication}");
        Assert.Equal("Grid", applicationRoot.Name.LocalName);
        Assert.Equal("Stretch", applicationRoot.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Stretch", applicationRoot.Attribute("VerticalAlignment")?.Value);
        var applicationContent = applicationRoot.Elements().Single(element => element.Name.LocalName == "StackPanel");
        Assert.Equal("Left", applicationContent.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Top", applicationContent.Attribute("VerticalAlignment")?.Value);
        Assert.Equal("420", applicationContent.Attribute("MaxWidth")?.Value);

        var explorerRoot = FindTabContentRoot(settingsWindow, "{Binding L.SettingsTabExplorerMenu}");
        Assert.Equal("ScrollViewer", explorerRoot.Name.LocalName);
        Assert.Equal("Stretch", explorerRoot.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Stretch", explorerRoot.Attribute("VerticalAlignment")?.Value);

        var installationRoot = FindTabContentRoot(settingsWindow, "{Binding L.SettingsTabInstallation}");
        Assert.Equal("ScrollViewer", installationRoot.Name.LocalName);
        Assert.Equal("Stretch", installationRoot.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Stretch", installationRoot.Attribute("VerticalAlignment")?.Value);

        var helpRoot = FindTabContentRoot(settingsWindow, "{Binding L.SettingsTabHelpAbout}");
        Assert.Equal("Grid", helpRoot.Name.LocalName);
        Assert.Equal("Stretch", helpRoot.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Stretch", helpRoot.Attribute("VerticalAlignment")?.Value);
        var helpContent = helpRoot.Elements().Single(element => element.Name.LocalName == "StackPanel");
        Assert.Equal("Left", helpContent.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Top", helpContent.Attribute("VerticalAlignment")?.Value);
        Assert.Equal("560", helpContent.Attribute("MaxWidth")?.Value);

        var dangerRoot = FindTabContentRoot(settingsWindow, "{Binding L.SettingsTabDangerZone}");
        Assert.Equal("Grid", dangerRoot.Name.LocalName);
        Assert.Equal("Stretch", dangerRoot.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Stretch", dangerRoot.Attribute("VerticalAlignment")?.Value);
        var dangerCard = dangerRoot.Elements().Single(element => element.Name.LocalName == "Border");
        Assert.Equal("{StaticResource DangerBannerStyle}", dangerCard.Attribute("Style")?.Value);
        Assert.Equal("Left", dangerCard.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("Top", dangerCard.Attribute("VerticalAlignment")?.Value);
        Assert.Equal("560", dangerCard.Attribute("MaxWidth")?.Value);
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
        Assert.Contains("HelpAboutSection", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("OpenHelpCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("DangerZone", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("PreviewChanges", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("PreviewChangesTooltip", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpInfoGlyphStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpTooltipTextStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("InlineActionButtonStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("StatusPillStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("PathRowContainerStyle", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("SettingsTabApplication", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("SettingsTabDangerZone", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("OpenInstalledAppPathCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("CopyCommandHostPathCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("OpenFolderTooltip", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("OpenLocationTooltip", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("CopyPathTooltip", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("DryRunCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("RegisterExplorerCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.Contains("ResetMenuCommand", settingsWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"{Binding L.OpenLocation}\"", settingsWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Style=\"{StaticResource HelpIconButtonStyle}\"", settingsWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Проверить план", settingsWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Check plan", settingsWindowText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HelpWindow_IsResizableScrollableAndLocalized()
    {
        var helpWindow = LoadXml("src", "Foldora.App", "HelpWindow.xaml");
        var root = helpWindow.Root ?? throw new InvalidOperationException("HelpWindow root was not found.");
        var helpWindowText = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "Foldora.App", "HelpWindow.xaml"));

        Assert.Equal("CanResize", root.Attribute("ResizeMode")?.Value);
        Assert.Equal("Manual", root.Attribute("SizeToContent")?.Value);
        Assert.Equal("560", root.Attribute("MinWidth")?.Value);
        Assert.Equal("420", root.Attribute("MinHeight")?.Value);
        Assert.Contains("HelpWindowTitle", helpWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpWhatFoldoraDoesBody", helpWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpUseStepEnableExplorer", helpWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpMenuHostBody", helpWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpUninstallUserDataWarning", helpWindowText, StringComparison.Ordinal);
        Assert.Contains("PageHeaderContainerStyle", helpWindowText, StringComparison.Ordinal);
        Assert.Contains("HelpStepContainerStyle", helpWindowText, StringComparison.Ordinal);
        Assert.DoesNotContain("Foldora lets you create", helpWindowText, StringComparison.Ordinal);

        var scrollViewer = helpWindow.Descendants()
            .SingleOrDefault(element => element.Name.LocalName == "ScrollViewer");
        Assert.NotNull(scrollViewer);
        Assert.Equal("1", scrollViewer!.Attribute("Grid.Row")?.Value);
        Assert.Equal("Auto", scrollViewer.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("Disabled", scrollViewer.Attribute("HorizontalScrollBarVisibility")?.Value);
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

    private static XElement FindTabContentRoot(XDocument document, string header)
    {
        return document.Descendants()
            .Single(element => element.Name.LocalName == "TabItem" && element.Attribute("Header")?.Value == header)
            .Elements()
            .Single();
    }

    private static string? GetSetterValue(XElement style, string property)
    {
        return style.Elements()
            .Where(element => element.Name.LocalName == "Setter")
            .Single(element => element.Attribute("Property")?.Value == property)
            .Attribute("Value")?.Value;
    }

    private static string? GetOptionalSetterValue(XElement style, string property)
    {
        return style.Elements()
            .Where(element => element.Name.LocalName == "Setter")
            .SingleOrDefault(element => element.Attribute("Property")?.Value == property)
            ?.Attribute("Value")
            ?.Value;
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
