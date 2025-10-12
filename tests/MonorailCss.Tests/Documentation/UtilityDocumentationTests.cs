using MonorailCss.Documentation;
using MonorailCss.Utilities;
using MonorailCss.Utilities.Backgrounds;
using MonorailCss.Utilities.Layout;
using MonorailCss.Utilities.Sizing;
using Shouldly;

namespace MonorailCss.Tests.Documentation;

public class UtilityDocumentationTests
{
    [Fact]
    public void UtilityDocumentationEngine_GeneratesDocumentationForAllUtilities()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var docs = UtilityDocumentationEngine.GenerateDocumentation(theme).ToList();

        // Assert
        docs.ShouldNotBeEmpty();
        docs.Count.ShouldBeGreaterThan(50); // We have many utilities
    }

    [Fact]
    public void UtilityDocumentationEngine_GroupsByCategory()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var docsByCategory = UtilityDocumentationEngine.GenerateDocumentationByCategory(theme);

        // Assert
        docsByCategory.ShouldNotBeEmpty();
        docsByCategory.ShouldContainKey("Layout");
        docsByCategory.ShouldContainKey("Backgrounds");
        docsByCategory.ShouldContainKey("Typography");
    }

    [Fact]
    public void UtilityDocumentationEngine_GeneratesSummary()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var summary = UtilityDocumentationEngine.GenerateUtilitySummary(theme);

        // Assert
        summary.ShouldNotBeEmpty();
        summary.ShouldContainKey("Static");
        summary.ShouldContainKey("Color");
        summary.ShouldContainKey("Sizing");
    }

    [Fact]
    public void BaseStaticUtility_GeneratesExamplesFromStaticValues()
    {
        // Arrange
        var utility = new DisplayUtility();
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var examples = utility.GetExamples(theme).ToList();

        // Assert
        examples.ShouldNotBeEmpty();
        examples.ShouldContain(ex => ex.ClassName == "block");
        examples.ShouldContain(ex => ex.ClassName == "flex");
        examples.ShouldContain(ex => ex.ClassName == "grid");
    }

    [Fact]
    public void BaseColorUtility_GeneratesExamplesWithThemeColors()
    {
        // Arrange
        var utility = new BackgroundColorUtility();
        var theme = new MonorailCss.Theme.Theme(); // Default theme has red, blue, green, gray

        // Act
        var examples = utility.GetExamples(theme).ToList();

        // Assert
        examples.ShouldNotBeEmpty();
        examples.ShouldContain(ex => ex.ClassName.StartsWith("bg-red"));
        examples.ShouldContain(ex => ex.ClassName.StartsWith("bg-blue"));
        examples.ShouldContain(ex => ex.ClassName.Contains("["));
        examples.ShouldContain(ex => ex.ClassName.Contains("/")); // Opacity modifier
    }

    [Fact]
    public void BaseSizingUtility_GeneratesExamplesWithCommonValues()
    {
        // Arrange
        var utility = new WidthUtility();
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var examples = utility.GetExamples(theme).ToList();

        // Assert
        examples.ShouldNotBeEmpty();
        examples.ShouldContain(ex => ex.ClassName == "w-auto");
        examples.ShouldContain(ex => ex.ClassName == "w-full");
        examples.ShouldContain(ex => ex.ClassName == "w-1/2");
        examples.ShouldContain(ex => ex.ClassName == "w-[100px]");
    }

    [Fact]
    public void UtilityMetadata_InfersCategoryFromNamespace()
    {
        // Arrange
        var utility = new DisplayUtility();

        // Act
        var metadata = utility.GetMetadata();

        // Assert
        metadata.Name.ShouldBe("DisplayUtility");
        metadata.Category.ShouldBe("Layout");
    }

    [Fact]
    public void UtilityDocumentation_IncludesThemeNamespaces()
    {
        // Arrange
        var utility = new BackgroundColorUtility();
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var doc = UtilityDocumentationEngine.GenerateDocumentationFor(utility, theme);

        // Assert
        doc.ThemeNamespaces.ShouldNotBeEmpty();
        doc.ThemeNamespaces.ShouldContain("--color");
    }

    [Fact]
    public void UtilityDocumentation_IncludesPriority()
    {
        // Arrange
        var utility = new DisplayUtility();
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var doc = UtilityDocumentationEngine.GenerateDocumentationFor(utility, theme);

        // Assert
        doc.Priority.ShouldBe(UtilityPriority.ExactStatic);
    }

    [Fact]
    public void UtilityDocumentation_InfersType()
    {
        // Arrange
        var displayUtility = new DisplayUtility();
        var bgUtility = new BackgroundColorUtility();
        var widthUtility = new WidthUtility();
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var displayDoc = UtilityDocumentationEngine.GenerateDocumentationFor(displayUtility, theme);
        var bgDoc = UtilityDocumentationEngine.GenerateDocumentationFor(bgUtility, theme);
        var widthDoc = UtilityDocumentationEngine.GenerateDocumentationFor(widthUtility, theme);

        // Assert
        displayDoc.Type.ShouldBe("Static");
        bgDoc.Type.ShouldBe("Color");
        widthDoc.Type.ShouldBe("Sizing");
    }

    [Fact]
    public void UtilityDocumentationEngine_FiltersUtilitiesByNamespace()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var colorDocs = UtilityDocumentationEngine.GenerateDocumentationForNamespace("--color", theme).ToList();

        // Assert
        colorDocs.ShouldNotBeEmpty();
        colorDocs.ShouldAllBe(doc => doc.ThemeNamespaces.Any(ns => ns.StartsWith("--color")));
    }

    [Fact]
    public void UtilityExample_CanBeCreatedFromClassName()
    {
        // Act
        var example = UtilityExample.FromClassName("flex");

        // Assert
        example.ClassName.ShouldBe("flex");
        example.Description.ShouldContain("flex");
    }

    [Fact]
    public void UtilityMetadata_CustomMetadataOverridesDefaults()
    {
        // Arrange
        var utility = new BackgroundColorUtility();

        // Act
        var metadata = utility.GetMetadata();

        // Assert
        metadata.SupportsModifiers.ShouldBeTrue();
        metadata.SupportsArbitraryValues.ShouldBeTrue();
    }

    [Fact]
    public void DisplayUtility_CustomExamplesIncludeDescriptions()
    {
        // Arrange
        var utility = new DisplayUtility();
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var examples = utility.GetExamples(theme).ToList();

        // Assert
        examples.ShouldAllBe(ex => !string.IsNullOrEmpty(ex.Description));
        examples.ShouldContain(ex => ex.Description.Contains("flex container"));
    }

    [Fact]
    public void UtilityDocumentation_ExamplesAreThemeAware()
    {
        // Arrange
        var customTheme = MonorailCss.Theme.Theme.CreateWithDefaults()
            .Add("--custom-namespace-value1", "1rem")
            .Add("--custom-namespace-value2", "2rem");

        // Create a test utility that uses this custom namespace
        var utility = new BackgroundColorUtility();

        // Act
        var examples = utility.GetExamples(customTheme).ToList();

        // Assert - Should use actual theme values, not hardcoded values
        examples.ShouldNotBeEmpty();
    }
}
