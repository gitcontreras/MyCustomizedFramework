using System.Data.Common;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.UnitTests.Application.SchemaExplorer;

public sealed class GetTablesHandlerTests
{
    private static readonly DatabaseConnectionDetails ValidConnection =
        new("localhost", null, "SampleDb", "sa", "CHANGE_ME");

    [Fact]
    public async Task HandleAsyncReturnsValidationErrorWhenServerIsEmpty()
    {
        var handler = new GetTablesHandler(new StubSchemaProviderFactory(new StubSchemaProvider([])));
        var connection = ValidConnection with { Server = string.Empty };

        var result = await handler.HandleAsync(new GetTablesQuery(connection, DatabaseEngine.SqlServer));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationErrorWhenDatabaseIsEmpty()
    {
        var handler = new GetTablesHandler(new StubSchemaProviderFactory(new StubSchemaProvider([])));
        var connection = ValidConnection with { Database = string.Empty };

        var result = await handler.HandleAsync(new GetTablesQuery(connection, DatabaseEngine.SqlServer));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncReturnsMappedTablesOnSuccess()
    {
        var tables = new[] { DatabaseTable.Create("dbo", "Customers") };
        var handler = new GetTablesHandler(new StubSchemaProviderFactory(new StubSchemaProvider(tables)));

        var result = await handler.HandleAsync(new GetTablesQuery(ValidConnection, DatabaseEngine.SqlServer));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(new TableDto("dbo", "Customers"), result.Value.Single());
    }

    [Fact]
    public async Task HandleAsyncReturnsFailureWhenProviderThrowsDbException()
    {
        var handler = new GetTablesHandler(new StubSchemaProviderFactory(new ThrowingSchemaProvider()));

        var result = await handler.HandleAsync(new GetTablesQuery(ValidConnection, DatabaseEngine.SqlServer));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Failure, result.Error.Type);
    }

    private sealed class StubSchemaProviderFactory(ISchemaProvider provider) : ISchemaProviderFactory
    {
        public ISchemaProvider Resolve(DatabaseEngine engine) => provider;
    }

    private sealed class StubSchemaProvider(IReadOnlyCollection<DatabaseTable> tables) : ISchemaProvider
    {
        public Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
            DatabaseConnectionDetails connection,
            CancellationToken cancellationToken = default) => Task.FromResult(tables);

        public Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
            DatabaseConnectionDetails connection,
            string tableName,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TableColumn>>([]);
    }

    private sealed class ThrowingSchemaProvider : ISchemaProvider
    {
        public Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
            DatabaseConnectionDetails connection,
            CancellationToken cancellationToken = default) => throw new FakeDbException("Connection refused.");

        public Task<IReadOnlyCollection<TableColumn>> GetColumnsAsync(
            DatabaseConnectionDetails connection,
            string tableName,
            CancellationToken cancellationToken = default) => throw new FakeDbException("Connection refused.");
    }

    private sealed class FakeDbException(string message) : DbException(message);
}
