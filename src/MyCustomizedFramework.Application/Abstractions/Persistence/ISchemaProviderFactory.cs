using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.Abstractions.Persistence;

/// <summary>
/// Resolves the <see cref="ISchemaProvider"/> implementation for a given <see cref="DatabaseEngine"/>.
/// </summary>
public interface ISchemaProviderFactory
{
    ISchemaProvider Resolve(DatabaseEngine engine);
}
