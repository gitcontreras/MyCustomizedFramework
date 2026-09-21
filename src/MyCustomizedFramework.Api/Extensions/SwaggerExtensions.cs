using System.Reflection;

namespace MyCustomizedFramework.Api.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        // Evitar cargar extensiones o configuraciones que requieran una versión concreta
        // de Swashbuckle si existe inconsistencia en tiempo de ejecución.
        // Solo configuramos la inclusión de XML de forma segura.
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
                // no crítico si no se pueden cargar comentarios
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
            options.RoutePrefix = "swagger"; // acceso en /swagger
            options.DocumentTitle = "MyCustomizedFramework API Docs";
            options.DefaultModelsExpandDepth(-1); // esconder modelos por defecto para limpieza
            options.DisplayRequestDuration();
        });

        return app;
    }
}
