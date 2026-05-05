using System.Reflection;
using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(MonorailCss.Discovery.MonorailCssReloader))]

namespace MonorailCss.Discovery;

/// <summary>
/// Hot-reload entry point. The runtime invokes <see cref="UpdateApplication"/> after applying
/// an EnC delta to the in-memory metadata image. We forward the change to every registered
/// <see cref="ClassDiscoveryService"/> so it can rescan and regenerate CSS.
/// </summary>
internal static class MonorailCssReloader
{
    private static readonly List<WeakReference<ClassDiscoveryService>> Services = new();
    private static readonly object Lock = new();

    public static void RegisterService(ClassDiscoveryService service)
    {
        lock (Lock)
        {
            // Sweep dead references opportunistically.
            for (var i = Services.Count - 1; i >= 0; i--)
            {
                if (!Services[i].TryGetTarget(out _))
                {
                    Services.RemoveAt(i);
                }
            }

            Services.Add(new WeakReference<ClassDiscoveryService>(service));
        }
    }

    public static void UnregisterService(ClassDiscoveryService service)
    {
        lock (Lock)
        {
            for (var i = Services.Count - 1; i >= 0; i--)
            {
                if (Services[i].TryGetTarget(out var target) && ReferenceEquals(target, service))
                {
                    Services.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Called by the runtime on hot-reload deltas. Even when <paramref name="updatedTypes"/>
    /// is null we still rescan, because the reload might have only added/removed strings rather
    /// than changing types.
    /// </summary>
    public static void UpdateApplication(Type[]? updatedTypes)
    {
        var changed = updatedTypes is { Length: > 0 }
            ? updatedTypes.Select(t => t.Assembly).Distinct().ToArray()
            : Array.Empty<Assembly>();

        ClassDiscoveryService[] snapshot;
        lock (Lock)
        {
            snapshot = Services
                .Select(r => r.TryGetTarget(out var t) ? t : null)
                .Where(s => s is not null)
                .Cast<ClassDiscoveryService>()
                .ToArray();
        }

        foreach (var svc in snapshot)
        {
            svc.OnAssembliesChanged(changed);
        }
    }
}
