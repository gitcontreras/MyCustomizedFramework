using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.Abstractions.CodeGeneration;

/// <summary>
/// Scaffolds the shared "kernel" every generated CRUD file assumes already exists in the target
/// solution: the Result pattern (Domain.Common), IDbConnectionFactory (Application.Abstractions.Persistence),
/// a concrete connection factory for the chosen engine, and ResultExtensions (Api.Common). Unlike
/// <see cref="ICrudFileGenerator"/>, this is generated once per solution, not once per table.
/// </summary>
public interface IKernelFileGenerator
{
    IReadOnlyCollection<GeneratedFile> Generate(DatabaseEngine engine, string rootNamespace);
}
