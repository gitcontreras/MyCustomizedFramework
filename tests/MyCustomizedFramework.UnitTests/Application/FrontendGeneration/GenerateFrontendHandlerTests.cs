using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Application.FrontendGeneration;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.UnitTests.Application.FrontendGeneration;

public sealed class GenerateFrontendHandlerTests
{
    private static readonly DatabaseConnectionDetails Connection = new("localhost", null, "SampleDb", "sa", "CHANGE_ME");
    private static readonly FrontendGenerationOptions Options = new("react", "zustand", "tailwind", "/api");

    [Fact]
    public async Task ReturnsValidationErrorWhenStackIsUnsupported()
    {
        var generator = new CapturingGenerator { Unsupported = "nope" };
        var handler = CreateHandler(new FakeProvider(), generator);

        var result = await handler.HandleAsync(Query("Orders"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task ReturnsValidationErrorForCompositeKeys()
    {
        var provider = new FakeProvider();
        provider.Columns["Orders"] =
        [
            TableColumn.Create("A", "int", false, true, 1),
            TableColumn.Create("B", "int", false, true, 2)
        ];

        var result = await CreateHandler(provider, new CapturingGenerator()).HandleAsync(Query("Orders"));

        Assert.True(result.IsFailure);
        Assert.Equal("Frontend.SinglePrimaryKeyRequired", result.Error.Code);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenTableHasNoColumns()
    {
        var result = await CreateHandler(new FakeProvider(), new CapturingGenerator()).HandleAsync(Query("Missing"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task BuildsRelationUsingPreferredDisplayColumn()
    {
        var generator = new CapturingGenerator();
        var provider = OrdersWithCustomers(
            TableColumn.Create("Id", "int", false, true, 1),
            TableColumn.Create("Phone", "string", true, false, 2, 20),
            TableColumn.Create("Name", "string", false, false, 3, 100));

        var result = await CreateHandler(provider, generator).HandleAsync(Query("Orders"));

        Assert.True(result.IsSuccess);
        var relation = Assert.Single(generator.Input!.Relations);
        Assert.Equal("CustomerId", relation.ColumnName);
        Assert.Equal("Customers", relation.ReferencedTable);
        Assert.Equal("Name", relation.DisplayColumn);
    }

    [Fact]
    public async Task FallsBackToFirstTextColumnWhenNoPreferredName()
    {
        var generator = new CapturingGenerator();
        var provider = OrdersWithCustomers(
            TableColumn.Create("Id", "int", false, true, 1),
            TableColumn.Create("Age", "int", false, false, 2),
            TableColumn.Create("Alias", "string", false, false, 3, 20));

        await CreateHandler(provider, generator).HandleAsync(Query("Orders"));

        Assert.Equal("Alias", generator.Input!.Relations.Single().DisplayColumn);
    }

    [Fact]
    public async Task FallsBackToKeyWhenReferencedTableHasNoTextColumn()
    {
        var generator = new CapturingGenerator();
        var provider = OrdersWithCustomers(
            TableColumn.Create("Id", "int", false, true, 1),
            TableColumn.Create("Age", "int", false, false, 2));

        await CreateHandler(provider, generator).HandleAsync(Query("Orders"));

        Assert.Equal("Id", generator.Input!.Relations.Single().DisplayColumn);
    }

    [Fact]
    public async Task ReturnsAmbiguousSchemaErrorWhenTableExistsInSeveralSchemas()
    {
        var provider = new FakeProvider();
        provider.Tables = [DatabaseTable.Create("a", "Orders"), DatabaseTable.Create("b", "Orders")];

        var result = await CreateHandler(provider, new CapturingGenerator()).HandleAsync(Query("Orders", schema: null));

        Assert.True(result.IsFailure);
        Assert.Equal("Frontend.AmbiguousSchema", result.Error.Code);
    }

    private static FakeProvider OrdersWithCustomers(params TableColumn[] customerColumns)
    {
        var provider = new FakeProvider();
        provider.Columns["Orders"] =
        [
            TableColumn.Create("Id", "int", false, true, 1),
            TableColumn.Create("CustomerId", "int", false, false, 2)
        ];
        provider.Columns["Customers"] = customerColumns;
        provider.ForeignKeys = [TableForeignKey.Create("CustomerId", "dbo", "Customers", "Id")];
        return provider;
    }

    private static GenerateFrontendHandler CreateHandler(FakeProvider provider, CapturingGenerator generator) =>
        new(new FakeFactory(provider), generator);

    private static GenerateFrontendQuery Query(string table, string? schema = "dbo") =>
        new(Connection, DatabaseEngine.SqlServer, schema, table, Options);

    private sealed class FakeFactory(ISchemaProvider provider) : ISchemaProviderFactory
    {
        public ISchemaProvider Resolve(DatabaseEngine engine) => provider;
    }

    private sealed class FakeProvider : ISchemaProvider
    {
        public Dictionary<string, TableColumn[]> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<DatabaseTable> Tables { get; set; } = [];

        public IReadOnlyCollection<TableForeignKey> ForeignKeys { get; set; } = [];

        public Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
            DatabaseConnectionDetails connection, CancellationToken cancellationToken = default) =>
            Task.FromResult(Tables);

        public Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
            DatabaseConnectionDetails connection, string? schema, string tableName, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TableColumn>>(Columns.GetValueOrDefault(tableName) ?? []);

        public Task<IReadOnlyCollection<TableForeignKey>> GetForeignKeysAsync(
            DatabaseConnectionDetails connection, string? schema, string tableName, CancellationToken cancellationToken = default) =>
            Task.FromResult(ForeignKeys);
    }

    private sealed class CapturingGenerator : IFrontendFileGenerator
    {
        public string? Unsupported { get; set; }

        public FrontendTableInput? Input { get; private set; }

        public string? GetUnsupportedReason(FrontendGenerationOptions options) => Unsupported;

        public IReadOnlyCollection<GeneratedFile> GenerateKernel(FrontendGenerationOptions options) => [];

        public IReadOnlyCollection<GeneratedFile> Generate(FrontendTableInput table, FrontendGenerationOptions options)
        {
            Input = table;
            return [];
        }
    }
}
