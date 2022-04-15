using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using MonorailCss.Css;
using MonorailCss.Plugins;
using MonorailCss.Variants;

namespace MonorailCss;

/// <summary>
/// A full Monorail CSS theme based on a design system and variants.
/// </summary>
public class CssFramework
{
    private readonly DesignSystem _designSystem;
    private readonly Dictionary<Type, object> _settings = new();
    private readonly ConcurrentDictionary<string, IUtilityPlugin[]> _namespacePluginsMap = new();

    private ImmutableList<Type> _pluginTypes;
    private ImmutableDictionary<string, string> _applies = ImmutableDictionary<string, string>.Empty;

    private string _elementPrefix = string.Empty;
    private string _rootElement = string.Empty;
    private char _separator = ':';
    private string? _cssResetContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssFramework"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public CssFramework(DesignSystem designSystem)
    {
        _designSystem = designSystem;

        // order, unfortunately, matters right here. GetPluginInstance relies on the design system being set prior
        // to being called.
        _pluginTypes = typeof(IUtilityPlugin)
            .Assembly
            .GetTypes()
            .Where(type => type.IsClass
                           && !type.IsAbstract
                           && type.IsAssignableTo(typeof(IUtilityPlugin))
                           && type.GetCustomAttributes(typeof(PluginNotIncludedAutomaticallyAttribute), true).Length ==
                           0)
            .ToImmutableList();
    }

    /// <summary>
    /// Adds a new plugin setting.
    /// </summary>
    /// <param name="settings">The settings. If not specified defaults will be used.</param>
    /// <typeparam name="T">The plugin type.</typeparam>
    /// <returns>The current instance.</returns>
    public CssFramework WithSettings<T>(ISettings<T> settings)
        where T : class, IUtilityPlugin
    {
        var genericSettingsType = typeof(ISettings<>).MakeGenericType(typeof(T));
        _settings.Add(genericSettingsType, settings);
        return this;
    }

    /// <summary>
    /// Adds additional CSS elements to the stylesheet.
    /// </summary>
    /// <param name="selector">The selector.</param>
    /// <param name="css">A space seperated list of utility classes to apply.</param>
    /// <returns>The current instance.</returns>
    public CssFramework Apply(string selector, string css)
    {
        _applies = _applies.Add(selector, css);
        return this;
    }

    /// <summary>
    /// Marks theme to use a prefix for all CSS utilities generated.
    /// </summary>
    /// <param name="prefix">The prefix to use.</param>
    /// <returns>The current instance.</returns>
    public CssFramework WithElementPrefix(string prefix)
    {
        _elementPrefix = prefix;
        return this;
    }

    /// <summary>
    /// Sets the root element for the generated CSS class. Useful if you want to restrict all generated
    /// code to an element like #app.
    /// </summary>
    /// <param name="selector">The CSS selector.</param>
    /// <returns>The current instance.</returns>
    public CssFramework WithRootElement(string selector)
    {
        _rootElement = selector;
        return this;
    }

    /// <summary>
    /// Sets a custom CSS reset.
    /// </summary>
    /// <param name="css">The CSS reset to use, or an empty string for none.</param>
    /// <returns>The current instance.</returns>
    public CssFramework WithCssReset(string css)
    {
        _cssResetContent = css;
        return this;
    }

    /// <summary>
    /// Marks the theme to use a different separator between variants and the utilities
    /// e.g. a separator of _ would give you dark_sm_bg-red-100.
    /// </summary>
    /// <param name="separator">The new separator character.</param>
    /// <returns>The current instance.</returns>
    public CssFramework WithSeparator(char separator)
    {
        _separator = separator;
        return this;
    }

    /// <summary>
    /// Removes a plugin from the framework.
    /// </summary>
    /// <typeparam name="T">The plugin to remove.</typeparam>
    /// <returns>The current instance.</returns>
    public CssFramework RemovePlugin<T>()
        where T : IUtilityPlugin
    {
        if (!_pluginTypes.Contains(typeof(T)))
        {
            throw new InvalidOperationException($"Plug-in {typeof(T)} not found.");
        }

        _pluginTypes = _pluginTypes.Remove(typeof(T));
        return this;
    }

