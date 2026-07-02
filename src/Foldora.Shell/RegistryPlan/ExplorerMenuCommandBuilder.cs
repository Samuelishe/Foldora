using Foldora.Shell.ContextMenu;

namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Строит command strings для registry command values.
/// </summary>
public sealed class ExplorerMenuCommandBuilder
{
    public string BuildCreateFolderCommand(
        string commandHostPath,
        ExplorerMenuTargetKind targetKind,
        string entryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandHostPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        return string.Join(
            " ",
            CommandLineQuoter.Quote(commandHostPath),
            CommandLineQuoter.Quote("create"),
            CommandLineQuoter.Quote("--target"),
            CommandLineQuoter.QuoteAlways(ExplorerMenuShellTargetPlaceholder.GetPlaceholder(targetKind)),
            CommandLineQuoter.Quote("--entry-id"),
            CommandLineQuoter.QuoteAlways(entryId));
    }
}
