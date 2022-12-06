using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text size plugin.
/// </summary>
public class TextSize : IUtilityNamespacePlugin
{
    private const string Namespace = "text";

    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextSize"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public TextSize(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Namespace) || namespaceSyntax.Suffix == default)
        {
            yield break;
        }

        if (!_designSystem.Typography.TryGetValue(namespaceSyntax.Suffix, out var value))
        {
            yield break;
        }

        yield return new CssRuleSet(
            namespaceSyntax.OriginalSyntax,
            DeclarationList(value));
    }

    private static CssDeclarationList DeclarationList(MonorailCss.Typography value)
    {
        return new CssDeclarationList
        {
            (CssProperties.FontSize, value.FontSize), (CssProperties.LineHeight, value.LineHeight),
        };
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _designSystem
            .Typography
            .Select(typography => new CssRuleSet($"text-{typography.Key}", DeclarationList(typography.Value)));
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { "text" }.ToImmutableArray();
}