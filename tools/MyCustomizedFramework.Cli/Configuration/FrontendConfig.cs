using System.Text.Json.Serialization;

namespace MyCustomizedFramework.Cli.Configuration;

internal sealed class FrontendConfig
{
    [JsonPropertyName("framework")]
    public string? Framework { get; set; }

    [JsonPropertyName("stateManagement")]
    public string? StateManagement { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("styles")]
    public string? Styles { get; set; }

    [JsonPropertyName("apiBaseUrl")]
    public string? ApiBaseUrl { get; set; }
}

/// <summary>Frontend settings after defaults and validation; every field is non-null and known-valid.</summary>
internal sealed record FrontendOptions(
    string Framework,
    string StateManagement,
    string Path,
    string Styles,
    string ApiBaseUrl);

internal static class FrontendOptionsResolver
{
    public const string BucketName = "Web";

    private static readonly IReadOnlyDictionary<string, string[]> StatesByFramework =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["react"] = ["zustand"],
            ["vue"] = ["pinia"],
            ["angular"] = ["signals", "ngrx"]
        };

    private static readonly string[] AllowedStyles = ["tailwind"];

    /// <summary>
    /// Precedence: CLI flag &gt; crudgen.config.json &gt; default. Returns null and fills
    /// <paramref name="error"/> when the combination is invalid.
    /// </summary>
    public static FrontendOptions? Resolve(
        FrontendConfig? config,
        IReadOnlyDictionary<string, string?> flags,
        out string? error)
    {
        error = null;

        var framework = (flags.GetValueOrDefault("frontend") ?? config?.Framework ?? "react").Trim().ToLowerInvariant();
        if (!StatesByFramework.TryGetValue(framework, out var allowedStates))
        {
            error = $"Unsupported frontend framework '{framework}'. Use: {string.Join(", ", StatesByFramework.Keys)}.";
            return null;
        }

        var state = (flags.GetValueOrDefault("state") ?? config?.StateManagement ?? allowedStates[0]).Trim().ToLowerInvariant();
        if (!allowedStates.Contains(state, StringComparer.OrdinalIgnoreCase))
        {
            error = $"State management '{state}' is not valid for {framework}. Use: {string.Join(", ", allowedStates)}.";
            return null;
        }

        var styles = (flags.GetValueOrDefault("styles") ?? config?.Styles ?? AllowedStyles[0]).Trim().ToLowerInvariant();
        if (!AllowedStyles.Contains(styles, StringComparer.OrdinalIgnoreCase))
        {
            error = $"Unsupported styles '{styles}'. Use: {string.Join(", ", AllowedStyles)}.";
            return null;
        }

        var path = string.IsNullOrWhiteSpace(config?.Path) ? "frontend/src" : config.Path.Trim();
        if (System.IO.Path.IsPathRooted(path) || path.Split('/', '\\').Contains(".."))
        {
            error = "frontend.path must be a relative path inside the repository (no '..' segments).";
            return null;
        }

        var apiBaseUrl = string.IsNullOrWhiteSpace(config?.ApiBaseUrl) ? "/api" : config.ApiBaseUrl.Trim().TrimEnd('/');

        return new FrontendOptions(framework, state, path, styles, apiBaseUrl);
    }
}


