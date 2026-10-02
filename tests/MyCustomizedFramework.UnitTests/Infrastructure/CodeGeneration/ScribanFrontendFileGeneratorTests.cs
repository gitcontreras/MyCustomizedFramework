using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Domain.SchemaExplorer;
using MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

namespace MyCustomizedFramework.UnitTests.Infrastructure.CodeGeneration;

public sealed class ScribanFrontendFileGeneratorTests
{
    private static readonly FrontendGenerationOptions React =
        new("react", "zustand", "tailwind", "/api");

    private static readonly IReadOnlyCollection<TableColumn> StudentColumns =
    [
        TableColumn.Create("Id", "int", isNullable: false, isPrimaryKey: true, ordinalPosition: 1),
        TableColumn.Create("FirstName", "string", isNullable: false, isPrimaryKey: false, ordinalPosition: 2, maxLength: 50),
        TableColumn.Create("Bio", "string", isNullable: true, isPrimaryKey: false, ordinalPosition: 3),
        TableColumn.Create("IsActive", "bool", isNullable: false, isPrimaryKey: false, ordinalPosition: 4),
        TableColumn.Create("EnrolledOn", "DateTime?", isNullable: true, isPrimaryKey: false, ordinalPosition: 5),
        TableColumn.Create("CourseId", "int", isNullable: false, isPrimaryKey: false, ordinalPosition: 6)
    ];

    private static readonly FrontendTableInput Students = new(
        null, "Students", StudentColumns,
        [new FrontendRelation("CourseId", "Courses", "Id", "Name")]);

    private static readonly FrontendGenerationOptions Vue = React with { Framework = "vue", StateManagement = "pinia" };

    private static readonly FrontendGenerationOptions Angular = React with { Framework = "angular", StateManagement = "signals" };

    public static TheoryData<string> Stacks => new() { "react", "vue", "angular", "angular-ngrx" };

    [Theory]
    [MemberData(nameof(Stacks))]
    public void KernelAndFeatureRenderWithoutTemplateErrors(string stack)
    {
        var generator = new ScribanFrontendFileGenerator();
        var options = stack switch { "vue" => Vue, "angular" => Angular, "angular-ngrx" => Angular with { StateManagement = "ngrx" }, _ => React };

        var files = generator.GenerateKernel(options).Concat(generator.Generate(Students, options)).ToList();

        Assert.All(files, file => Assert.False(string.IsNullOrWhiteSpace(file.Content), file.RelativePath));
        Assert.All(files, file => Assert.DoesNotContain("{{", file.Content));

        var dump = Environment.GetEnvironmentVariable("FRONTEND_DUMP_DIR");
        if (dump is not null)
        {
            foreach (var file in files)
            {
                var path = Path.Combine(dump, file.RelativePath.Replace("Web/", string.Empty));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, file.Content);
            }
        }
    }

    [Fact]
    public void UnsupportedStackIsReported()
    {
        var generator = new ScribanFrontendFileGenerator();

        Assert.NotNull(generator.GetUnsupportedReason(React with { Framework = "angular", StateManagement = "pinia" }));
        Assert.Null(generator.GetUnsupportedReason(Vue));
        Assert.Null(generator.GetUnsupportedReason(React));
    }
}





