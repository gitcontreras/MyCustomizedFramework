using MyCustomizedFramework.Application.Abstractions.CodeGeneration;

namespace MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

internal sealed class VuePiniaPlan : IFrontendFrameworkPlan
{
    private const string Bucket = "Web";

    private static readonly string[] SharedControls =
        ["FormField", "TextInput", "TextAreaInput", "NumberInput", "ToggleInput", "DateTimeInput", "SelectInput", "Alert"];

    public bool Supports(FrontendGenerationOptions options) =>
        options.Framework == "vue"
        && options.StateManagement is "pinia"
        && options.Styles == "tailwind";

    public IEnumerable<TemplateOutput> KernelOutputs(FrontendGenerationOptions options)
    {
        yield return new("_shared/kernel/api-error.ts", $"{Bucket}/shared/api/api-error.ts");
        yield return new("_shared/kernel/http-client.ts", $"{Bucket}/shared/api/http-client.ts");
        yield return new("vue/kernel/form-classes.ts", $"{Bucket}/shared/ui/form-classes.ts");

        foreach (var control in SharedControls)
        {
            yield return new($"vue/kernel/{control}.vue", $"{Bucket}/shared/ui/{control}.vue");
        }
    }

    public IEnumerable<TemplateOutput> FeatureOutputs(FrontendTableModel model, FrontendGenerationOptions options)
    {
        var root = $"{Bucket}/features/{model.FolderName}";
        var entity = model.EntityName;

        yield return new("_shared/feature/model.ts", $"{root}/model/{model.FileStem}.types.ts");
        yield return new("_shared/feature/validation.ts", $"{root}/model/{model.FileStem}.validation.ts");
        yield return new("_shared/feature/api.ts", $"{root}/api/{model.FolderName}.api.ts");
        yield return new("vue/feature/store.ts", $"{root}/store/{model.FolderName}.store.ts");
        yield return new("vue/feature/List.vue", $"{root}/components/{entity}List.vue");
        yield return new("vue/feature/Form.vue", $"{root}/components/{entity}Form.vue");
        yield return new("vue/feature/Detail.vue", $"{root}/components/{entity}Detail.vue");
        yield return new("vue/feature/ListView.vue", $"{root}/views/{model.PluralName}View.vue");
        yield return new("vue/feature/CreateView.vue", $"{root}/views/{entity}CreateView.vue");
        yield return new("vue/feature/EditView.vue", $"{root}/views/{entity}EditView.vue");
        yield return new("vue/feature/DetailView.vue", $"{root}/views/{entity}DetailView.vue");
        yield return new("vue/feature/routes.ts", $"{root}/{model.FolderName}.routes.ts");
        yield return new("vue/feature/index.ts", $"{root}/index.ts");
    }
}
