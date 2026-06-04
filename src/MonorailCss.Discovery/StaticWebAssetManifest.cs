using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonorailCss.Discovery;

/// <summary>
/// Resolves an ASP.NET Core static web assets <em>runtime</em> manifest
/// (<c>{EntryAssembly}.staticwebassets.runtime.json</c>) into physical on-disk file paths.
/// <para>
/// Component packages (Razor Class Libraries) ship assets — JavaScript especially — under
/// their <c>wwwroot</c>, served at <c>_content/&lt;Package&gt;/…</c>. Those files live in the
/// NuGet package cache, never enter the assembly's IL, and are not inside any directory the
/// source-file scan walks. Discovery therefore can't reach utility classes baked into, say,
/// <c>_content/Pennington.UI/scripts.js</c> through either the IL scan or the source scan.
/// The runtime manifest is the one artifact that maps each served asset to its physical
/// location, so reading it closes that gap.
/// </para>
/// <para>
/// Only explicitly enumerated assets are resolved. Pattern-based content roots (the app's own
/// globbed <c>wwwroot</c>) are intentionally not expanded — every package/RCL <c>_content</c>
/// asset is enumerated, and the app's own assets live under the content root the source-file
/// scan already covers.
/// </para>
/// </summary>
public static class StaticWebAssetManifest
{
    /// <summary>
    /// One resolved asset: the URL path it is served at (e.g.
    /// <c>_content/Pennington.UI/scripts.js</c>) and its physical file path on disk.
    /// </summary>
    /// <param name="UrlPath">The served path, built from the manifest trie keys.</param>
    /// <param name="PhysicalPath">The absolute on-disk path
    /// (<c>ContentRoots[index]</c> + <c>SubPath</c>).</param>
    public readonly record struct ResolvedAsset(string UrlPath, string PhysicalPath);

    /// <summary>
    /// Returns the conventional runtime-manifest path for <paramref name="assembly"/>:
    /// <c>{assemblyLocation-without-extension}.staticwebassets.runtime.json</c> next to the
    /// assembly. Returns <see langword="null"/> when the assembly has no on-disk location
    /// (single-file publish, in-memory, etc.), where no manifest can be found.
    /// </summary>
    /// <param name="assembly">The assembly whose manifest to locate — typically the entry assembly.</param>
    /// <returns>The expected manifest path, or <see langword="null"/> when it can't be derived.</returns>
    public static string? GetManifestPath(Assembly assembly)
    {
        var location = assembly.Location;
        if (string.IsNullOrEmpty(location))
        {
            return null;
        }

        var dir = Path.GetDirectoryName(location);
        var name = Path.GetFileNameWithoutExtension(location);
        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name))
        {
            return null;
        }

        return Path.Combine(dir, name + ".staticwebassets.runtime.json");
    }

    /// <summary>
    /// Parses a static web assets runtime manifest and resolves every enumerated asset to its
    /// physical path. Returns an empty list on any parse error or when the manifest is
    /// structurally empty.
    /// </summary>
    /// <param name="manifestJson">The raw manifest JSON.</param>
    /// <returns>Every enumerated asset, paired with its resolved physical path.</returns>
    public static IReadOnlyList<ResolvedAsset> Resolve(string manifestJson)
    {
        if (string.IsNullOrWhiteSpace(manifestJson))
        {
            return [];
        }

        StaticWebAssetRuntimeManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize(
                manifestJson,
                StaticWebAssetManifestJsonContext.Default.StaticWebAssetRuntimeManifest);
        }
        catch (JsonException)
        {
            return [];
        }

        if (manifest?.Root is null || manifest.ContentRoots is null || manifest.ContentRoots.Length == 0)
        {
            return [];
        }

        var roots = manifest.ContentRoots;
        var result = new List<ResolvedAsset>();

        // Iterative DFS over the trie. Each node's accumulated key path is the served URL; a
        // node carrying an Asset is a leaf we can resolve to a physical file.
        var stack = new Stack<(string Path, StaticWebAssetNode Node)>();
        stack.Push((string.Empty, manifest.Root));

        while (stack.Count > 0)
        {
            var (prefix, node) = stack.Pop();

            if (node.Asset is { } asset
                && asset.ContentRootIndex >= 0
                && asset.ContentRootIndex < roots.Length
                && !string.IsNullOrEmpty(roots[asset.ContentRootIndex]))
            {
                var physical = Path.Combine(roots[asset.ContentRootIndex]!, asset.SubPath ?? string.Empty);
                result.Add(new ResolvedAsset(prefix, physical));
            }

            if (node.Children is null)
            {
                continue;
            }

            foreach (var (segment, child) in node.Children)
            {
                if (child is null)
                {
                    continue;
                }

                var childPath = prefix.Length == 0 ? segment : prefix + "/" + segment;
                stack.Push((childPath, child));
            }
        }

        return result;
    }
}

/// <summary>Root of the static web assets runtime manifest: the content-root table plus the asset trie.</summary>
internal sealed class StaticWebAssetRuntimeManifest
{
    /// <summary>Gets or sets the base directories assets resolve against, indexed by <see cref="StaticWebAssetEntry.ContentRootIndex"/>.</summary>
    public string?[]? ContentRoots { get; set; }

    /// <summary>Gets or sets the root trie node whose descendants enumerate the served assets.</summary>
    public StaticWebAssetNode? Root { get; set; }
}

/// <summary>One trie node: optional child segments and an optional asset payload when the node is a leaf.</summary>
internal sealed class StaticWebAssetNode
{
    /// <summary>Gets or sets the child segments keyed by URL path segment.</summary>
    public Dictionary<string, StaticWebAssetNode>? Children { get; set; }

    /// <summary>Gets or sets the asset this node resolves to, when it is a leaf.</summary>
    public StaticWebAssetEntry? Asset { get; set; }
}

/// <summary>The physical-location payload of a leaf trie node.</summary>
internal sealed class StaticWebAssetEntry
{
    /// <summary>Gets or sets the index into <see cref="StaticWebAssetRuntimeManifest.ContentRoots"/>.</summary>
    public int ContentRootIndex { get; set; }

    /// <summary>Gets or sets the path under the content root that locates the file.</summary>
    public string? SubPath { get; set; }
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(StaticWebAssetRuntimeManifest))]
internal sealed partial class StaticWebAssetManifestJsonContext : JsonSerializerContext;
