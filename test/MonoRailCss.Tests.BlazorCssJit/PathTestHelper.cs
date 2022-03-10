using System.Runtime.CompilerServices;

namespace MonoRailCss.Tests.BlazorCssJit;

public static class PathTestHelper
{
    public static string GetPath(string path)
    {
        return new FileInfo(Path.Combine(GetIntegrationTestRootDirectory(), path)).FullName;
    }

    private static string GetIntegrationTestRootDirectory([CallerFilePath] string? filePath = default)
    {
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)
                                              ?? throw new InvalidOperationException("path shouldn't be null"));
        return directoryInfo.FullName;
    }
}