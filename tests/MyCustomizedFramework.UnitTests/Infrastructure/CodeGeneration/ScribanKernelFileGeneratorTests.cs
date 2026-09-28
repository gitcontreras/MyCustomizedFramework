using MyCustomizedFramework.Domain.SchemaExplorer;
using MyCustomizedFramework.Infrastructure.CodeGeneration;

namespace MyCustomizedFramework.UnitTests.Infrastructure.CodeGeneration;

public sealed class ScribanKernelFileGeneratorTests
{
    [Fact]
    public void GenerateProducesTheSixExpectedKernelFiles()
    {
        var generator = new ScribanKernelFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Acme.Payroll");

        Assert.Equal(6, files.Count);
        Assert.Contains(files, file => file.RelativePath == "Domain/Common/Result.cs");
        Assert.Contains(files, file => file.RelativePath == "Domain/Common/Error.cs");
        Assert.Contains(files, file => file.RelativePath == "Domain/Common/ErrorType.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/Abstractions/Persistence/IDbConnectionFactory.cs");
        Assert.Contains(files, file => file.RelativePath == "Infrastructure/Persistence/SqlServerConnectionFactory.cs");
        Assert.Contains(files, file => file.RelativePath == "Api/Common/ResultExtensions.cs");
    }

    [Fact]
    public void GenerateUsesTheGivenRootNamespaceEverywhere()
    {
        var generator = new ScribanKernelFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Acme.Payroll");
        var result = files.Single(file => file.RelativePath == "Domain/Common/Result.cs").Content;
        var resultExtensions = files.Single(file => file.RelativePath == "Api/Common/ResultExtensions.cs").Content;

        Assert.Contains("namespace Acme.Payroll.Domain.Common;", result);
        Assert.Contains("namespace Acme.Payroll.Api.Common;", resultExtensions);
        Assert.DoesNotContain("MyCustomizedFramework", result);
        Assert.DoesNotContain("MyCustomizedFramework", resultExtensions);
    }

    [Fact]
    public void GenerateErrorSuppressesCA1716SoItCompilesRegardlessOfTheTargetProjectsAnalyzerSettings()
    {
        var generator = new ScribanKernelFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Acme.Payroll");
        var error = files.Single(file => file.RelativePath == "Domain/Common/Error.cs").Content;

        Assert.Contains("#pragma warning disable CA1716", error);
        Assert.Contains("#pragma warning restore CA1716", error);
    }

    [Theory]
    [InlineData(DatabaseEngine.SqlServer, "SqlServerConnectionFactory.cs", "using Microsoft.Data.SqlClient;", "new SqlConnection(")]
    [InlineData(DatabaseEngine.PostgreSql, "PostgreSqlConnectionFactory.cs", "using Npgsql;", "new NpgsqlConnection(")]
    [InlineData(DatabaseEngine.MySql, "MySqlConnectionFactory.cs", "using MySqlConnector;", "new MySqlConnection(")]
    [InlineData(DatabaseEngine.Oracle, "OracleConnectionFactory.cs", "using Oracle.ManagedDataAccess.Client;", "new OracleConnection(")]
    public void GenerateConnectionFactoryMatchesTheChosenEngine(
        DatabaseEngine engine,
        string expectedFileName,
        string expectedUsing,
        string expectedConstructorCall)
    {
        var generator = new ScribanKernelFileGenerator();

        var files = generator.Generate(engine, "Acme.Payroll");
        var connectionFactory = files.Single(file => file.RelativePath == $"Infrastructure/Persistence/{expectedFileName}").Content;

        Assert.Contains(expectedUsing, connectionFactory);
        Assert.Contains(expectedConstructorCall, connectionFactory);
        Assert.Contains("configuration.GetConnectionString(\"Default\")", connectionFactory);
    }
}
