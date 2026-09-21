using System.Data;
using Dapper;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

/// <summary>
/// Template method base class: each engine only supplies its <see cref="IDbConnection"/> implementation
/// and its metadata query; the connection lifecycle and row mapping are shared here.
/// </summary>
internal abstract class SchemaProviderBase : ISchemaProvider
{
    public async Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection(connectionString);

        var command = new CommandDefinition(TablesQuery, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<TableRow>(command);

        return rows
            .Select(row => DatabaseTable.Create(row.Schema, row.Name))
            .ToArray();
    }

    protected abstract IDbConnection CreateConnection(string connectionString);

    protected abstract string TablesQuery { get; }

    protected sealed record TableRow(string Schema, string Name);
}
