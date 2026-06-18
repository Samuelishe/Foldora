using Foldora.Core.DesktopIni;
using Foldora.Core.Validation;

namespace Foldora.Cli;

/// <summary>
/// Создаёт тестовые папки для ручной проверки desktop.ini attribute policies.
/// </summary>
public sealed class DesktopIniPolicyDiagnosticsRunner
{
    private const string TestFolderPrefix = "Foldora Policy Test - ";

    private readonly DesktopIniService desktopIniService;
    private readonly IconFileValidator iconFileValidator;

    public DesktopIniPolicyDiagnosticsRunner(
        DesktopIniService? desktopIniService = null,
        IconFileValidator? iconFileValidator = null)
    {
        this.desktopIniService = desktopIniService ?? new DesktopIniService();
        this.iconFileValidator = iconFileValidator ?? new IconFileValidator();
    }

    public async Task<IReadOnlyList<string>> RunAsync(
        string targetDirectory,
        string iconPath,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(iconPath);
        ArgumentNullException.ThrowIfNull(output);

        var target = new DirectoryInfo(targetDirectory);
        if (!target.Exists)
        {
            throw new DirectoryNotFoundException($"Target directory was not found: {target.FullName}");
        }

        iconFileValidator.EnsureValid(iconPath);

        var createdFolders = new List<string>();
        foreach (var policy in DesktopIniAttributePolicy.Supported)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var folderPath = Path.Combine(target.FullName, TestFolderPrefix + policy.DiagnosticFolderName);
            Directory.CreateDirectory(folderPath);

            await desktopIniService.ApplyIconAsync(
                new DesktopIniOptions(folderPath, iconPath, policy),
                cancellationToken);

            createdFolders.Add(folderPath);
            output.WriteLine($"{policy.Name}: {folderPath}");
        }

        output.WriteLine();
        output.WriteLine("Manual checklist:");
        output.WriteLine("1. Посмотри, появилась ли кастомная иконка.");
        output.WriteLine("2. Обнови Explorer / F5.");
        output.WriteLine("3. Закрой и открой папку.");
        output.WriteLine("4. Попробуй удалить каждую тестовую папку.");
        output.WriteLine("5. Зафиксируй, где появляется warning про системный desktop.ini.");

        return createdFolders;
    }
}
