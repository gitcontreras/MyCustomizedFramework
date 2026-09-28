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
            var schema = query.Schema;

            if (string.IsNullOrWhiteSpace(schema))
            {
                var tables = await provider.GetTablesAsync(query.Connection, cancellationToken);
                var matches = tables
                    .Where(table => string.Equals(table.Name, query.TableName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (matches.Length > 1)
                {
                    var schemas = string.Join(", ", matches.Select(table => table.Schema));
                    return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                        Error.Validation(
                            "Crud.AmbiguousSchema",
                            $"Table '{query.TableName}' exists in more than one schema ({schemas}); "
                            + $"specify which one with 'schema.{query.TableName}'."));
                }

                if (matches.Length == 1)
                {
                    schema = matches[0].Schema;
                }
            }

            var columns = await provider.GetColumnsAsync(query.Connection, schema, query.TableName, cancellationToken);

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

            var files = crudFileGenerator.Generate(query.Engine, schema, query.TableName, columns, query.RootNamespace);

            return Result.Success(files);
        }
        catch (DbException exception)
        {
            return Result.Failure<IReadOnlyCollection<GeneratedFile>>(
                Error.Failure("Crud.ConnectionFailed", exception.Message));
        }
    }
}
