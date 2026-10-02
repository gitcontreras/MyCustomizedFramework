using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.CrudGeneration;
using MyCustomizedFramework.Application.FrontendGeneration;
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
            Console.Error.WriteLine("Provide --table <[Schema.]Name>[,<[Schema.]Name>...] or --all-tables.");
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

        var frontendOnly = options.ContainsKey("frontend-only");
        var withFrontend = frontendOnly || options.ContainsKey("with-frontend");

        FrontendOptions? frontend = null;
        if (withFrontend)
        {
            frontend = FrontendOptionsResolver.Resolve(config?.Frontend, options, out var frontendError);
            if (frontend is null)
            {
                Console.Error.WriteLine($"Error: {frontendError}");
                return 1;
            }
        }

        if (!frontendOnly && (config is null || config.Paths.Count == 0))
        {
            Console.Error.WriteLine("No 'paths' mapping found. Run 'crudgen init' and fill in crudgen.config.json first.");
            return 1;
        }

        var rootNamespace = options.GetValueOrDefault("root-namespace") ?? config?.RootNamespace ?? "MyCustomizedFramework";
        var repoRoot = Path.GetDirectoryName(Path.GetFullPath(configPath))!;
        var force = options.ContainsKey("force");

        var paths = new Dictionary<string, string>(config?.Paths ?? [], StringComparer.OrdinalIgnoreCase);
        if (frontend is not null)
        {
            paths[FrontendOptionsResolver.BucketName] = frontend.Path;
        }

        await using var provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();

        var connectionDetails = new DatabaseConnectionDetails(
            connection.Server, connection.Port, connection.Database, connection.User, connection.Password);

        TableIdentifier[] tables;
        if (allTables)
        {
            var tablesHandler = provider.GetRequiredService<GetTablesHandler>();
            var tablesResult = await tablesHandler.HandleAsync(new GetTablesQuery(connectionDetails, engineResult.Value));
            if (tablesResult.IsFailure)
            {
                Console.Error.WriteLine($"Error: {tablesResult.Error.Code}: {tablesResult.Error.Message}");
                return 1;
            }

            tables = tablesResult.Value.Select(table => new TableIdentifier(table.Schema, table.Name)).ToArray();
            if (tables.Length == 0)
            {
                Console.Error.WriteLine("No tables found for that connection.");
                return 1;
            }
        }
        else
        {
            tables = tableOption!
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(TableIdentifier.Parse)
                .ToArray();

            if (tables.Length == 0)
            {
                Console.Error.WriteLine("Provide at least one table name in --table.");
                return 1;
            }
        }

        var generateHandler = provider.GetRequiredService<GenerateCrudHandler>();
        var frontendHandler = provider.GetRequiredService<GenerateFrontendHandler>();

        var frontendGeneration = frontend is null
            ? null
            : new FrontendGenerationOptions(frontend.Framework, frontend.StateManagement, frontend.Styles, frontend.ApiBaseUrl);

        var totalWritten = 0;
        var totalSkipped = 0;

        if (frontendGeneration is not null)
        {
            var kernel = provider.GetRequiredService<GenerateFrontendKernelHandler>().Handle(frontendGeneration);
            if (kernel.IsFailure)
            {
                Console.Error.WriteLine($"Error: {kernel.Error.Code}: {kernel.Error.Message}");
                return 1;
            }

            Console.WriteLine("== frontend shared kernel ==");
            var kernelWrite = GeneratedFileWriter.Write(kernel.Value, paths, repoRoot, force);
            PrintWrite(kernelWrite);
            totalWritten += kernelWrite.Written.Count;
            totalSkipped += kernelWrite.Skipped.Count;
        }

        var failedTables = new List<string>();

        foreach (var table in tables)
        {
            var label = table.Schema is null ? table.Name : $"{table.Schema}.{table.Name}";
            Console.WriteLine($"== {label} ==");

            var tableFiles = new List<GeneratedFile>();

            if (!frontendOnly)
            {
                var result = await generateHandler.HandleAsync(
                    new GenerateCrudQuery(connectionDetails, engineResult.Value, table.Schema, table.Name, rootNamespace));

                if (result.IsFailure)
                {
                    Console.Error.WriteLine($"  skipped table (error): {result.Error.Code}: {result.Error.Message}");
                    failedTables.Add(label);
                    continue;
                }

                tableFiles.AddRange(result.Value);
            }

            if (frontendGeneration is not null)
            {
                var webResult = await frontendHandler.HandleAsync(
                    new GenerateFrontendQuery(connectionDetails, engineResult.Value, table.Schema, table.Name, frontendGeneration));

                if (webResult.IsFailure)
                {
                    Console.Error.WriteLine($"  skipped frontend (error): {webResult.Error.Code}: {webResult.Error.Message}");
                    failedTables.Add(label);
                    continue;
                }

                tableFiles.AddRange(webResult.Value);
            }

            WriteResult writeResult;
            try
            {
                writeResult = GeneratedFileWriter.Write(tableFiles, paths, repoRoot, force);
            }
            catch (InvalidOperationException exception)
            {
                Console.Error.WriteLine($"  Error: {exception.Message}");
                failedTables.Add(label);
                continue;
            }

            PrintWrite(writeResult);
            totalWritten += writeResult.Written.Count;
            totalSkipped += writeResult.Skipped.Count;
        }
        if (frontend is not null)
        {
            var barrel = FrontendRoutesBarrel.Write(Path.Combine(repoRoot, frontend.Path), frontend.Framework);
            if (barrel is not null)
            {
                Console.WriteLine($"  routes barrel: {barrel}");
            }
        }

        var succeeded = tables.Length - failedTables.Count;
        Console.WriteLine();
        Console.WriteLine($"{succeeded}/{tables.Length} tables generated, {totalWritten} files written, {totalSkipped} files skipped.");

        if (failedTables.Count > 0)
        {
            Console.WriteLine($"Tables with errors: {string.Join(", ", failedTables)}");
        }

        return succeeded == 0 ? 1 : 0;
    }

    private static void PrintWrite(WriteResult writeResult)
    {
        foreach (var path in writeResult.Written)
        {
            Console.WriteLine($"  written: {path}");
        }

        foreach (var path in writeResult.Skipped)
        {
            Console.WriteLine($"  skipped (already exists, use --force to overwrite): {path}");
        }
    }
}


