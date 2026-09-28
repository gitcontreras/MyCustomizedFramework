using MyCustomizedFramework.Cli;

namespace MyCustomizedFramework.Cli.Tests;

public sealed class TableIdentifierTests
{
    [Fact]
    public void ParseReturnsNullSchemaWhenNoDotIsPresent()
    {
        var identifier = TableIdentifier.Parse("Orders");

        Assert.Null(identifier.Schema);
        Assert.Equal("Orders", identifier.Name);
    }

    [Fact]
    public void ParseSplitsSchemaAndTableOnTheFirstDot()
    {
        var identifier = TableIdentifier.Parse("sales.Orders");

        Assert.Equal("sales", identifier.Schema);
        Assert.Equal("Orders", identifier.Name);
    }

    [Fact]
    public void ParseTrimsWhitespaceAroundEachPart()
    {
        var identifier = TableIdentifier.Parse("  sales . Orders  ");

        Assert.Equal("sales", identifier.Schema);
        Assert.Equal("Orders", identifier.Name);
    }

    [Theory]
    [InlineData(".Orders")]
    [InlineData("Orders.")]
    public void ParseFallsBackToTheWholeTrimmedStringWhenTheDotIsAtEitherEdge(string raw)
    {
        var identifier = TableIdentifier.Parse(raw);

        Assert.Null(identifier.Schema);
        Assert.Equal(raw, identifier.Name);
    }
}
