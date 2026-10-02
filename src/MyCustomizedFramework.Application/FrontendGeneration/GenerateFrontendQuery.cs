using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.FrontendGeneration;

public sealed record GenerateFrontendQuery(
    DatabaseConnectionDetails Connection,
    DatabaseEngine Engine,
    string? Schema,
    string TableName,
    FrontendGenerationOptions Options);
