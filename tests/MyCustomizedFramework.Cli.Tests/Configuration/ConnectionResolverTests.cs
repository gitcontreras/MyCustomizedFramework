using MyCustomizedFramework.Cli.Configuration;

namespace MyCustomizedFramework.Cli.Tests.Configuration;

public sealed class ConnectionResolverTests
{
    [Fact]
    public void ResolveReturnsNullAndWritesAnErrorWhenRequiredFieldsAreMissing()
    {
        var errorWriter = new StringWriter();
        var options = new Dictionary<string, string?>();

        var connection = ConnectionResolver.Resolve(config: null, options, () => "unused", errorWriter);

        Assert.Null(connection);
        Assert.Contains("Missing required connection info", errorWriter.ToString());
    }

    [Fact]
    public void ResolvePrefersFlagsOverConfigValues()
    {
        var config = new CrudGenConfig
        {
            Connection = new ConnectionConfig { Engine = "postgres", Server = "config-server", Database = "config-db", Port = 5432 }
        };
        var options = new Dictionary<string, string?>
        {
            ["engine"] = "sql",
            ["server"] = "flag-server",
            ["database"] = "flag-db",
            ["password"] = "flag-password"
        };

        var connection = ConnectionResolver.Resolve(config, options, () => throw new InvalidOperationException(), TextWriter.Null);

        Assert.NotNull(connection);
        Assert.Equal("sql", connection!.Engine);
        Assert.Equal("flag-server", connection.Server);
        Assert.Equal("flag-db", connection.Database);
        Assert.Equal("flag-password", connection.Password);
    }

    [Fact]
    public void ResolveFallsBackToConfigWhenNoFlagsAreGiven()
    {
        var config = new CrudGenConfig
        {
            Connection = new ConnectionConfig { Engine = "postgres", Server = "config-server", Database = "config-db", Port = 5432, User = "config-user" }
        };

        var connection = ConnectionResolver.Resolve(config, new Dictionary<string, string?>(), () => "prompted", TextWriter.Null);

        Assert.NotNull(connection);
        Assert.Equal("postgres", connection!.Engine);
        Assert.Equal("config-server", connection.Server);
        Assert.Equal("config-db", connection.Database);
        Assert.Equal(5432, connection.Port);
        Assert.Equal("config-user", connection.User);
        Assert.Equal("prompted", connection.Password);
    }
}
