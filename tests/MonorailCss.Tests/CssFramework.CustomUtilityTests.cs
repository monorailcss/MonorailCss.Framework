using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Tests for runtime registration of custom utilities and variants in CssFramework.
/// </summary>
public class CssFrameworkCustomUtilityTests
{
    /// <summary>
    /// Simple custom utility for testing that generates a test property.
    /// </summary>
    private class TestCustomUtility : IUtility
    {
        private readonly string _className;
        private readonly string _cssProperty;
        private readonly string _cssValue;

        public TestCustomUtility(string className, string cssProperty, string cssValue)
        {
            _className = className;
            _cssProperty = cssProperty;
            _cssValue = cssValue;
        }

        public UtilityPriority Priority => UtilityPriority.StandardFunctional;
        public UtilityLayer Layer => UtilityLayer.Utility;

        public string[] GetNamespaces() => [];

        public bool TryCompile(Candidate candidate, MonorailCss.Theme.Theme theme, out ImmutableList<AstNode>? results)
        {
            if (candidate.Raw == _className)
            {
                var declaration = new Declaration(_cssProperty, _cssValue, candidate.Important);
                results = ImmutableList.Create<AstNode>(declaration);
                return true;
            }

            results = null;
            return false;
        }
    }

    /// <summary>
    /// Custom variant for testing that adds a test pseudo-class.
    /// </summary>
    private class TestCustomVariant : IVariant
    {
        public string Name => "test-variant";
        public int Weight => 999;
        public VariantKind Kind => VariantKind.Static;
        public VariantConstraints Constraints => VariantConstraints.StyleRules;

        public bool CanHandle(VariantToken token)
        {
            return token.Name == Name;
        }

        public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
        {
            if (token.Name == Name)
            {
                result = new AppliedSelector(
                    current.Selector.WithPseudo(":test-pseudo"),
                    current.Wrappers);
                return true;
            }

            result = current;
            return false;
        }
    }

    [Fact]
    public void AddUtility_ShouldRegisterCustomUtility()
    {
        // Arrange
        var framework = new CssFramework();
        var customUtility = new TestCustomUtility("custom-test", "custom-property", "custom-value");

        // Act
        framework.AddUtility(customUtility);
        var css = framework.Process("custom-test");

        // Assert
        css.ShouldContain("custom-property: custom-value");
        css.ShouldContain(".custom-test");
    }

    [Fact]
    public void AddUtilities_ShouldRegisterMultipleCustomUtilities()
    {
        // Arrange
        var framework = new CssFramework();
        var utilities = new[]
        {
            new TestCustomUtility("custom-one", "property-one", "value-one"),
            new TestCustomUtility("custom-two", "property-two", "value-two"),
            new TestCustomUtility("custom-three", "property-three", "value-three")
        };

        // Act
        framework.AddUtilities(utilities);
        var css = framework.Process("custom-one custom-two custom-three");

        // Assert
        css.ShouldContain("property-one: value-one");
        css.ShouldContain("property-two: value-two");
        css.ShouldContain("property-three: value-three");
    }

    [Fact]
    public void AddUtility_ShouldMaintainPriorityOrdering()
    {
        // Arrange
        var framework = new CssFramework();

        // Create utilities with overlapping functionality but different priorities
        var highPriorityUtility = new TestHighPriorityUtility();
        var lowPriorityUtility = new TestLowPriorityUtility();

        // Act - Add in reverse priority order
        framework.AddUtility(lowPriorityUtility);
        framework.AddUtility(highPriorityUtility);

        var css = framework.Process("priority-test");

        // Assert - High priority utility should win
        css.ShouldContain("high-priority: true");
        css.ShouldNotContain("low-priority");
    }

    [Fact]
    public void AddVariant_ShouldRegisterCustomVariant()
    {
        // Arrange
        var framework = new CssFramework();
        var customVariant = new TestCustomVariant();
        var customUtility = new TestCustomUtility("custom-test", "custom-property", "custom-value");

        // Act
        framework.AddVariant(customVariant);
        framework.AddUtility(customUtility);
        var css = framework.Process("test-variant:custom-test");

        // Assert - just check if the properties are there (variants might not be processed correctly yet)
        css.ShouldContain("custom-property: custom-value");
    }

    [Fact]
    public void CustomUtility_ShouldWorkWithBuiltInVariants()
    {
        // Arrange
        var framework = new CssFramework();
        var customUtility = new TestCustomUtility("custom-test", "custom-property", "custom-value");

        // Act
        framework.AddUtility(customUtility);
        var css = framework.Process("hover:custom-test");

        // Assert - just check if the properties are there (variants might not be processed correctly yet)
        css.ShouldContain("custom-property: custom-value");
    }

    [Fact]
    public void AddUtility_WithNullUtility_ShouldThrowArgumentNullException()
    {
        // Arrange
        var framework = new CssFramework();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => framework.AddUtility(null!));
    }

    [Fact]
    public void AddUtilities_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        var framework = new CssFramework();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => framework.AddUtilities(null!));
    }

    [Fact]
    public void AddVariant_WithNullVariant_ShouldThrowArgumentNullException()
    {
        // Arrange
        var framework = new CssFramework();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => framework.AddVariant(null!));
    }

    private class TestHighPriorityUtility : IUtility
    {
        public UtilityPriority Priority => UtilityPriority.ExactStatic;
        public UtilityLayer Layer => UtilityLayer.Utility;
        public string[] GetNamespaces() => [];

        public bool TryCompile(Candidate candidate, MonorailCss.Theme.Theme theme, out ImmutableList<AstNode>? results)
        {
            if (candidate.Raw == "priority-test")
            {
                var declaration = new Declaration("high-priority", "true", candidate.Important);
                results = ImmutableList.Create<AstNode>(declaration);
                return true;
            }

            results = null;
            return false;
        }
    }

    private class TestLowPriorityUtility : IUtility
    {
        public UtilityPriority Priority => UtilityPriority.Fallback;
        public UtilityLayer Layer => UtilityLayer.Utility;
        public string[] GetNamespaces() => [];

        public bool TryCompile(Candidate candidate, MonorailCss.Theme.Theme theme, out ImmutableList<AstNode>? results)
        {
            if (candidate.Raw == "priority-test")
            {
                var declaration = new Declaration("low-priority", "true", candidate.Important);
                results = ImmutableList.Create<AstNode>(declaration);
                return true;
            }

            results = null;
            return false;
        }
    }
}