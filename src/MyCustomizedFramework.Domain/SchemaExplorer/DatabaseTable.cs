namespace MyCustomizedFramework.Domain.SchemaExplorer;

public sealed class DatabaseTable
{
    private DatabaseTable(string schema, string name)
    {
        Schema = schema;
        Name = name;
    }

    public string Schema { get; }

    public string Name { get; }

    public static DatabaseTable Create(string schema, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new DatabaseTable(schema.Trim(), name.Trim());
    }
}
