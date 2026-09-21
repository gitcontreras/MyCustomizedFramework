namespace MyCustomizedFramework.Api.Contracts.Tables;

/// <summary>
/// <paramref name="Engine"/> is a free-form string ("sql", "postgres", "mysql", "oracle", ...)
/// so the caller does not need to know the exact domain enum member names.
/// </summary>
public sealed record GetTablesRequest(string ConnectionString, string Engine);
