namespace MonorailCss.Css;

internal readonly record struct Selector(string Value)
{
    /// <summary>
    /// Creates a selector from an escaped class name.
    /// </summary>
    public static Selector FromClass(string escapedClassName) => new($".{escapedClassName}");

    /// <summary>
    /// Appends a pseudo-class or pseudo-element to the selector.
    /// Example: ".btn" + ":hover" = ".btn:hover".
    /// </summary>
    public Selector WithPseudo(string pseudo) => new($"{Value}{pseudo}");

    /// <summary>
    /// Replaces "&amp;" in the relative selector with this selector's value.
    /// Example: ".btn".Relativize("[&amp;&gt;*]") = ".btn &gt; *".
    /// </summary>
    public Selector Relativize(string relative) => new(relative.Replace("&", Value));

    /// <summary>
    /// Makes this selector a descendant of the ancestor selector.
    /// Example: ".btn".DescendantOf(".dark") = ".dark .btn".
    /// </summary>
    public Selector DescendantOf(string ancestor) => new($"{ancestor} {Value}");

    /// <summary>
    /// Makes this selector a sibling of another selector with the given combinator.
    /// Example: ".btn".SiblingOf(".icon", "~") = ".icon ~ .btn".
    /// </summary>
    public Selector SiblingOf(string sibling, string combinator) => new($"{sibling} {combinator} {Value}");

    /// <summary>
    /// Prefixes the selector with a class selector (for group/peer variants).
    /// Example: ".btn".WithClassPrefix(".group:hover") = ".group:hover .btn".
    /// </summary>
    public Selector WithClassPrefix(string classSelector) => new($"{classSelector} {Value}");

    /// <summary>
    /// Combines this selector with another using the given combinator.
    /// Example: ".btn".Combine(".active", "") = ".btn.active"
    /// Example: ".parent".Combine(".child", " &gt; ") = ".parent &gt; .child".
    /// </summary>
    public Selector Combine(string otherSelector, string combinator = "") =>
        string.IsNullOrEmpty(combinator)
            ? new($"{Value}{otherSelector}")
            : new($"{Value}{combinator}{otherSelector}");

    /// <summary>
    /// Wraps the selector with an attribute selector.
    /// Example: ".btn".WithAttribute("[dir=\"rtl\"]") = "[dir=\"rtl\"] .btn".
    /// </summary>
    public Selector WithAttribute(string attribute) => new($"{attribute} {Value}");

    /// <summary>
    /// Adds a :where() wrapper to reduce specificity.
    /// Example: ".btn".InWhere() = ":where(.btn)".
    /// </summary>
    public Selector InWhere() => new($":where({Value})");

    /// <summary>
    /// Creates a complex :where() clause with multiple selectors.
    /// Example: "&amp;".InWhereWithMultiple(".dark", ".dark *") = "&amp;:where(.dark, .dark *)"
    /// Used for custom variant definitions like dark mode.
    /// </summary>
    public Selector InWhereWithMultiple(params string[] selectors) =>
        new($"{Value}:where({string.Join(", ", selectors)})");

    /// <summary>
    /// Adds a :is() wrapper for grouping selectors.
    /// Example: ".btn".InIs() = ":is(.btn)".
    /// </summary>
    public Selector InIs() => new($":is({Value})");

    /// <summary>
    /// Adds a :not() wrapper for negation.
    /// Example: ".btn".InNot() = ":not(.btn)".
    /// </summary>
    public Selector InNot() => new($":not({Value})");

    /// <summary>
    /// Adds a complex :not(:where()) wrapper for prose-style exclusion.
    /// Example: "p".WithNotProse("not-prose") = ":where(p):not(:where([class~="not-prose"],[class~="not-prose"] *))".
    /// </summary>
    public Selector WithNotProse(string excludeClass) =>
        new($":where({Value}):not(:where([class~=\"{excludeClass}\"],[class~=\"{excludeClass}\"] *))");

    /// <summary>
    /// Adds prose-style element targeting with parent selector and not-prose exclusion.
    /// Example: ".prose", "p", "not-prose" = ".prose :where(p):not(:where([class~="not-prose"],[class~="not-prose"] *))".
    /// </summary>
    public static Selector ProseElement(string parentClass, string element, string excludeClass = "not-prose") =>
        new($"{parentClass} :where({element}):not(:where([class~=\"{excludeClass}\"],[class~=\"{excludeClass}\"] *))");

    public override string ToString() => Value;
}