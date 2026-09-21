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

    protected override string ColumnsQuery =>
        """
        SELECT column_name AS "Name",
               data_type AS "DataType",
               CASE WHEN is_nullable = 'YES' THEN 1 ELSE 0 END AS "IsNullable",
               ordinal_position AS "OrdinalPosition"
        FROM information_schema.columns
        WHERE table_name = @TableName
        ORDER BY ordinal_position
        """;

    protected override string PrimaryKeyColumnsQuery =>
        """
        SELECT ku.column_name AS "ColumnName"
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage ku
            ON tc.constraint_name = ku.constraint_name AND tc.table_name = ku.table_name
        WHERE tc.constraint_type = 'PRIMARY KEY' AND tc.table_name = @TableName
        """;

    protected override string MapToCSharpType(string dataType, bool isNullable) => AsNullable(
        dataType.ToLowerInvariant() switch
        {
            "integer" or "int4" or "serial" => "int",
            "bigint" or "int8" or "bigserial" => "long",
            "smallint" or "int2" or "smallserial" => "short",
            "boolean" => "bool",
            "numeric" or "decimal" or "money" => "decimal",
            "double precision" or "float8" => "double",
            "real" or "float4" => "float",
            "date" => "DateTime",
            "timestamp without time zone" or "timestamp" => "DateTime",
            "timestamp with time zone" or "timestamptz" => "DateTimeOffset",
            "time" or "time without time zone" => "TimeSpan",
            "uuid" => "Guid",
            "bytea" => "byte[]",
            _ => "string"
        },
        isNullable);
}
