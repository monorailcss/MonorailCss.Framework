using System.Collections.Immutable;
using MonorailCss.Variants;

namespace MonorailCss.Candidates;

internal abstract record Candidate
{
    public required string Raw { get; init; }
    public required ImmutableArray<VariantToken> Variants { get; init; }
    public Modifier? Modifier { get; init; }
    public bool Important { get; init; }
    public string? Normalized { get; init; }

    protected Candidate()
    {
        Variants = ImmutableArray<VariantToken>.Empty;
    }
}

internal record StaticUtility : Candidate
{
    public required string Root { get; init; }

    public override string ToString() => $"StaticUtility({Root})";
}

internal record FunctionalUtility : Candidate
{
    public required string Root { get; init; }
    public required CandidateValue? Value { get; init; }

    public override string ToString() => Value != null ? $"FunctionalUtility({Root}-{Value})" : $"FunctionalUtility({Root})";
}

internal record ArbitraryProperty : Candidate
{
    public required string Property { get; init; }
    public required string Value { get; init; }

    public override string ToString() => $"ArbitraryProperty([{Property}:{Value}])";
}