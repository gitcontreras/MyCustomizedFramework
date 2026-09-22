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

        var tableName = options.GetValueOrDefault("table");
        if (string.IsNullOrWhiteSpace(tableName))
        {
            Console.Error.WriteLine("Missing required --table <name>.");
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

        await using var provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        var handler = provider.GetRequiredService<GenerateCrudHandler>();

        var connectionDetails = new DatabaseConnectionDetails(
            connection.Server, connection.Port, connection.Database, connection.User, connection.Password);

        var result = await handler.HandleAsync(
            new GenerateCrudQuery(connectionDetails, engineResult.Value, tableName, rootNamespace));

        if (result.IsFailure)
        {
            Console.Error.WriteLine($"Error: {result.Error.Code}: {result.Error.Message}");
            return 1;
        }

        var repoRoot = Path.GetDirectoryName(Path.GetFullPath(configPath))!;
        var force = options.ContainsKey("force");

        WriteResult writeResult;
        try
        {
            writeResult = GeneratedFileWriter.Write(result.Value, config.Paths, repoRoot, force);
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 1;
        }

        foreach (var path in writeResult.Written)
        {
            Console.WriteLine($"written: {path}");
        }

        foreach (var path in writeResult.Skipped)
        {
            Console.WriteLine($"skipped (already exists, use --force to overwrite): {path}");
        }

        Console.WriteLine($"{writeResult.Written.Count} written, {writeResult.Skipped.Count} skipped.");
        return 0;
    }
}
