using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Api.Contracts.Tables;

public static class TablesMapper
{
    public static Result<GetTablesQuery> ToQuery(GetTablesRequest request)
    {
        var engineResult = ParseEngine(request.Engine);

        if (engineResult.IsFailure)
        {
            return Result.Failure<GetTablesQuery>(engineResult.Error);
        }

        var connection = new DatabaseConnectionDetails(
            request.Server,
            request.Port,
            request.Database,
            request.User,
            request.Password);

        return Result.Success(new GetTablesQuery(connection, engineResult.Value));
    }

    public static TableResponse ToResponse(TableDto dto) => new(dto.Schema, dto.Name);

    private static Result<DatabaseEngine> ParseEngine(string engine)
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
