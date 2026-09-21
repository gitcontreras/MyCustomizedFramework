namespace MyCustomizedFramework.Api.Contracts.Crud;

public sealed record GenerateCrudRequest(
    string Engine,
    string Server,
    int? Port,
    string Database,
    string User,
    string Password,
    string TableName);
