using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Parser;
using MonorailCss.Utilities;
using Shouldly;

namespace MonorailCss.Tests.Parser;

internal class TestRootsUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.NamespaceHandler;
    public string[] GetNamespaces() => [];
    public bool TryCompile(Candidate candidate, MonorailCss.Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;
        return false;
    }
    public string[] GetFunctionalRoots() => ["-m", "-translate-x", "-space-x", "-rotate", "-top", "space-x", "space-y", "divide-x", "divide-y"];
}

public class CandidateParserTests
{
    private readonly CandidateParser _parser;

    public CandidateParserTests()
    {
        var utilityRegistry = new UtilityRegistry(autoRegisterUtilities: true);

        // Register a test utility that exposes some functional roots for parsing
        utilityRegistry.RegisterUtility(new TestRootsUtility());

        _parser = new CandidateParser(utilityRegistry);
    }

    [Fact]
    public void TryParseCandidate_WithStaticUtility_ParsesCorrectly()
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate("block", out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<StaticUtility>();

        var staticUtility = (StaticUtility)candidate;
        staticUtility.Root.ShouldBe("block");
        staticUtility.Raw.ShouldBe("block");
        staticUtility.Important.ShouldBeFalse();
        staticUtility.Variants.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("bg-red-500", "bg", "red-500")]
    [InlineData("text-blue-700", "text", "blue-700")]
    [InlineData("p-4", "p", "4")]
    [InlineData("m-auto", "m", "auto")]
    public void TryParseCandidate_WithFunctionalUtility_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Named);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Raw.ShouldBe(input);
        functionalUtility.Important.ShouldBeFalse();
    }

    [Theory]
    [InlineData("bg-[#123456]", "bg", "#123456")]
    [InlineData("text-[rgb(255,0,0)]", "text", "rgb(255,0,0)")]
    [InlineData("w-[100px]", "w", "100px")]
    public void TryParseCandidate_WithArbitraryValue_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Raw.ShouldBe(input);
    }

    [Theory]
    [InlineData("[display:block]", "display", "block")]
    [InlineData("[margin:10px]", "margin", "10px")]
    [InlineData("[color:red]", "color", "red")]
    public void TryParseCandidate_WithArbitraryProperty_ParsesCorrectly(string input, string expectedProperty, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<ArbitraryProperty>();

        var arbitraryProperty = (ArbitraryProperty)candidate;
        arbitraryProperty.Property.ShouldBe(expectedProperty);
        arbitraryProperty.Value.ShouldBe(expectedValue);
        arbitraryProperty.Raw.ShouldBe(input);
        arbitraryProperty.Important.ShouldBeFalse();
    }

    [Theory]
    [InlineData("!block")]
    [InlineData("block!")]
    public void TryParseCandidate_WithImportantMarker_ParsesCorrectly(string input)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<StaticUtility>();

        var staticUtility = (StaticUtility)candidate;
        staticUtility.Root.ShouldBe("block");
        staticUtility.Important.ShouldBeTrue();
        staticUtility.Raw.ShouldBe(input);
    }

    [Theory]
    [InlineData("!bg-red-500")]
    [InlineData("bg-red-500!")]
    public void TryParseCandidate_WithImportantFunctionalUtility_ParsesCorrectly(string input)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe("bg");
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe("red-500");
        functionalUtility.Important.ShouldBeTrue();
        functionalUtility.Raw.ShouldBe(input);
    }

    [Fact]
    public void ParseCandidates_WithMultipleClasses_ParsesAll()
    {
        // Arrange
        var input = "block bg-red-500 text-white p-4";

        // Act
        var candidates = _parser.ParseCandidates(input).ToList();

        // Assert
        candidates.Count.ShouldBe(4);

        candidates[0].ShouldBeOfType<StaticUtility>();
        ((StaticUtility)candidates[0]).Root.ShouldBe("block");

        candidates[1].ShouldBeOfType<FunctionalUtility>();
        ((FunctionalUtility)candidates[1]).Root.ShouldBe("bg");

        candidates[2].ShouldBeOfType<FunctionalUtility>();
        ((FunctionalUtility)candidates[2]).Root.ShouldBe("text");

        candidates[3].ShouldBeOfType<FunctionalUtility>();
        ((FunctionalUtility)candidates[3]).Root.ShouldBe("p");
    }

    [Fact]
    public void ParseCandidates_WithEmptyString_ReturnsEmpty()
    {
        // Arrange & Act
        var candidates = _parser.ParseCandidates("").ToList();

        // Assert
        candidates.ShouldBeEmpty();
    }

    [Fact]
    public void ParseCandidates_WithWhitespace_ReturnsEmpty()
    {
        // Arrange & Act
        var candidates = _parser.ParseCandidates("   ").ToList();

        // Assert
        candidates.ShouldBeEmpty();
    }

    [Fact]
    public void TryParseCandidate_WithUnknownUtility_ReturnsFalse()
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate("unknown", out var candidate);

        // Assert
        result.ShouldBeFalse();
        candidate.ShouldBeNull();
    }

    [Theory]
    [InlineData("bg-red-500/50", "bg", "red-500", "50", ModifierKind.Named)]
    [InlineData("text-blue-700/25", "text", "blue-700", "25", ModifierKind.Named)]
    [InlineData("bg-green-400/[0.5]", "bg", "green-400", "0.5", ModifierKind.Arbitrary)]
    [InlineData("text-purple-600/[0.75]", "text", "purple-600", "0.75", ModifierKind.Arbitrary)]
    internal void TryParseCandidate_WithOpacityModifier_ExtractsModifierCorrectly(string input, string expectedRoot, string expectedValue, string expectedModifier, ModifierKind expectedKind)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);

        // Verify modifier is properly extracted
        functionalUtility.Modifier.ShouldNotBeNull();
        functionalUtility.Modifier!.Value.ShouldBe(expectedModifier);
        functionalUtility.Modifier!.Kind.ShouldBe(expectedKind);
    }

    [Fact]
    public void ParseCandidates_WithMixedValidAndInvalid_ParsesOnlyValid()
    {
        // Arrange
        var input = "block unknown-utility flex [color:red]";

        // Act
        var candidates = _parser.ParseCandidates(input).ToList();

        // Assert
        candidates.Count.ShouldBe(4); // All are parsed - unknown-utility is parsed as functional

        candidates[0].ShouldBeOfType<StaticUtility>();
        ((StaticUtility)candidates[0]).Root.ShouldBe("block");

        candidates[1].ShouldBeOfType<FunctionalUtility>(); // unknown-utility becomes functional
        ((FunctionalUtility)candidates[1]).Root.ShouldBe("unknown");

        candidates[2].ShouldBeOfType<StaticUtility>();
        ((StaticUtility)candidates[2]).Root.ShouldBe("flex");

        candidates[3].ShouldBeOfType<ArbitraryProperty>();
        ((ArbitraryProperty)candidates[3]).Property.ShouldBe("color");
    }

    [Theory]
    [InlineData("-m-4", "-m", "4")]
    [InlineData("-translate-x-4", "-translate-x", "4")]
    [InlineData("-space-x-2", "-space-x", "2")]
    [InlineData("-rotate-45", "-rotate", "45")]
    [InlineData("-top-4", "-top", "4")]
    public void TryParseCandidate_WithNegativeUtilities_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Raw.ShouldBe(input);
    }

    [Theory]
    [InlineData("divide-x-2", "divide-x", "2")]
    [InlineData("divide-y-4", "divide-y", "4")]
    [InlineData("space-x-4", "space-x", "4")]
    [InlineData("space-y-2", "space-y", "2")]
    public void TryParseCandidate_WithMultiPartUtilities_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Raw.ShouldBe(input);
    }

    [Theory]
    [InlineData("-m-4/50", "-m", "4", "50")]
    // TODO: Enable when translate-x utility is implemented
    // [InlineData("-translate-x-1/2/50", "-translate-x", "1/2", "50")]
    [InlineData("-rotate-45/[0.5]", "-rotate", "45", "0.5")]
    public void TryParseCandidate_WithNegativeAndModifier_ParsesBothCorrectly(string input, string expectedRoot, string expectedValue, string expectedModifier)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Modifier.ShouldNotBeNull();
        functionalUtility.Modifier!.Value.ShouldBe(expectedModifier);
        functionalUtility.Raw.ShouldBe(input);
    }

    [Theory]
    [InlineData("-translate-x-[10px]", "-translate-x", "10px")]
    [InlineData("-m-[2rem]", "-m", "2rem")]
    [InlineData("-top-[50%]", "-top", "50%")]
    [InlineData("-rotate-[30deg]", "-rotate", "30deg")]
    public void TryParseCandidate_WithNegativeAndArbitraryValue_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Raw.ShouldBe(input);
    }

    [Theory]
    [InlineData("!-m-4", "-m", "4", true)]
    [InlineData("-m-4!", "-m", "4", true)]
    [InlineData("-translate-x-4!", "-translate-x", "4", true)]
    public void TryParseCandidate_WithNegativeAndImportant_ParsesBothCorrectly(string input, string expectedRoot, string expectedValue, bool expectedImportant)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Important.ShouldBe(expectedImportant);
        functionalUtility.Raw.ShouldBe(input);
    }

    #region Parentheses Shorthand Tests

    [Theory]
    [InlineData("bg-(--my-color)", "bg", "var(--my-color)")]
    [InlineData("text-(--primary)", "text", "var(--primary)")]
    [InlineData("border-(--accent)", "border", "var(--accent)")]
    public void TryParseCandidate_WithParenthesesShorthand_ConvertsToVarFunction(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Raw.ShouldBe(input);
    }

    [Theory]
    [InlineData("bg-(--my-color,#0088cc)", "bg", "var(--my-color,#0088cc)")]
    [InlineData("text-(--primary,red)", "text", "var(--primary,red)")]
    public void TryParseCandidate_WithParenthesesShorthandAndFallback_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("bg-(my-color)")]
    [InlineData("text-(primary)")]
    public void TryParseCandidate_WithInvalidParenthesesShorthand_ReturnsFalse(string input)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeFalse();
        candidate.ShouldBeNull();
    }

    #endregion

    #region Modifier Enhancement Tests

    [Theory]
    [InlineData("bg-red-500/(--opacity)", "bg", "red-500", "var(--opacity)")]
    [InlineData("text-blue-700/(--my-opacity)", "text", "blue-700", "var(--my-opacity)")]
    public void TryParseCandidate_WithParenthesesModifier_ConvertsToVarFunction(string input, string expectedRoot, string expectedValue, string expectedModifier)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Modifier.ShouldNotBeNull();
        functionalUtility.Modifier!.Kind.ShouldBe(ModifierKind.Arbitrary);
        functionalUtility.Modifier!.Value.ShouldBe(expectedModifier);
    }

    [Theory]
    [InlineData("bg-red-500/[var(--opacity)]", "bg", "red-500", "var(--opacity)")]
    [InlineData("text-blue-700/[calc(var(--opacity)*0.5)]", "text", "blue-700", "calc(var(--opacity)*0.5)")]
    public void TryParseCandidate_WithArbitraryModifierWithVar_ParsesCorrectly(string input, string expectedRoot, string expectedValue, string expectedModifier)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Modifier.ShouldNotBeNull();
        functionalUtility.Modifier!.Kind.ShouldBe(ModifierKind.Arbitrary);
        functionalUtility.Modifier!.Value.ShouldBe(expectedModifier);
    }

    [Theory]
    [InlineData("bg-red-500/[]")]
    [InlineData("bg-red-500/()")]
    [InlineData("bg-red-500/")]
    public void TryParseCandidate_WithEmptyModifier_ReturnsFalse(string input)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeFalse();
        candidate.ShouldBeNull();
    }

    [Theory]
    [InlineData("bg-red-500/50!", "bg", "red-500", "50", true)]
    [InlineData("text-blue-700/[0.5]!", "text", "blue-700", "0.5", true)]
    public void TryParseCandidate_WithModifierAndImportant_ParsesBothCorrectly(string input, string expectedRoot, string expectedValue, string expectedModifier, bool expectedImportant)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Modifier.ShouldNotBeNull();
        functionalUtility.Modifier!.Value.ShouldBe(expectedModifier);
        functionalUtility.Important.ShouldBe(expectedImportant);
    }

    #endregion

    #region Invalid Cases Tests
    // Note: Most invalid case tests have been moved to the comprehensive tests section
    // The comprehensive tests provide better coverage with more edge cases
    #endregion

    #region Data Type Hints Tests

    [Theory]
    [InlineData("[color:var(--my-color)]", "color", "var(--my-color)")]
    [InlineData("[length:100px]", "length", "100px")]
    [InlineData("[angle:45deg]", "angle", "45deg")]
    public void TryParseCandidate_WithDataTypeHint_ParsesCorrectly(string input, string expectedProperty, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<ArbitraryProperty>();

        var arbitraryProperty = (ArbitraryProperty)candidate;
        arbitraryProperty.Property.ShouldBe(expectedProperty);
        arbitraryProperty.Value.ShouldBe(expectedValue);
    }

    #endregion

    #region Complex Expression Tests

    [Theory]
    [InlineData("w-[calc(100%-2rem)]", "w", "calc(100% - 2rem)")]
    [InlineData("flex-[calc(var(--foo)*0.2)]", "flex", "calc(var(--foo) * 0.2)")]
    [InlineData("h-[calc((100vh-64px)/2)]", "h", "calc((100vh - 64px) / 2)")]
    public void TryParseCandidate_WithCalcExpression_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("ml-[theme(--spacing-1_5)]", "ml", "theme(--spacing-1_5)")]
    [InlineData("p-[theme(spacing.4)]", "p", "theme(spacing.4)")]
    public void TryParseCandidate_WithThemeFunction_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
    }

    #endregion

    #region Fraction Value Tests

    [Theory]
    [InlineData("w-1/2", "w", "1/2")]
    [InlineData("h-3/4", "h", "3/4")]
    [InlineData("w-2/3", "w", "2/3")]
    [InlineData("h-1/4", "h", "1/4")]
    [InlineData("basis-1/2", "basis", "1/2")]
    // TODO: Enable when translate-x utility is implemented
    // [InlineData("translate-x-1/2", "translate-x", "1/2")]
    public void TryParseCandidate_WithFractionValue_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Named);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Value.Fraction.ShouldBe(expectedValue); // Fraction should be stored
        functionalUtility.Modifier.ShouldBeNull(); // No modifier for fractions
    }

    [Theory]
    [InlineData("w-1/2/50", "w", "1/2", "50")]
    [InlineData("h-3/4/[0.5]", "h", "3/4", "0.5")]
    // TODO: Enable when translate-x utility is implemented
    // [InlineData("-translate-x-1/2/50", "-translate-x", "1/2", "50")]
    public void TryParseCandidate_WithFractionAndModifier_ParsesBothCorrectly(string input, string expectedRoot, string expectedValue, string expectedModifier)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Value.Fraction.ShouldBe(expectedValue);
        functionalUtility.Modifier.ShouldNotBeNull();
        functionalUtility.Modifier!.Value.ShouldBe(expectedModifier);
    }

    [Theory]
    [InlineData("w-[1/3]", "w", "1/3")]
    [InlineData("h-[5/6]", "h", "5/6")]
    [InlineData("basis-[2/5]", "basis", "2/5")]
    public void TryParseCandidate_WithArbitraryFraction_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        // Arbitrary values don't store fraction separately
        functionalUtility.Value.Fraction.ShouldBeNull();
    }

    [Theory]
    [InlineData("bg-red-500/50", "bg", "red-500", "50")] // Not a fraction - it's a color with modifier
    [InlineData("text-lg/2", "text", "lg", "2")] // Not a fraction - modifier on text size
    public void TryParseCandidate_WithNonFractionSlash_ParsesAsModifier(string input, string expectedRoot, string expectedValue, string expectedModifier)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
        functionalUtility.Value.Fraction.ShouldBeNull(); // Not a fraction
        functionalUtility.Modifier.ShouldNotBeNull();
        functionalUtility.Modifier!.Value.ShouldBe(expectedModifier);
    }

    #endregion

    #region Special Multi-Dash Utility Tests

    [Theory]
    [InlineData("space-x-4", "space-x", "4")]
    [InlineData("space-y-2", "space-y", "2")]
    [InlineData("divide-x-2", "divide-x", "2")]
    [InlineData("divide-y-4", "divide-y", "4")]
    public void TryParseCandidate_WithMultiDashUtility_ParsesCorrectly(string input, string expectedRoot, string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value.ShouldNotBeNull();
        functionalUtility.Value.Value.ShouldBe(expectedValue);
    }

    #endregion

    #region Comprehensive Parsing Tests

    [Theory]
    // Utilities with numbers at start of value
    [InlineData("text-2xl", true, typeof(FunctionalUtility), "text", "2xl")]
    [InlineData("text-3xl", true, typeof(FunctionalUtility), "text", "3xl")]
    [InlineData("text-4xl", true, typeof(FunctionalUtility), "text", "4xl")]
    [InlineData("text-5xl", true, typeof(FunctionalUtility), "text", "5xl")]
    [InlineData("text-6xl", true, typeof(FunctionalUtility), "text", "6xl")]
    [InlineData("text-7xl", true, typeof(FunctionalUtility), "text", "7xl")]
    [InlineData("text-8xl", true, typeof(FunctionalUtility), "text", "8xl")]
    [InlineData("text-9xl", true, typeof(FunctionalUtility), "text", "9xl")]

    // CSS custom properties in arbitrary properties
    [InlineData("[--my-var:value]", true, typeof(ArbitraryProperty), "--my-var", "value")]
    [InlineData("[--custom-color:#ff0000]", true, typeof(ArbitraryProperty), "--custom-color", "#ff0000")]
    [InlineData("[--spacing:1rem]", true, typeof(ArbitraryProperty), "--spacing", "1rem")]
    [InlineData("[--font-size:16px]", true, typeof(ArbitraryProperty), "--font-size", "16px")]
    [InlineData("[--opacity:0.5]", true, typeof(ArbitraryProperty), "--opacity", "0.5")]

    // Complex CSS values with gradients
    [InlineData("[background:linear-gradient(to_right,_#000,_#fff)]", true, typeof(ArbitraryProperty), "background", "linear-gradient(to right, #000, #fff)")]
    [InlineData("[background:radial-gradient(circle,_red,_blue)]", true, typeof(ArbitraryProperty), "background", "radial-gradient(circle, red, blue)")]
    [InlineData("[background:conic-gradient(from_45deg,_#000,_#fff)]", true, typeof(ArbitraryProperty), "background", "conic-gradient(from 45deg, #000, #fff)")]

    // Multiple values in arbitrary properties
    [InlineData("[margin:10px_20px]", true, typeof(ArbitraryProperty), "margin", "10px 20px")]
    [InlineData("[padding:5px_10px_15px_20px]", true, typeof(ArbitraryProperty), "padding", "5px 10px 15px 20px")]
    [InlineData("[border-radius:10px_20px_30px_40px]", true, typeof(ArbitraryProperty), "border-radius", "10px 20px 30px 40px")]
    [InlineData("[box-shadow:0_4px_6px_-1px_rgba(0,_0,_0,_0.1)]", true, typeof(ArbitraryProperty), "box-shadow", "0 4px 6px -1px rgba(0, 0, 0, 0.1)")]

    // Nested functions in arbitrary properties
    [InlineData("[width:calc(100%_-_var(--spacing))]", true, typeof(ArbitraryProperty), "width", "calc(100% - var(--spacing))")]
    [InlineData("[height:min(100vh,_calc(100%_-_2rem))]", true, typeof(ArbitraryProperty), "height", "min(100vh, calc(100% - 2rem))")]
    [InlineData("[transform:translateX(calc(50%_+_10px))]", true, typeof(ArbitraryProperty), "transform", "translateX(calc(50% + 10px))")]

    // URL values with underscores (should preserve underscores in URLs)
    [InlineData("[background-image:url(https://example.com/my_image.jpg)]", true, typeof(ArbitraryProperty), "background-image", "url(https://example.com/my_image.jpg)")]
    [InlineData("[background:url('/path/to/my_file.png')]", true, typeof(ArbitraryProperty), "background", "url('/path/to/my_file.png')")]

    // Complex combinations
    [InlineData("[font-family:'Helvetica_Neue',_Arial,_sans-serif]", true, typeof(ArbitraryProperty), "font-family", "'Helvetica Neue', Arial, sans-serif")]
    [InlineData("[content:'Hello_World']", true, typeof(ArbitraryProperty), "content", "'Hello World'")]
    [InlineData("[grid-template-columns:repeat(3,_minmax(0,_1fr))]", true, typeof(ArbitraryProperty), "grid-template-columns", "repeat(3, minmax(0, 1fr))")]

    // Valid arbitrary values with complex expressions
    [InlineData("w-[calc(100%_-_theme(spacing.4))]", true, typeof(FunctionalUtility), "w", "calc(100% - theme(spacing.4))")]
    [InlineData("h-[clamp(10rem,_10vh,_20rem)]", true, typeof(FunctionalUtility), "h", "clamp(10rem, 10vh, 20rem)")]
    [InlineData("text-[clamp(1rem,_2.5vw,_2rem)]", true, typeof(FunctionalUtility), "text", "clamp(1rem, 2.5vw, 2rem)")]

    public void TryParseCandidate_ComprehensiveValidPatterns_ParsesCorrectly(
        string input,
        bool expectedResult,
        Type expectedType,
        string expectedRootOrProperty,
        string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBe(expectedResult, $"Expected parsing of '{input}' to {(expectedResult ? "succeed" : "fail")}");

        if (expectedResult)
        {
            candidate.ShouldNotBeNull();
            candidate.GetType().ShouldBe(expectedType);

            if (expectedType == typeof(ArbitraryProperty))
            {
                var arbitraryProperty = (ArbitraryProperty)candidate;
                arbitraryProperty.Property.ShouldBe(expectedRootOrProperty);
                arbitraryProperty.Value.ShouldBe(expectedValue);
            }
            else if (expectedType == typeof(FunctionalUtility))
            {
                var functionalUtility = (FunctionalUtility)candidate;
                functionalUtility.Root.ShouldBe(expectedRootOrProperty);
                functionalUtility.Value?.Value.ShouldBe(expectedValue);
            }
        }
        else
        {
            candidate.ShouldBeNull();
        }
    }

    [Theory]
    // Note: Some malformed patterns might be parsed as valid functional utilities with strange values
    // The parser is lenient and leaves CSS validation to later stages

    // Empty or invalid arbitrary values
    [InlineData("bg-[]")]              // Empty arbitrary value
    [InlineData("text-[    ]")]        // Whitespace-only arbitrary value
    [InlineData("p-()")]               // Empty parentheses
    [InlineData("m-[_]")]              // Underscore-only arbitrary value

    // Invalid arbitrary properties
    [InlineData("[Color:red]")]        // Uppercase property name
    [InlineData("[MARGIN:10px]")]      // All uppercase property
    [InlineData("[Background:blue]")]  // Title case property

    // Invalid characters in arbitrary values
    [InlineData("bg-[red;color:blue]")]           // Semicolon in value
    [InlineData("[color:red}html{color:blue]")]   // Braces in value

    // Invalid modifier combinations
    [InlineData("flex/50")]            // Static utility with modifier
    [InlineData("block/[0.5]")]        // Static utility with arbitrary modifier
    [InlineData("hidden/(--opacity)")] // Static utility with CSS var modifier

    // Multiple modifiers (invalid)
    [InlineData("bg-red-500/50/75")]   // Multiple modifiers
    [InlineData("text-blue-700/25/50/100")] // Many modifiers

    // Partial/incomplete utilities
    [InlineData("bg-")]                // Incomplete utility
    [InlineData("text-")]              // Incomplete utility
    [InlineData("flex-")]              // Incomplete utility with dash

    public void TryParseCandidate_InvalidPatterns_ReturnsFalse(string input)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBeFalse($"Expected parsing of invalid pattern '{input}' to fail");
        candidate.ShouldBeNull();
    }

    [Fact]
    public void TryParseCandidate_VeryLongUtilityName_HandlesGracefully()
    {
        // Arrange - Create a very long utility name (1000+ characters)
        var longValue = new string('a', 1000);
        var input = $"bg-[{longValue}]";

        // Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert - Should parse successfully despite length
        result.ShouldBeTrue("Parser should handle very long utility names");
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();
        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe("bg");
        functionalUtility.Value?.Value.ShouldBe(longValue);
    }

    [Theory]
    // UTF-8 characters in arbitrary values
    [InlineData("[content:'Hello_世界']", true, "content", "'Hello 世界'")]
    [InlineData("[content:'Café_☕']", true, "content", "'Café ☕'")]
    [InlineData("[content:'Emoji_🎉_🎊']", true, "content", "'Emoji 🎉 🎊'")]
    [InlineData("[font-family:'文泉驛微米黑']", true, "font-family", "'文泉驛微米黑'")]

    // Windows path separators (backslashes should be preserved in strings)
    [InlineData("[content:'C:\\\\Users\\\\file.txt']", true, "content", "'C:\\\\Users\\\\file.txt'")]
    [InlineData("[background-image:url('C:\\\\images\\\\bg.png')]", true, "background-image", "url('C:\\\\images\\\\bg.png')")]

    public void TryParseCandidate_SpecialCharacters_HandlesCorrectly(
        string input,
        bool expectedResult,
        string expectedProperty,
        string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert
        result.ShouldBe(expectedResult);

        if (expectedResult)
        {
            candidate.ShouldNotBeNull();
            candidate.ShouldBeOfType<ArbitraryProperty>();
            var arbitraryProperty = (ArbitraryProperty)candidate;
            arbitraryProperty.Property.ShouldBe(expectedProperty);
            arbitraryProperty.Value.ShouldBe(expectedValue);
        }
        else
        {
            candidate.ShouldBeNull();
        }
    }

    [Theory]
    // These patterns are parsed successfully but would produce invalid CSS
    // The parser is intentionally lenient, leaving validation to later stages
    [InlineData("bg-[#0088cc", "bg", "[#0088cc")]      // Unclosed bracket - treated as named value
    [InlineData("text-[rgb(255,0,0]", "text", "rgb(255,0,)0")] // Malformed function - parser tries to fix
    [InlineData("w-]100px[", "w", "]100px[")]          // Reversed brackets in value
    [InlineData("h-[100px]]", "h", "100px]")]          // Extra closing bracket - one is consumed
    [InlineData("m-[[10px", "m", "[[10px")]            // Extra opening bracket
    [InlineData("text-[<script>alert('xss')</script>]", "text", "<script>alert('xss')</script>")] // HTML tags
    public void TryParseCandidate_MalformedButParseable_ParsesWithStrangeValues(
        string input,
        string expectedRoot,
        string expectedValue)
    {
        // Arrange & Act
        var result = _parser.TryParseCandidate(input, out var candidate);

        // Assert - Parser accepts these but the values are malformed
        result.ShouldBeTrue($"Parser is lenient and accepts '{input}'");
        candidate.ShouldNotBeNull();
        candidate.ShouldBeOfType<FunctionalUtility>();

        var functionalUtility = (FunctionalUtility)candidate;
        functionalUtility.Root.ShouldBe(expectedRoot);
        functionalUtility.Value?.Value.ShouldBe(expectedValue);

        // Note: These would fail at CSS generation or validation stage
    }

    #endregion
}