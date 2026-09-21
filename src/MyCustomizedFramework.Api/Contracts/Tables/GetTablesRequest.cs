namespace MyCustomizedFramework.Api.Contracts.Tables;

/// <summary>
/// Discrete connection fields instead of a raw connection string: the Api never accepts a pre-built
/// connection string from the caller. <paramref name="Engine"/> is a free-form string
/// ("sql", "postgres", "mysql", "oracle", ...) so the caller does not need to know the exact domain enum
/// member names.
/// </summary>
public sealed record GetTablesRequest(
    string Engine,
    string Server,
    int? Port,
    string Database,
    string User,
    string Password);
