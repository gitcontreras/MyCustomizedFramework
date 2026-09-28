namespace MyCustomizedFramework.Cli;

/// <summary>
/// Parses a --table entry that may be schema-qualified ("sales.Orders" -> Schema="sales", Name="Orders")
/// or not ("Orders" -> Schema=null, Name="Orders"), splitting on the first '.'.
/// </summary>
internal sealed record TableIdentifier(string? Schema, string Name)
{
    public static TableIdentifier Parse(string raw)
    {
        var trimmed = raw.Trim();
        var separatorIndex = trimmed.IndexOf('.');

        if (separatorIndex <= 0 || separatorIndex == trimmed.Length - 1)
        {
            return new TableIdentifier(null, trimmed);
        }

        var schema = trimmed[..separatorIndex].Trim();
        var name = trimmed[(separatorIndex + 1)..].Trim();

        return new TableIdentifier(schema, name);
    }
}
