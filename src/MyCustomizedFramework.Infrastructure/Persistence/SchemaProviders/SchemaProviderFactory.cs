using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Domain.SchemaExplorer;

namespace MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

internal sealed class SchemaProviderFactory(IServiceProvider serviceProvider) : ISchemaProviderFactory
{
    public ISchemaProvider Resolve(DatabaseEngine engine) =>
        serviceProvider.GetRequiredKeyedService<ISchemaProvider>(engine);
}
