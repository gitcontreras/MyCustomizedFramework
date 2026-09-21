using Humanizer;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Domain.SchemaExplorer;
using Scriban;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration;

internal sealed class ScribanCrudFileGenerator : ICrudFileGenerator
{
    private static readonly string TemplatesDirectory =
        Path.Combine(AppContext.BaseDirectory, "CodeGeneration", "Templates");

    private static readonly Dictionary<string, Template> Templates = LoadTemplates();

    public IReadOnlyCollection<GeneratedFile> Generate(
        DatabaseEngine engine,
        string tableName,
        IReadOnlyCollection<TableColumn> columns)
    {
        var primaryKey = columns.Single(column => column.IsPrimaryKey);
        var insertableColumns = columns.Where(column => !column.IsPrimaryKey).ToArray();
        var fragments = SqlDialect.Build(engine, tableName, primaryKey, insertableColumns);

        var model = new CrudTemplateModel
        {
            TableName = tableName,
            PluralName = tableName.Pluralize(),
            CamelName = tableName.Camelize(),
            Engine = engine.ToString(),
            Columns = columns.Select(ToColumnModel).ToArray(),
            InsertableColumns = insertableColumns.Select(ToColumnModel).ToArray(),
            PrimaryKey = ToColumnModel(primaryKey),
            OraclePrimaryKeyDbType = MapToDbTypeName(primaryKey.CSharpType),
            InsertSql = fragments.Insert,
            SelectAllSql = fragments.SelectAll,
            SelectByIdSql = fragments.SelectById,
            UpdateSql = fragments.Update,
            DeleteSql = fragments.Delete
        };

        return
        [
            new GeneratedFile($"Domain/{tableName}.cs", Render("DomainEntity", model)),
            new GeneratedFile($"Infrastructure/{tableName}DbEntity.cs", Render("DbEntity", model)),
            new GeneratedFile($"Application/{tableName}Request.cs", Render("Request", model)),
            new GeneratedFile($"Application/{tableName}Result.cs", Render("Result", model)),
            new GeneratedFile($"Application/I{tableName}Repository.cs", Render("IRepository", model)),
            new GeneratedFile($"Infrastructure/{tableName}Repository.cs", Render("Repository", model)),
            new GeneratedFile($"Application/I{tableName}Service.cs", Render("IService", model)),
            new GeneratedFile($"Application/{tableName}Service.cs", Render("Service", model))
        ];
    }

    private static ColumnModel ToColumnModel(TableColumn column) => new()
    {
        Name = column.Name,
        CamelName = column.Name.Camelize(),
        Type = column.CSharpType,
        IsNullable = column.IsNullable,
        IsPrimaryKey = column.IsPrimaryKey
    };

    private static string MapToDbTypeName(string csharpType) => csharpType.TrimEnd('?') switch
    {
        "int" => "Int32",
        "long" => "Int64",
        "short" => "Int16",
        "decimal" => "Decimal",
        "double" => "Double",
        "float" => "Single",
        "Guid" => "Guid",
        "DateTime" => "DateTime",
        _ => "String"
    };

    private static string Render(string templateName, CrudTemplateModel model) => Templates[templateName].Render(model);

    private static Dictionary<string, Template> LoadTemplates()
    {
        string[] names = ["DomainEntity", "DbEntity", "Request", "Result", "IRepository", "Repository", "IService", "Service"];

        return names.ToDictionary(
            name => name,
            name => Template.Parse(File.ReadAllText(Path.Combine(TemplatesDirectory, $"{name}.sbn"))));
    }
}
