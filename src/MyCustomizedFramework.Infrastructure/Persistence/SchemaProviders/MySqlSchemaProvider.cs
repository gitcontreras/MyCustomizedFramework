using System.Data;
using MySqlConnector;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class MySqlSchemaProvider : SchemaProviderBase
{
    protected override IDbConnection CreateConnection(string connectionString) => new MySqlConnection(connectionString);

    protected override string TablesQuery =>
        """
        SELECT table_schema AS `Schema`, table_name AS `Name`
        FROM information_schema.tables
        WHERE table_type = 'BASE TABLE'
          AND table_schema = DATABASE()
        ORDER BY table_name
        """;
}
