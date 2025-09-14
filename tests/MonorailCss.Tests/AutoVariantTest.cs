using Shouldly;

namespace MonorailCss.Tests;

public class AutoVariantTest
{
    [Fact]
    public void Height_With_Variant_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false
        });
        var result = framework.Process("md:h-4");
        Console.WriteLine(result);
        result.ShouldContain("height: calc(var(--spacing) * 4);");

    }

    [Fact]
    public void Margin_with_variant_works()
    {
        // verify it isn't all single letter utilities
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false
        });
        var result = framework.Process("md:m-4");
        Console.WriteLine(result);
        result.ShouldContain("margin: calc(var(--spacing) * 4);");

    }


    [Fact]
    public void H_Without_Variant_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false
        });
        var result = framework.Process("h-4");
        Console.WriteLine(result);
        result.ShouldContain("height: calc(var(--spacing) * 4);");

    }
}