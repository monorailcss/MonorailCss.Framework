using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for group interactions (group-hover, group-focus, etc.).
/// </summary>
internal class GroupVariant : IVariant
{
    public GroupVariant(int weight)
    {
        Weight = weight;
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

        // For compound group variants (group-hover, group-focus, etc.)
        if (token.Value != null)
        {
            var pseudoClass = GetPseudoClass(token.Value);
            if (pseudoClass != null)
            {
                // Use modern Tailwind v4 selector: :is(:where(.group):hover *)
                var groupClass = token.Modifier != null
                    ? $".group\\/{token.Modifier}"
                    : ".group";

                // Transform to: .selector:is(:where(.group):hover *)
                result = current.TransformSelector(s =>
                    new Selector($"{s.Value}:is(:where({groupClass}){pseudoClass} *)"));
                return true;
            }
        }

        // For simple "group" variant (used as a marker class)
        // This typically wouldn't transform the selector itself
        return false;
    }

    private string? GetPseudoClass(string value)
    {
        return value switch
        {
            "hover" => ":hover",
            "focus" => ":focus",
            "focus-visible" => ":focus-visible",
            "active" => ":active",
            "visited" => ":visited",
            "disabled" => ":disabled",
            "checked" => ":checked",
            _ => null,
        };
    }
}

/// <summary>
/// Variant for peer interactions (peer-hover, peer-focus, etc.).
/// </summary>
internal class PeerVariant : IVariant
{
    public PeerVariant(int weight)
    {
        Weight = weight;
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

        // For compound peer variants (peer-hover, peer-focus, etc.)
        if (token.Value != null)
        {
            var pseudoClass = GetPseudoClass(token.Value);
            if (pseudoClass != null)
            {
                // Use modern Tailwind v4 selector: :is(:where(.peer):focus ~ *)
                var peerClass = token.Modifier != null
                    ? $".peer\\/{token.Modifier}"
                    : ".peer";

                // Transform to: .selector:is(:where(.peer):focus ~ *)
                result = current.TransformSelector(s =>
                    new Selector($"{s.Value}:is(:where({peerClass}){pseudoClass} ~ *)"));
                return true;
            }
        }

        // For simple "peer" variant (used as a marker class)
        // This typically wouldn't transform the selector itself
        return false;
    }

    private string? GetPseudoClass(string value)
    {
        return value switch
        {
            "hover" => ":hover",
            "focus" => ":focus",
            "focus-visible" => ":focus-visible",
            "active" => ":active",
            "visited" => ":visited",
            "disabled" => ":disabled",
            "checked" => ":checked",
            _ => null,
        };
    }
}