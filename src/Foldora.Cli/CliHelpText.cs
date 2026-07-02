namespace Foldora.Cli;

public static class CliHelpText
{
    public const string Text = """
Foldora CLI

Usage:
  foldora apply --folder "<folder>" --icon "<absolute-icon-path>"
  foldora apply --folder "<folder>" --entry-id "<entry-id>"
  foldora create --target "<directory>" --entry-id "<entry-id>"
  foldora clear --folder "<folder>"
  foldora menu list
  foldora menu add --icon "<absolute-icon-path>" [--name "<display-name>"] [--folder-name "<default-folder-name>"] [--group "<group-name>"]
  foldora menu remove --entry-id "<entry-id>"
  foldora menu reset --yes
  foldora convert-icon --input "<image-path>" --output "<ico-path>" [--force]
  foldora register-menu [--dry-run] [--host-path "<absolute-path-to-Foldora.MenuHost.exe>"] [--cli-path "<legacy-dev-override>"]
  foldora unregister-menu
  foldora diagnostics desktop-ini-policy --target "<directory>" --icon "<absolute-icon-path>"
  foldora diagnostics desktop-icon-position --name "<desktop item name>" --x <int> --y <int> [--coordinate-space screen|view]
  foldora import-pack --path "<pack-path>"
  foldora list-packs
  foldora list-styles
  foldora settings

Implemented now:
  apply --folder --icon
  apply --folder --entry-id
  create --target --entry-id
  clear --folder
  menu list
  menu add --icon [--name] [--folder-name] [--group]
  menu remove --entry-id
  menu reset --yes
  convert-icon --input --output [--force]
  register-menu [--dry-run] [--host-path] [--cli-path]
  unregister-menu
  diagnostics desktop-ini-policy --target --icon
  diagnostics desktop-icon-position --name --x --y [--coordinate-space]

The --style flow, pack import, batch icon conversion, Explorer restart, icon cache reset, and production create-under-cursor behavior are not implemented in this step.
""";
}
