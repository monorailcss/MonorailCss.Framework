using JetBrains.Annotations;

namespace MonorailCss.Plugins;

/// <summary>
/// Represents the settings for a plugin.
/// </summary>
/// <typeparam name="T">A plugin type.</typeparam>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface ISettings<T> : ISettings
    where T : class, IUtilityPlugin
{
}

/// <summary>
/// Represents settings for a plugin.
/// </summary>
public interface ISettings
{
}
