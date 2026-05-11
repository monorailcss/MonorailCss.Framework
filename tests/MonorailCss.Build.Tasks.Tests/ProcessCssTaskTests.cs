using Shouldly;

namespace MonorailCss.Build.Tasks.Tests;

public class ProcessCssTaskTests
{
    [Fact]
    public void Execute_WithSimpleUtility_GeneratesOutputFile()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """@import "tailwindcss";""");
        ws.WriteFile("index.html", """
            <html>
            <body class="bg-red-500">
                <h1>Hello World</h1>
            </body>
            </html>
            """);
        var outputPath = ws.PathFor("wwwroot/app.css");

        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldNotBeEmpty();
        output.ShouldContain("bg-red-500");
    }

    [Fact]
    public void Execute_CreatesOutputDirectory_WhenItDoesNotExist()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """@import "tailwindcss";""");
        ws.WriteFile("index.html", "<div class='p-4'></div>");
        var outputPath = ws.PathFor("output/nested/app.css");

        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        Directory.Exists(ws.PathFor("output/nested")).ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();
    }

    [Fact]
    public void Execute_WithCustomUtility_GeneratesUtilityRule()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss";

            @utility scrollbar-hide {
                scrollbar-width: none;
            }
            """);
        ws.WriteFile("index.html", """
            <html>
            <body>
                <div class="scrollbar-hide">Hidden scrollbars</div>
            </body>
            </html>
            """);
        var outputPath = ws.PathFor("wwwroot/app.css");

        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain(".scrollbar-hide");
        output.ShouldContain("scrollbar-width: none");
    }

    [Fact]
    public void Execute_WithCustomUtilityMixingDeclarationsAndNestedSelectors_GeneratesOutput()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss";

            @utility custom-button {
                background: linear-gradient(to right, red, blue);
                color: white;
            }
            """);
        ws.WriteFile("index.html", """
            <html>
            <body>
                <button class="custom-button">Click me</button>
            </body>
            </html>
            """);
        var outputPath = ws.PathFor("wwwroot/app.css");

        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain(".custom-button");
        output.ShouldContain("linear-gradient");
    }
}
