namespace MyCustomizedFramework.Infrastructure.CodeGeneration;

/// <summary>Everything the Kernel/*.sbn templates need. See <see cref="CrudTemplateModel"/> for the
/// per-table equivalent.</summary>
internal sealed class KernelTemplateModel
{
    public required string RootNamespace { get; init; }

    /// <summary>The domain enum member name (e.g. "SqlServer"), used both as the generated connection
    /// factory's class name prefix and to pick its ADO.NET connection type/using.</summary>
    public required string Engine { get; init; }

    /// <summary>The ADO.NET connection type name for <see cref="Engine"/> (e.g. "SqlConnection").</summary>
    public required string ConnectionTypeName { get; init; }
}
