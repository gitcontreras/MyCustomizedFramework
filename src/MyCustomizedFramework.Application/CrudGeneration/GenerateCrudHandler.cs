using System.Data.Common;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.Common;

namespace MyCustomizedFramework.Application.CrudGeneration;

public sealed class GenerateCrudHandler(ISchemaProviderFactory schemaProviderFactory, ICrudFileGenerator crudFileGenerator)
{
    public async Task<Result<IReadOnlyCollection<GeneratedFile>>> HandleAsync(
        GenerateCrudQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Connection.Server))
        {
            return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                Error.Validation("Crud.ServerRequired", "The server is required."));
        }

        if (string.IsNullOrWhiteSpace(query.Connection.Database))
        {
            return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                Error.Validation("Crud.DatabaseRequired", "The database name is required."));
        }

        if (string.IsNullOrWhiteSpace(query.TableName))
        {
            return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                Error.Validation("Crud.TableNameRequired", "The table name is required."));
        }

        var provider = schemaProviderFactory.Resolve(query.Engine);

        try
        {
            var columns = await provider.GetColumnsAsync(query.Connection, query.TableName, cancellationToken);

            if (columns.Count == 0)
            {
                return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                    Error.NotFound("Crud.TableNotFound", $"Table '{query.TableName}' was not found or has no columns."));
            }

            var primaryKeyCount = columns.Count(column => column.IsPrimaryKey);
            if (primaryKeyCount != 1)
            {
                return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                    Error.Validation(
                        "Crud.SinglePrimaryKeyRequired",
                        $"Table '{query.TableName}' must have exactly one primary key column; composite keys are not supported yet."));
            }

            var files = crudFileGenerator.Generate(query.Engine, query.TableName, columns, query.RootNamespace);

            return Result.Success(files);
        }
        catch (DbException exception)
        {
            return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                Error.Failure("Crud.ConnectionFailed", exception.Message));
        }
    }
}
