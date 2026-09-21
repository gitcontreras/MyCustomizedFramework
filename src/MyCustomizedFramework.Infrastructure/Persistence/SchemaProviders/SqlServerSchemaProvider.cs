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
               ORDINAL_POSITION AS OrdinalPosition
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = @TableName
        ORDER BY ORDINAL_POSITION
        """;

    protected override string PrimaryKeyColumnsQuery =>
        """
        SELECT ku.COLUMN_NAME
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
            ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME AND tc.TABLE_NAME = ku.TABLE_NAME
        WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND tc.TABLE_NAME = @TableName
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
