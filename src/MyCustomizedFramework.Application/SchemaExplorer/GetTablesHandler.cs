using System.Data.Common;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.Common;

namespace MyCustomizedFramework.Application.SchemaExplorer;

public sealed class GetTablesHandler(ISchemaProviderFactory schemaProviderFactory)
{
    public async Task<Result<IReadOnlyCollection<TableDto>>> HandleAsync(
        GetTablesQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Connection.Server))
        {
            return Result.Failure<IReadOnlyCollection<TableDto>>(
                Error.Validation("Tables.ServerRequired", "The server is required."));
        }

        if (string.IsNullOrWhiteSpace(query.Connection.Database))
        {
            return Result.Failure<IReadOnlyCollection<TableDto>>(
                Error.Validation("Tables.DatabaseRequired", "The database name is required."));
        }

        var provider = schemaProviderFactory.Resolve(query.Engine);

        try
        {
            var tables = await provider.GetTablesAsync(query.Connection, cancellationToken);

            IReadOnlyCollection<TableDto> dtos = tables
                .Select(table => new TableDto(table.Schema, table.Name))
                .ToArray();

            return Result.Success(dtos);
        }
        catch (DbException exception)
        {
            return Result.Failure<IReadOnlyCollection<TableDto>>(
                Error.Failure("Tables.ConnectionFailed", exception.Message));
        }
    }
}
