using Microsoft.Build.Utilities;

namespace MonorailCss.Build.Tasks.Scanning;

/// <summary>
/// Interface for scanning DLL files for utility classes.
/// </summary>
internal interface IDllScanner
{
    /// <summary>
    /// Scans a DLL file for utility classes.
    /// </summary>
    /// <param name="dllPath">The path to the DLL file.</param>
    /// <param name="log">The MSBuild task logger.</param>
    /// <returns>A set of utility class names found in the DLL.</returns>
    HashSet<string> ScanDllForUtilities(string dllPath, TaskLoggingHelper log);
}

/// <summary>
/// Placeholder implementation of DLL scanner.
/// Future implementation will use reflection to scan embedded resources and attributes.
/// </summary>
internal class DllScanner : IDllScanner
{
    /// <summary>
    /// Scans a DLL file for utility classes.
    /// </summary>
    /// <param name="dllPath">The path to the DLL file.</param>
    /// <param name="log">The MSBuild task logger.</param>
    /// <returns>A set of utility class names found in the DLL.</returns>
    public HashSet<string> ScanDllForUtilities(string dllPath, TaskLoggingHelper log)
    {
        // TODO: Implement DLL scanning functionality
        // Future implementation should:
        // 1. Load the assembly using reflection
        // 2. Scan for embedded resources (e.g., .razor, .html files)
        // 3. Look for custom attributes that contain utility classes
        // 4. Extract and return utility class names

        log.LogWarning($"DLL scanning is not yet implemented. Skipping: {dllPath}");
        log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low,
            "Future implementation will scan DLL for embedded resources and utility class attributes.");

        return new HashSet<string>();
    }
}
