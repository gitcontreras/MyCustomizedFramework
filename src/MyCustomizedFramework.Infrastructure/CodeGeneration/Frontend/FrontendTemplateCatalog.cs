using System.Collections.Concurrent;
using Scriban;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

/// <summary>Loads and caches Frontend/*.sbn templates by relative name (e.g. "react/feature/Form.tsx" -> Form.tsx.sbn).</summary>
internal sealed partial class FrontendTemplateCatalog
{
    private static readonly string TemplatesDirectory =
        Path.Combine(AppContext.BaseDirectory, "CodeGeneration", "Templates", "Frontend");

    private readonly ConcurrentDictionary<string, Template> _cache = new();

    [System.Text.RegularExpressions.GeneratedRegex(@"\n{3,}")]
    private static partial System.Text.RegularExpressions.Regex BlankLines();

    public string Render(string templateName, object model)
    {
        var template = _cache.GetOrAdd(templateName, Load);
        return BlankLines().Replace(template.Render(model).Replace("\r\n", "\n"), "\n\n");
    }

    private static Template Load(string templateName)
    {
        var path = Path.Combine(TemplatesDirectory, templateName.Replace('/', Path.DirectorySeparatorChar) + ".sbn");
        var template = Template.Parse(File.ReadAllText(path), path);

        if (template.HasErrors)
        {
            throw new InvalidOperationException(
                $"Template '{templateName}' has errors: {string.Join("; ", template.Messages)}");
        }

        return template;
    }
}
