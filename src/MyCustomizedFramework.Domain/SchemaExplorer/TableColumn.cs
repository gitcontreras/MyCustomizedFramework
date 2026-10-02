namespace MyCustomizedFramework.Domain.SchemaExplorer;

public sealed class TableColumn
{
    private TableColumn(
        string name, string csharpType, bool isNullable, bool isPrimaryKey, int ordinalPosition, int? maxLength)
    {
        Name = name;
        CSharpType = csharpType;
        IsNullable = isNullable;
        IsPrimaryKey = isPrimaryKey;
        OrdinalPosition = ordinalPosition;
        MaxLength = maxLength;
    }

    public string Name { get; }

    public string CSharpType { get; }

    public bool IsNullable { get; }

    public bool IsPrimaryKey { get; }

    public int OrdinalPosition { get; }

    /// <summary>Maximum character length for text columns; null when unbounded (e.g. text/MAX) or not applicable.</summary>
    public int? MaxLength { get; }

    public static TableColumn Create(
        string name, string csharpType, bool isNullable, bool isPrimaryKey, int ordinalPosition, int? maxLength = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(csharpType);

        return new TableColumn(
            name.Trim(), csharpType.Trim(), isNullable, isPrimaryKey, ordinalPosition,
            maxLength is > 0 ? maxLength : null);
    }
}
