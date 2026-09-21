using MyCustomizedFramework.Api.Contracts.Shared;
using MyCustomizedFramework.Application.CrudGeneration;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Api.Contracts.Crud;

public static class CrudMapper
{
    public static Result<GenerateCrudQuery> ToQuery(GenerateCrudRequest request)
    {
        var engineResult = DatabaseEngineParser.Parse(request.Engine);

        if (engineResult.IsFailure)
        {
            return Result.Failure<GenerateCrudQuery>(engineResult.Error);
        }

        var connection = new DatabaseConnectionDetails(
            request.Server,
            request.Port,
            request.Database,
            request.User,
            request.Password);

        return Result.Success(new GenerateCrudQuery(connection, engineResult.Value, request.TableName));
    }
}
