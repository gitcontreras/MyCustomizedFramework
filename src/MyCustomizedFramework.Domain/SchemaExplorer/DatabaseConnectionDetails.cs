namespace MyCustomizedFramework.Domain.SchemaExplorer;

/// <summary>
/// Discrete connection parameters supplied by the caller. Each <see cref="Application.Abstractions.Persistence.ISchemaProvider"/>
/// implementation is responsible for turning these into a provider-specific connection string using a typed,
/// safe <c>DbConnectionStringBuilder</c> instead of the caller assembling a raw connection string.
/// </summary>
public sealed record DatabaseConnectionDetails(
    string Server,
    int? Port,
    string Database,
    string User,
    string Password);
