using System.Data.Common;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Domain.Common;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.UnitTests.Application.SchemaExplorer;

public sealed class GetTablesHandlerTests
{
    [Fact]
    public async Task HandleAsyncReturnsValidationErrorWhenConnectionStringIsEmpty()
    {
        var handler = new GetTablesHandler(new StubSchemaProviderFactory(new StubSchemaProvider([])));

        var result = await handler.HandleAsync(new GetTablesQuery(string.Empty, DatabaseEngine.SqlServer));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task HandleAsyncReturnsMappedTablesOnSuccess()
    {
        var tables = new[] { DatabaseTable.Create("dbo", "Customers") };
        var handler = new GetTablesHandler(new StubSchemaProviderFactory(new StubSchemaProvider(tables)));

        var result = await handler.HandleAsync(new GetTablesQuery("Server=localhost;", DatabaseEngine.SqlServer));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(new TableDto("dbo", "Customers"), result.Value.Single());
    }

    [Fact]
    public async Task HandleAsyncReturnsFailureWhenProviderThrowsDbException()
    {
        var handler = new GetTablesHandler(new StubSchemaProviderFactory(new ThrowingSchemaProvider()));

        var result = await handler.HandleAsync(new GetTablesQuery("Server=unreachable;", DatabaseEngine.SqlServer));

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
            string connectionString,
            CancellationToken cancellationToken = default) => Task.FromResult(tables);
    }

    private sealed class ThrowingSchemaProvider : ISchemaProvider
    {
        public Task<IReadOnlyCollection<DatabaseTable>> GetTablesAsync(
            string connectionString,
            CancellationToken cancellationToken = default) => throw new FakeDbException("Connection refused.");
    }

    private sealed class FakeDbException(string message) : DbException(message);
}
