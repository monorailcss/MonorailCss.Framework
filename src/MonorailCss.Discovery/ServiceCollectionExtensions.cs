using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MonorailCss.Discovery;

/// <summary>
/// DI extensions for the MonorailCss runtime discovery pipeline.
/// </summary>
public static class MonorailCssServiceCollectionExtensions
{
    /// <summary>
    /// Registers runtime CSS class discovery and the built-in CSS-serving middleware.
    /// With no configuration, the discovery service auto-loads every non-BCL referenced
    /// assembly, watches the project's source directory in development, and reads
    /// <c>wwwroot/app.css</c> as the source CSS prefix when present.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional callback to override the auto-detected defaults.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMonorailCss(
        this IServiceCollection services,
        Action<MonorailDiscoveryOptions>? configure = null)
    {
        AddMonorailClassDiscovery(services, configure);
        return services;
    }

    /// <summary>
    /// Registers the discovery pipeline without the CSS-serving middleware. Use this when
    /// you have your own CSS endpoint or service that consumes <see cref="IClassRegistry"/>
    /// directly — Pennington, custom integrations, tooling that just wants the class set.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional callback to override the auto-detected defaults.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <remarks>
    /// Discovery inspects loaded assemblies at runtime (reference-graph walk, IL metadata scan,
    /// static web asset manifests). This is fundamentally a reflection feature: under aggressive
    /// trimming or Native AOT the assembly graph and on-disk paths may be incomplete, so discovery
    /// degrades gracefully to finding fewer classes rather than failing. It is intended for
    /// JIT/development and standard server deployments, not fully-trimmed single-file/AOT apps.
    /// </remarks>
    public static IServiceCollection AddMonorailClassDiscovery(
        this IServiceCollection services,
        Action<MonorailDiscoveryOptions>? configure = null)
    {
        services.Configure(configure ?? (_ => { }));

        services.TryAddSingleton<ClassDiscoveryService>();
        services.TryAddSingleton<IClassRegistry>(sp => sp.GetRequiredService<ClassDiscoveryService>());
        services.AddHostedService(sp => sp.GetRequiredService<ClassDiscoveryService>());

        return services;
    }
}
