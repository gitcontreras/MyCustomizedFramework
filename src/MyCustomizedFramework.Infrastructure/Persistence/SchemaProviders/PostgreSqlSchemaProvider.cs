using System.Data;
using Npgsql;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class PostgreSqlSchemaProvider : SchemaProviderBase
{
    private const int DefaultPort = 5432;

    protected override IDbConnection CreateConnection(DatabaseConnectionDetails connection)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connection.Server,
            Port = connection.Port ?? DefaultPort,
            Database = connection.Database,
            Username = connection.User,
            Password = connection.Password
        };

        return new NpgsqlConnection(builder.ConnectionString);
    }

    protected override string TablesQuery =>
        """
        SELECT table_schema AS "Schema", table_name AS "Name"
        FROM information_schema.tables
        WHERE table_type = 'BASE TABLE'
          AND table_schema NOT IN ('pg_catalog', 'information_schema')
        ORDER BY table_schema, table_name
        """;
}
