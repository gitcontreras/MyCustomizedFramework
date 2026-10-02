using MyCustomizedFramework.Cli.Configuration;

namespace MyCustomizedFramework.Cli.Tests.Configuration;

public sealed class FrontendOptionsResolverTests
{
    private static readonly IReadOnlyDictionary<string, string?> NoFlags = new Dictionary<string, string?>();

    [Fact]
    public void DefaultsToReactZustandTailwind()
    {
        var options = FrontendOptionsResolver.Resolve(null, NoFlags, out var error);

        Assert.Null(error);
        Assert.Equal(new FrontendOptions("react", "zustand", "frontend/src", "tailwind", "/api"), options);
    }

    [Theory]
    [InlineData("vue", "pinia")]
    [InlineData("angular", "signals")]
    public void DefaultStateIsFirstAllowedForFramework(string framework, string expectedState)
    {
        var options = FrontendOptionsResolver.Resolve(new FrontendConfig { Framework = framework }, NoFlags, out _);

        Assert.Equal(expectedState, options!.StateManagement);
    }

    [Fact]
    public void FlagsOverrideConfig()
    {
        var config = new FrontendConfig { Framework = "react", StateManagement = "zustand" };
        var flags = new Dictionary<string, string?> { ["frontend"] = "angular", ["state"] = "ngrx" };

        var options = FrontendOptionsResolver.Resolve(config, flags, out var error);

        Assert.Null(error);
        Assert.Equal("angular", options!.Framework);
        Assert.Equal("ngrx", options.StateManagement);
    }

    [Fact]
    public void ValuesAreNormalizedToLowercase()
    {
        var options = FrontendOptionsResolver.Resolve(new FrontendConfig { Framework = " Vue ", StateManagement = "PINIA" }, NoFlags, out _);

        Assert.Equal("vue", options!.Framework);
        Assert.Equal("pinia", options.StateManagement);
    }

    [Theory]
    [InlineData("react", "pinia")]
    [InlineData("vue", "ngrx")]
    [InlineData("angular", "zustand")]
    [InlineData("react", "none")]
    public void RejectsStateNotValidForFramework(string framework, string state)
    {
        var options = FrontendOptionsResolver.Resolve(
            new FrontendConfig { Framework = framework, StateManagement = state }, NoFlags, out var error);

        Assert.Null(options);
        Assert.NotNull(error);
    }

    [Fact]
    public void RejectsUnknownFrameworkAndStyles()
    {
        Assert.Null(FrontendOptionsResolver.Resolve(new FrontendConfig { Framework = "svelte" }, NoFlags, out var frameworkError));
        Assert.NotNull(frameworkError);

        Assert.Null(FrontendOptionsResolver.Resolve(new FrontendConfig { Styles = "css-modules" }, NoFlags, out var stylesError));
        Assert.NotNull(stylesError);
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("a/../../b")]
    [InlineData("C:\\abs")]
    public void RejectsUnsafePaths(string path)
    {
        var options = FrontendOptionsResolver.Resolve(new FrontendConfig { Path = path }, NoFlags, out var error);

        Assert.Null(options);
        Assert.NotNull(error);
    }

    [Fact]
    public void ApiBaseUrlIsTrimmedOfTrailingSlash()
    {
        var options = FrontendOptionsResolver.Resolve(new FrontendConfig { ApiBaseUrl = " https://x.test/api/ " }, NoFlags, out _);

        Assert.Equal("https://x.test/api", options!.ApiBaseUrl);
    }
}
