using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.Abstractions.Persistence;

/// <summary>
/// Reads table metadata from a specific database engine using a caller-supplied connection string.
/// </summary>
public interface ISchemaProvider
{
    Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
        string connectionString,
        CancellationToken cancellationToken = default);
}
