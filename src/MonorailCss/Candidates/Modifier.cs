namespace MonorailCss.Candidates;

internal enum ModifierKind
{
    Named,
    Arbitrary,
}

internal record Modifier(ModifierKind Kind, string Value)
{
    public static Modifier Named(string value) => new(ModifierKind.Named, value);
    public static Modifier Arbitrary(string value) => new(ModifierKind.Arbitrary, value);

    public override string ToString() => Kind == ModifierKind.Arbitrary ? $"[{Value}]" : Value;
}