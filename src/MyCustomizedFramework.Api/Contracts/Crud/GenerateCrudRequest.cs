namespace MyCustomizedFramework.Api.Contracts.Crud;

/// <summary>
/// <paramref name="RootNamespace"/> is optional - when omitted, the generated code keeps using
/// "MyCustomizedFramework" (this solution's own namespace). Set it to your target solution's root
/// namespace (e.g. "Acme.Payroll") so every generated file - including the shared Result/Error/
/// IDbConnectionFactory usings - drops in ready to compile, with no manual find-and-replace.
/// </summary>
public sealed record GenerateCrudRequest(
    string Engine,
    string Server,
    int? Port,
    string Database,
    string User,
    string Password,
    string TableName,
    string? RootNamespace = null);
