using MonorailCss.Merging;
using Shouldly;

namespace MonorailCss.Tests.Merging;

public class ClassMergerTests
{
    private static readonly ClassMerger _merger =
        new(new CssFramework(new CssFrameworkSettings { IncludePreflight = false }));

    [Theory]
    // Same group: later wins.
    [InlineData("p-4 p-2", "p-2")]
    [InlineData("block flex", "flex")]
    [InlineData("bg-red-500 bg-blue-500", "bg-blue-500")]
    [InlineData("w-4 w-8", "w-8")]
    [InlineData("p-4 p-4", "p-4")]
    // Cross-group shorthands: a later shorthand removes its longhands; a later longhand decomposes
    // an earlier shorthand into the sides that survive.
    [InlineData("px-2 p-4", "p-4")]
    [InlineData("p-4 px-2", "py-4 px-2")]
    [InlineData("pl-2 px-4", "px-4")]
    [InlineData("ps-2 px-4", "px-4")]
    [InlineData("pt-2 py-4", "py-4")]
    [InlineData("-mt-2 mt-4", "mt-4")]
    [InlineData("mt-4 -mt-2", "-mt-2")]
    [InlineData("rounded-t-md rounded-lg", "rounded-lg")]
    [InlineData("rounded-lg rounded-t-md", "rounded-b-lg rounded-t-md")]
    [InlineData("left-4 inset-x-2", "inset-x-2")]
    [InlineData("inset-x-2 inset-0", "inset-0")]
    [InlineData("gap-x-2 gap-4", "gap-4")]
    [InlineData("overflow-x-auto overflow-hidden", "overflow-hidden")]
    [InlineData("overflow-hidden overflow-x-auto", "overflow-hidden overflow-x-auto")]
    // Partial overrides decompose the earlier shorthand into the longhand sides that survive,
    // breaking down only the conflicting sub-axis and preserving each input's position.
    [InlineData("my-4 mt-6", "mb-4 mt-6")]
    [InlineData("m-4 mt-6", "mx-4 mb-4 mt-6")]
    [InlineData("mb-2 my-4 mt-6", "mb-4 mt-6")]
    [InlineData("-my-4 mt-6", "-mb-4 mt-6")]
    [InlineData("my-[10px] mt-4", "mb-[10px] mt-4")]
    [InlineData("py-4 px-2 pt-1", "pb-4 px-2 pt-1")]
    [InlineData("inset-x-2 left-4", "right-2 left-4")]
    [InlineData("gap-4 gap-x-2", "gap-y-4 gap-x-2")]
    [InlineData("size-4 w-8", "h-4 w-8")]
    [InlineData("hover:my-4 hover:mt-6", "hover:mb-4 hover:mt-6")]
    [InlineData("my-4 hover:mt-6", "my-4 hover:mt-6")]
    [InlineData("my-4! mt-6!", "mb-4! mt-6!")]
    [InlineData("my-4! mt-6", "my-4! mt-6")]
    // Decomposition reverts to the original when it would not actually resolve a conflict — here the
    // physical mx axis and the logical ms side don't reduce to the same longhand.
    [InlineData("mx-4 ms-2", "mx-4 ms-2")]
    // Deviation from tailwind-merge, matching real CSS: grid-column resets grid-column-start.
    [InlineData("col-start-2 col-span-3", "col-span-3")]
    // Variants isolate conflicts.
    [InlineData("hover:p-2 p-4", "hover:p-2 p-4")]
    [InlineData("hover:p-2 hover:p-4", "hover:p-4")]
    [InlineData("md:p-4 md:p-2", "md:p-2")]
    [InlineData("md:p-4 lg:p-4", "md:p-4 lg:p-4")]
    [InlineData("dark:hover:bg-red-500 dark:hover:bg-blue-500", "dark:hover:bg-blue-500")]
    // Variant order is normalized — except for order-sensitive pseudo-element variants.
    [InlineData("hover:focus:p-2 focus:hover:p-4", "focus:hover:p-4")]
    [InlineData("before:hover:p-2 hover:before:p-4", "before:hover:p-2 hover:before:p-4")]
    [InlineData("before:p-2 before:p-4", "before:p-4")]
    // Important participates in the conflict key.
    [InlineData("p-4! p-2", "p-4! p-2")]
    [InlineData("p-4! p-2!", "p-2!")]
    [InlineData("!p-4 !p-2", "!p-2")]
    // Arbitrary values and arbitrary properties.
    [InlineData("w-4 w-[32px]", "w-[32px]")]
    [InlineData("[padding:1rem] p-4", "p-4")]
    [InlineData("p-4 [padding:1rem]", "[padding:1rem]")]
    [InlineData("[color:red] [color:blue]", "[color:blue]")]
    // Postfix modifiers conflict with their base group.
    [InlineData("bg-red-500/50 bg-blue-500", "bg-blue-500")]
    [InlineData("text-red-500/50 text-lg", "text-red-500/50 text-lg")]
    // Composable utilities: siblings writing different --tw-* variables don't conflict,
    // reset roots remove them.
    [InlineData("touch-pan-x touch-pan-y", "touch-pan-x touch-pan-y")]
    [InlineData("touch-pan-x touch-none", "touch-none")]
    [InlineData("touch-none touch-pan-x", "touch-none touch-pan-x")]
    [InlineData("ordinal slashed-zero", "ordinal slashed-zero")]
    [InlineData("ordinal normal-nums", "normal-nums")]
    [InlineData("blur-sm grayscale", "blur-sm grayscale")]
    [InlineData("blur-sm blur-lg", "blur-lg")]
    [InlineData("shadow-lg shadow-red-500", "shadow-lg shadow-red-500")]
    [InlineData("shadow-lg shadow-xl", "shadow-xl")]
    [InlineData("ring-2 shadow-lg", "ring-2 shadow-lg")]
    [InlineData("space-x-2 space-x-4", "space-x-4")]
    [InlineData("space-x-2 space-y-2", "space-x-2 space-y-2")]
    [InlineData("space-x-2 mx-4", "space-x-2 mx-4")]
    // Font size sets line-height via var(--tw-leading, ...), so it overrides earlier leading.
    [InlineData("leading-7 text-lg", "text-lg")]
    [InlineData("text-lg leading-7", "text-lg leading-7")]
    // Var-fallback composition keeps both regardless of order.
    [InlineData("duration-200 transition", "duration-200 transition")]
    [InlineData("transition duration-200", "transition duration-200")]
    [InlineData("ease-in ease-out", "ease-out")]
    // bg color, gradient and text size/color live in different property groups.
    [InlineData("bg-red-500 bg-linear-to-r", "bg-red-500 bg-linear-to-r")]
    [InlineData("text-red-500 text-lg", "text-red-500 text-lg")]
    // Unknown classes and component-layer classes pass through, position preserved.
    [InlineData("my-custom-class p-4 unknown-thing", "my-custom-class p-4 unknown-thing")]
    [InlineData("my-custom-class my-custom-class", "my-custom-class my-custom-class")]
    [InlineData("prose text-lg", "prose text-lg")]
    [InlineData("text-lg prose", "text-lg prose")]
    [InlineData("container w-4", "container w-4")]
    // The motivating example: only overridden classes drop, relative order is preserved.
    [InlineData("px-2 p-4 bg-red-500 hover:p-2 bg-blue-500", "p-4 hover:p-2 bg-blue-500")]
    public void Merge_ResolvesConflicts(string input, string expected)
    {
        _merger.Merge(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Merge_HandlesEmptyInput(string? input, string expected)
    {
        _merger.Merge(input).ShouldBe(expected);
    }

    [Fact]
    public void Merge_NormalizesWhitespace()
    {
        _merger.Merge(" p-4   p-2 \n bg-red-500 ").ShouldBe("p-2 bg-red-500");
    }

    [Fact]
    public void Merge_MultipleLists_LaterListsWin()
    {
        _merger.Merge("p-4 bg-red-500", null, "p-2").ShouldBe("bg-red-500 p-2");
    }

    [Fact]
    public void Merge_IsStableAcrossRepeatedCalls()
    {
        var first = _merger.Merge("px-2 p-4 hover:p-2");
        var second = _merger.Merge("px-2 p-4 hover:p-2");
        second.ShouldBe(first);
    }

    [Fact]
    public void Framework_Merge_DelegatesToMerger()
    {
        var framework = new CssFramework(new CssFrameworkSettings { IncludePreflight = false });
        framework.Merge("px-2 p-4 bg-red-500 hover:p-2 bg-blue-500")
            .ShouldBe("p-4 hover:p-2 bg-blue-500");
        framework.Merge("p-4 bg-red-500", null, "p-2").ShouldBe("bg-red-500 p-2");
    }
}
