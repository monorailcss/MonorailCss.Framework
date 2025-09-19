using MonorailCss.Parser.Custom;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Comprehensive integration tests for custom utilities with variants, responsive modifiers, and priority ordering.
/// </summary>
public class CssFrameworkCustomUtilityIntegrationTests
{
    private readonly CustomUtilityCssParser _parser = new();

    [Fact]
    public void CustomUtility_WithResponsiveModifiers_ShouldGenerateMediaQueries()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-bg {
                      background: red;
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act & Assert - Small breakpoint
        var smOutput = framework.Process("sm:custom-bg");
        smOutput.ShouldContain("background: red");
        smOutput.ShouldContain("@media");
        smOutput.ShouldContain("640px"); // sm breakpoint

        // Act & Assert - Medium breakpoint
        var mdOutput = framework.Process("md:custom-bg");
        mdOutput.ShouldContain("background: red");
        mdOutput.ShouldContain("@media");
        mdOutput.ShouldContain("768px"); // md breakpoint

        // Act & Assert - Large breakpoint
        var lgOutput = framework.Process("lg:custom-bg");
        lgOutput.ShouldContain("background: red");
        lgOutput.ShouldContain("@media");
        lgOutput.ShouldContain("1024px"); // lg breakpoint

        // Act & Assert - XL breakpoint
        var xlOutput = framework.Process("xl:custom-bg");
        xlOutput.ShouldContain("background: red");
        xlOutput.ShouldContain("@media");
        xlOutput.ShouldContain("1280px"); // xl breakpoint

        // Act & Assert - 2XL breakpoint
        var xxlOutput = framework.Process("2xl:custom-bg");
        xxlOutput.ShouldContain("background: red");
        xxlOutput.ShouldContain("@media");
        xxlOutput.ShouldContain("1536px"); // 2xl breakpoint
    }

    [Fact]
    public void CustomUtility_WithMultipleVariants_ShouldCombineCorrectly()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-text {
                      color: blue;
                      font-weight: bold;
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act - Hover and Focus combined
        var output = framework.Process("hover:focus:custom-text");

        // Assert
        output.ShouldContain("color: blue");
        output.ShouldContain("font-weight: bold");
        output.ShouldContain(":hover");
        output.ShouldContain(":focus");
    }

    [Fact]
    public void CustomUtility_WithDarkMode_ShouldGenerateDarkModeVariant()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-theme {
                      background-color: white;
                      color: black;
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act
        var output = framework.Process("dark:custom-theme");

        // Assert
        output.ShouldContain("background-color: white");
        output.ShouldContain("color: black");
        // Dark mode uses class-based selector in Tailwind v4 style, not media query
        output.ShouldContain(":where(.dark, .dark *)");
    }

    [Fact]
    public void CustomUtilities_WithPriorityOrdering_ShouldRespectPriorities()
    {
        // Arrange
        var framework = new CssFramework();

        // Create two utilities that match the same pattern but with different content
        // Static utilities have higher priority than dynamic ones
        var staticCss = """

                        @utility test-priority {
                            content: 'static';
                        }

                        """;

        var dynamicCss = """

                         @utility test-priority-* {
                             content: 'dynamic';
                         }

                         """;

        var staticDefs = _parser.ParseCustomUtilities(staticCss);
        var dynamicDefs = _parser.ParseCustomUtilities(dynamicCss);

        var staticUtils = CustomUtilityFactory.CreateUtilities(staticDefs);
        var dynamicUtils = CustomUtilityFactory.CreateUtilities(dynamicDefs);

        // Add dynamic first, then static - static should win due to priority
        framework.AddUtilities(dynamicUtils);
        framework.AddUtilities(staticUtils);

        // Act
        var output = framework.Process("test-priority");

        // Assert - Static utility should win
        output.ShouldContain("content: 'static'");
        output.ShouldNotContain("content: 'dynamic'");
    }

    [Fact]
    public void DynamicCustomUtility_WithArbitraryValues_ShouldProcessCorrectly()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-size-* {
                      width: *;
                      height: *;
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act - Named value
        var namedOutput = framework.Process("custom-size-10");
        namedOutput.ShouldContain("width: 10");
        namedOutput.ShouldContain("height: 10");

        // Act - Arbitrary value
        var arbitraryOutput = framework.Process("custom-size-[250px]");
        arbitraryOutput.ShouldContain("width: 250px");
        arbitraryOutput.ShouldContain("height: 250px");

        // Act - Complex arbitrary value
        var complexOutput = framework.Process("custom-size-[calc(100%-20px)]");
        complexOutput.ShouldContain("width: calc(100% - 20px)");
        complexOutput.ShouldContain("height: calc(100% - 20px)");
    }

