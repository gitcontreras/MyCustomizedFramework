using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.CrudGeneration;
using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Cli.Configuration;
using MyCustomizedFramework.Cli.Writing;
using MyCustomizedFramework.Domain.SchemaExplorer;
using MyCustomizedFramework.Infrastructure;

namespace MyCustomizedFramework.Cli.Commands;

internal static class GenerateCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        var options = ArgsParser.Parse(args);

        var tableOption = options.GetValueOrDefault("table");
        var allTables = options.ContainsKey("all-tables");

        if (allTables && !string.IsNullOrWhiteSpace(tableOption))
        {
            Console.Error.WriteLine("Cannot combine --table with --all-tables.");
            return 1;
        }

        if (!allTables && string.IsNullOrWhiteSpace(tableOption))
        {
            Console.Error.WriteLine("Provide --table <Name>[,<Name>...] or --all-tables.");
            return 1;
        }

        var configPath = options.GetValueOrDefault("config") ?? "crudgen.config.json";
        var config = File.Exists(configPath) ? CrudGenConfig.Load(configPath) : null;

        var connection = ConnectionResolver.Resolve(config, options, PasswordResolver.ReadMasked, Console.Error);
        if (connection is null)
        {
            return 1;
        }

        var engineResult = DatabaseEngineParser.Parse(connection.Engine);
        if (engineResult.IsFailure)
        {
            Console.Error.WriteLine($"Error: {engineResult.Error.Code}: {engineResult.Error.Message}");
            return 1;
        }

        if (config is null || config.Paths.Count == 0)
        {
            Console.Error.WriteLine("No 'paths' mapping found. Run 'crudgen init' and fill in crudgen.config.json first.");
            return 1;
        }

        var rootNamespace = options.GetValueOrDefault("root-namespace") ?? config.RootNamespace ?? "MyCustomizedFramework";
        var repoRoot = Path.GetDirectoryName(Path.GetFullPath(configPath))!;
        var force = options.ContainsKey("force");

        await using var provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();

        var connectionDetails = new DatabaseConnectionDetails(
            connection.Server, connection.Port, connection.Database, connection.User, connection.Password);

        string[] tableNames;
        if (allTables)
        {
            var tablesHandler = provider.GetRequiredService<GetTablesHandler>();
            var tablesResult = await tablesHandler.HandleAsync(new GetTablesQuery(connectionDetails, engineResult.Value));
            if (tablesResult.IsFailure)
            {
                Console.Error.WriteLine($"Error: {tablesResult.Error.Code}: {tablesResult.Error.Message}");
                return 1;
            }

            tableNames = tablesResult.Value.Select(table => table.Name).ToArray();
            if (tableNames.Length == 0)
            {
                Console.Error.WriteLine("No tables found for that connection.");
                return 1;
            }
        }
        else
        {
            tableNames = tableOption!
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();

            if (tableNames.Length == 0)
            {
                Console.Error.WriteLine("Provide at least one table name in --table.");
                return 1;
            }
        }

        var generateHandler = provider.GetRequiredService<GenerateCrudHandler>();

        var totalWritten = 0;
        var totalSkipped = 0;
        var failedTables = new List<string>();

        foreach (var tableName in tableNames)
        {
            Console.WriteLine($"== {tableName} ==");

            var result = await generateHandler.HandleAsync(
                new GenerateCrudQuery(connectionDetails, engineResult.Value, tableName, rootNamespace));

            if (result.IsFailure)
            {
                Console.Error.WriteLine($"  skipped table (error): {result.Error.Code}: {result.Error.Message}");
                failedTables.Add(tableName);
                continue;
            }

            WriteResult writeResult;
            try
            {
                writeResult = GeneratedFileWriter.Write(result.Value, config.Paths, repoRoot, force);
            }
            catch (InvalidOperationException exception)
            {
                Console.Error.WriteLine($"  Error: {exception.Message}");
                failedTables.Add(tableName);
                continue;
            }

            foreach (var path in writeResult.Written)
            {
                Console.WriteLine($"  written: {path}");
            }

            foreach (var path in writeResult.Skipped)
            {
                Console.WriteLine($"  skipped (already exists, use --force to overwrite): {path}");
            }

            totalWritten += writeResult.Written.Count;
            totalSkipped += writeResult.Skipped.Count;
        }

        var succeeded = tableNames.Length - failedTables.Count;
        Console.WriteLine();
        Console.WriteLine($"{succeeded}/{tableNames.Length} tables generated, {totalWritten} files written, {totalSkipped} files skipped.");

        if (failedTables.Count > 0)
        {
            Console.WriteLine($"Tables with errors: {string.Join(", ", failedTables)}");
        }

        return succeeded == 0 ? 1 : 0;
    }
}
