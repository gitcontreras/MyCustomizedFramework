using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Domain.Common;

namespace MyCustomizedFramework.Application.FrontendGeneration;

/// <summary>Produces the once-per-project shared files (HTTP client, error type, form controls). No database needed.</summary>
public sealed class GenerateFrontendKernelHandler(IFrontendFileGenerator frontendFileGenerator)
{
    public Result<IReadOnlyCollection<GeneratedFile>> Handle(FrontendGenerationOptions options)
    {
        var unsupported = frontendFileGenerator.GetUnsupportedReason(options);

        return unsupported is null
            ? Result.Success(frontendFileGenerator.GenerateKernel(options))
            : Result.Failure<IReadOnlyCollection<GeneratedFile>>(Error.Validation("Frontend.UnsupportedStack", unsupported));
    }
}
