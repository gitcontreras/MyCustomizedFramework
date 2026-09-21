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

    protected override string ColumnsQuery =>
        """
        SELECT column_name AS Name,
               data_type AS DataType,
               CASE WHEN is_nullable = 'YES' THEN 1 ELSE 0 END AS IsNullable,
               ordinal_position AS OrdinalPosition
        FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = @TableName
        ORDER BY ordinal_position
        """;

    protected override string PrimaryKeyColumnsQuery =>
        """
        SELECT column_name
        FROM information_schema.key_column_usage
        WHERE table_schema = DATABASE() AND table_name = @TableName AND constraint_name = 'PRIMARY'
        """;

    protected override string MapToCSharpType(string dataType, bool isNullable) => AsNullable(
        dataType.ToLowerInvariant() switch
        {
            "int" or "mediumint" => "int",
            "bigint" => "long",
            "smallint" => "short",
            "tinyint" => "byte",
            "bool" or "boolean" => "bool",
            "decimal" or "numeric" => "decimal",
            "double" => "double",
            "float" => "float",
            "date" or "datetime" or "timestamp" => "DateTime",
            "time" => "TimeSpan",
            "binary" or "varbinary" or "blob" or "tinyblob" or "mediumblob" or "longblob" => "byte[]",
            _ => "string"
        },
        isNullable);
}
