// Polyfill so records / init-only setters compile on netstandard2.0 (required for Roslyn analyzers).
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