    [Fact]
    public void CustomUtility_WithStateVariants_ShouldGeneratePseudoClasses()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-interactive {
                      cursor: pointer;
                      user-select: none;
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act & Assert - Hover
        var hoverOutput = framework.Process("hover:custom-interactive");
        hoverOutput.ShouldContain("cursor: pointer");
        hoverOutput.ShouldContain(":hover");

        // Act & Assert - Focus
        var focusOutput = framework.Process("focus:custom-interactive");
        focusOutput.ShouldContain("cursor: pointer");
        focusOutput.ShouldContain(":focus");

        // Act & Assert - Active
        var activeOutput = framework.Process("active:custom-interactive");
        activeOutput.ShouldContain("cursor: pointer");
        activeOutput.ShouldContain(":active");

        // Act & Assert - Disabled
        var disabledOutput = framework.Process("disabled:custom-interactive");
        disabledOutput.ShouldContain("cursor: pointer");
        disabledOutput.ShouldContain(":disabled");
    }

    [Fact]
    public void CustomUtility_WithGroupVariant_ShouldGenerateGroupSelector()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-group-effect {
                      opacity: 0.5;
                      transform: scale(0.95);
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act
        var output = framework.Process("group-hover:custom-group-effect");

        // Assert
        output.ShouldContain("opacity: 0.5");
        output.ShouldContain("transform: scale(0.95)");
        // Group-hover uses the modern Tailwind v4 selector pattern
        output.ShouldContain(":is(:where(.group):hover *)");
    }

    [Fact]
    public void CustomUtility_WithResponsiveAndStateVariants_ShouldCombineCorrectly()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-button {
                      padding: 10px;
                      border-radius: 5px;
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act - Responsive + Hover
        var output = framework.Process("md:hover:custom-button");

        // Assert
        output.ShouldContain("padding: 10px");
        output.ShouldContain("border-radius: 5px");
        output.ShouldContain("@media");
        output.ShouldContain("768px"); // md breakpoint
        output.ShouldContain(":hover");
    }

    [Fact]
    public void CustomUtility_WithImportantAndVariants_ShouldApplyImportantCorrectly()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-override {
                      display: flex;
                      justify-content: center;
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act - Important with variant
        var output = framework.Process("hover:!custom-override");

        // Assert
        output.ShouldContain("display: flex !important");
        output.ShouldContain("justify-content: center !important");
        output.ShouldContain(":hover");
    }

    [Fact]
    public void CustomUtilities_MixedWithBuiltIn_ShouldWorkTogether()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-card {
                      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act - Mix custom with built-in utilities
        var output = framework.Process("p-4 bg-white custom-card rounded-lg");

        // Assert
        // p-4 generates padding with theme spacing value
        output.ShouldContain("padding:");
        // bg-white generates background color
        output.ShouldContain("background-color:");
        // custom-card should have the exact shadow value
        output.ShouldContain("box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1)");
        // rounded-lg generates border radius
        output.ShouldContain("border-radius:");
    }

    [Fact]
    public void DynamicCustomUtility_WithThemeColors_ShouldResolveCorrectly()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-border-* {
                      border: 2px solid --value(--color-*);
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act - Theme color
        var output = framework.Process("custom-border-red-500");

        // Assert
        output.ShouldContain("border: 2px solid");
        // Color might be resolved as CSS variable or actual value
        var hasColorValue = output.Contains("#ef4444") ||
                          output.Contains("var(--color-red-500)") ||
                          output.Contains("oklch");
        hasColorValue.ShouldBeTrue("Output should contain theme color value in some format");
    }

    [Fact]
    public void CustomUtility_WithNestedSelectors_AndVariants_ShouldWorkCorrectly()
    {
        // Arrange
        var framework = new CssFramework();
        var css = """

                  @utility custom-scrollbar {
                      scrollbar-width: thin;
                      &::-webkit-scrollbar {
                          width: 8px;
                          height: 8px;
                      }
                  }

                  """;

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        framework.AddUtilities(utilities);

        // Act - With hover variant
        var output = framework.Process("hover:custom-scrollbar");

        // Assert
        output.ShouldContain("scrollbar-width: thin");
        output.ShouldContain("::-webkit-scrollbar");
        output.ShouldContain("width: 8px");
        output.ShouldContain(":hover");
    }
}