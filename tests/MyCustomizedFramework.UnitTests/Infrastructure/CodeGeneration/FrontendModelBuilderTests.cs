using MyCustomizedFramework.Application.Abstractions.CodeGeneration;
using MyCustomizedFramework.Domain.SchemaExplorer;
using MyCustomizedFramework.Infrastructure.CodeGeneration.Frontend;

namespace MyCustomizedFramework.UnitTests.Infrastructure.CodeGeneration;

public sealed class FrontendModelBuilderTests
{
    private static readonly FrontendGenerationOptions Options = new("react", "zustand", "tailwind", "/api");

    [Theory]
    [InlineData("int", "number")]
    [InlineData("long", "number")]
    [InlineData("decimal", "number")]
    [InlineData("double?", "number")]
    [InlineData("bool", "boolean")]
    [InlineData("string", "string")]
    [InlineData("Guid", "string")]
    [InlineData("DateTime", "string")]
    public void ToTsTypeMapsCSharpTypes(string csharpType, string expected) =>
        Assert.Equal(expected, FrontendModelBuilder.ToTsType(csharpType));

    [Theory]
    [InlineData("int", 50, "number")]
    [InlineData("bool", null, "checkbox")]
    [InlineData("DateTime", null, "datetime")]
    [InlineData("DateTimeOffset", null, "datetime")]
    [InlineData("TimeSpan", null, "time")]
    [InlineData("byte[]", null, "none")]
    [InlineData("string", 100, "text")]
    [InlineData("string", 255, "text")]
    [InlineData("string", 256, "textarea")]
    [InlineData("string", null, "textarea")]
    public void ControlIsChosenFromTypeAndLength(string csharpType, int? maxLength, string expected)
    {
        var model = Build(TableColumn.Create("Value", csharpType, false, false, 2, maxLength));

        Assert.Equal(expected, model.Fields.Single(field => field.Key == "value").Control);
    }

    [Theory]
    [InlineData("UserPassword", "password")]
    [InlineData("ContactEmail", "email")]
    public void ControlIsChosenFromColumnName(string name, string expected)
    {
        var model = Build(TableColumn.Create(name, "string", false, false, 2, 100));

        Assert.Equal(expected, model.Fields.Single(field => field.ColumnName == name).Control);
    }

    [Fact]
    public void PrimaryKeyIsExcludedFromRequestAndFormFields()
    {
        var model = Build(TableColumn.Create("Name", "string", false, false, 2, 50));

        Assert.Equal("Id", model.PrimaryKey.ColumnName);
        Assert.DoesNotContain(model.RequestFields, field => field.IsPrimaryKey);
        Assert.DoesNotContain(model.FormFields, field => field.IsPrimaryKey);
        Assert.Equal("id", model.PrimaryKey.Key);
    }

    [Fact]
    public void BinaryColumnsAreNotFormOrListFields()
    {
        var model = Build(TableColumn.Create("Photo", "byte[]", true, false, 2));

        Assert.Contains(model.RequestFields, field => field.Key == "photo");
        Assert.DoesNotContain(model.FormFields, field => field.Key == "photo");
        Assert.DoesNotContain(model.ListFields, field => field.Key == "photo");
    }

    [Fact]
    public void LongTextIsExcludedFromListFields()
    {
        var model = Build(TableColumn.Create("Notes", "string", true, false, 2));

        Assert.DoesNotContain(model.ListFields, field => field.Key == "notes");
        Assert.Contains(model.FormFields, field => field.Key == "notes");
    }

    [Fact]
    public void ListFieldsAreCappedAtSixWithKeyFirst()
    {
        var columns = Enumerable.Range(2, 10)
            .Select(i => TableColumn.Create($"Col{i}", "int", false, false, i))
            .ToArray();

        var model = Build(columns);

        Assert.Equal(6, model.ListFields.Count);
        Assert.True(model.ListFields[0].IsPrimaryKey);
    }

    [Fact]
    public void NullabilityDrivesRequiredAndBooleansAreNeverRequired()
    {
        var model = Build(
            TableColumn.Create("Name", "string", false, false, 2, 50),
            TableColumn.Create("Nick", "string", true, false, 3, 50),
            TableColumn.Create("Active", "bool", false, false, 4));

        Assert.True(model.Fields.Single(f => f.Key == "name").IsRequired);
        Assert.False(model.Fields.Single(f => f.Key == "nick").IsRequired);
        Assert.False(model.Fields.Single(f => f.Key == "active").IsRequired);
    }

    [Theory]
    [InlineData("int", "null", "number | null")]
    [InlineData("bool", "false", "boolean")]
    [InlineData("string", "''", "string")]
    public void InitialValueAndFormTypeFollowTsType(string csharpType, string initial, string formType)
    {
        var field = Build(TableColumn.Create("Value", csharpType, false, false, 2, 10))
            .Fields.Single(f => f.Key == "value");

        Assert.Equal(initial, field.InitialValue);
        Assert.Equal(formType, field.FormTsType);
    }

    [Fact]
    public void ForeignKeyBecomesSelectWithRelation()
    {
        var input = new FrontendTableInput(
            "dbo",
            "Orders",
            [
                TableColumn.Create("Id", "int", false, true, 1),
                TableColumn.Create("CustomerId", "int", false, false, 2)
            ],
            [new FrontendRelation("CustomerId", "Customers", "Id", "Name")]);

        var model = FrontendModelBuilder.Build(input, Options);
        var field = model.Fields.Single(f => f.Key == "customerId");

        Assert.Equal("select", field.Control);
        Assert.NotNull(field.Relation);
        Assert.Equal("Customer", field.Relation!.EntityName);
        Assert.Equal("customers", field.Relation.RouteSegment);
        Assert.Equal("id", field.Relation.ValueKey);
        Assert.Equal("name", field.Relation.LabelKey);
        Assert.Equal("number", field.Relation.ValueTsType);
        Assert.True(model.HasRelations);
    }

    [Fact]
    public void NamesAreDerivedFromTableName()
    {
        var model = Build(TableColumn.Create("Name", "string", false, false, 2, 50), "OrderItems");

        Assert.Equal("OrderItem", model.EntityName);
        Assert.Equal("OrderItems", model.PluralName);
        Assert.Equal("order-item", model.FileStem);
        Assert.Equal("order-items", model.FolderName);
        Assert.Equal("orderitems", model.RouteSegment);
        Assert.Equal("/api", model.ApiBaseUrl);
    }

    private static FrontendTableModel Build(params TableColumn[] extra) => Build(extra, "Products");

    private static FrontendTableModel Build(TableColumn extra, string table) => Build([extra], table);

    private static FrontendTableModel Build(TableColumn[] extra, string table)
    {
        var columns = new[] { TableColumn.Create("Id", "int", false, true, 1) }.Concat(extra).ToArray();

        return FrontendModelBuilder.Build(new FrontendTableInput("dbo", table, columns, []), Options);
    }
}
