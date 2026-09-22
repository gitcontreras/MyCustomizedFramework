namespace MyCustomizedFramework.Cli.Configuration;

internal sealed class ConnectionOptions
{
    public required string Engine { get; init; }

    public required string Server { get; init; }

    public int? Port { get; init; }

    public required string Database { get; init; }

    public string User { get; init; } = string.Empty;

    public required string Password { get; init; }
}
