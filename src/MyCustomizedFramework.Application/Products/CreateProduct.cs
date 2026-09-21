using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.Products;

namespace MyCustomizedFramework.Application.Products;

public sealed record CreateProductCommand(string Name, decimal Price);

public sealed class CreateProductHandler(IProductRepository repository)
{
    public async Task<ProductDto> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = Product.Create(command.Name, command.Price);
        await repository.AddAsync(product, cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Price);
    }
}
