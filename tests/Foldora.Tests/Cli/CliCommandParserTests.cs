using Foldora.Cli;

namespace Foldora.Tests.Cli;

public sealed class CliCommandParserTests
{
    [Fact]
    public void Parse_AcceptsApplyWithFolderAndIcon()
    {
        var folder = @"C:\Users\User\Папка с пробелами";
        var icon = @"C:\Users\User\Иконки\folder icon.ico";

        var command = CliCommandParser.Parse(["apply", "--folder", folder, "--icon", icon]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.Apply, command.Kind);
        Assert.Equal(folder, command.FolderPath);
        Assert.Equal(icon, command.IconPath);
    }

    [Fact]
    public void Parse_AcceptsClearWithFolder()
    {
        var folder = @"C:\Users\User\Папка с пробелами";

        var command = CliCommandParser.Parse(["clear", "--folder", folder]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.Clear, command.Kind);
        Assert.Equal(folder, command.FolderPath);
    }

    [Fact]
    public void Parse_ReportsMissingApplyIcon()
    {
        var command = CliCommandParser.Parse(["apply", "--folder", @"C:\Temp"]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.Apply, command.Kind);
        Assert.Contains("--icon", command.Error);
        Assert.Contains("--entry-id", command.Error);
    }

    [Fact]
    public void Parse_AcceptsApplyWithFolderAndEntryId()
    {
        var folder = @"C:\Users\User\Папка с пробелами";

        var command = CliCommandParser.Parse(["apply", "--folder", folder, "--entry-id", "entry-123"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.Apply, command.Kind);
        Assert.Equal(folder, command.FolderPath);
        Assert.Equal("entry-123", command.EntryId);
        Assert.Null(command.IconPath);
    }

    [Fact]
    public void Parse_RejectsApplyWithIconAndEntryId()
    {
        var command = CliCommandParser.Parse(
            ["apply", "--folder", @"C:\Temp", "--icon", @"C:\Icons\a.ico", "--entry-id", "entry-123"]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.Apply, command.Kind);
        Assert.Contains("mutually exclusive", command.Error);
    }

    [Fact]
    public void Parse_AcceptsCreateWithTargetAndEntryId()
    {
        var command = CliCommandParser.Parse(["create", "--target", @"C:\Temp", "--entry-id", "entry-123"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.Create, command.Kind);
        Assert.Equal(@"C:\Temp", command.TargetPath);
        Assert.Equal("entry-123", command.EntryId);
    }

    [Fact]
    public void Parse_RejectsCreateWithoutEntryId()
    {
        var command = CliCommandParser.Parse(["create", "--target", @"C:\Temp"]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.Create, command.Kind);
        Assert.Contains("--entry-id", command.Error);
    }

    [Fact]
    public void Parse_AcceptsMenuList()
    {
        var command = CliCommandParser.Parse(["menu", "list"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuList, command.Kind);
    }

    [Fact]
    public void Parse_AcceptsMenuAddWithName()
    {
        var icon = @"C:\Users\User\Иконки\skull.ico";

        var command = CliCommandParser.Parse(["menu", "add", "--icon", icon, "--name", "Череп"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuAdd, command.Kind);
        Assert.Equal(icon, command.IconPath);
        Assert.Equal("Череп", command.DisplayName);
    }

    [Fact]
    public void Parse_AcceptsMenuAddWithFolderName()
    {
        var icon = @"C:\Users\User\Иконки\skull.ico";

        var command = CliCommandParser.Parse(
            ["menu", "add", "--icon", icon, "--name", "Череп", "--folder-name", "Новая папка"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuAdd, command.Kind);
        Assert.Equal(icon, command.IconPath);
        Assert.Equal("Череп", command.DisplayName);
        Assert.Equal("Новая папка", command.DefaultFolderName);
    }

    [Fact]
    public void Parse_AcceptsMenuAddWithGroup()
    {
        var icon = @"C:\Users\User\Иконки\blue.ico";

        var command = CliCommandParser.Parse(
            ["menu", "add", "--icon", icon, "--name", "Синяя", "--folder-name", "Синяя", "--group", "Цветные"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuAdd, command.Kind);
        Assert.Equal("Цветные", command.GroupName);
    }

    [Fact]
    public void Parse_AcceptsMenuAddWithEmptyFolderName()
    {
        var icon = @"C:\Users\User\Иконки\skull.ico";

        var command = CliCommandParser.Parse(
            ["menu", "add", "--icon", icon, "--folder-name", ""]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuAdd, command.Kind);
        Assert.Equal(string.Empty, command.DefaultFolderName);
    }

    [Fact]
    public void Parse_AcceptsMenuAddWithoutName()
    {
        var icon = @"C:\Users\User\Иконки\skull.ico";

        var command = CliCommandParser.Parse(["menu", "add", "--icon", icon]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuAdd, command.Kind);
        Assert.Equal(icon, command.IconPath);
        Assert.Null(command.DisplayName);
    }

    [Fact]
    public void Parse_AcceptsMenuRemove()
    {
        var command = CliCommandParser.Parse(["menu", "remove", "--entry-id", "entry-123"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuRemove, command.Kind);
        Assert.Equal("entry-123", command.EntryId);
    }

    [Fact]
    public void Parse_AcceptsMenuResetWithYes()
    {
        var command = CliCommandParser.Parse(["menu", "reset", "--yes"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.MenuReset, command.Kind);
        Assert.True(command.Yes);
    }

    [Fact]
    public void Parse_AcceptsConvertIconWithInputAndOutput()
    {
        var command = CliCommandParser.Parse(["convert-icon", "--input", @"C:\Images\source.png", "--output", @"C:\Icons\folder.ico"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.ConvertIcon, command.Kind);
        Assert.Equal(@"C:\Images\source.png", command.InputPath);
        Assert.Equal(@"C:\Icons\folder.ico", command.OutputPath);
        Assert.False(command.Force);
    }

    [Fact]
    public void Parse_AcceptsConvertIconWithForce()
    {
        var command = CliCommandParser.Parse(["convert-icon", "--input", "source.jpg", "--output", "folder.ico", "--force"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.ConvertIcon, command.Kind);
        Assert.True(command.Force);
    }

    [Fact]
    public void Parse_RejectsConvertIconWithoutInput()
    {
        var command = CliCommandParser.Parse(["convert-icon", "--output", "folder.ico"]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.ConvertIcon, command.Kind);
        Assert.Contains("--input", command.Error);
    }

    [Fact]
    public void Parse_RejectsConvertIconWithoutOutput()
    {
        var command = CliCommandParser.Parse(["convert-icon", "--input", "source.png"]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.ConvertIcon, command.Kind);
        Assert.Contains("--output", command.Error);
    }

    [Fact]
    public void Parse_RejectsConvertIconUnknownOption()
    {
        var command = CliCommandParser.Parse(["convert-icon", "--input", "source.png", "--output", "folder.ico", "--recursive"]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.ConvertIcon, command.Kind);
        Assert.Contains("Unknown option", command.Error);
    }

    [Fact]
    public void HelpText_IncludesConvertIcon()
    {
        Assert.Contains("convert-icon --input", CliHelpText.Text);
        Assert.Contains("convert-icon --input --output [--force]", CliHelpText.Text);
    }

    [Fact]
    public void Parse_RejectsMenuResetWithoutYes()
    {
        var command = CliCommandParser.Parse(["menu", "reset"]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.MenuReset, command.Kind);
        Assert.Contains("--yes", command.Error);
    }

    [Fact]
    public void Parse_AcceptsRegisterMenu()
    {
        var command = CliCommandParser.Parse(["register-menu"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.RegisterMenu, command.Kind);
        Assert.False(command.DryRun);
    }

    [Fact]
    public void Parse_AcceptsRegisterMenuDryRun()
    {
        var command = CliCommandParser.Parse(["register-menu", "--dry-run"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.RegisterMenu, command.Kind);
        Assert.True(command.DryRun);
    }

    [Fact]
    public void Parse_AcceptsRegisterMenuCliPath()
    {
        var cliPath = @"C:\Program Files\Фолдора\Foldora.Cli.exe";

        var command = CliCommandParser.Parse(["register-menu", "--cli-path", cliPath]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.RegisterMenu, command.Kind);
        Assert.Equal(cliPath, command.CliExecutablePath);
        Assert.Equal(cliPath, command.CommandHostPath);
    }

    [Fact]
    public void Parse_AcceptsRegisterMenuHostPath()
    {
        var hostPath = @"C:\Program Files\Фолдора\Foldora.MenuHost.exe";

        var command = CliCommandParser.Parse(["register-menu", "--host-path", hostPath]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.RegisterMenu, command.Kind);
        Assert.Equal(hostPath, command.CommandHostPath);
        Assert.Null(command.CliExecutablePath);
    }

    [Fact]
    public void Parse_RegisterMenuHostPathWinsOverLegacyCliPath()
    {
        var cliPath = @"C:\Program Files\Фолдора\Foldora.Cli.exe";
        var hostPath = @"C:\Program Files\Фолдора\Foldora.MenuHost.exe";

        var command = CliCommandParser.Parse(["register-menu", "--cli-path", cliPath, "--host-path", hostPath]);

        Assert.True(command.IsValid);
        Assert.Equal(hostPath, command.CommandHostPath);
        Assert.Equal(cliPath, command.CliExecutablePath);
    }

    [Fact]
    public void Parse_AcceptsUnregisterMenu()
    {
        var command = CliCommandParser.Parse(["unregister-menu"]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.UnregisterMenu, command.Kind);
    }

    [Fact]
    public void Parse_AcceptsDesktopIniPolicyDiagnostics()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-ini-policy",
            "--target",
            @"C:\Temp\Foldora Tests",
            "--icon",
            @"C:\Icons\folder.ico"
        ]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIniPolicy, command.Kind);
        Assert.Equal(@"C:\Temp\Foldora Tests", command.TargetPath);
        Assert.Equal(@"C:\Icons\folder.ico", command.IconPath);
    }

    [Fact]
    public void Parse_RejectsDesktopIniPolicyDiagnosticsWithoutTarget()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-ini-policy",
            "--icon",
            @"C:\Icons\folder.ico"
        ]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIniPolicy, command.Kind);
        Assert.Contains("--target", command.Error);
    }

    [Fact]
    public void Parse_RejectsDesktopIniPolicyDiagnosticsWithoutIcon()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-ini-policy",
            "--target",
            @"C:\Temp\Foldora Tests"
        ]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIniPolicy, command.Kind);
        Assert.Contains("--icon", command.Error);
    }

    [Fact]
    public void Parse_AcceptsDesktopIconPositionDiagnosticsWithDefaultScreenCoordinates()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-icon-position",
            "--name",
            "Foldora Test",
            "--x",
            "100",
            "--y",
            "200"
        ]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIconPosition, command.Kind);
        Assert.Equal("Foldora Test", command.DisplayName);
        Assert.Equal(100, command.X);
        Assert.Equal(200, command.Y);
        Assert.Equal("screen", command.CoordinateSpace);
    }

