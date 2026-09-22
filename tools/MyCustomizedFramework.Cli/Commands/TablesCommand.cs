using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Cli.Configuration;
using MyCustomizedFramework.Domain.SchemaExplorer;
using MyCustomizedFramework.Infrastructure;

namespace MyCustomizedFramework.Cli.Commands;

internal static class TablesCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        var options = ArgsParser.Parse(args);
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

        await using var provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        var handler = provider.GetRequiredService<GetTablesHandler>();

        var connectionDetails = new DatabaseConnectionDetails(
            connection.Server, connection.Port, connection.Database, connection.User, connection.Password);

        var result = await handler.HandleAsync(new GetTablesQuery(connectionDetails, engineResult.Value));
        if (result.IsFailure)
        {
            Console.Error.WriteLine($"Error: {result.Error.Code}: {result.Error.Message}");
            return 1;
        }

        Console.WriteLine($"{"Schema",-24}Name");
        foreach (var table in result.Value)
        {
            Console.WriteLine($"{table.Schema,-24}{table.Name}");
        }

        return 0;
    }
}
