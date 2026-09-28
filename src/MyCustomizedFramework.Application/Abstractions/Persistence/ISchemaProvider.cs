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

    /// <summary>
    /// <paramref name="schema"/> is optional - when null, no schema filter is applied and the
    /// connection's own default schema resolution decides which table is read.
    /// </summary>
    Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
        DatabaseConnectionDetails connection,
        string? schema,
        string tableName,
        CancellationToken cancellationToken = default);
}
