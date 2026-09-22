using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Api.Contracts.Tables;

public static class TablesMapper
{
    public static Result<GetTablesQuery> ToQuery(GetTablesRequest request)
    {
        var engineResult = DatabaseEngineParser.Parse(request.Engine);

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
}
