using Microsoft.Build.Framework;
using Shouldly;
using System.IO.Abstractions.TestingHelpers;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

namespace MonorailCss.Build.Tasks.Tests;

public class ProcessCssTaskTests
{
    [Fact]
    public void Execute_WithSimpleUtility_GeneratesOutputFile()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        // Create input CSS file with simple content
        var inputPath = XFS.Path(@"C:\project\app.css");
        var outputPath = XFS.Path(@"C:\project\wwwroot\app.css");
        const string inputContent = """
            @import "tailwindcss";
            """;

        fileSystem.AddFile(inputPath, new MockFileData(inputContent));

        // Create a simple HTML file with a utility class
        var htmlPath = XFS.Path(@"C:\project\index.html");
        const string htmlContent = """
            <html>
            <body class="bg-red-500">
                <h1>Hello World</h1>
            </body>
            </html>
            """;

        fileSystem.AddFile(htmlPath, new MockFileData(htmlContent));

        // Create the task with mocked file system
        var task = new ProcessCssTask(fileSystem)
        {
            InputFile = inputPath,
            OutputFile = outputPath,
            BuildEngine = new MockBuildEngine()
        };

        // Act
        var result = task.Execute();

        // Assert
        result.ShouldBeTrue();
        fileSystem.File.Exists(outputPath).ShouldBeTrue();

        var outputContent = fileSystem.File.ReadAllText(outputPath);
        outputContent.ShouldNotBeEmpty();

        // The output should contain CSS for the bg-red-500 utility
        outputContent.ShouldContain("bg-red-500");
    }

    [Fact]
    public void Execute_CreatesOutputDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var inputPath = XFS.Path(@"C:\project\app.css");
        var outputPath = XFS.Path(@"C:\project\output\nested\app.css");
        const string inputContent = """
            @import "tailwindcss";
            """;

        fileSystem.AddFile(inputPath, new MockFileData(inputContent));

        // Create a simple HTML file
        var htmlPath = XFS.Path(@"C:\project\index.html");
        fileSystem.AddFile(htmlPath, new MockFileData("<div class='p-4'></div>"));

        var task = new ProcessCssTask(fileSystem)
        {
            InputFile = inputPath,
            OutputFile = outputPath,
            BuildEngine = new MockBuildEngine()
        };

        // Act
        var result = task.Execute();

        // Assert
        result.ShouldBeTrue();
        fileSystem.Directory.Exists(XFS.Path(@"C:\project\output\nested")).ShouldBeTrue();
        fileSystem.File.Exists(outputPath).ShouldBeTrue();
    }

    [Fact]
    public void Execute_WithCustomUtilityUsingApply_GeneratesCorrectOutput()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var inputPath = XFS.Path(@"C:\project\app.css");
        var outputPath = XFS.Path(@"C:\project\wwwroot\app.css");
        const string inputContent = """
            @import "tailwindcss";

            @utility bordered-link {
                @apply font-semibold leading-tight text-current border-b border-current hover:border-b-2;
            }
            """;

        fileSystem.AddFile(inputPath, new MockFileData(inputContent));

        // Create an HTML file that uses the custom utility
        var htmlPath = XFS.Path(@"C:\project\index.html");
        const string htmlContent = """
            <html>
            <body>
                <a class="bordered-link" href="#">Click me</a>
            </body>
            </html>
            """;

        fileSystem.AddFile(htmlPath, new MockFileData(htmlContent));

        var task = new ProcessCssTask(fileSystem)
        {
            InputFile = inputPath,
            OutputFile = outputPath,
            BuildEngine = new MockBuildEngine()
        };

        // Act
        var result = task.Execute();

        // Assert
        result.ShouldBeTrue();
        fileSystem.File.Exists(outputPath).ShouldBeTrue();

        var outputContent = fileSystem.File.ReadAllText(outputPath);
        outputContent.ShouldNotBeEmpty();

        // The output should contain the custom utility class
        outputContent.ShouldContain("bordered-link");
    }

    [Fact]
    public void Execute_WithCustomUtilityMixedApplyAndDeclarations_GeneratesOutput()
    {
        // Arrange
        var fileSystem = new MockFileSystem();

        var inputPath = XFS.Path(@"C:\project\app.css");
        var outputPath = XFS.Path(@"C:\project\wwwroot\app.css");
        const string inputContent = """
            @import "tailwindcss";

            @utility custom-button {
                @apply font-bold px-4 py-2;
                background: linear-gradient(to right, red, blue);
            }
            """;

        fileSystem.AddFile(inputPath, new MockFileData(inputContent));

        // Create an HTML file that uses the custom utility
        var htmlPath = XFS.Path(@"C:\project\index.html");
        const string htmlContent = """
            <html>
            <body>
                <button class="custom-button">Click me</button>
            </body>
            </html>
            """;

        fileSystem.AddFile(htmlPath, new MockFileData(htmlContent));

        var task = new ProcessCssTask(fileSystem)
        {
            InputFile = inputPath,
            OutputFile = outputPath,
            BuildEngine = new MockBuildEngine()
        };

        // Act
        var result = task.Execute();

        // Assert
        result.ShouldBeTrue();
        fileSystem.File.Exists(outputPath).ShouldBeTrue();

        var outputContent = fileSystem.File.ReadAllText(outputPath);
        outputContent.ShouldNotBeEmpty();
        outputContent.ShouldContain("custom-button");
    }
}

/// <summary>
/// Mock implementation of IBuildEngine for testing MSBuild tasks.
/// </summary>
internal class MockBuildEngine : IBuildEngine
{
    public bool ContinueOnError => false;
    public int LineNumberOfTaskNode => 0;
    public int ColumnNumberOfTaskNode => 0;
    public string ProjectFileOfTaskNode => string.Empty;

    public bool BuildProjectFile(string projectFileName, string[] targetNames,
        System.Collections.IDictionary globalProperties,
        System.Collections.IDictionary targetOutputs)
    {
        return true;
    }

    public void LogCustomEvent(CustomBuildEventArgs e)
    {
        // No-op for testing
    }

    public void LogErrorEvent(BuildErrorEventArgs e)
    {
        // No-op for testing
    }

    public void LogMessageEvent(BuildMessageEventArgs e)
    {
        // No-op for testing
    }

    public void LogWarningEvent(BuildWarningEventArgs e)
    {
        // No-op for testing
    }
}
