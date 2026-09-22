using MyCustomizedFramework.Application.Abstractions.CodeGeneration;

namespace MyCustomizedFramework.Cli.Writing;

/// <summary>
/// Writes the files GenerateCrudHandler produced directly to disk, using crudgen.config.json's "paths" map
/// to translate each bucket (e.g. "Domain") into a real folder under the target repo. Never overwrites an
/// existing file unless <paramref name="force"/> is true.
/// </summary>
internal static class GeneratedFileWriter
{
    public static WriteResult Write(
        IReadOnlyCollection<GeneratedFile> files,
        IReadOnlyDictionary<string, string> pathMappings,
        string repoRoot,
        bool force)
    {
        var written = new List<string>();
        var skipped = new List<string>();

        foreach (var file in files)
        {
            var segments = file.RelativePath.Split('/', 2);
            if (segments.Length != 2)
            {
                throw new InvalidOperationException(
                    $"Unexpected generated path '{file.RelativePath}': no bucket prefix (expected e.g. 'Domain/File.cs').");
            }

            var bucket = segments[0];
            var remainder = segments[1];

            if (!pathMappings.TryGetValue(bucket, out var mappedFolder))
            {
                throw new InvalidOperationException(
                    $"No path mapping for '{bucket}' in crudgen.config.json. Add a \"{bucket}\" entry under \"paths\".");
            }

            var targetPath = Path.Combine(repoRoot, mappedFolder, remainder.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(targetPath) && !force)
            {
                skipped.Add(targetPath);
                continue;
            }

            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.WriteAllText(targetPath, file.Content);
            written.Add(targetPath);
        }

        return new WriteResult(written, skipped);
    }
}

internal sealed record WriteResult(IReadOnlyList<string> Written, IReadOnlyList<string> Skipped);
