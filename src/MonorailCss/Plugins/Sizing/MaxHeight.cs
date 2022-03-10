using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MaxHeight : BaseUtilityNamespacePlugin
{
    private const string Namespace = "max-h";

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxHeight"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public MaxHeight(DesignSystem designSystem)
    {
        Values = new Dictionary<string, string>()
            {
                { "full", "100%" },
                { "screen", "100vh" },
                { "min", "min-content" },
                { "max", "max-content" },
                { "fit", "fit-content" },
            }.ToImmutableDictionary()
            .AddRange(designSystem.Spacing);
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new() { { Namespace, "max-height" }, };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}