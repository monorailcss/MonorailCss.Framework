using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Layout;

/// <summary>
/// The container plugin.
/// </summary>
public class Container : IUtilityNamespacePlugin
{
    private const string Ns = "container";
    private readonly Settings _settings;
    private readonly ImmutableDictionary<string, string> _screens;

    /// <summary>
    /// Settings for container.
    /// </summary>
    public class Settings : ISettings<Container>
    {
        /// <summary>
        /// Gets a value indicating whether gets whether the container should be centered by default. Defaults to false.
        /// </summary>
        public bool Center { get; init; }

        /// <summary>
        /// Gets a mapping of padding to apply by default for a padding. The key should match the screens list, and a
        /// DEFAULT value will match any screens not in the list.
        /// </summary>
        public ImmutableDictionary<string, string> Padding { get; init; } = ImmutableDictionary<string, string>.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Container"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="settings">The settings.</param>
    public Container(DesignSystem designSystem, Settings settings)
    {
        _settings = settings;
        _screens = designSystem.Screens;
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { Ns }.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        CssDeclarationList declarations;
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Ns))
        {
            yield break;
        }

        var paddingKey = namespaceSyntax.Suffix ?? "DEFAULT";

        if (namespaceSyntax.Suffix == null)
        {
            declarations = GetDeclarations("width", "100%", paddingKey);
        }
        else
        {
            if (!_screens.TryGetValue(namespaceSyntax.Suffix, out var value))
            {
                yield break;
            }

            declarations = GetDeclarations("max-width", value, paddingKey);
        }

        yield return new CssRuleSet(syntax.OriginalSyntax, declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        yield break;
    }

    private CssDeclarationList GetDeclarations(string property, string value, string paddingKey)
    {
        var declarationList = new CssDeclarationList { (property, value), };
        if (_settings.Center)
        {
            declarationList.Add(new CssDeclaration(CssProperties.MarginLeft, "auto"));
            declarationList.Add(new CssDeclaration(CssProperties.MarginRight, "auto"));
        }

        if (_settings.Padding.TryGetValue(paddingKey, out var padding))
        {
            declarationList.Add(new CssDeclaration(CssProperties.PaddingLeft, padding));
            declarationList.Add(new CssDeclaration(CssProperties.PaddingRight, padding));
        }

        return declarationList;
    }
}