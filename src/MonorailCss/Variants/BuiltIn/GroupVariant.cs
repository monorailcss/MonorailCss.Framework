using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for group interactions (group-hover, group-focus, group-aria-expanded, etc.).
/// </summary>
internal class GroupVariant : IVariant
{
    private readonly VariantRegistry _registry;

    public GroupVariant(int weight, VariantRegistry registry)
    {
        Weight = weight;
        _registry = registry;
    }

    public string Name => "group";
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Compound;
    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        // Handles both "group" and compound variants like "group-hover"
        return token.Name == "group" ||
               (token.Name == "group" && token.Value != null);
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        if (token.Value == null)
        {
            // Bare "group" is just a marker class; no selector transform.
            return false;
        }

        var groupClass = token.Modifier != null
            ? $".group\\/{token.Modifier}"
            : ".group";

        // Arbitrary selector form: group-[.is-splitted], group-[&.dark], group-[[data-x]]
        // Tailwind v4 semantics: if the bracket body contains `&`, the ampersand is replaced
        // with the group reference; otherwise the body is appended directly so a leading `.`,
        // `:`, or `[` reads as an additional qualifier on the same group element.
        if (IsArbitraryValue(token.Value, out var inner))
        {
            var groupSelector = inner.Contains('&')
                ? inner.Replace("&", $":where({groupClass})")
                : $":where({groupClass}){inner}";

            result = current.TransformSelector(s =>
                new Selector($"{s.Value}:is({groupSelector} *)"));
            return true;
        }

        // Named sub-variant (group-hover, group-focus, group-aria-expanded, group-open,
        // group-data-[state=open], ...). Delegate to the full variant vocabulary by applying the
        // sub-variant to the group reference, then descend from it: `:is(:where(.group)<sub> *)`.
        if (_registry.TryApplySubVariant(token.Value, new Selector($":where({groupClass})"), out var composed))
        {
            result = current.TransformSelector(s =>
                new Selector($"{s.Value}:is({composed.Value} *)"));
            return true;
        }

        return false;
    }

    private static bool IsArbitraryValue(string value, out string inner)
    {
        if (value.Length >= 2 && value[0] == '[' && value[^1] == ']')
        {
            inner = value[1..^1];
            return true;
        }

        inner = string.Empty;
        return false;
    }
}

/// <summary>
/// Variant for peer interactions (peer-hover, peer-focus, peer-aria-expanded, etc.).
/// </summary>
internal class PeerVariant : IVariant
{
    private readonly VariantRegistry _registry;

    public PeerVariant(int weight, VariantRegistry registry)
    {
        Weight = weight;
        _registry = registry;
    }

    public string Name => "peer";
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Compound;
    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        // Handles both "peer" and compound variants like "peer-hover"
        return token.Name == "peer" ||
               (token.Name == "peer" && token.Value != null);
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        if (token.Value == null)
        {
            return false;
        }

        var peerClass = token.Modifier != null
            ? $".peer\\/{token.Modifier}"
            : ".peer";

        if (IsArbitraryValue(token.Value, out var inner))
        {
            var peerSelector = inner.Contains('&')
                ? inner.Replace("&", $":where({peerClass})")
                : $":where({peerClass}){inner}";

            result = current.TransformSelector(s =>
                new Selector($"{s.Value}:is({peerSelector} ~ *)"));
            return true;
        }

        // Named sub-variant — delegate as with group, but as a following sibling: `~ *`.
        if (_registry.TryApplySubVariant(token.Value, new Selector($":where({peerClass})"), out var composed))
        {
            result = current.TransformSelector(s =>
                new Selector($"{s.Value}:is({composed.Value} ~ *)"));
            return true;
        }

        return false;
    }

    private static bool IsArbitraryValue(string value, out string inner)
    {
        if (value.Length >= 2 && value[0] == '[' && value[^1] == ']')
        {
            inner = value[1..^1];
            return true;
        }

        inner = string.Empty;
        return false;
    }
}
