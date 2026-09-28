using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Cli.Configuration;
using MyCustomizedFramework.Cli.Writing;
using MyCustomizedFramework.Infrastructure;

namespace MyCustomizedFramework.Cli.Commands;

/// <summary>
/// Deliberately a separate command from <see cref="InitCommand"/>: scaffolding the kernel needs the
/// real rootNamespace/engine/paths already in crudgen.config.json. Folding this into 'init' meant a
/// fresh run wrote the placeholder config and immediately used those same placeholders for the kernel
/// files - wrong namespaces, wrong folders. Requiring 'init' first and this command second forces the
/// edit step in between.
/// </summary>
internal static class InitKernelCommand
{
    private const string PlaceholderRootNamespace = "YourCompany.YourProject";

    public static int Run(string[] args)
    {
        var options = ArgsParser.Parse(args);
        var configPath = options.GetValueOrDefault("config") ?? "crudgen.config.json";
        var force = options.ContainsKey("force");

        if (!File.Exists(configPath))
        {
            Console.Error.WriteLine(
                $"'{configPath}' was not found. Run 'crudgen init' first, edit it with your real values, "
                + "then run 'crudgen init-kernel'.");
            return 1;
        }

        var config = CrudGenConfig.Load(configPath);

        var rootNamespace = options.GetValueOrDefault("root-namespace") ?? config.RootNamespace;
        if (string.IsNullOrWhiteSpace(rootNamespace) || rootNamespace == PlaceholderRootNamespace)
        {
            Console.Error.WriteLine(
                $"'{configPath}' still has the placeholder rootNamespace (\"{PlaceholderRootNamespace}\"). "
                + "Edit the file with your real rootNamespace and paths before running 'crudgen init-kernel'.");
            return 1;
        }

        var engineRaw = options.GetValueOrDefault("engine") ?? config.Connection.Engine;
        if (string.IsNullOrWhiteSpace(engineRaw))
        {
            Console.Error.WriteLine("Cannot scaffold the kernel: no --engine given and none set in crudgen.config.json.");
            return 1;
        }

        var engineResult = DatabaseEngineParser.Parse(engineRaw);
        if (engineResult.IsFailure)
        {
            Console.Error.WriteLine($"Error: {engineResult.Error.Code}: {engineResult.Error.Message}");
            return 1;
        }

        if (config.Paths.Count == 0)
        {
            Console.Error.WriteLine("Cannot scaffold the kernel: crudgen.config.json has no 'paths' mapping.");
            return 1;
        }

        var repoRoot = Path.GetDirectoryName(Path.GetFullPath(configPath))!;

        using var provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        var generator = provider.GetRequiredService<IKernelFileGenerator>();

        var files = generator.Generate(engineResult.Value, rootNamespace);

        WriteResult writeResult;
        try
        {
            writeResult = GeneratedFileWriter.Write(files, config.Paths, repoRoot, force);
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

        Console.WriteLine($"Kernel scaffolding: {writeResult.Written.Count} written, {writeResult.Skipped.Count} skipped.");
        Console.WriteLine(
            "Next steps: 1) add a \"Default\" connection string under \"ConnectionStrings\" in your appsettings.json, "
            + "2) register IDbConnectionFactory in your DI composition, e.g. "
            + $"services.AddScoped<IDbConnectionFactory, {engineResult.Value}ConnectionFactory>().");

        return 0;
    }
}
