using Shouldly;

namespace MonorailCss.Tests.Sorting;

/// <summary>
/// Tests for verifying that variant ordering matches Tailwind CSS's canonical ordering.
/// </summary>
public class VariantOrderingTests
{
    private readonly CssFramework _cssFramework = new();

    [Fact]
    public void VariantOrdering_DarkShouldComeAfterInteractiveVariants()
    {
        // Arrange
        var input = "hover:text-red-900 dark:hover:text-green-50/90 data-[selected=true]:text-orange-800 dark:data-[selected=true]:text-indigo-50";

        // Act
        var result = _cssFramework.Process(input);

        // Assert - Check that classes appear in the correct order
        // Expected order based on Tailwind CSS:
        // 1. hover:text-red-900
        // 2. data-[selected=true]:text-orange-800
        // 3. dark:hover:text-green-50/90
        // 4. dark:data-[selected=true]:text-indigo-50

        // Find the positions of each class in the output
        var hoverPos = result.IndexOf(@".hover\:text-red-900:hover", StringComparison.Ordinal);
        var dataPos = result.IndexOf(@".data-\[selected\=true\]\:text-orange-800", StringComparison.Ordinal);
        var darkHoverPos = result.IndexOf(@".dark\:hover\:text-green-50\/90", StringComparison.Ordinal);
        var darkDataPos = result.IndexOf(@".dark\:data-\[selected\=true\]\:text-indigo-50", StringComparison.Ordinal);

        // All classes should be present
        hoverPos.ShouldBeGreaterThan(-1, "hover:text-red-900 should be in output");
        dataPos.ShouldBeGreaterThan(-1, "data-[selected=true]:text-orange-800 should be in output");
        darkHoverPos.ShouldBeGreaterThan(-1, "dark:hover:text-green-50/90 should be in output");
        darkDataPos.ShouldBeGreaterThan(-1, "dark:data-[selected=true]:text-indigo-50 should be in output");

        // Verify ordering
        hoverPos.ShouldBeLessThan(dataPos, "hover variant should come before data attribute");
        dataPos.ShouldBeLessThan(darkHoverPos, "data attribute should come before dark:hover");
        darkHoverPos.ShouldBeLessThan(darkDataPos, "dark:hover should come before dark:data");
    }

    [Theory]
    [InlineData("hover:bg-red-500 dark:bg-blue-500", @".hover\:bg-red-500", @".dark\:bg-blue-500")]
    [InlineData("focus:text-white dark:focus:text-black", @".focus\:text-white", @".dark\:focus\:text-black")]
    [InlineData("active:border-2 dark:active:border-4", @".active\:border-2", @".dark\:active\:border-4")]
    public void VariantOrdering_DarkVariantShouldComeAfterPseudoClassVariants(string input, string firstClass, string secondClass)
    {
        // Act
        var result = _cssFramework.Process(input);

        // Assert
        var firstPos = result.IndexOf(firstClass, StringComparison.Ordinal);
        var secondPos = result.IndexOf(secondClass, StringComparison.Ordinal);

        firstPos.ShouldBeGreaterThan(-1, $"{firstClass} should be in output");
        secondPos.ShouldBeGreaterThan(-1, $"{secondClass} should be in output");
        firstPos.ShouldBeLessThan(secondPos, "Non-dark variant should come before dark variant");
    }

    [Fact]
    public void VariantOrdering_ComplexVariantCombinations()
    {
        // Arrange - Mix of different variant types
        var input = "rtl:text-left ltr:text-right hover:bg-gray-100 focus:ring-2 dark:text-white dark:hover:bg-gray-800 print:hidden";

        // Act
        var result = _cssFramework.Process(input);

        // Assert - Verify general ordering principles
        // RTL/LTR should come first (lower weights)
        var rtlPos = result.IndexOf(@".rtl\:text-left", StringComparison.Ordinal);
        var ltrPos = result.IndexOf(@".ltr\:text-right", StringComparison.Ordinal);
        var hoverPos = result.IndexOf(@".hover\:bg-gray-100", StringComparison.Ordinal);
        var focusPos = result.IndexOf(@".focus\:ring-2", StringComparison.Ordinal);
        var printPos = result.IndexOf(@".print\:hidden", StringComparison.Ordinal);
        var darkPos = result.IndexOf(@".dark\:text-white", StringComparison.Ordinal);
        var darkHoverPos = result.IndexOf(@".dark\:hover\:bg-gray-800", StringComparison.Ordinal);

        // All should be present
        rtlPos.ShouldBeGreaterThan(-1);
        ltrPos.ShouldBeGreaterThan(-1);
        hoverPos.ShouldBeGreaterThan(-1);
        focusPos.ShouldBeGreaterThan(-1);
        printPos.ShouldBeGreaterThan(-1);
        darkPos.ShouldBeGreaterThan(-1);
        darkHoverPos.ShouldBeGreaterThan(-1);

        // Verify relative ordering
        rtlPos.ShouldBeLessThan(hoverPos, "RTL should come before hover");
        ltrPos.ShouldBeLessThan(hoverPos, "LTR should come before hover");
        hoverPos.ShouldBeLessThan(printPos, "Hover should come before print");
        printPos.ShouldBeLessThan(darkPos, "Print should come before dark");
        darkPos.ShouldBeLessThan(darkHoverPos, "Dark should come before dark:hover");
    }
}