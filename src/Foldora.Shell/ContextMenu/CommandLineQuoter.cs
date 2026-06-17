using System.Text;

namespace Foldora.Shell.ContextMenu;

/// <summary>
/// Безопасно экранирует аргументы командной строки для registry command values.
/// </summary>
public static class CommandLineQuoter
{
    public static string Quote(string argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return Quote(argument, forceQuotes: false);
    }

    public static string QuoteAlways(string argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return Quote(argument, forceQuotes: true);
    }

    private static string Quote(string argument, bool forceQuotes)
    {
        ArgumentNullException.ThrowIfNull(argument);

        if (argument.Length == 0)
        {
            return "\"\"";
        }

        var requiresQuotes = forceQuotes || argument.Any(char.IsWhiteSpace) || argument.Contains('"');
        if (!requiresQuotes)
        {
            return argument;
        }

        var builder = new StringBuilder();
        builder.Append('"');

        var backslashCount = 0;
        foreach (var character in argument)
        {
            if (character == '\\')
            {
                backslashCount++;
                continue;
            }

            if (character == '"')
            {
                builder.Append('\\', backslashCount * 2 + 1);
                builder.Append('"');
                backslashCount = 0;
                continue;
            }

            builder.Append('\\', backslashCount);
            builder.Append(character);
            backslashCount = 0;
        }

        builder.Append('\\', backslashCount * 2);
        builder.Append('"');
        return builder.ToString();
    }

    public static string Join(params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return string.Join(" ", arguments.Select(Quote));
    }
}
