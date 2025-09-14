using System.Collections.Immutable;
using MonorailCss.Candidates;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Candidates;

public class CandidateTests
{
    [Fact]
    public void StaticUtility_Should_Create_With_Root()
    {
        var candidate = new StaticUtility
        {
            Raw = "block",
            Root = "block",
            Variants = ImmutableArray<VariantToken>.Empty
        };

        candidate.Raw.ShouldBe("block");
        candidate.Root.ShouldBe("block");
        candidate.Variants.ShouldBeEmpty();
        candidate.Important.ShouldBe(false);
        candidate.Modifier.ShouldBeNull();
    }

    [Fact]
    public void FunctionalUtility_Should_Create_With_Root_And_Value()
    {
        var value = CandidateValue.Named("red-500");
        var candidate = new FunctionalUtility
        {
            Raw = "bg-red-500",
            Root = "bg",
            Value = value,
            Variants = ImmutableArray<VariantToken>.Empty
        };

        candidate.Raw.ShouldBe("bg-red-500");
        candidate.Root.ShouldBe("bg");
        candidate.Value.ShouldBe(value);
        candidate.Value.Kind.ShouldBe(ValueKind.Named);
        candidate.Value.Value.ShouldBe("red-500");
    }

    [Fact]
    public void FunctionalUtility_Should_Support_Arbitrary_Value()
    {
        var value = CandidateValue.Arbitrary("#123456");
        var candidate = new FunctionalUtility
        {
            Raw = "bg-[#123456]",
            Root = "bg",
            Value = value,
            Variants = ImmutableArray<VariantToken>.Empty
        };

        candidate.Value.Kind.ShouldBe(ValueKind.Arbitrary);
        candidate.Value.Value.ShouldBe("#123456");
    }

    [Fact]
    public void ArbitraryProperty_Should_Create_With_Property_And_Value()
    {
        var candidate = new ArbitraryProperty
        {
            Raw = "[display:flex]",
            Property = "display",
            Value = "flex",
            Variants = ImmutableArray<VariantToken>.Empty
        };

        candidate.Raw.ShouldBe("[display:flex]");
        candidate.Property.ShouldBe("display");
        candidate.Value.ShouldBe("flex");
    }

    [Fact]
    public void Candidate_Should_Support_Important_Flag()
    {
        var candidate = new StaticUtility
        {
            Raw = "!block",
            Root = "block",
            Important = true,
            Variants = ImmutableArray<VariantToken>.Empty
        };

        candidate.Important.ShouldBe(true);
    }

    [Fact]
    public void Candidate_Should_Support_Modifier()
    {
        var modifier = Modifier.Named("50");
        var candidate = new FunctionalUtility
        {
            Raw = "bg-red-500/50",
            Root = "bg",
            Value = CandidateValue.Named("red-500"),
            Modifier = modifier,
            Variants = ImmutableArray<VariantToken>.Empty
        };

        candidate.Modifier.ShouldBe(modifier);
        candidate.Modifier.Kind.ShouldBe(ModifierKind.Named);
        candidate.Modifier.Value.ShouldBe("50");
    }

    [Fact]
    public void Candidate_Should_Support_Variants()
    {
        var variants = ImmutableArray.Create(
            new VariantToken("hover", null, null, "hover"),
            new VariantToken("focus", null, null, "focus")
        );

        var candidate = new StaticUtility
        {
            Raw = "hover:focus:block",
            Root = "block",
            Variants = variants
        };

        candidate.Variants.Length.ShouldBe(2);
        candidate.Variants[0].ShouldBeOfType<VariantToken>();
        candidate.Variants[0].Name.ShouldBe("hover");
        candidate.Variants[1].ShouldBeOfType<VariantToken>();
        candidate.Variants[1].Name.ShouldBe("focus");
    }

    [Fact]
    public void CandidateValue_Named_Should_Create_Named_Value()
    {
        var value = CandidateValue.Named("red-500");

        value.Kind.ShouldBe(ValueKind.Named);
        value.Value.ShouldBe("red-500");
        value.ToString().ShouldBe("red-500");
    }

    [Fact]
    public void CandidateValue_Arbitrary_Should_Create_Arbitrary_Value()
    {
        var value = CandidateValue.Arbitrary("#123456");

        value.Kind.ShouldBe(ValueKind.Arbitrary);
        value.Value.ShouldBe("#123456");
        value.ToString().ShouldBe("[#123456]");
    }

    [Fact]
    public void Modifier_Named_Should_Create_Named_Modifier()
    {
        var modifier = Modifier.Named("50");

        modifier.Kind.ShouldBe(ModifierKind.Named);
        modifier.Value.ShouldBe("50");
        modifier.ToString().ShouldBe("50");
    }

    [Fact]
    public void Modifier_Arbitrary_Should_Create_Arbitrary_Modifier()
    {
        var modifier = Modifier.Arbitrary("0.25");

        modifier.Kind.ShouldBe(ModifierKind.Arbitrary);
        modifier.Value.ShouldBe("0.25");
        modifier.ToString().ShouldBe("[0.25]");
    }

    [Fact]
    public void Candidate_Equality_Should_Work()
    {
        var candidate1 = new StaticUtility
        {
            Raw = "block",
            Root = "block",
            Variants = ImmutableArray<VariantToken>.Empty
        };

        var candidate2 = new StaticUtility
        {
            Raw = "block",
            Root = "block",
            Variants = ImmutableArray<VariantToken>.Empty
        };

        candidate1.ShouldBe(candidate2);
    }
}