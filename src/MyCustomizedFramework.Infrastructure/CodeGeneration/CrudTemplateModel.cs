namespace MyCustomizedFramework.Infrastructure.CodeGeneration;

/// <summary>
/// Everything the .sbn templates need to render one table's CRUD scaffolding. Scriban renames these
/// PascalCase properties to snake_case automatically (e.g. <see cref="TableName"/> -> table_name).
/// </summary>
internal sealed class CrudTemplateModel
{
    public required string TableName { get; init; }

    public required string PluralName { get; init; }

    public required string CamelName { get; init; }

    /// <summary>The domain enum member name (e.g. "SqlServer", "Oracle") used only to branch the
    /// insert-and-return-id logic, which is the one place the generated Repository truly differs per engine.</summary>
    public required string Engine { get; init; }

    /// <summary>All columns, in ordinal order - used by DbEntity/DomainEntity/Result.</summary>
    public required IReadOnlyList<ColumnModel> Columns { get; init; }

    /// <summary>All columns except the primary key - used by Request/Insert/Update.</summary>
    public required IReadOnlyList<ColumnModel> InsertableColumns { get; init; }

    public required ColumnModel PrimaryKey { get; init; }

    /// <summary>System.Data.DbType member name matching <see cref="PrimaryKey"/>'s type, needed only for
    /// Oracle's output-bind-parameter insert.</summary>
    public required string OraclePrimaryKeyDbType { get; init; }

    public required string InsertSql { get; init; }

    public required string SelectAllSql { get; init; }

    public required string SelectByIdSql { get; init; }

    public required string UpdateSql { get; init; }

    public required string DeleteSql { get; init; }
}

internal sealed class ColumnModel
{
    public required string Name { get; init; }

    /// <summary>camelCase version of <see cref="Name"/> (e.g. "FirstName" -> "firstName"), precomputed with
    /// Humanizer since Scriban has no built-in camelCase string function.</summary>
    public required string CamelName { get; init; }

    public required string Type { get; init; }

    public required bool IsNullable { get; init; }

    public required bool IsPrimaryKey { get; init; }
}
