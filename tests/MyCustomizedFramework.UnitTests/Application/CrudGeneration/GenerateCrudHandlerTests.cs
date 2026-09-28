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

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, null, "Ghost", "MyCustomizedFramework"));

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

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, null, "NoKey", "MyCustomizedFramework"));

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

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, null, "Composite", "MyCustomizedFramework"));

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

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, null, "Student", "MyCustomizedFramework"));

        Assert.True(result.IsSuccess);
        Assert.Same(expectedFiles, result.Value);
    }

    [Fact]
    public async Task HandleAsyncPassesTheSchemaThroughToTheFileGenerator()
    {
        var generator = new StubCrudFileGenerator([]);
        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider(SingleKeyColumns)),
            generator);

        await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, "sales", "Orders", "MyCustomizedFramework"));

        Assert.Equal("sales", generator.LastSchema);
    }

    [Fact]
    public async Task HandleAsyncAutoDetectsTheSchemaWhenTheTableExistsInExactlyOneSchema()
    {
        IReadOnlyCollection<DatabaseTable> tables = [DatabaseTable.Create("sales", "Orders")];
        var generator = new StubCrudFileGenerator([]);
        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider(SingleKeyColumns, tables)),
            generator);

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, null, "Orders", "MyCustomizedFramework"));

        Assert.True(result.IsSuccess);
        Assert.Equal("sales", generator.LastSchema);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationErrorWhenTheTableExistsInMultipleSchemas()
    {
        IReadOnlyCollection<DatabaseTable> tables =
        [
            DatabaseTable.Create("sales", "Orders"),
            DatabaseTable.Create("archive", "Orders")
        ];
        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider(SingleKeyColumns, tables)),
            new StubCrudFileGenerator([]));

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, null, "Orders", "MyCustomizedFramework"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Crud.AmbiguousSchema", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsyncSkipsAutoDetectionWhenSchemaIsAlreadyGiven()
    {
        // Only one schema is "registered" here (sales) - if the handler tried to auto-detect despite
        // Schema being given explicitly, it would still resolve fine, so this alone wouldn't catch a
        // regression. The real assertion is that the explicitly-given "archive" (which GetTablesAsync
        // never even returns) still wins, proving GetTablesAsync was never consulted.
        IReadOnlyCollection<DatabaseTable> tables = [DatabaseTable.Create("sales", "Orders")];
        var generator = new StubCrudFileGenerator([]);
        var handler = new GenerateCrudHandler(
            new StubSchemaProviderFactory(new StubSchemaProvider(SingleKeyColumns, tables)),
            generator);

        var result = await handler.HandleAsync(new GenerateCrudQuery(ValidConnection, DatabaseEngine.SqlServer, "archive", "Orders", "MyCustomizedFramework"));

        Assert.True(result.IsSuccess);
        Assert.Equal("archive", generator.LastSchema);
    }

    private sealed class StubSchemaProviderFactory(ISchemaProvider provider) : ISchemaProviderFactory
    {
        public ISchemaProvider Resolve(DatabaseEngine engine) => provider;
    }

    private sealed class StubSchemaProvider(
        IReadOnlyCollection<TableColumn> columns,
        IReadOnlyCollection<DatabaseTable>? tables = null) : ISchemaProvider
    {
        public Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
            DatabaseConnectionDetails connection,
            CancellationToken cancellationToken = default) => Task.FromResult(tables ?? []);

        public Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
            DatabaseConnectionDetails connection,
            string? schema,
            string tableName,
            CancellationToken cancellationToken = default) => Task.FromResult(columns);
    }

    private sealed class StubCrudFileGenerator(IReadOnlyCollection<GeneratedFile> files) : ICrudFileGenerator
    {
        public string? LastSchema { get; private set; }

        public IReadOnlyCollection<GeneratedFile> Generate(
            DatabaseEngine engine,
            string? schema,
            string tableName,
            IReadOnlyCollection<TableColumn> columns,
            string rootNamespace)
        {
            LastSchema = schema;
            return files;
        }
    }
}
