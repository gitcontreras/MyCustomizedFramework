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

    protected override string ColumnsQuery =>
        """
        SELECT COLUMN_NAME AS "Name",
               DATA_TYPE AS "DataType",
               CASE WHEN NULLABLE = 'Y' THEN 1 ELSE 0 END AS "IsNullable",
               COLUMN_ID AS "OrdinalPosition"
        FROM USER_TAB_COLUMNS
        WHERE TABLE_NAME = :TableName
        ORDER BY COLUMN_ID
        """;

    protected override string PrimaryKeyColumnsQuery =>
        """
        SELECT cols.COLUMN_NAME
        FROM USER_CONSTRAINTS cons
        JOIN USER_CONS_COLUMNS cols ON cons.CONSTRAINT_NAME = cols.CONSTRAINT_NAME
        WHERE cons.CONSTRAINT_TYPE = 'P' AND cons.TABLE_NAME = :TableName
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
