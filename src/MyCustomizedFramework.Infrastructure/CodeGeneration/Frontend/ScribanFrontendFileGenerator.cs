using MyCustomizedFramework.Application.Abstractions.CodeGeneration;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

internal sealed class ScribanFrontendFileGenerator : IFrontendFileGenerator
{
    private static readonly IFrontendFrameworkPlan[] Plans = [new ReactZustandPlan(), new VuePiniaPlan(), new AngularPlan("signals", "angular/feature/store.ts"), new AngularPlan("ngrx", "angular-ngrx/feature/store.ts")];

    private readonly FrontendTemplateCatalog _catalog = new();

    public string? GetUnsupportedReason(FrontendGenerationOptions options) =>
        Plans.Any(plan => plan.Supports(options))
            ? null
            : $"The '{options.Framework}' + '{options.StateManagement}' + '{options.Styles}' stack is not available yet. "
              + "Currently supported: react + zustand + tailwind, vue + pinia + tailwind, angular + signals|ngrx + tailwind.";

    public IReadOnlyCollection<GeneratedFile> GenerateKernel(FrontendGenerationOptions options)
    {
        var plan = Resolve(options);
        var model = new FrontendKernelModel { ApiBaseUrl = options.ApiBaseUrl };

        return plan.KernelOutputs(options)
            .Select(output => new GeneratedFile(output.OutputPath, _catalog.Render(output.Template, model)))
            .ToArray();
    }

    public IReadOnlyCollection<GeneratedFile> Generate(FrontendTableInput table, FrontendGenerationOptions options)
    {
        var plan = Resolve(options);
        var model = FrontendModelBuilder.Build(table, options);

        return plan.FeatureOutputs(model, options)
            .Select(output => new GeneratedFile(output.OutputPath, _catalog.Render(output.Template, model)))
            .ToArray();
    }

    private static IFrontendFrameworkPlan Resolve(FrontendGenerationOptions options) =>
        Plans.FirstOrDefault(plan => plan.Supports(options))
        ?? throw new InvalidOperationException("Unsupported frontend stack; call GetUnsupportedReason first.");
}

internal sealed class FrontendKernelModel
{
    public required string ApiBaseUrl { get; init; }
}



