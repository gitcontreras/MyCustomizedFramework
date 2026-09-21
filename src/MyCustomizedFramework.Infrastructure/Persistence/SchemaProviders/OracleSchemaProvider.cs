using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class OracleSchemaProvider : SchemaProviderBase
{
    protected override IDbConnection CreateConnection(string connectionString) => new OracleConnection(connectionString);

    protected override string TablesQuery =>
        """
        SELECT USER AS "Schema", TABLE_NAME AS "Name"
        FROM USER_TABLES
        ORDER BY TABLE_NAME
        """;
}
