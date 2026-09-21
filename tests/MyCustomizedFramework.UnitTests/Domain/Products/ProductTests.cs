using MyCustomizedFramework.Domain.Products;

namespace MyCustomizedFramework.UnitTests.Domain.Products;

public sealed class ProductTests
{
    [Fact]
    public void CreateWithNegativePriceThrows()
    {
        var action = () => Product.Create("Keyboard", -1);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void CreateTrimsNameAndCreatesIdentity()
    {
        var product = Product.Create("  Keyboard  ", 25);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(25, product.Price);
    }
}
