using MonorailCss.Parser;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Variants;

public class SupportsVariantTests
{
    private readonly CssFramework _cssFramework;
    private readonly CandidateParser _parser;

    public SupportsVariantTests()
    {
        _cssFramework = new CssFramework();
        var theme = new MonorailCss.Theme.Theme();
        var utilityRegistry = new UtilityRegistry(autoRegisterUtilities: true);
        var registry = new VariantRegistry();
        registry.RegisterBuiltInVariants(theme);
        _parser = new CandidateParser(utilityRegistry);
    }

    [Fact]
    public void SupportsVariant_WithBackdropFilter_GeneratesCorrectCss()
    {
        var input = "supports-[backdrop-filter]:bg-white/60";
        var generated = _cssFramework.Process(input);

        generated.ShouldContain("@supports (backdrop-filter: var(--tw))");
        generated.ShouldContain("background-color: color-mix(in oklab, var(--color-white) 60%, transparent)");
    }

    [Fact]
    public void SupportsVariant_WithPropertyValue_GeneratesCorrectCss()
    {
        var input = "supports-[display:flex]:block";
        var generated = _cssFramework.Process(input);

        generated.ShouldContain("@supports (display:flex)");
        generated.ShouldContain("display: block");
    }

    [Fact]
    public void SupportsVariant_WithNotFunction_GeneratesCorrectCss()
    {
        var input = "supports-[not(display:flex)]:hidden";
        var generated = _cssFramework.Process(input);

        generated.ShouldContain("@supports not (display:flex)");
        generated.ShouldContain("display: none");
    }

    [Fact]
    public void SupportsVariant_WithDarkVariant_OrdersCorrectly()
    {
        // The base supports variant should come first, then dark:supports for proper specificity
        var input = "supports-[backdrop-filter]:bg-white/60 dark:supports-[backdrop-filter]:bg-black/60";
        var generated = _cssFramework.Process(input);

        // Find the positions of both rules
        var supportsOnlyIndex = generated.IndexOf(".supports-\\[backdrop-filter\\]\\:bg-white\\/60", StringComparison.Ordinal);
        var darkSupportsIndex = generated.IndexOf(".dark\\:supports-\\[backdrop-filter\\]\\:bg-black\\/60", StringComparison.Ordinal);

        // Assert that supports-only comes before dark:supports
        supportsOnlyIndex.ShouldBeGreaterThan(-1, "supports-[backdrop-filter]:bg-white/60 should be in output");
        darkSupportsIndex.ShouldBeGreaterThan(-1, "dark:supports-[backdrop-filter]:bg-black/60 should be in output");
        supportsOnlyIndex.ShouldBeLessThan(darkSupportsIndex,
            $"supports-[backdrop-filter]:bg-white/60 should come before dark:supports-[backdrop-filter]:bg-black/60.\n" +
            $"Found at positions: {supportsOnlyIndex} and {darkSupportsIndex}\n" +
            $"Generated CSS:\n{generated}");
    }

    [Fact]
    public void SupportsVariant_ComplexOrdering_MaintainsCorrectOrder()
    {
        // Test multiple variants to ensure supports has correct weight
        var input = "hover:bg-red-500 supports-[display:grid]:bg-blue-500 dark:bg-green-500";
        var generated = _cssFramework.Process(input);

        // Order should be: hover (300), supports (400), dark (680)
        var hoverIndex = generated.IndexOf(".hover\\:bg-red-500:hover", StringComparison.Ordinal);
        var supportsIndex = generated.IndexOf(".supports-\\[display\\:grid\\]\\:bg-blue-500", StringComparison.Ordinal);
        var darkIndex = generated.IndexOf(".dark\\:bg-green-500", StringComparison.Ordinal);

        hoverIndex.ShouldBeLessThan(supportsIndex, "hover should come before supports");
        supportsIndex.ShouldBeLessThan(darkIndex, "supports should come before dark");
    }

    [Fact]
    public void ParseSupportsVariant_ShouldExtractNameAndValue()
    {
        _parser.TryParseCandidate("supports-[display:flex]:block", out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].Name.ShouldBe("supports");
        candidate.Variants[0].Value.ShouldBe("display:flex");
    }
}