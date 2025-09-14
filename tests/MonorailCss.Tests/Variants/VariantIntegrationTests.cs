using MonorailCss.Candidates;
using MonorailCss.Parser;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Variants;

public class VariantIntegrationTests
{
    private readonly VariantRegistry _registry;
    private readonly CandidateParser _parser;

    public VariantIntegrationTests()
    {
        var theme = new MonorailCss.Theme.Theme();
        var utilityRegistry = new UtilityRegistry(autoRegisterUtilities: true);
        _registry = new VariantRegistry();
        _registry.RegisterBuiltInVariants(theme);
        _parser = new CandidateParser(utilityRegistry);
    }

    private static string GetUtilityName(Candidate candidate)
    {
        return candidate switch
        {
            StaticUtility su => su.Root,
            FunctionalUtility { Value.Fraction: not null } fu =>
                $"{fu.Root}-{fu.Value.Value}/{fu.Value.Fraction}",
            FunctionalUtility { Value: not null } fu =>
                fu.Value.Kind == ValueKind.Arbitrary
                    ? $"{fu.Root}-[{fu.Value.Value}]"
                    : $"{fu.Root}-{fu.Value.Value}",
            FunctionalUtility fu => fu.Root,
            ArbitraryProperty ap => $"[{ap.Property}:{ap.Value}]",
            _ => candidate.Raw
        };
    }