    [Fact]
    public void Parse_AcceptsDesktopIconPositionDiagnosticsWithViewCoordinates()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-icon-position",
            "--name",
            "Foldora Test",
            "--x",
            "10",
            "--y",
            "20",
            "--coordinate-space",
            "view"
        ]);

        Assert.True(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIconPosition, command.Kind);
        Assert.Equal("view", command.CoordinateSpace);
    }

    [Fact]
    public void Parse_RejectsDesktopIconPositionDiagnosticsWithoutName()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-icon-position",
            "--x",
            "100",
            "--y",
            "200"
        ]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIconPosition, command.Kind);
        Assert.Contains("--name", command.Error);
    }

    [Fact]
    public void Parse_RejectsDesktopIconPositionDiagnosticsWithNonIntegerCoordinate()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-icon-position",
            "--name",
            "Foldora Test",
            "--x",
            "left",
            "--y",
            "200"
        ]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIconPosition, command.Kind);
        Assert.Contains("--x", command.Error);
        Assert.Contains("integer", command.Error);
    }

    [Fact]
    public void Parse_RejectsDesktopIconPositionDiagnosticsWithUnknownCoordinateSpace()
    {
        var command = CliCommandParser.Parse([
            "diagnostics",
            "desktop-icon-position",
            "--name",
            "Foldora Test",
            "--x",
            "100",
            "--y",
            "200",
            "--coordinate-space",
            "cursor"
        ]);

        Assert.False(command.IsValid);
        Assert.Equal(CliCommandKind.DiagnosticsDesktopIconPosition, command.Kind);
        Assert.Contains("--coordinate-space", command.Error);
    }
}
