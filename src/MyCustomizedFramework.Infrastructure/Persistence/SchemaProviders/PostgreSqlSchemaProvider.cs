using System.Data;
using Npgsql;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class PostgreSqlSchemaProvider : SchemaProviderBase
{
    protected override IDbConnection CreateConnection(string connectionString) => new NpgsqlConnection(connectionString);

    protected override string TablesQuery =>
        """
        SELECT table_schema AS "Schema", table_name AS "Name"
        FROM information_schema.tables
        WHERE table_type = 'BASE TABLE'
          AND table_schema NOT IN ('pg_catalog', 'information_schema')
        ORDER BY table_schema, table_name
        """;
}
