using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.FrontendGeneration;
using MyCustomizedFramework.Cli.Configuration;
using MyCustomizedFramework.Cli.Writing;
using MyCustomizedFramework.Infrastructure;

namespace MyCustomizedFramework.Cli.Commands;

/// <summary>Writes the shared web kernel (HTTP client, error type, form controls). Needs no database.</summary>
internal static class InitWebCommand
{
    public static int Run(string[] args)
    {
        var options = ArgsParser.Parse(args);
        var configPath = options.GetValueOrDefault("config") ?? "crudgen.config.json";
        var config = File.Exists(configPath) ? CrudGenConfig.Load(configPath) : null;

        var frontend = FrontendOptionsResolver.Resolve(config?.Frontend, options, out var error);
        if (frontend is null)
        {
            Console.Error.WriteLine($"Error: {error}");
            return 1;
        }

        using var provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        var result = provider.GetRequiredService<GenerateFrontendKernelHandler>().Handle(
            new FrontendGenerationOptions(frontend.Framework, frontend.StateManagement, frontend.Styles, frontend.ApiBaseUrl));

        if (result.IsFailure)
        {
            Console.Error.WriteLine($"Error: {result.Error.Code}: {result.Error.Message}");
            return 1;
        }

        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [FrontendOptionsResolver.BucketName] = frontend.Path
        };
        var repoRoot = Path.GetDirectoryName(Path.GetFullPath(configPath))!;

        var write = GeneratedFileWriter.Write(result.Value, paths, repoRoot, options.ContainsKey("force"));
        foreach (var path in write.Written)
        {
            Console.WriteLine($"written: {path}");
        }

        foreach (var path in write.Skipped)
        {
            Console.WriteLine($"skipped (already exists, use --force to overwrite): {path}");
        }

        Console.WriteLine($"Web kernel: {write.Written.Count} written, {write.Skipped.Count} skipped.");
        return 0;
    }
}
