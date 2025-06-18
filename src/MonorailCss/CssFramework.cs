using System.Collections.Immutable;
using System.Text;
using MonorailCss.Css;
using MonorailCss.Framework.Processing;
using MonorailCss.Parser;
using MonorailCss.Plugins;
using MonorailCss.Variants;

namespace MonorailCss;

/// <summary>
/// Settings for the CSS Framework.
/// </summary>
public record CssFrameworkSettings
{
    /// <summary>
    /// Gets the design system.
    /// </summary>
    public DesignSystem DesignSystem { get; init; } = DesignSystem.Default;

    /// <summary>
    /// Gets the plugin-settings.
    /// </summary>
    public IList<ISettings> PluginSettings { get; init; } = ImmutableList<ISettings>.Empty;

    /// <summary>
    /// Gets the override the default CSS Reset.
    /// </summary>
    public string? CssResetOverride { get; init; }

    /// <summary>
    /// Gets the root element for all selectors.
    /// </summary>
    public string RootElement { get; init; } = string.Empty;

    /// <summary>
    /// Gets the element prefix.
    /// </summary>
    public string ElementPrefix { get; init; } = string.Empty;

    /// <summary>
    /// Gets the separator between variants and selectors. Cannot be a colon.
    /// </summary>
    public char Separator { get; init; } = ':';

    /// <summary>
    /// Gets an additional set of elements to include.
    /// </summary>
    public ImmutableDictionary<string, string> Applies { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets a value indicating whether to output colors as CSS variables in the :root element.
    /// </summary>
    public bool OutputColorsAsVariables { get; init; }
}

/// <summary>
/// A full Monorail CSS theme based on a design system and variants.
/// </summary>
public class CssFramework
{
    private readonly CssFrameworkSettings _frameworkSettings;
    private readonly PluginManager _pluginManager;
    private readonly VariantSystem _variantSystem;
    private readonly VariantProcessor _variantProcessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssFramework"/> class.
    /// </summary>
    /// <param name="frameworkSettings">The framework settings.</param>
    public CssFramework(CssFrameworkSettings? frameworkSettings = null)
    {
        _frameworkSettings = frameworkSettings ?? new CssFrameworkSettings();
        _pluginManager = new PluginManager(_frameworkSettings, this);
        _variantSystem = new VariantSystem(_frameworkSettings.DesignSystem, _pluginManager.AllPlugins.OfType<IVariantPluginProvider>().ToArray());
        _variantProcessor = new VariantProcessor(_variantSystem);
    }

    /// <summary>
    /// Returns a prefixed variable name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>A string in the format of --{prefix}-{name}.</returns>
    public static string GetVariableNameWithPrefix(string name)
    {
        return SelectorGenerator.GetVariableNameWithPrefix(name);
    }

    /// <summary>
    /// Returns a prefixed variable name along wrapped with var for using in CSS.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <returns>A string in the format of var(--{prefix}-{name}.</returns>
    public static string GetCssVariableWithPrefix(string name)
    {
        return SelectorGenerator.GetCssVariableWithPrefix(name);
    }

    /// <summary>
    /// Builds the CSS.
    /// </summary>
    /// <param name="cssClasses">List of CSS classes to use as the root.</param>
    /// <returns>A full CSS stylesheet.</returns>
    public string Process(IEnumerable<string> cssClasses)
    {
        var (cssReset, utilities) = ProcessSplit(cssClasses);
        return $"{cssReset}{Environment.NewLine}{utilities}";
    }

    /// <summary>
    /// Builds the CSS.
    /// </summary>
    /// <param name="cssClasses">List of CSS classes to use as the root.</param>
    /// <returns>A full CSS stylesheet.</returns>
    public (string CssReset, string Utilities) ProcessSplit(IEnumerable<string> cssClasses)
    {
        var r = ProcessSplitWithWarnings(cssClasses);
        return (r.CssReset, r.Utilities);
    }

