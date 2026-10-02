using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyCustomizedFramework.Cli.Configuration;

internal sealed class CrudGenConfig
{
    [JsonPropertyName("rootNamespace")]
    public string? RootNamespace { get; set; }

    [JsonPropertyName("connection")]
    public ConnectionConfig Connection { get; set; } = new();

    [JsonPropertyName("paths")]
    public Dictionary<string, string> Paths { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("frontend")]
    public FrontendConfig? Frontend { get; set; }

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static CrudGenConfig Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<CrudGenConfig>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Could not parse config file '{path}'.");
    }

    public const string TemplateJson =
        """
        {
          "rootNamespace": "YourCompany.YourProject",
          "connection": {
            "engine": "sql",
            "server": "localhost",
            "port": 1433,
            "database": "YourDatabase",
            "user": "YourUser"
          },
          "paths": {
            "Domain": "src/YourCompany.YourProject.Domain",
            "Application": "src/YourCompany.YourProject.Application",
            "Infrastructure": "src/YourCompany.YourProject.Infrastructure",
            "Api": "src/YourCompany.YourProject.Api",
            "Tests": "tests/YourCompany.YourProject.UnitTests"
          },
          // Optional: used by 'generate --with-frontend' / '--frontend-only'.
          "frontend": {
            "framework": "react",          // "react" | "angular" | "vue"
            "stateManagement": "zustand",  // react: zustand | vue: pinia | angular: signals, ngrx
            "path": "frontend/src",
            "styles": "tailwind"           // "tailwind"
          }
        }

        """;
}

internal sealed class ConnectionConfig
{
    [JsonPropertyName("engine")]
    public string? Engine { get; set; }

    [JsonPropertyName("server")]
    public string? Server { get; set; }

    [JsonPropertyName("port")]
    public int? Port { get; set; }

    [JsonPropertyName("database")]
    public string? Database { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}


