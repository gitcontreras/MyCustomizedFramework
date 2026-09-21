using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Application.Products;
using MyCustomizedFramework.Infrastructure.Persistence;

namespace MyCustomizedFramework.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<ListProductsHandler>();
        return services;
    }
}
