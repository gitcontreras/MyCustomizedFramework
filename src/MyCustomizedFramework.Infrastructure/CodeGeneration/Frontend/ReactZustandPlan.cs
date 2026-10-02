using MyCustomizedFramework.Application.Abstractions.CodeGeneration;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

internal sealed class ReactZustandPlan : IFrontendFrameworkPlan
{
    private const string Bucket = "Web";

    private static readonly string[] SharedControls =
        ["FormField", "TextInput", "TextAreaInput", "NumberInput", "ToggleInput", "DateTimeInput", "SelectInput", "Alert"];

    public bool Supports(FrontendGenerationOptions options) =>
        options.Framework == "react"
        && options.StateManagement is "zustand"
        && options.Styles == "tailwind";

    public IEnumerable<TemplateOutput> KernelOutputs(FrontendGenerationOptions options)
    {
        yield return new("_shared/kernel/api-error.ts", $"{Bucket}/shared/api/api-error.ts");
        yield return new("_shared/kernel/http-client.ts", $"{Bucket}/shared/api/http-client.ts");

        foreach (var control in SharedControls)
        {
            yield return new($"react/kernel/{control}.tsx", $"{Bucket}/shared/ui/{control}.tsx");
        }
    }

    public IEnumerable<TemplateOutput> FeatureOutputs(FrontendTableModel model, FrontendGenerationOptions options)
    {
        var root = $"{Bucket}/features/{model.FolderName}";
        var entity = model.EntityName;

        yield return new("_shared/feature/model.ts", $"{root}/model/{model.FileStem}.types.ts");
        yield return new("_shared/feature/validation.ts", $"{root}/model/{model.FileStem}.validation.ts");
        yield return new("_shared/feature/api.ts", $"{root}/api/{model.FolderName}.api.ts");
        yield return new("react/feature/store.ts", $"{root}/store/{model.FolderName}.store.ts");
        yield return new("react/feature/List.tsx", $"{root}/components/{entity}List.tsx");
        yield return new("react/feature/Form.tsx", $"{root}/components/{entity}Form.tsx");
        yield return new("react/feature/Detail.tsx", $"{root}/components/{entity}Detail.tsx");
        yield return new("react/feature/ListPage.tsx", $"{root}/pages/{model.PluralName}Page.tsx");
        yield return new("react/feature/CreatePage.tsx", $"{root}/pages/{entity}CreatePage.tsx");
        yield return new("react/feature/EditPage.tsx", $"{root}/pages/{entity}EditPage.tsx");
        yield return new("react/feature/DetailPage.tsx", $"{root}/pages/{entity}DetailPage.tsx");
        yield return new("react/feature/routes.tsx", $"{root}/{model.FolderName}.routes.tsx");
        yield return new("react/feature/index.ts", $"{root}/index.ts");
    }
}
