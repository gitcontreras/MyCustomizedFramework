using System.Data;
using System.Data.Common;
using Dapper;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

/// <summary>
/// Template method base class: each engine only supplies a <see cref="DbConnectionStringBuilder"/> built from
/// <see cref="DatabaseConnectionDetails"/> plus its metadata query; the connection lifecycle and row mapping
/// are shared here. Building the connection string via a typed builder (instead of string concatenation)
/// avoids malformed or unsafe connection strings.
/// </summary>
internal abstract class SchemaProviderBase : ISchemaProvider
{
    public async Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
        DatabaseConnectionDetails connection,
        CancellationToken cancellationToken = default)
    {
        using var dbConnection = CreateConnection(connection);

        var command = new CommandDefinition(TablesQuery, cancellationToken: cancellationToken);
        var rows = await dbConnection.QueryAsync<TableRow>(command);

        return rows
            .Select(row => DatabaseTable.Create(row.Schema, row.Name))
            .ToArray();
    }

    protected abstract IDbConnection CreateConnection(DatabaseConnectionDetails connection);

    protected abstract string TablesQuery { get; }

    protected sealed record TableRow(string Schema, string Name);
}
