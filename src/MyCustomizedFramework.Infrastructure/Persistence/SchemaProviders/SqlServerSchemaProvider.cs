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
}
