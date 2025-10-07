using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Microsoft.Build.Utilities;

namespace MonorailCss.Build.Tasks.Scanning;

/// <summary>
/// Scans DLL files for utility classes by extracting string literals from PE metadata.
/// </summary>
internal class DllScanner
{
    /// <summary>
    /// Scans a DLL file for utility classes by extracting user strings from IL code.
    /// </summary>
    /// <param name="dllPath">The path to the DLL file.</param>
    /// <param name="log">The MSBuild task logger.</param>
    /// <returns>A set of utility class names found in the DLL.</returns>
    public HashSet<string> ScanDllForUtilities(string dllPath, TaskLoggingHelper log)
    {
        if (!File.Exists(dllPath))
        {
            log.LogWarning($"DLL file not found: {dllPath}");
            return [];
        }

        try
        {
            log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, $"Scanning DLL for utilities: {dllPath}");

            using var stream = File.OpenRead(dllPath);
            using var peReader = new PEReader(stream);

            if (!peReader.HasMetadata)
            {
                log.LogWarning($"DLL does not contain metadata: {dllPath}");
                return [];
            }

            var metadataReader = peReader.GetMetadataReader();
            var strings = ScanForStringTokens(peReader, metadataReader, log);

            log.LogMessage(Microsoft.Build.Framework.MessageImportance.Normal,
                $"Found {strings.Count} string literals in {dllPath}");

            return strings;
        }
        catch (Exception ex)
        {
            log.LogWarning($"Failed to scan DLL {dllPath}: {ex.Message}");
            log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, ex.ToString());
            return [];
        }
    }

    /// <summary>
    /// Scans method bodies for ldstr instructions and extracts user strings.
    /// </summary>
    private HashSet<string> ScanForStringTokens(PEReader peReader, MetadataReader metadataReader, TaskLoggingHelper log)
    {
        var strings = new HashSet<string>();
        var methodsScanned = 0;
        var stringsFound = 0;

        var methodDefinitions = metadataReader.MethodDefinitions.Select(metadataReader.GetMethodDefinition).Where(methodDef => methodDef.RelativeVirtualAddress != 0);
        foreach (var methodDef in methodDefinitions)
        {
            try
            {
                var methodBody = peReader.GetMethodBody(methodDef.RelativeVirtualAddress);

                var ilBytes = methodBody.GetILContent().ToArray();

                // Scan for ldstr instructions (0x72)
                for (var i = 0; i < ilBytes.Length - 4; i++)
                {
                    if (ilBytes[i] != 0x72) // ldstr opcode
                    {
                        continue;
                    }

                    var token = BitConverter.ToInt32(ilBytes, i + 1);

                    // Check if it's a valid user string token (0x70000000 range)
                    if ((token & 0x70000000) != 0x70000000)
                    {
                        continue;
                    }

                    try
                    {
                        var handle = MetadataTokens.UserStringHandle(token);
                        var str = metadataReader.GetUserString(handle);
                        if (string.IsNullOrEmpty(str))
                        {
                            continue;
                        }

                        strings.Add(str);
                        stringsFound++;
                    }
                    catch
                    {
                        // Invalid token, skip
                    }
                }

                methodsScanned++;
            }
            catch
            {
                // Skip methods we can't read
            }
        }

        log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low,
            $"Scanned {methodsScanned} methods, found {stringsFound} string literals");

        return strings;
    }
}