    /// <summary>
    /// Adds a new plugin to the framework.
    /// </summary>
    /// <typeparam name="T">The type of plugin to add.</typeparam>
    /// <returns>The current instance.</returns>
    public CssFramework AddPlugin<T>()
        where T : IUtilityPlugin
    {
        _pluginTypes = _pluginTypes.Add(typeof(T));
        return this;
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

    private string GetElementNameWithPrefix(string name)
    {
        return string.IsNullOrWhiteSpace(_elementPrefix) ? name : $"{_elementPrefix}-{name}";
    }

    /// <summary>
    /// Builds the CSS.
    /// </summary>
    /// <param name="cssClasses">List of CSS classes to use as the root.</param>
    /// <returns>A full CSS stylesheet.</returns>
    public string Process(IEnumerable<string> cssClasses)
    {
        // we need all the namespaces up front so that the CSS parser can do its thing.
        var allPlugins = _pluginTypes
            .Select(GetPluginInstance)
            .OfType<IUtilityPlugin>().ToArray();

        var variantSystem = new VariantSystem(_designSystem, allPlugins.OfType<IVariantPluginProvider>().ToArray());
        var variants = variantSystem.Variants;

        var namespaces = allPlugins
            .OfType<IUtilityNamespacePlugin>()
            .SelectMany(i => i.Namespaces)
            .OrderByDescending(i => i.Length)
            .ToArray();

        var distinctCss = cssClasses.Distinct();
        var root = string.IsNullOrWhiteSpace(_rootElement) ? string.Empty : _rootElement.Trim() + " ";

        // create a list of all elements from applies with their root element, and all the ones passed in as parameter.
        // with a root of ""
        var applyItems = new List<(string Root, string CssClass)>();
        foreach (var apply in _applies)
        {
            var values = apply.Value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            applyItems.AddRange(values.Select(value => (apply.Key, value)));
        }

        var selectorWithClassListItems = applyItems
            .Select(i => new { Selector = $"{root}{i.Root}", CssClassList = i.CssClass })
            .Concat(distinctCss.Select(i => new { Selector = string.Empty, CssClassList = i }));

        var selectorWithSyntaxItems = selectorWithClassListItems
            .Select(i => new
            {
                i.Selector, Syntax = ClassHelper.Extract(i.CssClassList, namespaces, _elementPrefix, _separator),
            })
            .Where(i => i.Syntax != null);

        var pluginsWithDefaultsCalled = new List<IRegisterDefaults>();
        var mediaGrouping = new ConcurrentDictionary<string[], ImmutableList<CssRuleSet>>(new ModifierComparer());
        foreach (var item in selectorWithSyntaxItems)
        {
            var elementSelector = item.Selector;
            var syntax = item.Syntax!; // filtered with the where above.
            var mediaModifiers = GetMediaModifiers(syntax.Modifiers, variants);
            var declarations = new List<CssRuleSet>();
            var getPluginsForSyntax = GetPlugins(allPlugins, syntax);
            foreach (var plugin in getPluginsForSyntax)
            {
                var elements = plugin.Process(syntax);
                declarations.AddRange(elements.Select(ruleSet =>
                {
                    // typically we won't have an elementSelector. This only comes up when applies is being used.
                    // we'll generate it based on the selector syntax.
                    var selectorSyntax = string.IsNullOrWhiteSpace(elementSelector)
                        ? ClassHelper.GetSelectorSyntax(
                            ruleSet.Selector,
                            syntax.Modifiers.Select(i => !variants.ContainsKey(i) ? default : variants[i])
                                .Where(i => i != default).OfType<IVariant>())
                        : elementSelector;

                    var rootSelector = string.IsNullOrWhiteSpace(_rootElement) switch
                    {
                        false => _rootElement + " ",
                        true => string.Empty,
                    };

                    if (plugin is IRegisterDefaults registerDefaultPlugin)
                    {
                        if (!pluginsWithDefaultsCalled.Contains(registerDefaultPlugin))
                        {
                            pluginsWithDefaultsCalled.Add(registerDefaultPlugin);
                        }
                    }

                    return ruleSet with { Selector = $"{rootSelector}{selectorSyntax}" };
                }));
            }

            if (declarations.Count == 0 && item.Syntax != default)
            {
                var originalSyntax = item.Syntax.OriginalSyntax;
                if (originalSyntax.Contains("-") && !originalSyntax.StartsWith("roslyn") &&
                    !originalSyntax.StartsWith("language"))
                {
                    Console.WriteLine($"Found selector with no declarations returned - {originalSyntax}");
                }
            }

            mediaGrouping.AddOrUpdate(
                mediaModifiers,
                _ => declarations.ToImmutableList(),
                (_, existing) => existing.AddRange(declarations));
        }

        var mediaRules = mediaGrouping.Select(i => new CssMediaRule(GetFeatureList(i.Key, variants), i.Value))
            .ToImmutableList();

        var defaultVariableDeclarationList = new CssDeclarationList();
        foreach (var defaultPlugin in pluginsWithDefaultsCalled)
        {
            defaultVariableDeclarationList += defaultPlugin.GetDefaults();
        }

        var styleSheet = new CssStylesheet(mediaRules);

        var cssReset = _cssResetContent ?? GetDefaultCssReset();
        var sb = new StringBuilder(cssReset);
        CssWriter.CssWriter.AppendCssRules(defaultVariableDeclarationList, sb);
        CssWriter.CssWriter.AppendCssRules(styleSheet, sb);

        return sb.ToString();
    }

    private IEnumerable<IUtilityPlugin> GetPlugins(IUtilityPlugin[] allPlugins, IParsedClassNameSyntax syntax)
    {
        if (syntax is NamespaceSyntax namespaceSyntax)
        {
            return _namespacePluginsMap.GetOrAdd(namespaceSyntax.Namespace, ns =>
            {
                return allPlugins
                    .OfType<IUtilityNamespacePlugin>()
                    .Where(i => i.Namespaces.Contains(ns))
                    .Cast<IUtilityPlugin>()
                    .ToArray();
            });
        }

        return allPlugins;
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
        List<string> mediaModifiers = new();
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
                List<object> parameters = new();
                foreach (var parameterInfo in constructorInfo.GetParameters())
                {
                    if (parameterInfo.ParameterType == typeof(DesignSystem))
                    {
                        parameters.Add(_designSystem);
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
                            if (_settings.ContainsKey(settingsGenericType))
                            {
                                parameters.Add(_settings[settingsGenericType]);
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
}