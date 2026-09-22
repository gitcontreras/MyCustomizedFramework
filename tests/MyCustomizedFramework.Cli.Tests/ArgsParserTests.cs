namespace MyCustomizedFramework.Cli.Tests;

public sealed class ArgsParserTests
{
    [Fact]
    public void ParseReadsNameValuePairs()
    {
        var result = ArgsParser.Parse(["--engine", "sql", "--server", "localhost"]);

        Assert.Equal("sql", result["engine"]);
        Assert.Equal("localhost", result["server"]);
    }

    [Fact]
    public void ParseTreatsATrailingFlagWithNoValueAsPresentButNull()
    {
        var result = ArgsParser.Parse(["--table", "Student", "--force"]);

        Assert.Equal("Student", result["table"]);
        Assert.True(result.ContainsKey("force"));
        Assert.Null(result["force"]);
    }

    [Fact]
    public void ParseIsCaseInsensitiveForOptionNames()
    {
        var result = ArgsParser.Parse(["--Engine", "sql"]);

        Assert.Equal("sql", result["engine"]);
    }
}
