using MyCustomizedFramework.Cli.Configuration;

namespace MyCustomizedFramework.Cli.Tests.Configuration;

public sealed class CrudGenConfigTests
{
    [Fact]
    public void LoadParsesRootNamespaceConnectionAndPaths()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(
                path,
                """
                {
                  "rootNamespace": "Acme.Payroll",
                  "connection": {
                    "engine": "sql",
                    "server": "localhost",
                    "port": 1433,
                    "database": "SampleDb",
                    "user": "sa"
                  },
                  "paths": {
                    "Domain": "src/Acme.Payroll.Domain"
                  }
                }
                """);

            var config = CrudGenConfig.Load(path);

            Assert.Equal("Acme.Payroll", config.RootNamespace);
            Assert.Equal("sql", config.Connection.Engine);
            Assert.Equal("localhost", config.Connection.Server);
            Assert.Equal(1433, config.Connection.Port);
            Assert.Equal("SampleDb", config.Connection.Database);
            Assert.Equal("sa", config.Connection.User);
            Assert.Equal("src/Acme.Payroll.Domain", config.Paths["Domain"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TemplateJsonParsesBackIntoAValidConfig()
    {
        var config = System.Text.Json.JsonSerializer.Deserialize<CrudGenConfig>(
            CrudGenConfig.TemplateJson,
            CrudGenConfig.JsonOptions);

        Assert.NotNull(config);
        Assert.False(string.IsNullOrWhiteSpace(config!.RootNamespace));
        Assert.Equal(4, config.Paths.Count);
    }
}
