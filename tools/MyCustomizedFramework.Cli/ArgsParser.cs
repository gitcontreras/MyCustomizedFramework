namespace MyCustomizedFramework.Cli;

/// <summary>
/// Minimal "--name value" / "--flag" parser. No System.CommandLine: this tool only has 2-3
/// subcommands and a handful of options, which doesn't earn a (still-prerelease) dependency.
/// </summary>
internal static class ArgsParser
{
    public static Dictionary<string, string?> Parse(string[] args)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var name = arg[2..];
            var hasValue = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal);
            result[name] = hasValue ? args[++i] : null;
        }

        return result;
    }
}
