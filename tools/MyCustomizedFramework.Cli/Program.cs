using MyCustomizedFramework.Cli.Commands;

if (args.Length == 0)
{
    return PrintUsage();
}

var subcommand = args[0];
var rest = args[1..];

return subcommand.ToLowerInvariant() switch
{
    "tables" => await TablesCommand.RunAsync(rest),
    "generate" => await GenerateCommand.RunAsync(rest),
    "init" => InitCommand.Run(rest),
    _ => PrintUsage()
};

static int PrintUsage()
{
    Console.WriteLine(
        """
        Usage:
          crudgen tables   --engine <sql|postgres|mysql|oracle> --server <host> [--port N] --database <name> [--user <user>] [--password <pwd>] [--config <path>]
          crudgen generate --table <Name> [--force] [--root-namespace <Namespace>] [connection flags as above]
          crudgen init     [--config <path>] [--force]

        Connects directly to the database and generates files on disk - no API server needed.
        Connection flags fall back to crudgen.config.json when omitted.
        Password resolution order: --password > CRUDGEN_DB_PASSWORD env var > interactive prompt.
        """);
    return 1;
}
