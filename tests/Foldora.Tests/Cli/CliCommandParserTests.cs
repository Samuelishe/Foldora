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
}
