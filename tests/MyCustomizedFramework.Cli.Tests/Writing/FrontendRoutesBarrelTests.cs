using MyCustomizedFramework.Cli.Writing;

namespace MyCustomizedFramework.Cli.Tests.Writing;

public sealed class FrontendRoutesBarrelTests
{
    [Fact]
    public void ListsEveryFeatureWithRoutesFileAndIsIdempotent()
    {
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Touch(root, "products", "products.routes.ts", "{ path: 'products', x }, { path: 'products/new' }");
            Touch(root, "order-items", "order-items.routes.ts", "{ path: '/orderitems' }");
            Touch(root, "empty", "other.txt");

            var path = FrontendRoutesBarrel.Write(root, "angular");
            var first = File.ReadAllText(path!);
            FrontendRoutesBarrel.Write(root, "angular");

            Assert.Contains("import { orderItemsRoutes } from './order-items/order-items.routes';", first);
            Assert.Contains("import { productsRoutes } from './products/products.routes';", first);
            Assert.Contains("...orderItemsRoutes,", first);
            Assert.DoesNotContain("empty", first);
            Assert.Equal(first, File.ReadAllText(path!));

            var nav = File.ReadAllText(Path.Combine(root, "features", FrontendRoutesBarrel.NavFileName));
            Assert.Contains("{ label: 'Order Items', path: '/orderitems' },", nav);
            Assert.Contains("{ label: 'Products', path: '/products' },", nav);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ReturnsNullWhenNoFeatures()
    {
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Assert.Null(FrontendRoutesBarrel.Write(root, "react"));
    }

    private static void Touch(string root, string folder, string file, string content = "")
    {
        var dir = Path.Combine(root, "features", folder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, file), content);
    }
}
