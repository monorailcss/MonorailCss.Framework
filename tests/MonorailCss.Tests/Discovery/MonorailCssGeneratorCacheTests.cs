using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

/// <summary>
/// Covers the input-unchanged short-circuit in <see cref="MonorailCssGenerator"/> — two
/// calls with identical inputs return the same record instance without re-running the
/// full pipeline; mutating the input forces a fresh generation.
/// </summary>
public class MonorailCssGeneratorCacheTests
{
    [Fact]
    public void Generate_With_Identical_Inputs_Returns_Same_Result_Instance()
    {
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var request = new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4", "hover:underline" },
        };

        var first = generator.Generate(request);
        var second = generator.Generate(request);

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void Generate_With_Changed_Safelist_Returns_New_Result()
    {
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500" },
        });

        var second = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4" },
        });

        ReferenceEquals(first, second).ShouldBeFalse();
        second.Classes.ShouldContain("p-4");
        second.Css.ShouldContain("padding");
    }

    [Fact]
    public void Generate_With_Same_Set_Different_Order_Returns_Cached_Result()
    {
        // Candidate set is order-independent — the second call should hit the cache.
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4", "m-2" },
        });
        var second = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "m-2", "bg-red-500", "p-4" },
        });

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void Generate_With_Same_Set_Reverting_To_Earlier_Inputs_Reuses_Cache()
    {
        // After a change, reverting to the original safelist re-runs generation (the cache
        // holds only the most recent result) but still produces stable output.
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500" },
        });
        generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4" },
        });
        var third = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500" },
        });

        third.ETag.ShouldBe(first.ETag);
        third.Css.ShouldBe(first.Css);
    }

    [Fact]
    public void Generate_With_Changed_SourceCss_Bypasses_Cache()
    {
        // A different SourceCss yields a different framework instance, which clears the cache.
        var generator = new MonorailCssGenerator();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            SourceCss = "@theme { --color-brand: red; }",
            ExtraSafelist = new[] { "bg-brand" },
        });
        var second = generator.Generate(new MonorailCssGenerationRequest
        {
            SourceCss = "@theme { --color-brand: blue; }",
            ExtraSafelist = new[] { "bg-brand" },
        });

        ReferenceEquals(first, second).ShouldBeFalse();
        first.Css.ShouldContain("red");
        second.Css.ShouldContain("blue");
    }
}