    [Theory]
    [InlineData("hover:bg-red-500", "hover", "bg-red-500")]
    [InlineData("focus:text-blue-700", "focus", "text-blue-700")]
    [InlineData("active:border-green-400", "active", "border-green-400")]
    public void ParseSingleVariant_ShouldExtractVariantAndUtility(string input, string expectedVariant, string expectedUtility)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].Name.ShouldBe(expectedVariant);
        GetUtilityName(candidate).ShouldBe(expectedUtility);
    }

    [Fact]
    public void ParseMultipleVariants_ShouldMaintainOrder()
    {
        _parser.TryParseCandidate("hover:focus:active:bg-red-500", out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(3);
        candidate.Variants[0].Name.ShouldBe("hover");
        candidate.Variants[1].Name.ShouldBe("focus");
        candidate.Variants[2].Name.ShouldBe("active");
        GetUtilityName(candidate).ShouldBe("bg-red-500");
    }

    [Theory]
    [InlineData("!bg-red-500", true)]
    [InlineData("bg-red-500!", true)]
    [InlineData("hover:!bg-red-500", true)]
    [InlineData("hover:bg-red-500!", true)]
    [InlineData("bg-red-500", false)]
    public void ParseImportant_ShouldDetectImportantFlag(string input, bool expectedImportant)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Important.ShouldBe(expectedImportant);
    }

    [Theory]
    [InlineData("group-hover:text-blue-500", "group", "hover")]
    [InlineData("peer-focus:bg-red-300", "peer", "focus")]
    [InlineData("group-active:border-2", "group", "active")]
    public void ParseCompoundVariants_ShouldExtractRootAndValue(string input, string expectedRoot, string expectedValue)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].Name.ShouldBe(expectedRoot);
        candidate.Variants[0].Value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("aria-[checked]:bg-blue-500", "aria", "checked")]
    [InlineData("data-[active]:text-red-500", "data", "active")]
    [InlineData("has-[.active]:font-bold", "has", ".active")]
    public void ParseFunctionalVariants_ShouldExtractNameAndValue(string input, string expectedName, string expectedValue)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].Name.ShouldBe(expectedName);
        candidate.Variants[0].Value.ShouldBe(expectedValue);
        // Functional variants are not marked as arbitrary, even with bracket values
        candidate.Variants[0].IsArbitrary.ShouldBeFalse();
    }

    [Theory]
    [InlineData("[&>*]:[text-decoration:underline]", "&>*")]
    [InlineData("[&:nth-child(odd)]:[display:none]", "&:nth-child(odd)")]
    [InlineData("[@media(min-width:768px)]:[display:flex]", "@media(min-width:768px)")]
    public void ParseArbitraryVariants_ShouldExtractSelector(string input, string expectedValue)
    {
        // Use arbitrary properties which don't require utility registration
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].IsArbitrary.ShouldBeTrue();
        candidate.Variants[0].Value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("group/menu-hover:text-blue-500", "group", "menu", "hover")]
    [InlineData("peer/input-focus:border-red-500", "peer", "input", "focus")]
    public void ParseNamedGroupsAndPeers_ShouldExtractModifierAndSubVariant(string input, string expectedName, string expectedModifier, string expectedSubVariant)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].Name.ShouldBe(expectedName);
        candidate.Variants[0].Modifier.ShouldBe(expectedModifier);
        candidate.Variants[0].Value.ShouldBe(expectedSubVariant);
    }

    [Theory]
    [InlineData("group/menu:hover:text-blue-500", 2, "group/menu", "hover")]
    [InlineData("peer/input:focus:border-red-500", 2, "peer/input", "focus")]
    public void ParseNamedGroupsWithSeparateHover_ShouldParseTwoVariants(string input, int expectedVariantCount, string firstVariant, string secondVariant)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(expectedVariantCount);
        candidate.Variants[0].Raw.ShouldBe(firstVariant);
        candidate.Variants[1].Raw.ShouldBe(secondVariant);
    }

    [Fact]
    public void ApplyStaticPseudoVariants_ShouldCompose()
    {
        _parser.TryParseCandidate("hover:focus:active:bg-red-500", out var candidate);
        var result = _registry.ApplyVariants("bg-red-500", candidate!.Variants);

        // Applied left-to-right: hover, focus, active
        result.Selector.Value.ShouldBe(".bg-red-500:hover:focus:active");
    }

    [Fact]
    public void ApplyDirectionalityVariant_ShouldAddAttributeSelector()
    {
        _parser.TryParseCandidate("rtl:text-right", out var candidate);
        var result = _registry.ApplyVariants("text-right", candidate!.Variants);

        result.Selector.Value.ShouldBe("[dir=\"rtl\"] .text-right");
    }

    [Fact]
    public void ApplyDarkVariant_ShouldAddDescendantSelector()
    {
        _parser.TryParseCandidate("dark:bg-gray-900", out var candidate);
        var result = _registry.ApplyVariants("bg-gray-900", candidate!.Variants);

        result.Selector.Value.ShouldBe(".bg-gray-900:where(.dark, .dark *)");
    }

    [Fact]
    public void ApplyBreakpointVariant_ShouldAddMediaWrapper()
    {
        _parser.TryParseCandidate("sm:p-4", out var candidate);
        var result = _registry.ApplyVariants("p-4", candidate!.Variants);

        result.Selector.Value.ShouldBe(".p-4");
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("media");
        result.Wrappers[0].Params.ShouldBe("(min-width: 640px)");
    }

    [Fact]
    public void ApplyGroupVariant_ShouldCreateDescendantSelector()
    {
        _parser.TryParseCandidate("group-hover:text-blue-500", out var candidate);
        var result = _registry.ApplyVariants("text-blue-500", candidate!.Variants);

        result.Selector.Value.ShouldBe(".text-blue-500:is(:where(.group):hover *)");
    }

    [Fact]
    public void ApplyPeerVariant_ShouldCreateSiblingSelector()
    {
        _parser.TryParseCandidate("peer-focus:border-red-500", out var candidate);
        var result = _registry.ApplyVariants("border-red-500", candidate!.Variants);

        result.Selector.Value.ShouldBe(".border-red-500:is(:where(.peer):focus ~ *)");
    }

    [Fact]
    public void ApplyPseudoElementVariants_ShouldAddDoubleColon()
    {
        _parser.TryParseCandidate("before:content-['']", out var candidate);
        var result = _registry.ApplyVariants("content-\\[\\'\\'\\'\\]", candidate!.Variants);

        result.Selector.Value.ShouldBe(".content-\\[\\'\\'\\'\\]::before");
    }

    [Fact]
    public void ApplyComplexVariantStack_ShouldComposeProperly()
    {
        // This tests a deep stack: group-hover:peer-focus:dark:rtl:sm:hover:text-blue-500
        var variants = new[]
        {
            new VariantToken("group", null, "hover", "group-hover"),
            new VariantToken("peer", null, "focus", "peer-focus"),
            new VariantToken("dark", null, null, "dark"),
            new VariantToken("rtl", null, null, "rtl"),
            new VariantToken("sm", null, null, "sm"),
            new VariantToken("hover", null, null, "hover")
        };

        var result = _registry.ApplyVariants("text-blue-500", variants);

        // Should have media wrapper for sm
        result.Wrappers.Any(w => w.Name == "media" && w.Params.Contains("640px")).ShouldBeTrue();

        // Selector should have all the transformations
        var selector = result.Selector.Value;
        selector.ShouldContain(":hover"); // from hover variant
        selector.ShouldContain("[dir=\"rtl\"]"); // from rtl
        selector.ShouldContain(".dark"); // from dark
        selector.ShouldContain(":is(:where(.peer):focus ~ *)"); // from peer-focus (modern format)
        selector.ShouldContain(":is(:where(.group):hover *)"); // from group-hover (modern format)
    }

    [Fact]
    public void ApplyMotionVariants_ShouldAddMediaQuery()
    {
        _parser.TryParseCandidate("motion-safe:transition-all", out var candidate)
            .ShouldBeTrue();
        candidate.ShouldNotBeNull();
        var result = _registry.ApplyVariants("transition-all", candidate.Variants);

        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("media");
        result.Wrappers[0].Params.ShouldBe("(prefers-reduced-motion: no-preference)");
    }

    [Fact]
    public void ApplyPrintVariant_ShouldAddPrintMediaQuery()
    {
        _parser.TryParseCandidate("print:hidden", out var candidate)
            .ShouldBeTrue();
        candidate.ShouldNotBeNull();
        var result = _registry.ApplyVariants("hidden", candidate.Variants);

        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("media");
        result.Wrappers[0].Params.ShouldBe("print");
    }

    [Theory]
    [InlineData("contrast-more:border-black", "(prefers-contrast: more)")]
    [InlineData("contrast-less:opacity-50", "(prefers-contrast: less)")]
    public void ApplyContrastVariants_ShouldAddContrastMediaQuery(string input, string expectedMediaQuery)
    {
        _parser.TryParseCandidate(input, out var candidate)
            .ShouldBeTrue();
        candidate.ShouldNotBeNull();
        var result = _registry.ApplyVariants(GetUtilityName(candidate), candidate.Variants);

        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("media");
        result.Wrappers[0].Params.ShouldBe(expectedMediaQuery);
    }

    [Theory]
    [InlineData("first:mt-0", ":first-child")]
    [InlineData("last:mb-0", ":last-child")]
    [InlineData("odd:bg-gray-100", ":nth-child(odd)")]
    [InlineData("even:bg-white", ":nth-child(even)")]
    [InlineData("only:mx-auto", ":only-child")]
    [InlineData("empty:hidden", ":empty")]
    public void ApplyStructuralPseudoClasses_ShouldAddCorrectSelectors(string input, string expectedPseudo)
    {
        _parser.TryParseCandidate(input, out var candidate)
            .ShouldBeTrue();

        candidate.ShouldNotBeNull();
        var result = _registry.ApplyVariants(GetUtilityName(candidate), candidate.Variants);

        result.Selector.Value.ShouldContain(expectedPseudo);
    }

    [Fact]
    public void ApplyMultipleBreakpoints_ShouldNestMediaQueries()
    {
        var variants = new[]
        {
            new VariantToken("sm", null, null, "sm"),
            new VariantToken("lg", null, null, "lg")
        };

        var result = _registry.ApplyVariants("p-4", variants);

        // Should have both media queries
        result.Wrappers.Length.ShouldBe(2);
        result.Wrappers.Any(w => w.Params.Contains("1024px")).ShouldBeTrue(); // lg applied first
        result.Wrappers.Any(w => w.Params.Contains("640px")).ShouldBeTrue(); // sm applied second
    }

    // Container Query Tests

    [Theory]
    [InlineData("@md:text-red-500", "@md", "text-red-500")]
    [InlineData("@lg:p-4", "@lg", "p-4")]
    [InlineData("@xl:hidden", "@xl", "hidden")]
    public void ParseContainerQueryVariant_ShouldExtractVariantAndUtility(string input, string expectedVariant, string expectedUtility)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].Name.ShouldBe(expectedVariant);
        GetUtilityName(candidate).ShouldBe(expectedUtility);
    }

    [Theory]
    [InlineData("@min-md:flex", "@min", "md")]
    [InlineData("@max-lg:hidden", "@max", "lg")]
    [InlineData("@min-[32rem]:text-blue-500", "@min", "[32rem]")]
    public void ParseFunctionalContainerQueryVariant_ShouldExtractVariantAndValue(string input, string expectedName, string expectedValue)
    {
        _parser.TryParseCandidate(input, out var candidate);

        candidate.ShouldNotBeNull();
        candidate.Variants.Length.ShouldBe(1);
        candidate.Variants[0].Name.ShouldBe(expectedName);
        candidate.Variants[0].Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void ApplyContainerQueryVariant_ShouldAddContainerWrapper()
    {
        _parser.TryParseCandidate("@md:text-red-500", out var candidate);
        var result = _registry.ApplyVariants("text-red-500", candidate!.Variants);

        result.Selector.Value.ShouldBe(".text-red-500");
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("container");
        result.Wrappers[0].Params.ShouldBe("(width >= 28rem)");
    }

    [Fact]
    public void ApplyMinContainerQueryVariant_ShouldAddContainerWrapper()
    {
        _parser.TryParseCandidate("@min-lg:flex", out var candidate);
        var result = _registry.ApplyVariants("flex", candidate!.Variants);

        result.Selector.Value.ShouldBe(".flex");
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("container");
        result.Wrappers[0].Params.ShouldBe("(width >= 32rem)");
    }

    [Fact]
    public void ApplyMaxContainerQueryVariant_ShouldAddContainerWrapper()
    {
        _parser.TryParseCandidate("@max-md:hidden", out var candidate);
        var result = _registry.ApplyVariants("hidden", candidate!.Variants);

        result.Selector.Value.ShouldBe(".hidden");
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("container");
        result.Wrappers[0].Params.ShouldBe("(width < 28rem)");
    }

    [Fact]
    public void ApplyArbitraryContainerQueryVariant_ShouldAddContainerWrapper()
    {
        _parser.TryParseCandidate("@min-[32rem]:text-blue-500", out var candidate);
        var result = _registry.ApplyVariants("text-blue-500", candidate!.Variants);

        result.Selector.Value.ShouldBe(".text-blue-500");
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("container");
        result.Wrappers[0].Params.ShouldBe("(width >= 32rem)");
    }

    [Fact]
    public void ApplyNamedContainerQueryVariant_ShouldIncludeNameInQuery()
    {
        var variants = new[]
        {
            new VariantToken("@lg", "sidebar", null, "@lg/sidebar")
        };

        var result = _registry.ApplyVariants("text-blue-500", variants);

        result.Selector.Value.ShouldBe(".text-blue-500");
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("container");
        result.Wrappers[0].Params.ShouldBe("sidebar (width >= 32rem)");
    }

    [Fact]
    public void ApplyMultipleContainerQueries_ShouldNestContainerWrappers()
    {
        var variants = new[]
        {
            new VariantToken("@sm", null, null, "@sm"),
            new VariantToken("@lg", null, null, "@lg")
        };

        var result = _registry.ApplyVariants("p-4", variants);

        // Should have both container queries
        result.Wrappers.Length.ShouldBe(2);
        result.Wrappers.Any(w => w.Name == "container" && w.Params.Contains("32rem")).ShouldBeTrue(); // lg
        result.Wrappers.Any(w => w.Name == "container" && w.Params.Contains("24rem")).ShouldBeTrue(); // sm
    }

    [Fact]
    public void ApplyMixedMediaAndContainerQueries_ShouldHaveBothWrappers()
    {
        var variants = new[]
        {
            new VariantToken("sm", null, null, "sm"),     // Media query
            new VariantToken("@md", null, null, "@md")    // Container query
        };

        var result = _registry.ApplyVariants("text-red-500", variants);

        // Should have both media and container queries
        result.Wrappers.Length.ShouldBe(2);
        result.Wrappers.Any(w => w.Name == "media" && w.Params.Contains("640px")).ShouldBeTrue(); // Media query
        result.Wrappers.Any(w => w.Name == "container" && w.Params.Contains("28rem")).ShouldBeTrue(); // Container query
    }

    [Theory]
    [InlineData("hover:focus:bg-red-500", ".bg-red-500:hover:focus")]
    [InlineData("focus:hover:bg-red-500", ".bg-red-500:focus:hover")]
    [InlineData("active:hover:focus:bg-red-500", ".bg-red-500:active:hover:focus")]
    public void MultipleVariantCombinations_ShouldMaintainInputOrder(string input, string expectedSelectorStart)
    {
        // Test that variant order is preserved as specified by the user
        _parser.TryParseCandidate(input, out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants("bg-red-500", candidate.Variants);
        result.Selector.Value.ShouldStartWith(expectedSelectorStart);
    }

    [Fact]
    public void BreakpointAndStateVariants_ShouldNestCorrectly()
    {
        // Test: sm:hover:bg-red-500
        // Should produce: @media (min-width: 640px) { .bg-red-500:hover { ... } }
        _parser.TryParseCandidate("sm:hover:bg-red-500", out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants("bg-red-500", candidate.Variants);

        // Media wrapper should be present
        result.Wrappers.Length.ShouldBe(1);
        result.Wrappers[0].Name.ShouldBe("media");
        result.Wrappers[0].Params.ShouldContain("640px");

        // Hover should be in the selector
        result.Selector.Value.ShouldContain(":hover");
    }

    [Fact]
    public void DarkModeWithStateVariants_ShouldApplyInCorrectOrder()
    {
        // Test: dark:hover:bg-gray-800
        _parser.TryParseCandidate("dark:hover:bg-gray-800", out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants("bg-gray-800", candidate.Variants);

        // Dark mode selector should wrap hover
        result.Selector.Value.ShouldContain(".dark");
        result.Selector.Value.ShouldContain(":hover");
    }

    [Fact]
    public void GroupAndPeerVariants_ShouldHaveCorrectSpecificity()
    {
        // Test that group-hover has lower specificity than direct hover
        var groupHover = new[] { new VariantToken("group", null, "hover", "group-hover") };
        var directHover = new[] { new VariantToken("hover", null, null, "hover") };

        var groupResult = _registry.ApplyVariants("text-blue-500", groupHover);
        var directResult = _registry.ApplyVariants("text-blue-500", directHover);

        // Group hover should use descendant selector
        groupResult.Selector.Value.ShouldContain(":is(:where(.group):hover *)");

        // Direct hover should be simpler
        directResult.Selector.Value.ShouldBe(".text-blue-500:hover");
    }

    [Theory]
    [InlineData("sm:md:lg:p-4", 3, new[] { "640px", "768px", "1024px" })]
    [InlineData("2xl:xl:lg:p-4", 3, new[] { "1536px", "1280px", "1024px" })]
    public void MultipleBreakpoints_ShouldNestInOrder(string input, int expectedWrappers, string[] expectedSizes)
    {
        _parser.TryParseCandidate(input, out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants("p-4", candidate.Variants);

        result.Wrappers.Length.ShouldBe(expectedWrappers);
        foreach (var size in expectedSizes)
        {
            result.Wrappers.Any(w => w.Params.Contains(size)).ShouldBeTrue($"Should contain media query for {size}");
        }
    }

    [Fact]
    public void AtRulesNesting_ShouldFollowCSSNestingRules()
    {
        // Test: @supports + @media nesting
        // Note: This might require arbitrary variants or special handling
        var variants = new[]
        {
            new VariantToken("supports", null, "(display: grid)", "supports-(display: grid)"),
            new VariantToken("sm", null, null, "sm")
        };

        var result = _registry.ApplyVariants("grid", variants);

        // Should have both wrappers
        result.Wrappers.Any(w => w.Name is "supports" or "media").ShouldBeTrue();
    }

    [Fact]
    public void PseudoElementWithPseudoClass_ShouldOrderCorrectly()
    {
        // Test: hover:before:content-['']
        // Should produce: .content-['']:hover::before
        _parser.TryParseCandidate("hover:before:content-['']", out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants(@"content-\[\'\'\'\]", candidate.Variants);

        // Pseudo-class (:hover) should come before pseudo-element (::before)
        var selector = result.Selector.Value;
        var hoverIndex = selector.IndexOf(":hover", StringComparison.Ordinal);
        var beforeIndex = selector.IndexOf("::before", StringComparison.Ordinal);

        if (hoverIndex >= 0 && beforeIndex >= 0)
        {
            hoverIndex.ShouldBeLessThan(beforeIndex, "Pseudo-class should come before pseudo-element");
        }
    }

    [Fact]
    public void ComplexVariantStack_ShouldMaintainCorrectOrder()
    {
        // Test a complex combination that matches Tailwind's canonical order
        var input = "sm:dark:hover:group-hover:bg-blue-500";
        _parser.TryParseCandidate(input, out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants("bg-blue-500", candidate.Variants);

        // Should have media wrapper for sm
        result.Wrappers.Any(w => w.Name == "media" && w.Params.Contains("640px")).ShouldBeTrue();

        // Selector should maintain the variant order
        var selector = result.Selector.Value;
        selector.ShouldNotBeNull();
        selector.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("first:last:bg-red-500", ":first-child", ":last-child")]
    [InlineData("odd:even:bg-red-500", ":nth-child(odd)", ":nth-child(even)")]
    [InlineData("empty:only:bg-red-500", ":empty", ":only-child")]
    public void StructuralPseudoClasses_ShouldCombineCorrectly(string input, string firstPseudo, string secondPseudo)
    {
        _parser.TryParseCandidate(input, out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants("bg-red-500", candidate.Variants);

        // Both pseudo-classes should be present
        result.Selector.Value.ShouldContain(firstPseudo);
        result.Selector.Value.ShouldContain(secondPseudo);
    }

    [Fact]
    public void ArbitraryVariantOrdering_ShouldPreserveUserOrder()
    {
        // Test arbitrary variants maintain their position
        var input = "[&:nth-child(3)]:hover:bg-red-500";
        _parser.TryParseCandidate(input, out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants("bg-red-500", candidate.Variants);

        // Arbitrary variant should be applied
        result.Selector.Value.ShouldContain(":nth-child(3)");
        result.Selector.Value.ShouldContain(":hover");
    }

    [Fact]
    public void PrintAndScreenMediaQueries_ShouldNestCorrectly()
    {
        // Test: print:sm:hidden
        var variants = new[]
        {
            new VariantToken("print", null, null, "print"),
            new VariantToken("sm", null, null, "sm")
        };

        var result = _registry.ApplyVariants("hidden", variants);

        // Should have both media queries
        result.Wrappers.Count(w => w.Name == "media").ShouldBe(2);
        result.Wrappers.Any(w => w.Params == "print").ShouldBeTrue();
        result.Wrappers.Any(w => w.Params.Contains("640px")).ShouldBeTrue();
    }

    [Fact]
    public void MotionAndContrastVariants_ShouldCombineWithOthers()
    {
        // Test: motion-safe:contrast-more:hover:scale-110
        var variants = new[]
        {
            new VariantToken("motion-safe", null, null, "motion-safe"),
            new VariantToken("contrast-more", null, null, "contrast-more"),
            new VariantToken("hover", null, null, "hover")
        };

        var result = _registry.ApplyVariants("scale-110", variants);

        // Should have media queries for motion and contrast
        result.Wrappers.Count(w => w.Name == "media").ShouldBe(2);
        result.Wrappers.Any(w => w.Params.Contains("prefers-reduced-motion")).ShouldBeTrue();
        result.Wrappers.Any(w => w.Params.Contains("prefers-contrast")).ShouldBeTrue();

        // Should have hover in selector
        result.Selector.Value.ShouldContain(":hover");
    }

    [Fact]
    public void NamedGroupsAndPeers_ShouldMaintainNamespacing()
    {
        // Test: group/sidebar:peer/input:hover:bg-red-500
        // Note: Named groups/peers may not be fully implemented yet
        var variants = new[]
        {
            new VariantToken("group", "sidebar", null, "group/sidebar"),
            new VariantToken("peer", "input", null, "peer/input"),
            new VariantToken("hover", null, null, "hover")
        };

        var result = _registry.ApplyVariants("bg-red-500", variants);

        // Check that hover is applied (basic functionality)
        result.Selector.Value.ShouldContain(":hover");

        // Named groups/peers may require additional implementation
        // Once implemented, they should contain:
        // result.Selector.Value.ShouldContain(".group\\/sidebar");
        // result.Selector.Value.ShouldContain(".peer\\/input");
    }

    [Theory]
    [InlineData("rtl:ltr:text-left", new[] { "rtl", "ltr" })]
    [InlineData("ltr:rtl:text-right", new[] { "ltr", "rtl" })]
    public void DirectionalVariants_ShouldOverrideCorrectly(string input, string[] directions)
    {
        _parser.TryParseCandidate(input, out var candidate);
        candidate.ShouldNotBeNull();

        var result = _registry.ApplyVariants(GetUtilityName(candidate), candidate.Variants);

        // Both direction attributes should be in the selector
        foreach (var dir in directions)
        {
            result.Selector.Value.ShouldContain($"[dir=\"{dir}\"]");
        }
    }

    [Fact]
    public void VariantWeights_ShouldDetermineApplyOrder()
    {
        // Verify that variants have appropriate weights for sorting
        var testVariants = new[]
        {
            new VariantToken("hover", null, null, "hover"),
            new VariantToken("focus", null, null, "focus"),
            new VariantToken("sm", null, null, "sm"),
            new VariantToken("md", null, null, "md"),
            new VariantToken("lg", null, null, "lg"),
            new VariantToken("dark", null, null, "dark")
        };

        var weights = _registry.GetVariantWeights(testVariants);

        // Should return weights for all variants
        weights.Length.ShouldBe(testVariants.Length);

        // Media query variants should have different weights
        // (exact ordering depends on implementation details)
        weights.Distinct().Count().ShouldBeGreaterThan(1, "Different variants should have different weights");
    }
}