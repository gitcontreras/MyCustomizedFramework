using MyCustomizedFramework.Application.Abstractions.Persistence;

namespace MyCustomizedFramework.Application.Products;

public sealed class ListProductsHandler(IProductRepository repository)
{
    public async Task<IReadOnlyCollection<ProductDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await repository.ListAsync(cancellationToken);
        return products
            .Select(product => new ProductDto(product.Id, product.Name, product.Price))
            .ToArray();
    }
}
