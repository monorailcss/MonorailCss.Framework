using MonorailCss.Variants;
using MonorailCss.Variants.BuiltIn;
using Shouldly;

namespace MonorailCss.Tests.Variants;

public class VariantRegistryTests
{
    [Fact]
    public void Register_ShouldAddVariantToRegistry()
    {
        var registry = new VariantRegistry();
        var variant = new PseudoClassVariant("hover", ":hover", 300);

        registry.Register(variant);

        registry.TryGet("hover", out var retrieved).ShouldBeTrue();
        retrieved.ShouldBe(variant);
    }

    [Fact]
    public void Register_ShouldThrowWhenDuplicateWithoutOverwrite()
    {
        var registry = new VariantRegistry();
        var variant1 = new PseudoClassVariant("hover", ":hover", 300);
        var variant2 = new PseudoClassVariant("hover", ":hover", 310);

        registry.Register(variant1);

        Should.Throw<InvalidOperationException>(() => registry.Register(variant2));
    }

    [Fact]
    public void Register_ShouldReplaceWhenOverwriteTrue()
    {
        var registry = new VariantRegistry();
        var variant1 = new PseudoClassVariant("hover", ":hover", 300);
        var variant2 = new PseudoClassVariant("hover", ":hover", 310);

        registry.Register(variant1);
        registry.Register(variant2, overwrite: true);

        registry.TryGet("hover", out var retrieved).ShouldBeTrue();
        retrieved.ShouldNotBeNull();
        retrieved.Weight.ShouldBe(310);
    }

    [Fact]
    public void GetAll_ShouldReturnVariantsInWeightOrder()
    {
        var registry = new VariantRegistry();
        var dark = new ColorSchemeVariant("dark", 120);
        var hover = new PseudoClassVariant("hover", ":hover", 300);
        var rtl = new DirectionalityVariant("rtl", 100);

        // Register in random order
        registry.Register(hover);
        registry.Register(dark);
        registry.Register(rtl);

        var all = registry.GetAll();
        all.Count.ShouldBe(3);
        all[0].Name.ShouldBe("rtl"); // Weight 100
        all[1].Name.ShouldBe("dark"); // Weight 120
        all[2].Name.ShouldBe("hover"); // Weight 300
    }

    [Fact]
    public void ApplyVariants_ShouldApplyInReverseOrder()
    {
        var registry = new VariantRegistry();
        registry.Register(new PseudoClassVariant("hover", ":hover", 300));
        registry.Register(new PseudoClassVariant("focus", ":focus", 310));

        var variants = new[]
        {
            new VariantToken("hover", null, null, "hover"),
            new VariantToken("focus", null, null, "focus")
        };

        var result = registry.ApplyVariants("bg-red-500", variants);

        // Should apply hover first (leftmost), then focus
        result.Selector.Value.ShouldBe(".bg-red-500:hover:focus");
    }

    [Fact]
    public void ApplyVariants_ShouldHandleAtRuleWrappers()
    {
        var registry = new VariantRegistry();
        registry.Register(new BreakpointVariant("sm", "(min-width: 640px)", 600));
        registry.Register(new PseudoClassVariant("hover", ":hover", 300));

        var variants = new[]
        {
            new VariantToken("sm", null, null, "sm"),
            new VariantToken("hover", null, null, "hover")
        };

        var result = registry.ApplyVariants("bg-blue-500", variants);

        result.Selector.Value.ShouldBe(".bg-blue-500:hover");
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("media");
        result.Wrappers[0].Params.ShouldBe("(min-width: 640px)");
    }

    [Fact]
    public void ApplyVariants_ShouldHandleCompoundVariants()
    {
        var registry = new VariantRegistry();
        registry.Register(new GroupVariant(200));
        registry.Register(new PeerVariant(250));

        var variants = new[]
        {
            new VariantToken("group", null, "hover", "group-hover"),
            new VariantToken("peer", null, "focus", "peer-focus")
        };

        var result = registry.ApplyVariants("text-white", variants);

        // group-hover applied first (leftmost), then peer-focus using modern selector format
        result.Selector.Value.ShouldContain(":is(:where(.group):hover *)");
        result.Selector.Value.ShouldContain(":is(:where(.peer):focus ~ *)");
    }

    [Fact]
    public void GetVariantWeights_ShouldReturnCorrectWeights()
    {
        var registry = new VariantRegistry();
        registry.RegisterBuiltInVariants(MonorailCss.Theme.Theme.CreateWithDefaults());

        var variants = new[]
        {
            new VariantToken("dark", null, null, "dark"),
            new VariantToken("hover", null, null, "hover"),
            new VariantToken("sm", null, null, "sm")
        };

        var weights = registry.GetVariantWeights(variants);

        weights.Length.ShouldBe(3);
        weights[0].ShouldBe(680); // dark (moved to weight 680)
        weights[1].ShouldBe(300); // hover
        weights[2].ShouldBe(600); // sm
    }

    [Fact]
    public void GetVariantWeights_ShouldUseMaxValueForUnknown()
    {
        var registry = new VariantRegistry();

        var variants = new[]
        {
            new VariantToken("unknown", null, null, "unknown")
        };

        var weights = registry.GetVariantWeights(variants);

        weights.Length.ShouldBe(1);
        weights[0].ShouldBe(int.MaxValue);
    }

    [Fact]
    public void RegisterBuiltInVariants_ShouldRegisterAllExpectedVariants()
    {
        var registry = new VariantRegistry();
        registry.RegisterBuiltInVariants(MonorailCss.Theme.Theme.CreateWithDefaults());

        // Test a sample of expected variants
        registry.TryGet("hover", out _).ShouldBeTrue();
        registry.TryGet("focus", out _).ShouldBeTrue();
        registry.TryGet("dark", out _).ShouldBeTrue();
        registry.TryGet("rtl", out _).ShouldBeTrue();
        registry.TryGet("sm", out _).ShouldBeTrue();
        registry.TryGet("group", out _).ShouldBeTrue();
        registry.TryGet("peer", out _).ShouldBeTrue();
        registry.TryGet("before", out _).ShouldBeTrue();
        registry.TryGet("after", out _).ShouldBeTrue();

        // Verify weight ordering
        var all = registry.GetAll();
        var allList = all.ToList();
        var rtlIndex = allList.FindIndex(v => v.Name == "rtl");
        var darkIndex = allList.FindIndex(v => v.Name == "dark");
        var hoverIndex = allList.FindIndex(v => v.Name == "hover");
        var smIndex = allList.FindIndex(v => v.Name == "sm");
        var dataIndex = allList.FindIndex(v => v.Name == "data");

        rtlIndex.ShouldBeLessThan(hoverIndex);
        hoverIndex.ShouldBeLessThan(dataIndex); // data variants come after pseudo-classes
        dataIndex.ShouldBeLessThan(smIndex); // but before breakpoints
        smIndex.ShouldBeLessThan(darkIndex); // dark comes after breakpoints
    }
}