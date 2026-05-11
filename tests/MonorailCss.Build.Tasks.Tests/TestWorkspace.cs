using Microsoft.Build.Framework;

namespace MonorailCss.Build.Tasks.Tests;

/// <summary>
/// Per-test temp directory + helpers for writing fixture files and instantiating
/// <see cref="ProcessCssTask"/> against the real file system. The framework's
/// <c>CssSourceProcessor</c> reads through <c>File.ReadAllText</c>, so build-task
/// tests can't use a mock filesystem; an isolated temp directory keeps each test
/// hermetic instead. Disposed at end-of-test to wipe the directory.
/// </summary>
internal sealed class TestWorkspace : IDisposable
{
    public string Root { get; }

    public TestWorkspace()
    {
        Root = Path.Combine(Path.GetTempPath(), $"monorail-buildtask-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Root);
    }

    /// <summary>
    /// Resolves a workspace-relative path. Path separators in <paramref name="relative"/>
    /// can be forward or backslashes; both are normalized to <see cref="Path.DirectorySeparatorChar"/>.
    /// </summary>
    public string PathFor(string relative)
    {
        var normalized = relative.Replace('\\', System.IO.Path.DirectorySeparatorChar)
                                 .Replace('/', System.IO.Path.DirectorySeparatorChar);
        return System.IO.Path.Combine(Root, normalized);
    }

    /// <summary>
    /// Writes <paramref name="content"/> to a workspace-relative path, creating any intermediate
    /// directories. Returns the absolute path so the caller can pass it to the task.
    /// </summary>
    public string WriteFile(string relative, string content)
    {
        var full = PathFor(relative);
        var dir = System.IO.Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(full, content);
        return full;
    }

    /// <summary>
    /// Constructs a <see cref="ProcessCssTask"/> wired to the real file system with the supplied
    /// input/output paths and a no-op build engine. Mirrors the parameter shape every test was
    /// repeating; centralizing it removes ~5 lines of boilerplate per test.
    /// </summary>
    public ProcessCssTask CreateTask(string inputFile, string outputFile)
    {
        return new ProcessCssTask
        {
            InputFile = inputFile,
            OutputFile = outputFile,
            BuildEngine = new MockBuildEngine(),
        };
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
        catch
        {
            // Best-effort: a stray file lock from an interrupted test shouldn't fail the suite.
        }
    }
}

/// <summary>
/// No-op <see cref="IBuildEngine"/> for tests. The build task's logging goes nowhere; tests assert
/// on the generated CSS file content rather than on log messages.
/// </summary>
internal sealed class MockBuildEngine : IBuildEngine
{
    public bool ContinueOnError => false;

    public int LineNumberOfTaskNode => 0;

    public int ColumnNumberOfTaskNode => 0;

    public string ProjectFileOfTaskNode => string.Empty;

    public bool BuildProjectFile(
        string projectFileName,
        string[] targetNames,
        System.Collections.IDictionary globalProperties,
        System.Collections.IDictionary targetOutputs)
        => true;

    public void LogCustomEvent(CustomBuildEventArgs e) { }

    public void LogErrorEvent(BuildErrorEventArgs e) { }

    public void LogMessageEvent(BuildMessageEventArgs e) { }

    public void LogWarningEvent(BuildWarningEventArgs e) { }
}
