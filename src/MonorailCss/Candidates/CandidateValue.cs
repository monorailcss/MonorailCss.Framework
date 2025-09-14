namespace MonorailCss.Candidates;

internal enum ValueKind
{
    Named,
    Arbitrary,
}

internal record CandidateValue(ValueKind Kind, string Value, string? Fraction = null)
{
    public static CandidateValue Named(string value, string? fraction = null) => new(ValueKind.Named, value, fraction);
    public static CandidateValue Arbitrary(string value) => new(ValueKind.Arbitrary, value);

    public override string ToString() => Kind == ValueKind.Arbitrary ? $"[{Value}]" : Value;
}