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

    // ALL_* (not USER_*) so an explicit :Schema can target a table owned by a different user; when no
    // schema is given, NVL falls back to the connecting user's own schema - the previous USER_* behavior.
    protected override string ColumnsQuery =>
        """
        SELECT COLUMN_NAME AS "Name",
               DATA_TYPE AS "DataType",
               CASE WHEN NULLABLE = 'Y' THEN 1 ELSE 0 END AS "IsNullable",
               COLUMN_ID AS "OrdinalPosition",
               CHAR_LENGTH AS "MaxLength"
        FROM ALL_TAB_COLUMNS
        WHERE TABLE_NAME = :TableName
          AND OWNER = NVL(:Schema, USER)
        ORDER BY COLUMN_ID
        """;

    protected override string PrimaryKeyColumnsQuery =>
        """
        SELECT cols.COLUMN_NAME
        FROM ALL_CONSTRAINTS cons
        JOIN ALL_CONS_COLUMNS cols
            ON cons.CONSTRAINT_NAME = cols.CONSTRAINT_NAME
            AND cons.OWNER = cols.OWNER
        WHERE cons.CONSTRAINT_TYPE = 'P'
          AND cons.TABLE_NAME = :TableName
          AND cons.OWNER = NVL(:Schema, USER)
        """;

    protected override string ForeignKeysQuery =>
        """
        SELECT cols.COLUMN_NAME AS "ColumnName",
               rcols.OWNER AS "ReferencedSchema",
               rcols.TABLE_NAME AS "ReferencedTable",
               rcols.COLUMN_NAME AS "ReferencedColumn"
        FROM ALL_CONSTRAINTS c
        JOIN ALL_CONS_COLUMNS cols
            ON cols.OWNER = c.OWNER AND cols.CONSTRAINT_NAME = c.CONSTRAINT_NAME
        JOIN ALL_CONS_COLUMNS rcols
            ON rcols.OWNER = c.R_OWNER AND rcols.CONSTRAINT_NAME = c.R_CONSTRAINT_NAME AND rcols.POSITION = cols.POSITION
        WHERE c.CONSTRAINT_TYPE = 'R'
          AND c.TABLE_NAME = :TableName
          AND c.OWNER = NVL(:Schema, USER)
          AND (SELECT COUNT(*) FROM ALL_CONS_COLUMNS x
               WHERE x.OWNER = c.OWNER AND x.CONSTRAINT_NAME = c.CONSTRAINT_NAME) = 1
        """;

    protected override string MapToCSharpType(string dataType, bool isNullable) => AsNullable(
        dataType.ToUpperInvariant() switch
        {
            "NUMBER" => "decimal",
            "FLOAT" or "BINARY_FLOAT" => "float",
            "BINARY_DOUBLE" => "double",
            "DATE" or "TIMESTAMP" => "DateTime",
            "RAW" or "LONG RAW" or "BLOB" => "byte[]",
            _ => "string"
        },
        isNullable);
}
