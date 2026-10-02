using MyCustomizedFramework.Application.Abstractions.CodeGeneration;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

/// <summary>One template rendered to one output path; the path starts with the "Web" bucket the CLI maps to frontend.path.</summary>
internal sealed record TemplateOutput(string Template, string OutputPath);

/// <summary>
/// Strategy per framework/state-management stack: decides which templates run and where their output goes.
/// Adding a stack = one new plan + its templates; the generator itself never changes.
/// </summary>
internal interface IFrontendFrameworkPlan
{
    bool Supports(FrontendGenerationOptions options);

    IEnumerable<TemplateOutput> KernelOutputs(FrontendGenerationOptions options);

    IEnumerable<TemplateOutput> FeatureOutputs(FrontendTableModel model, FrontendGenerationOptions options);
}
