using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.CrudGeneration;

/// <summary>
/// <paramref name="Schema"/> is optional - when omitted, tables are resolved via the connection's own
/// default schema resolution, matching this feature's original (schema-less) behavior.
/// </summary>
public sealed record GenerateCrudQuery(
    DatabaseConnectionDetails Connection,
    DatabaseEngine Engine,
    string? Schema,
    string TableName,
    string RootNamespace);
