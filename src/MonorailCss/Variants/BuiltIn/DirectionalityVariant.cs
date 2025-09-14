using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for directionality (rtl, ltr).
/// </summary>
internal class DirectionalityVariant : IVariant
{
    public DirectionalityVariant(string name, int weight)
    {
        Name = name;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // Apply directionality attribute selector
        var dir = Name == "rtl" ? "rtl" : "ltr";
        result = current.TransformSelector(s => s.WithAttribute($"[dir=\"{dir}\"]"));
        return true;
    }
}

/// <summary>
/// Custom dark variant implementation that matches Tailwind v4's
/// @custom-variant dark (&amp;:where(.dark, .dark *)) syntax.
/// </summary>
internal class WhereClauseDarkVariant : IVariant
{
    public WhereClauseDarkVariant(int weight)
    {
        Weight = weight;
    }

    public string Name => "dark";
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // Apply the Tailwind v4 custom variant syntax: &:where(.dark, .dark *)
        // This means the element either has the .dark class or is a descendant of .dark
        result = current.TransformSelector(s => new Selector(s.Value).InWhereWithMultiple(".dark", ".dark *"));

        return true;
    }
}

/// <summary>
/// Variant for color scheme (dark mode).
/// </summary>
internal class ColorSchemeVariant : IVariant
{
    public ColorSchemeVariant(string name, int weight)
    {
        Name = name;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // Dark mode can use either class strategy or media query
        // For now, we'll use the class strategy (.dark ancestor)
        result = current.TransformSelector(s => s.DescendantOf(".dark"));

        // Alternative: Media query strategy
        // result = current.WithWrapper(AtRuleWrapper.Media("(prefers-color-scheme: dark)"));
        return true;
    }
}

/// <summary>
/// Variant for motion preferences (motion-safe, motion-reduce).
/// </summary>
internal class MotionVariant : IVariant
{
    public MotionVariant(string name, int weight)
    {
        Name = name;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // Apply motion preference media query
        var motionQuery = Name == "motion-safe"
            ? "(prefers-reduced-motion: no-preference)"
            : "(prefers-reduced-motion: reduce)";

        result = current.WithWrapper(AtRuleWrapper.Media(motionQuery));
        return true;
    }
}

/// <summary>
/// Variant for print media queries.
/// </summary>
internal class PrintVariant : IVariant
{
    public PrintVariant(int weight)
    {
        Weight = weight;
    }

    public string Name => "print";
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        result = current.WithWrapper(AtRuleWrapper.Media("print"));
        return true;
    }
}

/// <summary>
/// Variant for contrast preferences (contrast-more, contrast-less).
/// </summary>
internal class ContrastVariant : IVariant
{
    public ContrastVariant(string name, int weight)
    {
        Name = name;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // Apply contrast preference media query
        var contrastQuery = Name == "contrast-more"
            ? "(prefers-contrast: more)"
            : "(prefers-contrast: less)";

        result = current.WithWrapper(AtRuleWrapper.Media(contrastQuery));
        return true;
    }
}