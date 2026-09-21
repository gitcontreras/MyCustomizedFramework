using Microsoft.Extensions.DependencyInjection;
using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Application.Abstractions.Persistence;
using MyCustomizedFramework.Application.CrudGeneration;
using MyCustomizedFramework.Application.SchemaExplorer;
using MyCustomizedFramework.Domain.SchemaExplorer;
using MyCustomizedFramework.Infrastructure.CodeGeneration;
using MyCustomizedFramework.Infrastructure.Persistence.SchemaProviders;

namespace MyCustomizedFramework.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {

        services.AddKeyedScoped<ISchemaProvider, SqlServerSchemaProvider>(DatabaseEngine.SqlServer);
        services.AddKeyedScoped<ISchemaProvider, PostgreSqlSchemaProvider>(DatabaseEngine.PostgreSql);
        services.AddKeyedScoped<ISchemaProvider, MySqlSchemaProvider>(DatabaseEngine.MySql);
        services.AddKeyedScoped<ISchemaProvider, OracleSchemaProvider>(DatabaseEngine.Oracle);
        services.AddScoped<ISchemaProviderFactory, SchemaProviderFactory>();
        services.AddScoped<GetTablesHandler>();

        services.AddScoped<ICrudFileGenerator, ScribanCrudFileGenerator>();
        services.AddScoped<GenerateCrudHandler>();

        return services;
    }
}
