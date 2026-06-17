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
}
