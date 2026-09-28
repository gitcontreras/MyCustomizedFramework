using MyCustomizedFramework.Cli.Configuration;

namespace MyCustomizedFramework.Cli.Commands;

internal static class InitCommand
{
    public static int Run(string[] args)
    {
        var options = ArgsParser.Parse(args);
        var configPath = options.GetValueOrDefault("config") ?? "crudgen.config.json";
        var force = options.ContainsKey("force");

        if (File.Exists(configPath) && !force)
        {
            Console.Error.WriteLine($"'{configPath}' already exists. Pass --force to overwrite.");
            return 1;
        }

        File.WriteAllText(configPath, CrudGenConfig.TemplateJson);
        Console.WriteLine(
            $"Wrote template config to '{configPath}'. Edit it with your real rootNamespace/connection/paths, "
            + "then run 'crudgen init-kernel' to scaffold the shared Result/IDbConnectionFactory/ResultExtensions files.");

        return 0;
    }
}
