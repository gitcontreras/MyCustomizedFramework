using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Api.Contracts.Shared;

/// <summary>
/// Shared between every endpoint that accepts a free-form engine string ("sql", "postgres", ...) so the
/// alias list lives in exactly one place.
/// </summary>
public static class DatabaseEngineParser
{
    public static Result<DatabaseEngine> Parse(string engine)
    {
        var normalized = engine?.Trim().ToLowerInvariant();

        DatabaseEngine? resolved = normalized switch
        {
            "sql" or "sqlserver" or "mssql" => DatabaseEngine.SqlServer,
            "postgres" or "postgress" or "postgresql" or "pg" => DatabaseEngine.PostgreSql,
            "mysql" => DatabaseEngine.MySql,
            "oracle" => DatabaseEngine.Oracle,
            _ => null
        };

        return resolved is null
            ? Result.Failure<DatabaseEngine>(
                Error.Validation("Tables.UnknownEngine", $"Unknown database engine '{engine}'."))
            : Result.Success(resolved.Value);
    }
}
