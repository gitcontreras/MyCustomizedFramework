using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Application.CrudGeneration;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.UnitTests.Application.CrudGeneration;

public sealed class GenerateCrudHandlerTests
{
    private static readonly DatabaseConnectionDetails ValidConnection =
        new("localhost", null, "SampleDb", "sa", "CHANGE_ME");

    private static readonly IReadOnlyCollection<TableColumn> SingleKeyColumns =
    [
        TableColumn.Create("Id", "int", isNullable: false, isPrimaryKey: true, ordinalPosition: 1),
        TableColumn.Create("Name", "string", isNullable: false, isPrimaryKey: false, ordinalPosition: 2)
    ];

    [Fact]
    public async Task HandleAsyncReturnsNotFoundWhenTableHasNoColumns()
    {
        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider([])),
            new StubCrudFileGenerator([]));

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, "Ghost", "MyCustomizedFramework"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationErrorWhenTableHasNoPrimaryKey()
    {
        IReadOnlyCollection<TableColumn> noKeyColumns =
        [
            TableColumn.Create("Name", "string", isNullable: false, isPrimaryKey: false, ordinalPosition: 1)
        ];

        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider(noKeyColumns)),
            new StubCrudFileGenerator([]));

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, "NoKey", "MyCustomizedFramework"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationErrorWhenTableHasCompositePrimaryKey()
    {
        IReadOnlyCollection<TableColumn> compositeKeyColumns =
        [
            TableColumn.Create("Id1", "int", isNullable: false, isPrimaryKey: true, ordinalPosition: 1),
            TableColumn.Create("Id2", "int", isNullable: false, isPrimaryKey: true, ordinalPosition: 2)
        ];

        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider(compositeKeyColumns)),
            new StubCrudFileGenerator([]));

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, "Composite", "MyCustomizedFramework"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncReturnsGeneratedFilesOnSuccess()
    {
        var expectedFiles = new[] { new GeneratedFile("Domain/Student.cs", "public sealed class Student;") };
        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider(SingleKeyColumns)),
            new StubCrudFileGenerator(expectedFiles));

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, "Student", "MyCustomizedFramework"));

        Assert.True(result.IsSuccess);
        Assert.Same(expectedFiles, result.Value);
    }

    private sealed class StubSchemaProviderFactory(ISchemaProvider provider) : ISchemaProviderFactory
    {
        public ISchemaProvider Resolve(DatabaseEngine engine) => provider;
    }

    private sealed class StubSchemaProvider(IReadOnlyCollection<TableColumn> columns) : ISchemaProvider
    {
        public Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
            DatabaseConnectionDetails connection,
            CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<DatabaseTable>>([]);

        public Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
            DatabaseConnectionDetails connection,
            string tableName,
            CancellationToken cancellationToken = default) => Task.FromResult(columns);
    }

    private sealed class StubCrudFileGenerator(IReadOnlyCollection<GeneratedFile> files) : ICrudFileGenerator
    {
        public IReadOnlyCollection<GeneratedFile> Generate(
            DatabaseEngine engine,
            string tableName,
            IReadOnlyCollection<TableColumn> columns,
            string rootNamespace) => files;
    }
}
