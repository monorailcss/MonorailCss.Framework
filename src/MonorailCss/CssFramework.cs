using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using MonorailCss.Css;
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
    /// Gets the seperator between variants and selectors. Cannot be a colon.
    /// </summary>
    public char Separator { get; init; } = ':';

    /// <summary>
    /// Gets an additional set of elements to include.
    /// </summary>
    public IDictionary<string, string> Applies { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets a value indicating whether to output colors as CSS variables in the :root element.
    /// </summary>
    public bool OutputColorsAsVariables { get; init; } = false;
}

/// <summary>
/// A full Monorail CSS theme based on a design system and variants.
/// </summary>
public class CssFramework
{
    private readonly CssFrameworkSettings _frameworkSettings;
    private readonly IUtilityPlugin[] _allPlugins;
    private readonly VariantSystem _variantSystem;
    private readonly string[] _namespacesOrderedByLength;
    private readonly ImmutableDictionary<Type, ISettings> _pluginSettingsMap = ImmutableDictionary<Type, ISettings>.Empty;
    private ImmutableDictionary<string, IUtilityPlugin[]> _namespacePluginsMap = ImmutableDictionary<string, IUtilityPlugin[]>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssFramework"/> class.
    /// </summary>
    /// <param name="frameworkSettings">The framework settings.</param>
    public CssFramework(CssFrameworkSettings? frameworkSettings = null)
    {
        _frameworkSettings = frameworkSettings ?? new CssFrameworkSettings();
        var pluginTypes = typeof(IUtilityPlugin)
            .Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.IsAssignableTo(typeof(IUtilityPlugin))
                           && type.GetCustomAttributes(typeof(PluginNotIncludedAutomaticallyAttribute), true).Length ==
                           0)
            .ToImmutableList();

