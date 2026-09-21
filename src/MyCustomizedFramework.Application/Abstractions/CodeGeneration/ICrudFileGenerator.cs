using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.Abstractions.CodeGeneration;

/// <summary>
/// Renders the CRUD scaffolding (entity, DTOs, repository, service) for a table from its column metadata.
/// </summary>
public interface ICrudFileGenerator
{
    IReadOnlyCollection<GeneratedFile> Generate(
        DatabaseEngine engine,
        string tableName,
        IReadOnlyCollection<TableColumn> columns);
}
