using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Cli.Writing;

namespace MyCustomizedFramework.Cli.Tests.Writing;

public sealed class GeneratedFileWriterTests : IDisposable
{
    private readonly string tempRoot = Path.Combine(Path.GetTempPath(), "crudgen-tests-" + Guid.NewGuid());

    public GeneratedFileWriterTests()
    {
        Directory.CreateDirectory(tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void WriteWritesEachFileUnderItsMappedFolder()
    {
        var files = new[]
        {
            new GeneratedFile("Domain/Student.cs", "class Student;"),
            new GeneratedFile("Application/StudentRequest.cs", "record StudentRequest;")
        };
        var mappings = new Dictionary<string, string> { ["Domain"] = "src/Domain", ["Application"] = "src/Application" };

        var result = GeneratedFileWriter.Write(files, mappings, tempRoot, force: false);

        Assert.Equal(2, result.Written.Count);
        Assert.Empty(result.Skipped);
        Assert.Equal("class Student;", File.ReadAllText(Path.Combine(tempRoot, "src", "Domain", "Student.cs")));
        Assert.Equal("record StudentRequest;", File.ReadAllText(Path.Combine(tempRoot, "src", "Application", "StudentRequest.cs")));
    }

    [Fact]
    public void WriteSkipsAnExistingFileWithoutForce()
    {
        var existingPath = Path.Combine(tempRoot, "src", "Domain", "Student.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(existingPath)!);
        File.WriteAllText(existingPath, "hand-edited content");

        var files = new[] { new GeneratedFile("Domain/Student.cs", "regenerated content") };
        var mappings = new Dictionary<string, string> { ["Domain"] = "src/Domain" };

        var result = GeneratedFileWriter.Write(files, mappings, tempRoot, force: false);

        Assert.Empty(result.Written);
        Assert.Single(result.Skipped);
        Assert.Equal("hand-edited content", File.ReadAllText(existingPath));
    }

    [Fact]
    public void WriteOverwritesAnExistingFileWhenForceIsTrue()
    {
        var existingPath = Path.Combine(tempRoot, "src", "Domain", "Student.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(existingPath)!);
        File.WriteAllText(existingPath, "old content");

        var files = new[] { new GeneratedFile("Domain/Student.cs", "regenerated content") };
        var mappings = new Dictionary<string, string> { ["Domain"] = "src/Domain" };

        var result = GeneratedFileWriter.Write(files, mappings, tempRoot, force: true);

        Assert.Single(result.Written);
        Assert.Empty(result.Skipped);
        Assert.Equal("regenerated content", File.ReadAllText(existingPath));
    }

    [Fact]
    public void WriteThrowsAClearErrorWhenABucketHasNoMapping()
    {
        var files = new[] { new GeneratedFile("Unmapped/Student.cs", "content") };

        var exception = Assert.Throws<InvalidOperationException>(
            () => GeneratedFileWriter.Write(files, new Dictionary<string, string>(), tempRoot, force: false));

        Assert.Contains("Unmapped", exception.Message);
        Assert.Contains("paths", exception.Message);
    }
}
