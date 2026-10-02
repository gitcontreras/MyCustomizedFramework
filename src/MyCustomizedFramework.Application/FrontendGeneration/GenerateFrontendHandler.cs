using System.Data.Common;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.FrontendGeneration;

public sealed class GenerateFrontendHandler(ISchemaProviderFactory schemaProviderFactory, IFrontendFileGenerator frontendFileGenerator)
{
    private static readonly string[] PreferredDisplayNames = ["name", "title", "description", "label", "code"];

    public async Task<Result<IReadOnlyCollection<GeneratedFile>>> HandleAsync(
        GenerateFrontendQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Connection.Server))
        {
            return Failure(Error.Validation("Frontend.ServerRequired", "The server is required."));
        }

        if (string.IsNullOrWhiteSpace(query.Connection.Database))
        {
            return Failure(Error.Validation("Frontend.DatabaseRequired", "The database name is required."));
        }

        if (string.IsNullOrWhiteSpace(query.TableName))
        {
            return Failure(Error.Validation("Frontend.TableNameRequired", "The table name is required."));
        }

        var unsupported = frontendFileGenerator.GetUnsupportedReason(query.Options);
        if (unsupported is not null)
        {
            return Failure(Error.Validation("Frontend.UnsupportedStack", unsupported));
        }

        var provider = schemaProviderFactory.Resolve(query.Engine);

        try
        {
            var (schema, schemaError) = await ResolveSchemaAsync(provider, query, cancellationToken);
            if (schemaError is not null)
            {
                return Failure(schemaError);
            }

            var columns = await provider.GetColumnsAsync(query.Connection, schema, query.TableName, cancellationToken);
            if (columns.Count == 0)
            {
                return Failure(Error.NotFound("Frontend.TableNotFound", $"Table '{query.TableName}' was not found or has no columns."));
            }

            if (columns.Count(column => column.IsPrimaryKey) != 1)
            {
                return Failure(Error.Validation(
                    "Frontend.SinglePrimaryKeyRequired",
                    $"Table '{query.TableName}' must have exactly one primary key column; composite keys are not supported yet."));
            }

            var foreignKeys = await provider.GetForeignKeysAsync(query.Connection, schema, query.TableName, cancellationToken);
            var relations = await BuildRelationsAsync(provider, query, foreignKeys, cancellationToken);

            var input = new FrontendTableInput(schema, query.TableName, columns, relations);

            return Result.Success(frontendFileGenerator.Generate(input, query.Options));
        }
        catch (DbException exception)
        {
            return Failure(Error.Failure("Frontend.ConnectionFailed", exception.Message));
        }
    }

    private static async Task<(string? Schema, Error? Error)> ResolveSchemaAsync(
        ISchemaProvider provider,
        GenerateFrontendQuery query,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(query.Schema))
        {
            return (query.Schema, null);
        }

        var tables = await provider.GetTablesAsync(query.Connection, cancellationToken);
        var matches = tables
            .Where(table => string.Equals(table.Name, query.TableName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matches.Length > 1)
        {
            var schemas = string.Join(", ", matches.Select(table => table.Schema));
            return (null, Error.Validation(
                "Frontend.AmbiguousSchema",
                $"Table '{query.TableName}' exists in more than one schema ({schemas}); specify which one with 'schema.{query.TableName}'."));
        }

        return (matches.Length == 1 ? matches[0].Schema : null, null);
    }

    private static async Task<IReadOnlyCollection<FrontendRelation>> BuildRelationsAsync(
        ISchemaProvider provider,
        GenerateFrontendQuery query,
        IReadOnlyCollection<TableForeignKey> foreignKeys,
        CancellationToken cancellationToken)
    {
        var relations = new List<FrontendRelation>();

        foreach (var foreignKey in foreignKeys)
        {
            var referencedColumns = await provider.GetColumnsAsync(
                query.Connection, foreignKey.ReferencedSchema, foreignKey.ReferencedTable, cancellationToken);

            relations.Add(new FrontendRelation(
                foreignKey.ColumnName,
                foreignKey.ReferencedTable,
                foreignKey.ReferencedColumn,
                PickDisplayColumn(referencedColumns, foreignKey.ReferencedColumn)));
        }

        return relations;
    }

    /// <summary>The human-readable column shown in a dropdown: a well-known name, else the first text column, else the key itself.</summary>
    internal static string PickDisplayColumn(IReadOnlyCollection<TableColumn> columns, string fallbackKeyColumn)
    {
        var textColumns = columns
            .Where(column => !column.IsPrimaryKey && column.CSharpType.TrimEnd('?') == "string")
            .ToArray();

        var preferred = textColumns.FirstOrDefault(column =>
            PreferredDisplayNames.Contains(column.Name, StringComparer.OrdinalIgnoreCase));

        return (preferred ?? textColumns.FirstOrDefault())?.Name ?? fallbackKeyColumn;
    }

    private static Result<IReadOnlyCollection<GeneratedFile>> Failure(Error error) =>
        Result.Failure<IReadOnlyCollection<GeneratedFile>>(error);
}
