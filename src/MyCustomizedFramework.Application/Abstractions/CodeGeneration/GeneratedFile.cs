namespace MyCustomizedFramework.Application.Abstractions.CodeGeneration;

/// <summary>
/// One generated source file. <paramref name="RelativePath"/> mirrors this solution's own layer
/// folders (e.g. "Domain/Student.cs", "Application/StudentRequest.cs") so the caller can extract
/// the resulting zip directly on top of their solution.
/// </summary>
public sealed record GeneratedFile(string RelativePath, string Content);
