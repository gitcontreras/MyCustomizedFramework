using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Application.Abstractions.CodeGeneration;

/// <summary>Chosen stack for the generated web client (all values already validated and lowercase).</summary>
public sealed record FrontendGenerationOptions(string Framework, string StateManagement, string Styles, string ApiBaseUrl);

/// <summary>A foreign key plus what the form needs to render a dropdown for it.</summary>
public sealed record FrontendRelation(
    string ColumnName,
    string ReferencedTable,
    string ReferencedColumn,
    string DisplayColumn);

public sealed record FrontendTableInput(
    string? Schema,
    string TableName,
    IReadOnlyCollection<TableColumn> Columns,
    IReadOnlyCollection<FrontendRelation> Relations);

public interface IFrontendFileGenerator
{
    /// <summary>Null when the stack is supported; otherwise a human-readable reason it is not (yet).</summary>
    string? GetUnsupportedReason(FrontendGenerationOptions options);

    /// <summary>Files shared by every table (HTTP client, error type, reusable form controls); generated once.</summary>
    IReadOnlyCollection<GeneratedFile> GenerateKernel(FrontendGenerationOptions options);

    IReadOnlyCollection<GeneratedFile> Generate(FrontendTableInput table, FrontendGenerationOptions options);
}
