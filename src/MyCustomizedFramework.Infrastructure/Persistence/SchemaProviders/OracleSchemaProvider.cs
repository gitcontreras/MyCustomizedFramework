using System.Data;
using Oracle.ManagedDataAccess.Client;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class OracleSchemaProvider : SchemaProviderBase
{
    private const int DefaultPort = 1521;

    protected override IDbConnection CreateConnection(DatabaseConnectionDetails connection)
    {
        var builder = new OracleConnectionStringBuilder
        {
            DataSource = $"{connection.Server}:{connection.Port ?? DefaultPort}/{connection.Database}",
            UserID = connection.User,
            Password = connection.Password
        };

        return new OracleConnection(builder.ConnectionString);
    }

    protected override string TablesQuery =>
        """
        SELECT USER AS "Schema", TABLE_NAME AS "Name"
        FROM USER_TABLES
        ORDER BY TABLE_NAME
        """;
}
