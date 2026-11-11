using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Tests for <see cref="CssFramework.GetSuggestedClassNames"/>, the class-name list behind
/// editor autocomplete: names must come from the framework's own theme and every one must compile.
/// </summary>
public class SuggestedClassNamesTests
{
    private static readonly Lazy<IReadOnlyList<string>> DefaultNames = new(() => new CssFramework().GetSuggestedClassNames().ToList());

    [Theory]
    // Static utilities.
    [InlineData("flex")]
    [InlineData("hidden")]
    [InlineData("items-center")]
    [InlineData("outline-hidden")]
    // Functional roots over the spacing scale.
    [InlineData("p-4")]
    [InlineData("mt-0.5")]
    [InlineData("gap-px")]
    [InlineData("w-1/2")]
    [InlineData("w-full")]
    // Functional roots over their theme namespaces.
    [InlineData("bg-red-500")]
    [InlineData("text-white")]
    [InlineData("text-lg")]
    [InlineData("rounded-xl")]
    [InlineData("font-bold")]
    [InlineData("shadow-lg")]
    // Bare numbers / default values.
    [InlineData("opacity-50")]
    [InlineData("z-10")]
    [InlineData("rounded")]
    public void IncludesCommonUtilities(string className)
    {
        DefaultNames.Value.ShouldContain(className);
    }

    [Theory]
    [InlineData("text-lg--line-height")] // companion theme token, not a utility value
    [InlineData("-mt-4")] // negatives are deliberately left out
    [InlineData("bg-red-500/50")] // modifiers are not enumerated
    [InlineData("hover:flex")] // variants are not enumerated
    public void ExcludesNonCandidates(string className)
    {
        DefaultNames.Value.ShouldNotContain(className);
    }

    [Fact]
    public void EveryNameCompilesToCss()
    {
        // Through the full public pipeline (not just the generating utility), one class at a time.
        var framework = new CssFramework(new CssFrameworkSettings { IncludePreflight = false });

        var failures = DefaultNames.Value
            .Where(name =>
            {
                var result = framework.ProcessWithDetails(name);
                return result.ProcessedClasses.Length != 1 || !result.GeneratedCss.Contains("@layer utilities") && !result.GeneratedCss.Contains("@layer components");
            })
            .ToList();

        failures.ShouldBeEmpty();
    }

    [Fact]
    public void NamesAreDistinctAndNaturallyOrdered()
    {
        var names = DefaultNames.Value;
        names.Count.ShouldBe(names.Distinct(StringComparer.Ordinal).Count());

        IndexOf(names, "p-2").ShouldBeLessThan(IndexOf(names, "p-10"));
        IndexOf(names, "p-0.5").ShouldBeLessThan(IndexOf(names, "p-1"));
        IndexOf(names, "bg-red-50").ShouldBeLessThan(IndexOf(names, "bg-red-500"));

        static int IndexOf(IReadOnlyList<string> list, string name)
        {
            var index = list.ToList().IndexOf(name);
            index.ShouldBeGreaterThanOrEqualTo(0, $"{name} missing");
            return index;
        }
    }

    [Fact]
    public void GroupsByRootInNaturalOrder()
    {
        var names = DefaultNames.Value.ToList();

        // Roots in natural order, each root's group contiguous: all of bg-* sits between
        // `bg` and the next root.
        names.IndexOf("absolute").ShouldBeLessThan(names.IndexOf("bg-red-500"));
        names.IndexOf("bg-red-500").ShouldBeLessThan(names.IndexOf("block"));
        names.IndexOf("p-96").ShouldBeLessThan(names.IndexOf("px-0"));
    }

    [Fact]
    public void ReflectsCustomThemeTokens()
    {
        var theme = MonorailCss.Theme.Theme.CreateWithDefaults()
            .AddColorPalette("brand", new Dictionary<string, string> { ["500"] = "#123456" }.ToImmutableDictionary());
        var framework = new CssFramework(new CssFrameworkSettings { Theme = theme });

        var names = framework.GetSuggestedClassNames().ToList();

        names.ShouldContain("bg-brand-500");
        names.ShouldContain("text-brand-500");
        names.ShouldContain("border-brand-500");
        DefaultNames.Value.ShouldNotContain("bg-brand-500");
    }

    [Fact]
    public void IncludesCustomStaticUtilities()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CustomUtilities =
            [
                new MonorailCss.Parser.Custom.UtilityDefinition
                {
                    Pattern = "content-auto",
                    IsWildcard = false,
                    Declarations = [new MonorailCss.Parser.Custom.CssDeclaration("content-visibility", "auto")],
                },
            ],
        });

        framework.GetSuggestedClassNames().ShouldContain("content-auto");
    }
}
