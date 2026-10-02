namespace MyCustomizedFramework.Domain.SchemaExplorer;

/// <summary>A single-column foreign key: <see cref="ColumnName"/> references <see cref="ReferencedTable"/>.<see cref="ReferencedColumn"/>.</summary>
public sealed class TableForeignKey
{
    private TableForeignKey(string columnName, string? referencedSchema, string referencedTable, string referencedColumn)
    {
        ColumnName = columnName;
        ReferencedSchema = referencedSchema;
        ReferencedTable = referencedTable;
        ReferencedColumn = referencedColumn;
    }

    public string ColumnName { get; }

    public string? ReferencedSchema { get; }

    public string ReferencedTable { get; }

    public string ReferencedColumn { get; }

    public static TableForeignKey Create(string columnName, string? referencedSchema, string referencedTable, string referencedColumn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ArgumentException.ThrowIfNullOrWhiteSpace(referencedTable);
        ArgumentException.ThrowIfNullOrWhiteSpace(referencedColumn);

        return new TableForeignKey(columnName.Trim(), referencedSchema?.Trim(), referencedTable.Trim(), referencedColumn.Trim());
    }
}
