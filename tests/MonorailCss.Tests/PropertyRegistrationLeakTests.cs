using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Regression tests for the "@property leak" bug: the main matching loop probes every utility
/// per candidate, and some utilities registered their <c>@property</c> custom properties
/// <em>before</em> confirming they matched. That leaked unrelated <c>@property</c> blocks into the
/// output of completely unrelated classes (e.g. <c>bg-red-500</c> used to emit 11 spurious
/// <c>@property</c> declarations). Registration is now rolled back for probes that don't match,
/// so output should contain only the <c>@property</c> blocks the matched utility actually needs.
/// </summary>
public class PropertyRegistrationLeakTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { IncludePreflight = false });

    [Theory]
    [InlineData("bg-red-500")]
    [InlineData("flex")]
    [InlineData("p-4")]
    [InlineData("text-center")]
    [InlineData("m-2")]
    public void ClassesWithoutCustomProperties_EmitNoAtProperty(string utilityClass)
    {
        var css = _framework.Process(utilityClass);
        css.ShouldNotContain("@property", Case.Sensitive);
    }

    [Fact]
    public void Scale_EmitsOnlyScaleAtProperties()
    {
        var css = _framework.Process("scale-50");

        // Legitimate: scale needs its own three custom properties.
        css.ShouldContain("@property --tw-scale-x");
        css.ShouldContain("@property --tw-scale-y");
        css.ShouldContain("@property --tw-scale-z");

        // Leaked from unrelated utilities probed before ScaleUtility matched.
        css.ShouldNotContain("@property --tw-rotate-x");
        css.ShouldNotContain("@property --tw-skew-x");
        css.ShouldNotContain("@property --tw-backdrop-blur");
        css.ShouldNotContain("@property --tw-outline-style");
        css.ShouldNotContain("@property --tw-font-weight");
    }

    [Fact]
    public void Duration_EmitsOnlyDurationAtPropertyWithPreservedInitialValue()
    {
        var css = _framework.Process("duration-150");

        // The matched utility's own registration config must be preserved exactly.
        css.ShouldContain("@property --tw-duration");
        css.ShouldContain("initial-value: 150ms");

        css.ShouldNotContain("@property --tw-backdrop-blur");
        css.ShouldNotContain("@property --tw-scale-x");
        css.ShouldNotContain("@property --tw-outline-style");
    }

    [Fact]
    public void Blur_EmitsRegularFilterAtPropertiesButNotBackdrop()
    {
        var css = _framework.Process("blur-sm");

        // BlurUtility registers the full regular-filter stack.
        css.ShouldContain("@property --tw-blur");
        css.ShouldContain("@property --tw-brightness");

        // Backdrop filter stack and unrelated families must not leak in.
        css.ShouldNotContain("@property --tw-backdrop-blur");
        css.ShouldNotContain("@property --tw-outline-style");
        css.ShouldNotContain("@property --tw-font-weight");
    }
}
