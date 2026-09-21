using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.CrudGeneration;

public sealed record GenerateCrudQuery(DatabaseConnectionDetails Connection, DatabaseEngine Engine, string TableName);
