using MyCustomizedFramework.Application.CrudGeneration;
using MyCustomizedFramework.Application.SchemaExplorer;
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

        var rootNamespace = string.IsNullOrWhiteSpace(request.RootNamespace)
            ? "MyCustomizedFramework"
            : request.RootNamespace.Trim();

        return Result.Success(new GenerateCrudQuery(connection, engineResult.Value, request.TableName, rootNamespace));
    }
}
