using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
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
    public void BuildExcludeSet_IsEmpty_WhenNoUserAssembliesSupplied()
    {
        var task = new ProcessCssTask
        {
            InputFile = "ignored.css",
            OutputFile = "ignored-out.css",
            BuildEngine = new MockBuildEngine(),
            ExcludeAssemblies = null,
        };

        // Framework assemblies self-exclude via [assembly: MonorailCssNoScan]; the exclude
        // set now carries only user-supplied names.
        task.BuildExcludeSet().ShouldBeEmpty();
    }

    [Fact]
    public void BuildExcludeSet_CollectsUserSuppliedItems_CaseInsensitively()
    {
        var task = new ProcessCssTask
        {
            InputFile = "ignored.css",
            OutputFile = "ignored-out.css",
            BuildEngine = new MockBuildEngine(),
            ExcludeAssemblies = [new TaskItem("FluentValidation"), new TaskItem("LumexUI.Motion")],
        };

        var set = task.BuildExcludeSet();

        set.ShouldContain("FluentValidation");
        set.ShouldContain("LumexUI.Motion");

        // Case-insensitive lookup matches the OrdinalIgnoreCase comparer.
        set.Contains("FLUENTVALIDATION").ShouldBeTrue();
    }

    [Fact]
    public void Execute_ScansStaticWebAssetJavaScript()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """@import "tailwindcss";""");

        // The .js asset lives under a name the default content sweep ignores (it only globs
        // .html/.razor/etc.), and no markup references w-3/h-3 — so if those classes appear in
        // the output, the static-web-asset path is the only thing that could have surfaced them.
        var jsPath = ws.WriteFile("_pkg/scripts.js", "el.classList.add('w-3', 'h-3');");
        var outputPath = ws.PathFor("wwwroot/app.css");

        var task = ws.CreateTask(inputPath, outputPath);
        task.StaticWebAssets = [new TaskItem(jsPath)];

        task.Execute().ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain(".w-3");
        output.ShouldContain(".h-3");
    }

    [Fact]
    public void Execute_SkipsStaticWebAssets_FromExcludedPackage()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """@import "tailwindcss";""");
        var jsPath = ws.WriteFile("_pkg/scripts.js", "el.classList.add('w-3', 'h-3');");
        var outputPath = ws.PathFor("wwwroot/app.css");

        var asset = new TaskItem(jsPath);
        asset.SetMetadata("SourceId", "Excluded.Pkg");

        var task = ws.CreateTask(inputPath, outputPath);
        task.StaticWebAssets = [asset];
        task.ExcludeAssemblies = [new TaskItem("Excluded.Pkg")];

        task.Execute().ShouldBeTrue();

        // The owning package is excluded, so its JS must not contribute classes.
        var output = File.ReadAllText(outputPath);
        output.ShouldNotContain(".w-3");
        output.ShouldNotContain(".h-3");
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
