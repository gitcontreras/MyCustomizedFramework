using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Domain.SchemaExplorer;
using Scriban;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration;

internal sealed class ScribanKernelFileGenerator : IKernelFileGenerator
{
    private static readonly string TemplatesDirectory =
        Path.Combine(AppContext.BaseDirectory, "CodeGeneration", "Templates", "Kernel");

    private static readonly Dictionary<string, Template> Templates = LoadTemplates();

    public IReadOnlyCollection<GeneratedFile> Generate(DatabaseEngine engine, string rootNamespace)
    {
        var model = new KernelTemplateModel
        {
            RootNamespace = rootNamespace,
            Engine = engine.ToString(),
            ConnectionTypeName = MapToConnectionTypeName(engine)
        };

        return
        [
            new GeneratedFile("Domain/Common/Result.cs", Render("Result", model)),
            new GeneratedFile("Domain/Common/Error.cs", Render("Error", model)),
            new GeneratedFile("Domain/Common/ErrorType.cs", Render("ErrorType", model)),
            new GeneratedFile("Application/Abstractions/Persistence/IDbConnectionFactory.cs", Render("IDbConnectionFactory", model)),
            new GeneratedFile($"Infrastructure/Persistence/{model.Engine}ConnectionFactory.cs", Render("ConnectionFactory", model)),
            new GeneratedFile("Api/Common/ResultExtensions.cs", Render("ResultExtensions", model))
        ];
    }

    private static string MapToConnectionTypeName(DatabaseEngine engine) => engine switch
    {
        DatabaseEngine.SqlServer => "SqlConnection",
        DatabaseEngine.PostgreSql => "NpgsqlConnection",
        DatabaseEngine.MySql => "MySqlConnection",
        DatabaseEngine.Oracle => "OracleConnection",
        _ => throw new ArgumentOutOfRangeException(nameof(engine))
    };

    private static string Render(string templateName, KernelTemplateModel model) => Templates[templateName].Render(model);

    private static Dictionary<string, Template> LoadTemplates()
    {
        string[] names = ["Result", "Error", "ErrorType", "IDbConnectionFactory", "ConnectionFactory", "ResultExtensions"];

        return names.ToDictionary(
            name => name,
            name => Template.Parse(File.ReadAllText(Path.Combine(TemplatesDirectory, $"{name}.sbn"))));
    }
}
