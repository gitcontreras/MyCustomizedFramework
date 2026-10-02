using System.Data;
using Microsoft.Data.SqlClient;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class SqlServerSchemaProvider : SchemaProviderBase
{
    protected override IDbConnection CreateConnection(DatabaseConnectionDetails connection)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = connection.Port.HasValue ? $"{connection.Server},{connection.Port}" : connection.Server,
            InitialCatalog = connection.Database,
            UserID = connection.User,
            Password = connection.Password,
            TrustServerCertificate = true
        };

        return new SqlConnection(builder.ConnectionString);
    }

    protected override string TablesQuery =>
        """
        SELECT TABLE_SCHEMA AS [Schema], TABLE_NAME AS [Name]
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_SCHEMA, TABLE_NAME
        """;

    protected override string ColumnsQuery =>
        """
        SELECT COLUMN_NAME AS Name,
               DATA_TYPE AS DataType,
               CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS IsNullable,
               ORDINAL_POSITION AS OrdinalPosition,
               CHARACTER_MAXIMUM_LENGTH AS MaxLength
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = @TableName
          AND (@Schema IS NULL OR TABLE_SCHEMA = @Schema)
        ORDER BY ORDINAL_POSITION
        """;

    protected override string PrimaryKeyColumnsQuery =>
        """
        SELECT ku.COLUMN_NAME
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
            ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
            AND tc.TABLE_NAME = ku.TABLE_NAME
            AND tc.TABLE_SCHEMA = ku.TABLE_SCHEMA
        WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
          AND tc.TABLE_NAME = @TableName
          AND (@Schema IS NULL OR tc.TABLE_SCHEMA = @Schema)
        """;

    protected override string ForeignKeysQuery =>
        """
        SELECT cp.name AS ColumnName,
               SCHEMA_NAME(rt.schema_id) AS ReferencedSchema,
               rt.name AS ReferencedTable,
               cr.name AS ReferencedColumn
        FROM sys.foreign_key_columns fkc
        JOIN sys.tables pt ON pt.object_id = fkc.parent_object_id
        JOIN sys.columns cp ON cp.object_id = fkc.parent_object_id AND cp.column_id = fkc.parent_column_id
        JOIN sys.tables rt ON rt.object_id = fkc.referenced_object_id
        JOIN sys.columns cr ON cr.object_id = fkc.referenced_object_id AND cr.column_id = fkc.referenced_column_id
        WHERE pt.name = @TableName
          AND (@Schema IS NULL OR SCHEMA_NAME(pt.schema_id) = @Schema)
          AND (SELECT COUNT(*) FROM sys.foreign_key_columns x WHERE x.constraint_object_id = fkc.constraint_object_id) = 1
        """;

    protected override string MapToCSharpType(string dataType, bool isNullable) => AsNullable(
        dataType.ToLowerInvariant() switch
        {
            "int" => "int",
            "bigint" => "long",
            "smallint" => "short",
            "tinyint" => "byte",
            "bit" => "bool",
            "decimal" or "numeric" or "money" or "smallmoney" => "decimal",
            "float" => "double",
            "real" => "float",
            "date" or "datetime" or "datetime2" or "smalldatetime" => "DateTime",
            "datetimeoffset" => "DateTimeOffset",
            "time" => "TimeSpan",
            "uniqueidentifier" => "Guid",
            "binary" or "varbinary" or "image" or "rowversion" => "byte[]",
            _ => "string"
        },
        isNullable);
}
