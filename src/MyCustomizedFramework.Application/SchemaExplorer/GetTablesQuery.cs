using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.SchemaExplorer;

public sealed record GetTablesQuery(string ConnectionString, DatabaseEngine Engine);
