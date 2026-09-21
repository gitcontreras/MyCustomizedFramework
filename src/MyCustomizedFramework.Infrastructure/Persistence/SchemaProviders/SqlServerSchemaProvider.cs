using System.Data;
using Microsoft.Data.SqlClient;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class SqlServerSchemaProvider : SchemaProviderBase
{
    protected override IDbConnection CreateConnection(string connectionString) => new SqlConnection(connectionString);

    protected override string TablesQuery =>
        """
        SELECT TABLE_SCHEMA AS [Schema], TABLE_NAME AS [Name]
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_SCHEMA, TABLE_NAME
        """;
}