    /// <summary>
    /// Builds the CSS.
    /// </summary>
    /// <param name="cssClasses">List of CSS classes to use as the root.</param>
    /// <returns>A full CSS stylesheet.</returns>
    public (string CssReset, string Utilities, string[] Warnings) ProcessSplitWithWarnings(IEnumerable<string> cssClasses)
    {
        var distinctCss = new HashSet<string>();
        var separator = new[]
        {
            ' ', '\t',
        };
        foreach (var cssClass in cssClasses)
        {
            distinctCss.UnionWith(cssClass.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        }

        var root = string.IsNullOrWhiteSpace(_frameworkSettings.RootElement) ? string.Empty : _frameworkSettings.RootElement.Trim() + " ";

        // create a list of all elements from applies with their root element, and all the ones passed in as parameter.
        // with a root of ""
        var applyItems = new List<(string Root, string CssClass)>();
        foreach (var apply in _frameworkSettings.Applies)
        {
            var values = apply.Value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            applyItems.AddRange(values.Select(value => (apply.Key, value)));
        }

        var selectorWithClassListItems = applyItems
            .Select(i => new
            {
                Selector = $"{root}{i.Root}", i.CssClass, IsAppliesItem = true,
            })
            .Concat(distinctCss.Select(i => new
            {
                Selector = string.Empty, CssClass = i, IsAppliesItem = false,
            }));

        var classParser = new ClassParser(_pluginManager.NamespacesOrderedByLength, _frameworkSettings.ElementPrefix, _frameworkSettings.Separator);

        var selectorWithSyntaxItems = selectorWithClassListItems
            .Select(i => new
            {
                i.Selector, Syntax = classParser.Extract(i.CssClass), i.IsAppliesItem,
            })
            .Where(i => i.Syntax != null);

        var pluginsWithDefaultsCalled = new List<IRegisterDefaults>();
        var mediaGrouping = ImmutableDictionary.Create<string[], ImmutableList<CssRuleSet>>(new ModifierComparer());

        var missingSelectors = new List<string>();
        foreach (var item in selectorWithSyntaxItems)
        {
            var elementSelector = item.Selector;
            var syntax = item.Syntax!; // filtered with the where above.
            var mediaModifiers = _variantProcessor.GetMediaModifiers(syntax.Modifiers, _variantSystem.Variants);
            var declarations = new List<CssRuleSet>();
            var getPluginsForSyntax = _pluginManager.GetPlugins(syntax);
            foreach (var plugin in getPluginsForSyntax)
            {
                var elements = plugin.Process(syntax);
                foreach (var ruleSet in elements)
                {
                    // Handle selector generation for both applies items and regular classes
                    string selectorSyntax;
                    if (item.IsAppliesItem && !string.IsNullOrWhiteSpace(elementSelector))
                    {
                        // For applies items with variants, we need to generate the selector with variants
                        if (syntax.Modifiers.Length != 0)
                        {
                            var modifiers = syntax.Modifiers
                                .Select(i => _variantSystem.TryGetVariant(SelectorGenerator.StripNamedVariant(i)))
                                .Where(i => i != null).OfType<IVariant>()
                                .ToList();

                            // For applies items, we need to combine the element selector with the variant selector
                            // The elementSelector already contains the base selector (e.g., ".tab-list")
                            // We need to extract the class name and apply variants to it
                            var baseElementSelector = elementSelector.Trim();
                            if (baseElementSelector.StartsWith(root))
                            {
                                baseElementSelector = baseElementSelector.Substring(root.Length);
                            }

                            // Apply variants to the base element selector
                            selectorSyntax = SelectorGenerator.ApplyVariantsToElementSelector(baseElementSelector, modifiers);
                        }
                        else
                        {
                            selectorSyntax = elementSelector;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(elementSelector))
                    {
                        selectorSyntax = elementSelector;
                    }
                    else
                    {
                        var modifiers = syntax.Modifiers
                            .Select(i => _variantSystem.TryGetVariant(SelectorGenerator.StripNamedVariant(i)))
                            .Where(i => i != null).OfType<IVariant>()
                            .ToList();

                        selectorSyntax = SelectorGenerator.GetSelectorSyntax(
                            ruleSet.Selector,
                            modifiers);
                    }

                    var rootSelector = string.IsNullOrWhiteSpace(_frameworkSettings.RootElement) switch
                    {
                        false => $"{_frameworkSettings.RootElement} ",
                        true => string.Empty,
                    };

                    if (plugin is IRegisterDefaults registerDefaultPlugin)
                    {
                        if (!pluginsWithDefaultsCalled.Contains(registerDefaultPlugin))
                        {
                            pluginsWithDefaultsCalled.Add(registerDefaultPlugin);
                        }
                    }

                    declarations.Add(ruleSet with
                    {
                        Selector = $"{rootSelector}{selectorSyntax}",
                    });
                }
            }

            if (declarations.Count > 0)
            {
                var mediaGroupingDeclarations = mediaGrouping.TryGetValue(mediaModifiers, out var existingMediaGroupingDeclarations)
                    ? existingMediaGroupingDeclarations.AddRange(declarations)
                    : declarations.ToImmutableList();

                mediaGrouping = mediaGrouping.SetItem(mediaModifiers, mediaGroupingDeclarations);
            }
            else
            {
                if (item.Syntax != null)
                {
                    missingSelectors.Add(item.Syntax.OriginalSyntax);
                }
            }
        }

        var mediaRules = mediaGrouping.Select(i => new CssMediaRule(_variantProcessor.GetFeatureList(i.Key, _variantSystem.Variants), i.Value))
            .ToImmutableList();

        var defaultVariableDeclarationList = new CssDeclarationList();
        foreach (var defaultPlugin in pluginsWithDefaultsCalled)
        {
            defaultVariableDeclarationList += defaultPlugin.GetDefaults();
        }

        foreach (var defaultVariable in _frameworkSettings.DesignSystem.Variables)
        {
            var variableName = GetVariableNameWithPrefix(defaultVariable.Key);
            defaultVariableDeclarationList.Add(new CssDeclaration(variableName, defaultVariable.Value));
        }

        if (_frameworkSettings.OutputColorsAsVariables)
        {
            var flattenedColors = _frameworkSettings.DesignSystem.GetFlattenColors();
            foreach (var color in flattenedColors)
            {
                var variableName = GetVariableNameWithPrefix($"color-{color.Key}");
                defaultVariableDeclarationList.Add(new CssDeclaration(variableName, color.Value.AsString()));
            }
        }

        var styleSheet = new CssStylesheet(mediaRules);

        var cssReset = _frameworkSettings.CssResetOverride ?? CssResetProvider.GetDefaultCssReset();

        var sb = new StringBuilder();
        CssWriter.CssWriter.AppendDefaultCssRules(defaultVariableDeclarationList, sb);
        CssWriter.CssWriter.AppendCssRules(styleSheet, sb);

        return (cssReset, sb.ToString(), missingSelectors.ToArray());
    }

    /// <summary>
    /// Compares a string list of modifiers.
    /// </summary>
    private class ModifierComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[]? x, string[]? y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.SequenceEqual(y);
        }

        public int GetHashCode(string[] obj)
        {
            return string.Join(",", obj).GetHashCode();
        }
    }

    /// <summary>
    /// Gets all defined rules.
    /// </summary>
    /// <returns>A list of the defined rules and their CSS.</returns>
    public ImmutableDictionary<string, string> GetAllRules()
    {
        var dictBuilder = ImmutableDictionary.CreateBuilder<string, string>();
        var rules = _pluginManager.AllPlugins.SelectMany(plugin => plugin.GetAllRules());
        var sb = new StringBuilder();
        foreach (var cssRuleSet in rules)
        {
            sb.Clear();
            foreach (var ruleSet in cssRuleSet.DeclarationList)
            {
                sb.Append(ruleSet.ToCssString());
            }

            dictBuilder.Add(cssRuleSet.Selector.Selector, sb.ToString());
        }

        return dictBuilder.ToImmutable();
    }
}
