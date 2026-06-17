using Foldora.Shell.ContextMenu;

namespace Foldora.Tests.Shell;

public sealed class CommandLineQuoterTests
{
    [Fact]
    public void Quote_AddsQuotesForPathWithSpaces()
    {
        var quoted = CommandLineQuoter.Quote(@"C:\Program Files\Foldora\foldora.exe");

        Assert.Equal(@"""C:\Program Files\Foldora\foldora.exe""", quoted);
    }

    [Fact]
    public void Quote_DoesNotBreakCyrillic()
    {
        var quoted = CommandLineQuoter.Quote(@"C:\Папки\иконка.ico");

        Assert.Equal(@"C:\Папки\иконка.ico", quoted);
    }

    [Fact]
    public void Join_QuotesOnlyArgumentsThatNeedQuoting()
    {
        var command = CommandLineQuoter.Join(
            @"C:\Program Files\Foldora\foldora.exe",
            "apply",
            "--folder",
            @"C:\Users\User\Мои папки");

        Assert.Equal(@"""C:\Program Files\Foldora\foldora.exe"" apply --folder ""C:\Users\User\Мои папки""", command);
    }
}
