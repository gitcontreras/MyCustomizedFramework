using MyCustomizedFramework.Domain.Common;

namespace MyCustomizedFramework.Domain.Products;

public sealed class Product : Entity
{
    private Product(Guid id, string name, decimal price)
        : base(id)
    {
        Name = name;
        Price = price;
    }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public static Product Create(string name, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "The price cannot be negative.");
        }

        return new Product(Guid.NewGuid(), name.Trim(), price);
    }
}
