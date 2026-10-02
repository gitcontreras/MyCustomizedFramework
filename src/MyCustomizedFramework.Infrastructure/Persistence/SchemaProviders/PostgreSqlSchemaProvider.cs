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
               ordinal_position AS "OrdinalPosition",
               character_maximum_length AS "MaxLength"
        FROM information_schema.columns
        WHERE table_name = @TableName
          AND (@Schema::text IS NULL OR table_schema = @Schema)
        ORDER BY ordinal_position
        """;

    protected override string PrimaryKeyColumnsQuery =>
        """
        SELECT ku.column_name AS "ColumnName"
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage ku
            ON tc.constraint_name = ku.constraint_name
            AND tc.table_name = ku.table_name
            AND tc.table_schema = ku.table_schema
        WHERE tc.constraint_type = 'PRIMARY KEY'
          AND tc.table_name = @TableName
          AND (@Schema::text IS NULL OR tc.table_schema = @Schema)
        """;

    protected override string ForeignKeysQuery =>
        """
        SELECT a.attname AS "ColumnName",
               rn.nspname AS "ReferencedSchema",
               rc.relname AS "ReferencedTable",
               ra.attname AS "ReferencedColumn"
        FROM pg_constraint c
        JOIN pg_class pc ON pc.oid = c.conrelid
        JOIN pg_namespace pn ON pn.oid = pc.relnamespace
        JOIN pg_class rc ON rc.oid = c.confrelid
        JOIN pg_namespace rn ON rn.oid = rc.relnamespace
        JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = c.conkey[1]
        JOIN pg_attribute ra ON ra.attrelid = c.confrelid AND ra.attnum = c.confkey[1]
        WHERE c.contype = 'f'
          AND array_length(c.conkey, 1) = 1
          AND pc.relname = @TableName
          AND (@Schema::text IS NULL OR pn.nspname = @Schema)
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
