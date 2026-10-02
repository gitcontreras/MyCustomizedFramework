using System.Text.Json;
using Humanizer;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

/// <summary>Turns table metadata into the <see cref="FrontendTableModel"/> the templates render, applying the SQL -> TS -> UI-control mapping rules.</summary>
internal static class FrontendModelBuilder
{
    private const int MaxListColumns = 6;
    private const int TextareaThreshold = 255;

    public static FrontendTableModel Build(FrontendTableInput input, FrontendGenerationOptions options)
    {
        var entityName = input.TableName.Singularize();
        var pluralName = entityName.Pluralize();

        var relationsByColumn = input.Relations
            .GroupBy(relation => relation.ColumnName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => ToRelationModel(group.First(), input.Columns), StringComparer.OrdinalIgnoreCase);

        var fields = input.Columns
            .OrderBy(column => column.OrdinalPosition)
            .Select(column => ToField(column, relationsByColumn.GetValueOrDefault(column.Name)))
            .ToArray();

        var primaryKey = fields.Single(field => field.IsPrimaryKey);
        var requestFields = fields.Where(field => !field.IsPrimaryKey).ToArray();
        var formFields = requestFields.Where(field => field.Control != "none").ToArray();

        var listFields = new[] { primaryKey }
            .Concat(requestFields.Where(field => field.Control is not ("none" or "textarea" or "password")))
            .Take(MaxListColumns)
            .ToArray();

        return new FrontendTableModel
        {
            EntityName = entityName,
            PluralName = pluralName,
            CamelName = entityName.Camelize(),
            PluralCamelName = pluralName.Camelize(),
            FileStem = entityName.Kebaberize(),
            FolderName = pluralName.Kebaberize(),
            RouteSegment = pluralName.ToLowerInvariant(),
            Title = entityName.Humanize(LetterCasing.Title),
            PluralTitle = pluralName.Humanize(LetterCasing.Title),
            ApiBaseUrl = options.ApiBaseUrl,
            PrimaryKey = primaryKey,
            Fields = fields,
            RequestFields = requestFields,
            FormFields = formFields,
            ListFields = listFields,
            Relations = relationsByColumn.Values.DistinctBy(relation => relation.EntityName).ToArray()
        };
    }

    internal static string JsonKey(string columnName) => JsonNamingPolicy.CamelCase.ConvertName(columnName);

    internal static string ToTsType(string csharpType) => csharpType.TrimEnd('?') switch
    {
        "int" or "long" or "short" or "byte" or "decimal" or "double" or "float" => "number",
        "bool" => "boolean",
        _ => "string"
    };

    private static FrontendFieldModel ToField(TableColumn column, FrontendRelationModel? relation)
    {
        var tsType = ToTsType(column.CSharpType);
        var control = ResolveControl(column, tsType, relation);
        var isRequired = !column.IsNullable && tsType != "boolean";

        return new FrontendFieldModel
        {
            ColumnName = column.Name,
            Key = JsonKey(column.Name),
            Label = column.Name.Humanize(LetterCasing.Title),
            TsType = tsType,
            IsNullable = column.IsNullable,
            IsRequired = isRequired,
            IsPrimaryKey = column.IsPrimaryKey,
            Control = control,
            MaxLength = tsType == "string" ? column.MaxLength : null,
            InitialValue = tsType switch
            {
                "boolean" => "false",
                "number" => "null",
                _ => "''"
            },
            FormTsType = tsType == "number" ? "number | null" : tsType,
            Relation = relation
        };
    }

    private static string ResolveControl(TableColumn column, string tsType, FrontendRelationModel? relation)
    {
        var csharpType = column.CSharpType.TrimEnd('?');

        if (column.IsPrimaryKey)
        {
            return "text";
        }

        if (relation is not null)
        {
            return "select";
        }

        return csharpType switch
        {
            "byte[]" => "none",
            "bool" => "checkbox",
            "DateTime" or "DateTimeOffset" => "datetime",
            "TimeSpan" => "time",
            _ when tsType == "number" => "number",
            _ => ResolveTextControl(column)
        };
    }

    private static string ResolveTextControl(TableColumn column)
    {
        var name = column.Name;

        if (name.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            return "password";
        }

        if (name.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            return "email";
        }

        return column.MaxLength is null || column.MaxLength > TextareaThreshold ? "textarea" : "text";
    }

    private static FrontendRelationModel ToRelationModel(FrontendRelation relation, IReadOnlyCollection<TableColumn> ownColumns)
    {
        var entityName = relation.ReferencedTable.Singularize();
        var ownColumn = ownColumns.First(column => string.Equals(column.Name, relation.ColumnName, StringComparison.OrdinalIgnoreCase));
        var pluralName = entityName.Pluralize();

        return new FrontendRelationModel
        {
            FieldKey = JsonKey(relation.ColumnName),
            EntityName = entityName,
            CamelName = entityName.Camelize(),
            PluralName = pluralName,
            RouteSegment = pluralName.ToLowerInvariant(),
            ValueKey = JsonKey(relation.ReferencedColumn),
            LabelKey = JsonKey(relation.DisplayColumn),
            ValueTsType = ToTsType(ownColumn.CSharpType)
        };
    }
}
