using System.Reflection;

namespace MyCustomizedFramework.Api.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            try
            {
                var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile ?? string.Empty);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }
            catch
            {
                // no critical, continue without XML comments if the file is not found or any error occurs
            }
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyCustomizedFramework API v1");
            options.RoutePrefix = "swagger"; 
            options.DocumentTitle = "MyCustomizedFramework API Docs";
            options.DefaultModelsExpandDepth(-1); 
            options.DisplayRequestDuration();
        });

        return app;
    }
}
