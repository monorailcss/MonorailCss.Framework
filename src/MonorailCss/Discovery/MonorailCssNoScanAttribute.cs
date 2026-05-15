namespace MonorailCss.Discovery;

/// <summary>
/// Marks an assembly so the MonorailCss discovery scanners skip it entirely. Apply
/// <c>[assembly: MonorailCssNoScan]</c> to any assembly whose IL-embedded string literals
/// are utility-class-shaped templates, canonical-test fixtures, or other noise that would
/// inflate the generated CSS without contributing real utilities. The MonorailCss framework
/// assemblies apply it to themselves (they ship templates like <c>"bg-{color}-500"</c>);
/// consumers can apply it to their own assemblies as a code-side alternative to
/// <c>MonorailDiscoveryOptions.ExcludeAssemblies</c> / <c>@(MonorailCssExcludeAssembly)</c>.
/// </summary>
/// <remarks>
/// Detected by walking assembly-level custom attributes in IL metadata
/// (<see cref="IlMetadataScanner.HasMonorailCssNoScanAttribute"/>), so it works for both the
/// runtime in-memory scan and the build-time on-disk scan with no reflection or type load.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class MonorailCssNoScanAttribute : Attribute
{
}
