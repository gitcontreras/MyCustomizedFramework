namespace MyCustomizedFramework.Cli.Configuration;

/// <summary>
/// Merges connection info from --flags (highest priority) and crudgen.config.json (fallback),
/// then resolves the password separately via <see cref="PasswordResolver"/>.
/// </summary>
internal static class ConnectionResolver
{
    public static ConnectionOptions? Resolve(
        CrudGenConfig? config,
        IReadOnlyDictionary<string, string?> options,
        Func<string> promptForPassword,
        TextWriter errorWriter)
    {
        var engine = options.GetValueOrDefault("engine") ?? config?.Connection.Engine;
        var server = options.GetValueOrDefault("server") ?? config?.Connection.Server;
        var database = options.GetValueOrDefault("database") ?? config?.Connection.Database;
        var user = options.GetValueOrDefault("user") ?? config?.Connection.User;
        var portRaw = options.GetValueOrDefault("port") ?? config?.Connection.Port?.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(engine) || string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
        {
            errorWriter.WriteLine(
                "Missing required connection info (engine/server/database). Pass them as flags or set them in crudgen.config.json.");
            return null;
        }

        int? port = int.TryParse(portRaw, out var parsedPort) ? parsedPort : null;
        var password = new PasswordResolver(promptForPassword).Resolve(options.GetValueOrDefault("password"));

        return new ConnectionOptions
        {
            Engine = engine,
            Server = server,
            Port = port,
            Database = database,
            User = user ?? string.Empty,
            Password = password
        };
    }
}
