using System.Collections.Immutable;
using MonorailCss.Theme;

namespace MonorailCss.Tests;

public class ProseCustomizationTests
{
    [Fact]
    public void ProseCustomization_OverridesDefaultStyles()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("DEFAULT", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "a",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "font-weight", Value = "700" },
                                new ProseDeclaration { Property = "text-decoration", Value = "none" }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose");

        // Assert
        Assert.Contains("font-weight: 700", result);
        Assert.Contains("text-decoration: none", result);
    }

    [Fact]
    public void ProseCustomization_AddsNewElements()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("DEFAULT", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "mark",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "background-color", Value = "yellow" },
                                new ProseDeclaration { Property = "color", Value = "black" }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose");

        // Assert
        Assert.Contains("mark", result);
        Assert.Contains("background-color: yellow", result);
        Assert.Contains("color: black", result);
    }

    [Fact]
    public void ProseCustomization_WorksWithSizeModifiers()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("sm", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "code",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "font-size", Value = "0.8em" }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose-sm");

        // Assert
        Assert.Contains("code", result);
        Assert.Contains("font-size: 0.8em", result);
    }

    [Fact]
    public void ProseCustomization_WorksWithInvertModifier()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("invert", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "pre",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "font-weight", Value = "300" },
                                new ProseDeclaration { Property = "background-color", Value = "rgba(0, 0, 0, 0.75)" }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose-invert");

        // Assert
        Assert.Contains("pre", result);
        Assert.Contains("font-weight: 300", result);
        Assert.Contains("background-color: rgba(0, 0, 0, 0.75)", result);
    }

    [Fact]
    public void ProseCustomization_DynamicCustomizationWorks()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = theme =>
            {
                // Get a color from the theme (assuming we have primary colors)
                var primaryColor = theme.ResolveValue("500", new[] { "--color-primary" }) ?? "#3b82f6";

                return ImmutableDictionary<string, ProseElementRules>.Empty
                    .Add("DEFAULT", new ProseElementRules
                    {
                        Rules = ImmutableList.Create(
                            new ProseElementRule
                            {
                                Selector = "a",
                                Declarations = ImmutableList.Create(
                                    new ProseDeclaration { Property = "color", Value = primaryColor }
                                )
                            }
                        )
                    });
            }
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose");

        // Assert
        Assert.Contains("a", result);
        // The color value would be either the theme value or the fallback
        Assert.True(result.Contains("color:") || result.Contains("color :"));
    }

    [Fact]
    public void ProseCustomization_CanCombineMultipleDeclarations()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("DEFAULT", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "a",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "font-weight", Value = "700" },
                                new ProseDeclaration { Property = "text-decoration", Value = "none" }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose");

        // Assert
        Assert.Contains("a", result);
        Assert.Contains("font-weight: 700", result);
        Assert.Contains("text-decoration: none", result);
    }

    [Fact]
    public void ProseCustomization_SupportsImportantFlag()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("DEFAULT", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "a",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration
                                {
                                    Property = "color",
                                    Value = "red",
                                    Important = true
                                }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose");

        // Assert
        Assert.Contains("a", result);
        Assert.Contains("color: red !important", result);
    }

    [Fact]
    public void ProseCustomization_HandlesComplexSelectors()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("DEFAULT", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "a:not(:has(> code))",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "border-bottom-width", Value = "1px" },
                                new ProseDeclaration { Property = "border-bottom-color", Value = "currentColor" }
                            )
                        },
                        new ProseElementRule
                        {
                            Selector = ":not(pre) > code",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "padding", Value = "3px 8px" },
                                new ProseDeclaration { Property = "border-radius", Value = "0.4rem" }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose");

        // Assert
        Assert.Contains("a:not(:has(> code))", result);
        Assert.Contains("border-bottom-width: 1px", result);
        Assert.Contains(":not(pre) > code", result);
        Assert.Contains("padding: 3px 8px", result);
    }

    [Fact]
    public void ProseCustomization_AppliesBaseAndDefaultModifiers()
    {
        // Arrange
        var customization = new ProseCustomization
        {
            Customization = _ => ImmutableDictionary<string, ProseElementRules>.Empty
                .Add("DEFAULT", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "a",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "color", Value = "blue" }
                            )
                        }
                    )
                })
                .Add("base", new ProseElementRules
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "pre > code",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "font-size", Value = "inherit" }
                            )
                        }
                    )
                })
        };

        var settings = new CssFrameworkSettings
        {
            ProseCustomization = customization
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("prose");

        // Assert
        Assert.Contains("a", result);
        Assert.Contains("color: blue", result);
        Assert.Contains("pre > code", result);
        Assert.Contains("font-size: inherit", result);
    }
}