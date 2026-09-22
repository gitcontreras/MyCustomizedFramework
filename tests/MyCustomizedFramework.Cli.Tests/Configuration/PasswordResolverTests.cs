using MyCustomizedFramework.Cli.Configuration;

namespace MyCustomizedFramework.Cli.Tests.Configuration;

public sealed class PasswordResolverTests
{
    [Fact]
    public void ResolveUsesTheFlagValueWhenProvided()
    {
        var resolver = new PasswordResolver(() => throw new InvalidOperationException("Should not prompt."));

        var password = resolver.Resolve("from-flag");

        Assert.Equal("from-flag", password);
    }

    [Fact]
    public void ResolveFallsBackToTheEnvironmentVariableWhenNoFlagIsGiven()
    {
        Environment.SetEnvironmentVariable(PasswordResolver.EnvironmentVariableName, "from-env");
        try
        {
            var resolver = new PasswordResolver(() => throw new InvalidOperationException("Should not prompt."));

            var password = resolver.Resolve(flagValue: null);

            Assert.Equal("from-env", password);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PasswordResolver.EnvironmentVariableName, null);
        }
    }

    [Fact]
    public void ResolvePromptsWhenNeitherFlagNorEnvironmentVariableIsSet()
    {
        Environment.SetEnvironmentVariable(PasswordResolver.EnvironmentVariableName, null);
        var resolver = new PasswordResolver(() => "from-prompt");

        var password = resolver.Resolve(flagValue: null);

        Assert.Equal("from-prompt", password);
    }
}