        foreach (var pluginSetting in _frameworkSettings.PluginSettings)
        {
            var genericSettingsType = pluginSetting
                .GetType()
                .GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISettings<>));
            _pluginSettingsMap = _pluginSettingsMap.Add(genericSettingsType, pluginSetting);
        }

        // we need all the namespaces up front so that the CSS parser can do its thing.
        _allPlugins = pluginTypes
            .Select(GetPluginInstance)
            .OfType<IUtilityPlugin>().ToArray();

        _variantSystem = new VariantSystem(_frameworkSettings.DesignSystem, _allPlugins.OfType<IVariantPluginProvider>().ToArray());

        // it's important to order these by length. we traverse the list sequentially
        // looking for matching namespaces and we want the longer ones to match before shorter ones
        // e.g. if rounded-l is registered as well as rounded then we want to ensure the later comes first.
        _namespacesOrderedByLength = _allPlugins
            .OfType<IUtilityNamespacePlugin>()
            .SelectMany(i => i.Namespaces)
            .OrderByDescending(i => i.Length)
            .ToArray();
    }

    /// <summary>
    /// Returns a prefixed variable name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>A string in the format of --{prefix}-{name}.</returns>
    public static string GetVariableNameWithPrefix(string name)
    {
        return $"--monorail-{name}";
    }

    /// <summary>
    /// Returns a prefixed variable name along wrapped with var for using in CSS.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <returns>A string in the format of var(--{prefix}-{name}.</returns>
    public static string GetCssVariableWithPrefix(string name)
    {
        var cssVariableWithPrefix = $"var(--monorail-{name})";
        return cssVariableWithPrefix;
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
                Selector = $"{root}{i.Root}", i.CssClass,
            })
            .Concat(distinctCss.Select(i => new
            {
                Selector = string.Empty, CssClass = i,
            }));

        var classParser = new ClassParser(_namespacesOrderedByLength, _frameworkSettings.ElementPrefix, _frameworkSettings.Separator);

        var selectorWithSyntaxItems = selectorWithClassListItems
            .Select(i => new
            {
                i.Selector, Syntax = classParser.Extract(i.CssClass),
            })
            .Where(i => i.Syntax != null);

        var pluginsWithDefaultsCalled = new List<IRegisterDefaults>();
        var mediaGrouping = ImmutableDictionary.Create<string[], ImmutableList<CssRuleSet>>(new ModifierComparer());

        var missingSelectors = new List<string>();
        foreach (var item in selectorWithSyntaxItems)
        {
            var elementSelector = item.Selector;
            var syntax = item.Syntax!; // filtered with the where above.
            var mediaModifiers = GetMediaModifiers(syntax.Modifiers, _variantSystem.Variants);
            var declarations = new List<CssRuleSet>();
            var getPluginsForSyntax = GetPlugins(syntax);
            foreach (var plugin in getPluginsForSyntax)
            {
                var elements = plugin.Process(syntax);
                foreach (var ruleSet in elements)
                {
                    // typically we won't have an elementSelector. This only comes up when applies is being used.
                    // we'll generate it based on the selector syntax.
                    string selectorSyntax;
                    if (!string.IsNullOrWhiteSpace(elementSelector))
                    {
                        selectorSyntax = elementSelector;
                    }
                    else
                    {
                        var modifiers = syntax.Modifiers
                            .Select(i => _variantSystem.Variants.GetValueOrDefault(StripNamedVariant(i)))
                            .Where(i => i != null).OfType<IVariant>()
                            .ToList();

                        selectorSyntax = GetSelectorSyntax(
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

        var mediaRules = mediaGrouping.Select(i => new CssMediaRule(GetFeatureList(i.Key, _variantSystem.Variants), i.Value))
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

        var cssReset = _frameworkSettings.CssResetOverride ?? GetDefaultCssReset();

        var sb = new StringBuilder();
        CssWriter.CssWriter.AppendDefaultCssRules(defaultVariableDeclarationList, sb);
        CssWriter.CssWriter.AppendCssRules(styleSheet, sb);

        return (cssReset, sb.ToString(), missingSelectors.ToArray());
    }

    private IEnumerable<IUtilityPlugin> GetPlugins(IParsedClassNameSyntax syntax)
    {
        if (syntax is ArbitraryPropertySyntax)
        {
            return _allPlugins.OfType<ArbitraryPropertyPlugin>();
        }

        if (syntax is not NamespaceSyntax namespaceSyntax)
        {
            return _allPlugins;
        }

        if (_namespacePluginsMap.TryGetValue(namespaceSyntax.Namespace, out var existValue))
        {
            return existValue;
        }

        var plugins = _allPlugins
            .OfType<IUtilityNamespacePlugin>()
            .Where(i => i.Namespaces.Contains(namespaceSyntax.Namespace))
            .Cast<IUtilityPlugin>()
            .ToArray();

        _namespacePluginsMap = _namespacePluginsMap.Add(namespaceSyntax.Namespace, plugins);

        return _allPlugins;
    }

    private ImmutableList<MediaQueryVariant> GetFeatureList(
        string[] mediaModifiers,
        ImmutableDictionary<string, IVariant> variants) =>
        mediaModifiers.Select(m => variants[m])
            .OfType<MediaQueryVariant>()
            .Select(i => i).ToImmutableList();

    private static string GetDefaultCssReset()
    {
        const string ResourceName = "MonorailCss.reset.css";
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);

        Debug.Assert(stream != null, "stream should never be null");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private string[] GetMediaModifiers(IEnumerable<string> modifiers, ImmutableDictionary<string, IVariant> variants)
    {
        List<string> mediaModifiers = [];
        foreach (var modifier in modifiers)
        {
            if (!variants.TryGetValue(modifier, out var variant))
            {
                continue;
            }

            if (variant is MediaQueryVariant)
            {
                mediaModifiers.Add(modifier);
            }
        }

        return mediaModifiers.ToArray();
    }

    private IUtilityPlugin? GetPluginInstance(Type type)
    {
        IUtilityPlugin? plugin = null;
        var constructorInfos = type.GetConstructors();
        if (constructorInfos.Length == 0)
        {
            plugin = Activator.CreateInstance(type) as IUtilityPlugin;
        }
        else
        {
            foreach (var constructorInfo in constructorInfos)
            {
                List<object> parameters = [];
                foreach (var parameterInfo in constructorInfo.GetParameters())
                {
                    if (parameterInfo.ParameterType == typeof(DesignSystem))
                    {
                        parameters.Add(_frameworkSettings.DesignSystem);
                    }
                    else if (parameterInfo.ParameterType == typeof(CssFramework))
                    {
                        parameters.Add(this);
                    }
                    else
                    {
                        var settingsGenericType = typeof(ISettings<>).MakeGenericType(type);
                        if (parameterInfo.ParameterType.IsAssignableTo(settingsGenericType))
                        {
                            if (_pluginSettingsMap.TryGetValue(settingsGenericType, out var value))
                            {
                                parameters.Add(value);
                            }
                            else
                            {
                                var instance = Activator.CreateInstance(parameterInfo.ParameterType);
                                if (instance == null)
                                {
                                    throw new InvalidOperationException(
                                        $"Could not create instance of {parameterInfo.ParameterType} for plug-in {type}.");
                                }

                                parameters.Add(instance);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Constructor for {type} found with invalid parameter - {parameterInfo}.");
                        }
                    }
                }

                plugin = constructorInfo.Invoke(parameters.ToArray()) as IUtilityPlugin;
                if (plugin != null)
                {
                    break;
                }
            }
        }

        return plugin;
    }

    internal static string GetSelectorSyntax(CssSelector original, IList<IVariant> variants)
    {
        if (original.Selector.StartsWith('@'))
        {
            // keyframe, so we don't want to mess with the name.
            return original.Selector;
        }

        var spaceIndex = original.Selector.IndexOf(' ');
        var selector = spaceIndex >= 0
            ? EscapeFirstWord(original.Selector, spaceIndex)
            : EscapeCssClassSelector(original.Selector);

        selector = $".{selector}";

        if (original.PseudoClass != null)
        {
            selector = $"{selector}{original.PseudoClass}";
        }

        // Process non-conditional variants first
        var conditionalVariant = variants.OfType<NameConditionalVariant>().FirstOrDefault();
        var nonConditionalVariants = variants.Where(v => v is not NameConditionalVariant);

        selector = nonConditionalVariants.OrderBy(v => typeof(PseudoElementVariant) == v.GetType() ? 1 : 0).Aggregate(selector, (current, variant) => variant switch
        {
            SelectorVariant selectorVariant => $"{selectorVariant.Selector} {current}",
            ProseElementVariant proseElementVariant => $"{current} {proseElementVariant.Selector}",
            PseudoClassVariant pseudoClassVariant => $"{current}{pseudoClassVariant.PseudoClass}",
            PseudoElementVariant pseudoElementVariant => $"{current}{pseudoElementVariant.PseudoElement}",
            _ => current,
        });

        if (original.PseudoElement != null)
        {
            selector = $"{selector}{original.PseudoElement}";
        }

        // Add conditional variant if present
        if (conditionalVariant != null)
        {
            var name = string.Empty;

            if (original.Selector.Contains('/'))
            {
                // Find the position of slash and colon
                var slashIndex = original.Selector.IndexOf('/');
                var colonIndex = original.Selector.IndexOf(':');

                // If there's no colon after the slash, return the original string
                if (colonIndex > slashIndex)
                {
                    name = $@"\/{original.Selector.Substring(slashIndex + 1, colonIndex - slashIndex - 1)}";
                }
            }

            selector = $"{selector} {conditionalVariant.Condition(name)}";
        }

        return selector;
    }

    private static string EscapeFirstWord(string originalSelector, int spaceIndex)
    {
        var firstWord = originalSelector[..spaceIndex];

        firstWord = EscapeCssClassSelector(firstWord);

        return $"{firstWord}{originalSelector[spaceIndex..]}";
    }

    private static string StripNamedVariant(string original)
    {
        var colonIndex = original.IndexOf('/');
        if (colonIndex < 0)
        {
            return original;
        }

        return original[..colonIndex];
    }

    private static string EscapeCssClassSelector(string firstWord)
    {
        return firstWord
            .Replace("*", "\\*")
            .Replace(":", "\\:")
            .Replace("/", "\\/")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("#", "\\#")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace(".", "\\.")
            .Replace(",", "\\2c ")
            .Replace("?", "\\?")
            .Replace("=", "\\=")
            .Replace("&", "\\&");
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
        var rules = _allPlugins.SelectMany(plugin => plugin.GetAllRules());
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
