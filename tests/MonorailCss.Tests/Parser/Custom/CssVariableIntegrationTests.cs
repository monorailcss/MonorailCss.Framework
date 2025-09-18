using System.Collections.Immutable;
using MonorailCss.Parser.Custom;
using Shouldly;

namespace MonorailCss.Tests.Parser.Custom;

/// <summary>
/// Comprehensive tests for CSS variable integration in custom utilities.
/// Verifies that custom utilities correctly generate CSS variables that flow through the pipeline.
/// </summary>
public class CssVariableIntegrationTests
{
    private CssFramework _framework;

    public CssVariableIntegrationTests()
    {
        _framework = new CssFramework(new CssFrameworkSettings { IncludePreflight = false });
    }

    [Fact]
    public void CustomUtility_WithCssVariableDeclaration_GeneratesCorrectAst()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-gutter-stable",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-gutter", "stable"),
                new CssDeclaration("scrollbar-gutter", "var(--tw-scrollbar-gutter)")
            )
        };

        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-gutter-stable");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-gutter: stable");
        result.ShouldContain("scrollbar-gutter: var(--tw-scrollbar-gutter)");
    }

    [Fact]
    public void CustomUtility_WithMultipleCssVariables_GeneratesAllCorrectly()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-colors",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "#333"),
                new CssDeclaration("--tw-scrollbar-track-color", "#eee"),
                new CssDeclaration("--tw-scrollbar-hover-color", "#555"),
                new CssDeclaration("scrollbar-color", "var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)")
            )
        };

        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-colors");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-thumb-color: #333");
        result.ShouldContain("--tw-scrollbar-track-color: #eee");
        result.ShouldContain("--tw-scrollbar-hover-color: #555");
        result.ShouldContain("scrollbar-color: var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)");
    }

    [Fact]
    public void CustomUtility_WithCssVariableInNestedSelector_WorksCorrectly()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-webkit",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-size", "8px")
            ),
            NestedSelectors = ImmutableList.Create(
                new NestedSelector("&::-webkit-scrollbar", ImmutableList.Create(
                    new CssDeclaration("width", "var(--tw-scrollbar-size)"),
                    new CssDeclaration("height", "var(--tw-scrollbar-size)")
                ))
            )
        };

        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-webkit");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-size: 8px");
        result.ShouldContain("::-webkit-scrollbar");
        result.ShouldContain("width: var(--tw-scrollbar-size)");
        result.ShouldContain("height: var(--tw-scrollbar-size)");
    }

    [Fact]
    public void DynamicCustomUtility_WithCssVariablePattern_ResolvesCorrectly()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-width-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-width", "*"),
                new CssDeclaration("scrollbar-width", "var(--tw-scrollbar-width)")
            )
        };

        var utility = CustomUtilityFactory.CreateDynamicUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-width-thin");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-width: thin");
        result.ShouldContain("scrollbar-width: var(--tw-scrollbar-width)");
    }

    [Fact]
    public void CustomUtility_WithCssVariableChain_GeneratesCorrectOutput()
    {
        // Arrange
        var definition1 = new UtilityDefinition
        {
            Pattern = "scrollbar-base",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-base-size", "12px"),
                new CssDeclaration("--tw-scrollbar-computed-size", "calc(var(--tw-scrollbar-base-size) * var(--tw-scrollbar-scale, 1))")
            )
        };

        var definition2 = new UtilityDefinition
        {
            Pattern = "scrollbar-scale-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-scale", "*")
            )
        };

        _framework.AddUtility(CustomUtilityFactory.CreateStaticUtility(definition1));
        _framework.AddUtility(CustomUtilityFactory.CreateDynamicUtility(definition2));

        // Act
        var result = _framework.Process("scrollbar-base scrollbar-scale-1.5");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-base-size: 12px");
        result.ShouldContain("--tw-scrollbar-computed-size: calc(var(--tw-scrollbar-base-size) * var(--tw-scrollbar-scale, 1))");
        result.ShouldContain("--tw-scrollbar-scale: 1.5");
    }

    [Fact]
    public void CustomUtility_WithComplexCssVariableExpression_HandlesCorrectly()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-dynamic",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-start", "rgb(100 100 100)"),
                new CssDeclaration("--tw-scrollbar-thumb-end", "rgb(200 200 200)"),
                new CssDeclaration("background", "linear-gradient(var(--tw-scrollbar-thumb-start), var(--tw-scrollbar-thumb-end))"),
                new CssDeclaration("scrollbar-color", "rgb(from var(--tw-scrollbar-thumb-start) r g b / 0.5) transparent")
            )
        };

        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-dynamic");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-thumb-start: rgb(100 100 100)");
        result.ShouldContain("--tw-scrollbar-thumb-end: rgb(200 200 200)");
        result.ShouldContain("background: linear-gradient(var(--tw-scrollbar-thumb-start), var(--tw-scrollbar-thumb-end))");
        result.ShouldContain("scrollbar-color: rgb(from var(--tw-scrollbar-thumb-start) r g b / 0.5) transparent");
    }

    [Fact]
    public void CustomUtility_WithConditionalCssVariable_WorksWithVariants()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-hover",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-hover-opacity", "1"),
                new CssDeclaration("opacity", "var(--tw-scrollbar-hover-opacity)")
            )
        };

        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("hover:scrollbar-hover");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(":hover");
        result.ShouldContain("--tw-scrollbar-hover-opacity: 1");
        result.ShouldContain("opacity: var(--tw-scrollbar-hover-opacity)");
    }

    [Fact]
    public void CustomUtility_WithCssVariableFallback_GeneratesWithFallback()
    {
        // Arrange - Testing that pipeline adds fallbacks for known --tw- variables
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-shadow",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-shadow-color", "rgb(0 0 0 / 0.2)"),
                new CssDeclaration("box-shadow", "0 0 10px var(--tw-shadow-color)")
            )
        };

        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-shadow");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-shadow-color: rgb(0 0 0 / 0.2)");
        // The pipeline's VariableFallbackStage should add fallback for --tw-shadow-color
        result.ShouldContain("var(--tw-shadow-color");
    }

    [Fact]
    public void DynamicUtility_WithThemeVariableResolution_GeneratesCssVariable()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-bg-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-bg", "--value(--color-*)"),
                new CssDeclaration("background-color", "var(--tw-scrollbar-bg)")
            )
        };

        var utility = CustomUtilityFactory.CreateDynamicUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-bg-red-500");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-bg: var(--color-red-500)");
        result.ShouldContain("background-color: var(--tw-scrollbar-bg)");
    }

    [Fact]
    public void MultipleDynamicUtilities_WithSharedCssVariables_WorkTogether()
    {
        // Arrange
        var thumbDef = new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")
            )
        };

        var trackDef = new UtilityDefinition
        {
            Pattern = "scrollbar-track-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-track-color", "--value(--color-*)")
            )
        };

        var colorDef = new UtilityDefinition
        {
            Pattern = "scrollbar-color",
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-color", "var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)")
            )
        };

        _framework.AddUtility(CustomUtilityFactory.CreateDynamicUtility(thumbDef));
        _framework.AddUtility(CustomUtilityFactory.CreateDynamicUtility(trackDef));
        _framework.AddUtility(CustomUtilityFactory.CreateStaticUtility(colorDef));

        // Act
        var result = _framework.Process("scrollbar-thumb-blue-600 scrollbar-track-gray-100 scrollbar-color");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-thumb-color: var(--color-blue-600)");
        result.ShouldContain("--tw-scrollbar-track-color: var(--color-gray-100)");
        result.ShouldContain("scrollbar-color: var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)");
    }

    [Fact]
    public void CustomUtility_WithCssVariableInCalc_HandlesCorrectly()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-offset-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-offset", "*"),
                new CssDeclaration("margin-right", "calc(var(--tw-scrollbar-offset) + 10px)"),
                new CssDeclaration("padding-right", "calc(var(--tw-scrollbar-offset) + var(--tw-scrollbar-offset))")
            )
        };

        var utility = CustomUtilityFactory.CreateDynamicUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("scrollbar-offset-5px");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("--tw-scrollbar-offset: 5px");
        result.ShouldContain("margin-right: calc(var(--tw-scrollbar-offset) + 10px)");
        result.ShouldContain("padding-right: calc(var(--tw-scrollbar-offset) + var(--tw-scrollbar-offset))");
    }

    [Fact]
    public void CustomUtility_WithResponsiveVariant_GeneratesCssVariablesInMediaQuery()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-responsive",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-display", "block"),
                new CssDeclaration("display", "var(--tw-scrollbar-display)")
            )
        };

        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        _framework.AddUtility(utility);

        // Act
        var result = _framework.Process("md:scrollbar-responsive");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("@media");
        result.ShouldContain("--tw-scrollbar-display: block");
        result.ShouldContain("display: var(--tw-scrollbar-display)");
    }
}