namespace MyCustomizedFramework.Domain.SchemaExplorer;

public sealed class TableColumn
{
    private TableColumn(string name, string csharpType, bool isNullable, bool isPrimaryKey, int ordinalPosition)
    {
        Name = name;
        CSharpType = csharpType;
        IsNullable = isNullable;
        IsPrimaryKey = isPrimaryKey;
        OrdinalPosition = ordinalPosition;
    }

    public string Name { get; }

    public string CSharpType { get; }

    public bool IsNullable { get; }

    public bool IsPrimaryKey { get; }

    public int OrdinalPosition { get; }

    public static TableColumn Create(string name, string csharpType, bool isNullable, bool isPrimaryKey, int ordinalPosition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(csharpType);

        return new TableColumn(name.Trim(), csharpType.Trim(), isNullable, isPrimaryKey, ordinalPosition);
    }
}
