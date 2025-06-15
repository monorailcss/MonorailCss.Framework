using System.Diagnostics;
using System.Reflection;

namespace MonorailCss.Framework.Processing;

/// <summary>
/// Provides CSS reset functionality.
/// </summary>
internal static class CssResetProvider
{
    /// <summary>
    /// Gets the default CSS reset content.
    /// </summary>
    /// <returns>The default CSS reset as a string.</returns>
    public static string GetDefaultCssReset()
    {
        const string ResourceName = "MonorailCss.reset.css";
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);

        Debug.Assert(stream != null, "stream should never be null");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}