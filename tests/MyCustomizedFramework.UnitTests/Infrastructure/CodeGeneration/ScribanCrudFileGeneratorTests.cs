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
    public void GenerateProducesTheEightExpectedFilesForSqlServer()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Student", StudentColumns, "MyCustomizedFramework");

        Assert.Equal(8, files.Count);
        Assert.Contains(files, file => file.RelativePath == "Domain/Student.cs");
        Assert.Contains(files, file => file.RelativePath == "Infrastructure/StudentDbEntity.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/StudentRequest.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/StudentResult.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/IStudentRepository.cs");
        Assert.Contains(files, file => file.RelativePath == "Infrastructure/StudentRepository.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/IStudentService.cs");
        Assert.Contains(files, file => file.RelativePath == "Application/StudentService.cs");
    }

    [Fact]
    public void GenerateEntityHasAllColumnsWithPrivateSetters()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Student", StudentColumns, "MyCustomizedFramework");
        var entity = files.Single(file => file.RelativePath == "Domain/Student.cs").Content;

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

        var files = generator.Generate(DatabaseEngine.SqlServer, "Student", StudentColumns, "MyCustomizedFramework");
        var request = files.Single(file => file.RelativePath == "Application/StudentRequest.cs").Content;
        var result = files.Single(file => file.RelativePath == "Application/StudentResult.cs").Content;

        Assert.Contains("public sealed record StudentRequest(", request);
        Assert.DoesNotContain("int Id", request);
        Assert.Contains("public sealed record StudentResult(", result);
        Assert.Contains("int Id", result);
    }

    [Fact]
    public void GenerateRepositoryUsesQuerySingleForSqlServerInsertAndEmbedsTheOutputClause()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Student", StudentColumns, "MyCustomizedFramework");
        var repository = files.Single(file => file.RelativePath == "Infrastructure/StudentRepository.cs").Content;

        Assert.Contains("QuerySingleAsync<int>(command)", repository);
        Assert.Contains("OUTPUT INSERTED.[Id]", repository);
        Assert.DoesNotContain("ParameterDirection.Output", repository);
    }

    [Fact]
    public void GenerateRepositoryUsesOutputParameterForOracleInsert()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.Oracle, "Student", StudentColumns, "MyCustomizedFramework");
        var repository = files.Single(file => file.RelativePath == "Infrastructure/StudentRepository.cs").Content;

        Assert.Contains("ParameterDirection.Output", repository);
        Assert.Contains("RETURNING \"Id\" INTO :Id", repository);
    }

    [Fact]
    public void GenerateUsesTheGivenRootNamespaceInsteadOfThisSolutionsOwn()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Student", StudentColumns, "Acme.Payroll");
        var entity = files.Single(file => file.RelativePath == "Domain/Student.cs").Content;
        var service = files.Single(file => file.RelativePath == "Application/StudentService.cs").Content;

        Assert.Contains("namespace Acme.Payroll.Domain.Students;", entity);
        Assert.Contains("using Acme.Payroll.Domain.Common;", service);
        Assert.DoesNotContain("MyCustomizedFramework", entity);
        Assert.DoesNotContain("MyCustomizedFramework", service);
    }

    [Fact]
    public void GenerateServiceReturnsResultPatternTypes()
    {
        var generator = new ScribanCrudFileGenerator();

        var files = generator.Generate(DatabaseEngine.SqlServer, "Student", StudentColumns, "MyCustomizedFramework");
        var service = files.Single(file => file.RelativePath == "Application/StudentService.cs").Content;

        Assert.Contains("public sealed class StudentService(IStudentRepository repository) : IStudentService", service);
        Assert.Contains("Task<Result<StudentResult>> GetStudentByIdAsync(int id", service);
        Assert.Contains("Task<Result> DeleteStudentByIdAsync(int id", service);
    }
}
