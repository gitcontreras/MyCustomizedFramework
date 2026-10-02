namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

/// <summary>
/// Framework-agnostic view of one table for the frontend templates. Scriban renames PascalCase members to
/// snake_case (e.g. <see cref="EntityName"/> -> entity_name).
/// </summary>
internal sealed class FrontendTableModel
{
    /// <summary>Singular PascalCase type name, e.g. "Order".</summary>
    public required string EntityName { get; init; }

    /// <summary>Plural PascalCase, e.g. "Orders".</summary>
    public required string PluralName { get; init; }

    public required string CamelName { get; init; }

    public required string PluralCamelName { get; init; }

    /// <summary>Kebab-case singular used in file names, e.g. "order-item".</summary>
    public required string FileStem { get; init; }

    /// <summary>Feature folder and file stem, e.g. "orders" or "order-items".</summary>
    public required string FolderName { get; init; }

    /// <summary>Matches the generated backend controller route: api/{RouteSegment}.</summary>
    public required string RouteSegment { get; init; }

    public required string Title { get; init; }

    public required string PluralTitle { get; init; }

    public required string ApiBaseUrl { get; init; }

    public required FrontendFieldModel PrimaryKey { get; init; }

    /// <summary>Every column in the API result (includes the key).</summary>
    public required IReadOnlyList<FrontendFieldModel> Fields { get; init; }

    /// <summary>Columns accepted by create/update (everything except the key).</summary>
    public required IReadOnlyList<FrontendFieldModel> RequestFields { get; init; }

    /// <summary>Request fields rendered as form controls (excludes binary columns).</summary>
    public required IReadOnlyList<FrontendFieldModel> FormFields { get; init; }

    /// <summary>Columns shown in the list table (key first, long text and binary excluded, capped).</summary>
    public required IReadOnlyList<FrontendFieldModel> ListFields { get; init; }

    public required IReadOnlyList<FrontendRelationModel> Relations { get; init; }

    public bool HasRelations => Relations.Count > 0;
}

internal sealed class FrontendFieldModel
{
    /// <summary>Raw column name, used for display only.</summary>
    public required string ColumnName { get; init; }

    /// <summary>JSON/TypeScript property name, identical to what System.Text.Json emits for the C# property.</summary>
    public required string Key { get; init; }

    public required string Label { get; init; }

    /// <summary>"string" | "number" | "boolean".</summary>
    public required string TsType { get; init; }

    public required bool IsNullable { get; init; }

    public required bool IsRequired { get; init; }

    public required bool IsPrimaryKey { get; init; }

    /// <summary>"text" | "textarea" | "email" | "password" | "number" | "checkbox" | "datetime" | "time" | "select".</summary>
    public required string Control { get; init; }

    public int? MaxLength { get; init; }

    /// <summary>TypeScript literal used as the form's initial value for this field.</summary>
    public required string InitialValue { get; init; }

    /// <summary>TypeScript type of the field while it is being edited (numbers/selects can be empty -> null).</summary>
    public required string FormTsType { get; init; }

    public FrontendRelationModel? Relation { get; init; }

    public bool IsNumber => TsType == "number";

    public bool IsString => TsType == "string";

    public bool IsBoolean => TsType == "boolean";
}

internal sealed class FrontendRelationModel
{
    /// <summary>Key of the foreign-key field on the owning entity (e.g. "customerId").</summary>
    public required string FieldKey { get; init; }

    /// <summary>Singular PascalCase of the referenced table, e.g. "Customer".</summary>
    public required string EntityName { get; init; }

    public required string CamelName { get; init; }

    public required string PluralName { get; init; }

    public required string RouteSegment { get; init; }

    /// <summary>JSON key of the referenced primary key (e.g. "id").</summary>
    public required string ValueKey { get; init; }

    /// <summary>JSON key shown as the option label (e.g. "name").</summary>
    public required string LabelKey { get; init; }

    public required string ValueTsType { get; init; }
}
