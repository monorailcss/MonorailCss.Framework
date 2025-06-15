using System.Collections.Immutable;
using MonorailCss.Parser;
using MonorailCss.Plugins;

namespace MonorailCss.Framework.Processing;

/// <summary>
/// Manages plugin discovery, instantiation, and organization.
/// </summary>
internal class PluginManager
{
    private readonly CssFrameworkSettings _frameworkSettings;
    private readonly ImmutableDictionary<Type, ISettings> _pluginSettingsMap;
    private readonly IUtilityPlugin[] _allPlugins;
    private ImmutableDictionary<string, IUtilityPlugin[]> _namespacePluginsMap = ImmutableDictionary<string, IUtilityPlugin[]>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager"/> class.
    /// </summary>
    /// <param name="frameworkSettings">The framework settings.</param>
    /// <param name="cssFramework">The CSS framework instance for dependency injection.</param>
    public PluginManager(CssFrameworkSettings frameworkSettings, CssFramework cssFramework)
    {
        _frameworkSettings = frameworkSettings;

        // Build plugin settings map
        var pluginSettingsMapBuilder = ImmutableDictionary.CreateBuilder<Type, ISettings>();
        foreach (var pluginSetting in _frameworkSettings.PluginSettings)
        {
            var genericSettingsType = pluginSetting
                .GetType()
                .GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISettings<>));
            pluginSettingsMapBuilder.Add(genericSettingsType, pluginSetting);
        }

        _pluginSettingsMap = pluginSettingsMapBuilder.ToImmutable();

        // Discover and instantiate all plugins
        var pluginTypes = typeof(IUtilityPlugin)
            .Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.IsAssignableTo(typeof(IUtilityPlugin))
                           && type.GetCustomAttributes(typeof(PluginNotIncludedAutomaticallyAttribute), true).Length == 0)
            .ToImmutableList();

        _allPlugins = pluginTypes
            .Select(type => GetPluginInstance(type, cssFramework))
            .OfType<IUtilityPlugin>()
            .ToArray();

        // Build ordered namespaces for efficient matching
        NamespacesOrderedByLength = _allPlugins
            .OfType<IUtilityNamespacePlugin>()
            .SelectMany(i => i.Namespaces)
            .OrderByDescending(i => i.Length)
            .ToArray();
    }

    /// <summary>
    /// Gets all plugins.
    /// </summary>
    public IUtilityPlugin[] AllPlugins => _allPlugins;

    /// <summary>
    /// Gets namespaces ordered by length for parsing.
    /// </summary>
    public string[] NamespacesOrderedByLength { get; }

    /// <summary>
    /// Gets plugins that can handle the given syntax.
    /// </summary>
    /// <param name="syntax">The parsed class name syntax.</param>
    /// <returns>Array of plugins that can process the syntax.</returns>
    public IEnumerable<IUtilityPlugin> GetPlugins(IParsedClassNameSyntax syntax)
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

    /// <summary>
    /// Creates an instance of a plugin with dependency injection.
    /// </summary>
    /// <param name="type">The plugin type to instantiate.</param>
    /// <param name="cssFramework">The CSS framework instance for dependency injection.</param>
    /// <returns>Instantiated plugin or null if instantiation fails.</returns>
    private IUtilityPlugin? GetPluginInstance(Type type, CssFramework cssFramework)
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
                        parameters.Add(cssFramework);
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
}