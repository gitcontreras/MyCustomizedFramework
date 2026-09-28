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
    "init-kernel" => InitKernelCommand.Run(rest),
    _ => PrintUsage()
};

static int PrintUsage()
{
    Console.WriteLine(
        """
        Usage:
          crudgen tables      --engine <sql|postgres|mysql|oracle> --server <host> [--port N] --database <name> [--user <user>] [--password <pwd>] [--config <path>]
          crudgen generate    --table <Name>[,<Name>...] [--force] [--root-namespace <Namespace>] [connection flags as above]
          crudgen generate    --all-tables [--force] [--root-namespace <Namespace>] [connection flags as above]
          crudgen init        [--config <path>] [--force]
          crudgen init-kernel [--config <path>] [--force] [--engine ...] [--root-namespace ...]

        Run 'init' first and edit crudgen.config.json with your real values, THEN run 'init-kernel' to
        scaffold the shared Result/IDbConnectionFactory/ResultExtensions files every generated table needs.
        Connects directly to the database and generates files on disk - no API server needed.
        Connection flags fall back to crudgen.config.json when omitted.
        Password resolution order: --password > CRUDGEN_DB_PASSWORD env var > interactive prompt.
        """);
    return 1;
}
