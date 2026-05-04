using Shouldly;

namespace MonorailCss.Tests;

public class ColorEmissionModeTests
{
    [Fact]
    public void Default_Setting_Should_Be_UsedPalettes()
    {
        var settings = new CssFrameworkSettings();
        settings.ColorEmission.ShouldBe(ColorEmissionMode.UsedPalettes);
    }

    [Fact]
    public void Used_Mode_Should_Only_Emit_Referenced_Color_Variables()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false,
            ColorEmission = ColorEmissionMode.Used,
        });

        var css = framework.Process("bg-sky-100");

        css.ShouldContain("--color-sky-100:");
        css.ShouldNotContain("--color-sky-50:");
        css.ShouldNotContain("--color-sky-200:");
        css.ShouldNotContain("--color-sky-950:");
        css.ShouldNotContain("--color-red-500:");
    }

    [Fact]
    public void UsedPalettes_Mode_Should_Emit_Every_Shade_Of_A_Used_Palette()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false,
            ColorEmission = ColorEmissionMode.UsedPalettes,
        });

        var css = framework.Process("bg-sky-100");

        // Every sky shade comes along.
        foreach (var shade in new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" })
        {
            css.ShouldContain($"--color-sky-{shade}:", customMessage: $"missing --color-sky-{shade}");
        }

        // Other palettes are not pulled in.
        css.ShouldNotContain("--color-red-500:");
        css.ShouldNotContain("--color-emerald-500:");
    }

    [Fact]
    public void UsedPalettes_Mode_Should_Pull_Each_Used_Palette()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false,
            ColorEmission = ColorEmissionMode.UsedPalettes,
        });

        var css = framework.Process("bg-sky-100 text-red-500");

        css.ShouldContain("--color-sky-50:");
        css.ShouldContain("--color-sky-950:");
        css.ShouldContain("--color-red-50:");
        css.ShouldContain("--color-red-950:");
        css.ShouldNotContain("--color-emerald-500:");
    }

    [Fact]
    public void UsedPalettes_Mode_Should_Not_Pull_Unrelated_Palettes_For_Singletons()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false,
            ColorEmission = ColorEmissionMode.UsedPalettes,
        });

        var css = framework.Process("bg-black");

        css.ShouldContain("--color-black:");
        // Singleton (no shade) should not drag in any palette.
        css.ShouldNotContain("--color-sky-100:");
        css.ShouldNotContain("--color-red-500:");
    }

    [Fact]
    public void All_Mode_Should_Emit_Every_Color_Variable_Regardless_Of_Usage()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            IncludePreflight = false,
            ColorEmission = ColorEmissionMode.All,
        });

        // Use a non-color utility so the pipeline runs but no colors are referenced.
        var css = framework.Process("block");

        // Spot-check across distinct palettes and singletons.
        css.ShouldContain("--color-red-50:");
        css.ShouldContain("--color-sky-500:");
        css.ShouldContain("--color-emerald-950:");
        css.ShouldContain("--color-slate-200:");
        css.ShouldContain("--color-black:");
        css.ShouldContain("--color-white:");
    }
}
