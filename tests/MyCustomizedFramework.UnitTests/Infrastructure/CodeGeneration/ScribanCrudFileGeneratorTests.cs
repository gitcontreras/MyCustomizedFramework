using MyCustomizedFramework.Domain.SchemaExplorer;
using MyCustomizedFramework.Infrastructure.CodeGeneration;

namespace MyCustomizedFramework.UnitTests.Infrastructure.CodeGeneration;

public sealed class ScribanCrudFileGeneratorTests
{
    private static readonly IReadOnlyCollection<TableColumn> StudentColumns =
    [
        TableColumn.Create("Id", "int", isNullable: false, isPrimaryKey: true, ordinalPosition: 1),
        TableColumn.Create("FirstName", "string", isNullable: false, isPrimaryKey: false, ordinalPosition: 2),
        TableColumn.Create("LastName", "string", isNullable: false, isPrimaryKey: false, ordinalPosition: 3),
        TableColumn.Create("EnrolledOn", "DateTime?", isNullable: true, isPrimaryKey: false, ordinalPosition: 4)
    ];

    [Fact]
    public void GenerateProducesTheTenExpectedFilesUnderFeatureFolders()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");

        Assert.Equal(10, files.Count);
        Assert.Contains(files, file => file.RelativePath == "Domain/Students/Student.cs");
        Assert.Contains(files, file => file.RelativePath == "Infrastructure/Persistence/Students/StudentDbEntity.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/Students/StudentRequest.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/Students/StudentResult.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/Students/IStudentRepository.cs");
        Assert.Contains(files, file => file.RelativePath == "Infrastructure/Persistence/Students/StudentRepository.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/Students/IStudentService.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/Students/StudentService.cs");
        Assert.Contains(files, file => file.RelativePath == "Tests/Application/Students/StudentServiceTests.cs");
        Assert.Contains(files, file => file.RelativePath == "Api/Controllers/Students/StudentController.cs");
    }

    [Fact]
    public void GenerateEntityHasAllColumnsWithPrivateSetters()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var entity = files.Single(file => file.RelativePath == "Domain/Students/Student.cs").Content;

        Assert.Contains("public sealed class Student", entity);
        Assert.Contains("public int Id { get; private set; }", entity);
        Assert.Contains("public string FirstName { get; private set; }", entity);
        Assert.Contains("public DateTime? EnrolledOn { get; private set; }", entity);
        Assert.Contains("ArgumentException.ThrowIfNullOrWhiteSpace(firstName);", entity);
    }

    [Fact]
    public void GenerateRequestExcludesThePrimaryKeyButResultIncludesIt()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var request = files.Single(file => file.RelativePath == "Application/Students/StudentRequest.cs").Content;
        var result = files.Single(file => file.RelativePath == "Application/Students/StudentResult.cs").Content;

        Assert.Contains("public sealed record StudentRequest(", request);
        Assert.DoesNotContain("int Id", request);
        Assert.Contains("public sealed record StudentResult(", result);
        Assert.Contains("int Id", result);
    }

    [Fact]
    public void GenerateRepositoryUsesQuerySingleForSqlServerInsertAndEmbedsTheOutputClause()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var repository = files.Single(file => file.RelativePath == "Infrastructure/Persistence/Students/StudentRepository.cs").Content;

        Assert.Contains("QuerySingleAsync<int>(command)", repository);
        Assert.Contains("OUTPUT INSERTED.[Id]", repository);
        Assert.DoesNotContain("ParameterDirection.Output", repository);
    }

    [Fact]
    public void GenerateRepositoryUsesOutputParameterForOracleInsert()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.Oracle, null, "Student", StudentColumns, "MyCustomizedFramework");
        var repository = files.Single(file => file.RelativePath == "Infrastructure/Persistence/Students/StudentRepository.cs").Content;

        Assert.Contains("ParameterDirection.Output", repository);
        Assert.Contains("RETURNING \"Id\" INTO :Id", repository);
    }

    [Fact]
    public void GenerateUsesTheGivenRootNamespaceInsteadOfThisSolutionsOwn()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "Acme.Payroll");
        var entity = files.Single(file => file.RelativePath == "Domain/Students/Student.cs").Content;
        var service = files.Single(file => file.RelativePath == "Application/Students/StudentService.cs").Content;

        Assert.Contains("namespace Acme.Payroll.Domain.Students;", entity);
        Assert.Contains("using Acme.Payroll.Domain.Common;", service);
        Assert.DoesNotContain("MyCustomizedFramework", entity);
        Assert.DoesNotContain("MyCustomizedFramework", service);
    }

    [Fact]
    public void GenerateServiceTestsStubTheRepositoryAndCoverEveryServiceMethod()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var tests = files.Single(file => file.RelativePath == "Tests/Application/Students/StudentServiceTests.cs").Content;

        Assert.Contains("public sealed class StudentServiceTests", tests);
        Assert.Contains("private sealed class StubRepository : IStudentRepository", tests);
        Assert.Contains("Task AddStudentAsync_ReturnsSuccessAndMappedResult()", tests);
        Assert.Contains("Task GetStudentByIdAsync_ReturnsNotFound_WhenRepositoryReturnsNull()", tests);
        Assert.Contains("Task UpdateStudentByIdAsync_ReturnsSuccess_WhenFound()", tests);
        Assert.Contains("Task DeleteStudentByIdAsync_ReturnsSuccess_WhenDeleted()", tests);
    }

    [Fact]
    public void GenerateControllerExposesTheFiveRestEndpointsOverTheGeneratedService()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var controller = files.Single(file => file.RelativePath == "Api/Controllers/Students/StudentController.cs").Content;

        Assert.Contains("namespace MyCustomizedFramework.Api.Controllers.Students;", controller);
        Assert.Contains("[Route(\"api/students\")]", controller);
        Assert.Contains("public sealed class StudentController(IStudentService service) : ControllerBase", controller);
        Assert.Contains("[HttpGet]", controller);
        Assert.Contains("[HttpGet(\"{id}\")]", controller);
        Assert.Contains("[HttpPost]", controller);
        Assert.Contains("[HttpPut(\"{id}\")]", controller);
        Assert.Contains("[HttpDelete(\"{id}\")]", controller);
        Assert.Contains("result.ToActionResult(value => value);", controller);
    }

    [Fact]
    public void GenerateServiceReturnsResultPatternTypes()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var service = files.Single(file => file.RelativePath == "Application/Students/StudentService.cs").Content;

        Assert.Contains("public sealed class StudentService(IStudentRepository repository) : IStudentService", service);
        Assert.Contains("Task<Result<StudentResult>> GetStudentByIdAsync(int id", service);
        Assert.Contains("Task<Result> DeleteStudentByIdAsync(int id", service);
    }

    [Theory]
    [InlineData("Orders", "Order", "Orders")]
    [InlineData("Categories", "Category", "Categories")]
    [InlineData("Student", "Student", "Students")]
    public void GenerateSingularizesThePluralTableNameForTheDomainEntityButKeepsTheRealTableNameInSql(
        string tableInput,
        string expectedSingular,
        string expectedPlural)
    {
        IReadOnlyCollection<TableColumn> columns =
        [
            TableColumn.Create("Id", "int", isNullable: false, isPrimaryKey: true, ordinalPosition: 1),
            TableColumn.Create("Name", "string", isNullable: false, isPrimaryKey: false, ordinalPosition: 2)
        ];

        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, tableInput, columns, "MyCustomizedFramework");

        Assert.Contains(files, file => file.RelativePath == $"Domain/{expectedPlural}/{expectedSingular}.cs");

        var entity = files.Single(file => file.RelativePath == $"Domain/{expectedPlural}/{expectedSingular}.cs").Content;
        Assert.Contains($"public sealed class {expectedSingular}", entity);
        Assert.Contains($"namespace MyCustomizedFramework.Domain.{expectedPlural};", entity);

        var controller = files.Single(file => file.RelativePath == $"Api/Controllers/{expectedPlural}/{expectedSingular}Controller.cs").Content;
        Assert.Contains($"[Route(\"api/{expectedPlural.ToLowerInvariant()}\")]", controller);

        var repository = files.Single(file => file.RelativePath == $"Infrastructure/Persistence/{expectedPlural}/{expectedSingular}Repository.cs").Content;
        Assert.Contains($"FROM [{tableInput}]", repository); // the real SQL table name, never singularized
    }

    [Theory]
    [InlineData(DatabaseEngine.SqlServer, "FROM [sales].[Orders]")]
    [InlineData(DatabaseEngine.PostgreSql, "FROM \"sales\".\"Orders\"")]
    [InlineData(DatabaseEngine.MySql, "FROM `sales`.`Orders`")]
    [InlineData(DatabaseEngine.Oracle, "FROM \"sales\".\"Orders\"")]
    public void GenerateQualifiesTheSqlTableNameWithTheGivenSchema(DatabaseEngine engine, string expectedSelectAll)
    {
        IReadOnlyCollection<TableColumn> columns =
        [
            TableColumn.Create("Id", "int", isNullable: false, isPrimaryKey: true, ordinalPosition: 1),
            TableColumn.Create("Name", "string", isNullable: false, isPrimaryKey: false, ordinalPosition: 2)
        ];

        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(engine, "sales", "Orders", columns, "MyCustomizedFramework");
        var repository = files.Single(file => file.RelativePath == "Infrastructure/Persistence/Orders/OrderRepository.cs").Content;

        Assert.Contains(expectedSelectAll, repository);
    }

    [Fact]
    public void GenerateLeavesTheSqlTableNameUnqualifiedWhenNoSchemaIsGiven()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var repository = files.Single(file => file.RelativePath == "Infrastructure/Persistence/Students/StudentRepository.cs").Content;

        Assert.Contains("FROM [Student]", repository);
        Assert.DoesNotContain("[dbo]", repository);
    }

    [Fact]
    public void GenerateRepositoryAndServiceInterfacesShareTheSameFeatureNamespace()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, null, "Student", StudentColumns, "MyCustomizedFramework");
        var repositoryInterface = files.Single(file => file.RelativePath == "Application/Students/IStudentRepository.cs").Content;

        Assert.Contains("namespace MyCustomizedFramework.Application.Students;", repositoryInterface);
    }
}
