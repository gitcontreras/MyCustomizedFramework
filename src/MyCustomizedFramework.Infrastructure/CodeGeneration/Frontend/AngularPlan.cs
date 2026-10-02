using MyCustomizedFramework.Application.Abstractions.CodeGeneration;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

/// <summary>Angular plan shared by the Signals and NgRx SignalStore variants; only the store template differs.</summary>
internal sealed class AngularPlan(string stateManagement, string storeTemplate) : IFrontendFrameworkPlan
{
    private const string Bucket = "Web";

    private static readonly string[] SharedControls =
        ["form-field", "text-input", "text-area-input", "number-input", "toggle-input", "date-time-input", "select-input", "alert"];

    public bool Supports(FrontendGenerationOptions options) =>
        options.Framework == "angular"
        && options.StateManagement == stateManagement
        && options.Styles == "tailwind";

    public IEnumerable<TemplateOutput> KernelOutputs(FrontendGenerationOptions options)
    {
        yield return new("_shared/kernel/api-error.ts", $"{Bucket}/shared/api/api-error.ts");
        yield return new("_shared/kernel/http-client.ts", $"{Bucket}/shared/api/http-client.ts");
        yield return new("angular/kernel/form-classes.ts", $"{Bucket}/shared/ui/form-classes.ts");

        foreach (var control in SharedControls)
        {
            yield return new($"angular/kernel/{control}.component.ts", $"{Bucket}/shared/ui/{control}.component.ts");
        }
    }

    public IEnumerable<TemplateOutput> FeatureOutputs(FrontendTableModel model, FrontendGenerationOptions options)
    {
        var root = $"{Bucket}/features/{model.FolderName}";
        var stem = model.FileStem;

        yield return new("_shared/feature/model.ts", $"{root}/model/{stem}.types.ts");
        yield return new("_shared/feature/validation.ts", $"{root}/model/{stem}.validation.ts");
        yield return new("_shared/feature/api.ts", $"{root}/api/{model.FolderName}.api.ts");
        yield return new(storeTemplate, $"{root}/store/{model.FolderName}.store.ts");
        yield return new("angular/feature/list.component.ts", $"{root}/components/{stem}-list.component.ts");
        yield return new("angular/feature/form.component.ts", $"{root}/components/{stem}-form.component.ts");
        yield return new("angular/feature/detail.component.ts", $"{root}/components/{stem}-detail.component.ts");
        yield return new("angular/feature/list.page.ts", $"{root}/pages/{model.FolderName}.page.ts");
        yield return new("angular/feature/create.page.ts", $"{root}/pages/{stem}-create.page.ts");
        yield return new("angular/feature/edit.page.ts", $"{root}/pages/{stem}-edit.page.ts");
        yield return new("angular/feature/detail.page.ts", $"{root}/pages/{stem}-detail.page.ts");
        yield return new("angular/feature/routes.ts", $"{root}/{model.FolderName}.routes.ts");
        yield return new("angular/feature/index.ts", $"{root}/index.ts");
    }
}

