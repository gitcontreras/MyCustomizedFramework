using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.Abstractions.Persistence;

/// <summary>
/// Reads table metadata from a specific database engine using caller-supplied connection details.
/// Implementations own turning <see cref="DatabaseConnectionDetails"/> into a provider-specific connection string.
/// </summary>
public interface ISchemaProvider
{
    Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
        DatabaseConnectionDetails connection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
        DatabaseConnectionDetails connection,
        string tableName,
        CancellationToken cancellationToken = default);
}
