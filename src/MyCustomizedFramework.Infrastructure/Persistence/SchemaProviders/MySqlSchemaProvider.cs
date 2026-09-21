using System.Data;
using MySqlConnector;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class MySqlSchemaProvider : SchemaProviderBase
{
    private const uint DefaultPort = 3306;

    protected override IDbConnection CreateConnection(DatabaseConnectionDetails connection)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = connection.Server,
            Port = connection.Port.HasValue ? (uint)connection.Port.Value : DefaultPort,
            Database = connection.Database,
            UserID = connection.User,
            Password = connection.Password
        };

        return new MySqlConnection(builder.ConnectionString);
    }

    protected override string TablesQuery =>
        """
        SELECT table_schema AS `Schema`, table_name AS `Name`
        FROM information_schema.tables
        WHERE table_type = 'BASE TABLE'
          AND table_schema = DATABASE()
        ORDER BY table_name
        """;
}
