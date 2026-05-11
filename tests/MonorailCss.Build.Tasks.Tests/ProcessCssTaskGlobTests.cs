using Shouldly;

namespace MonorailCss.Build.Tasks.Tests;

public class ProcessCssTaskGlobTests
{
    [Fact]
    public void Execute_WithSimpleWildcardGlob_ScansMatchingFiles()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "bin/lumexui/*.cs";
            """);

        ws.WriteFile("bin/lumexui/Button.cs", """
            public class Button
            {
                public string CssClass => "class='bg-blue-500 text-white px-4 py-2'";
            }
            """);
        ws.WriteFile("bin/lumexui/Card.cs", """
            public class Card
            {
                public string CssClass => "class='border rounded-lg shadow-md p-6'";
            }
            """);

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain("bg-blue-500");
        output.ShouldContain("text-white");
        output.ShouldContain("px-4");
        output.ShouldContain("border");
        output.ShouldContain("rounded-lg");
        output.ShouldContain("shadow-md");
    }

    [Fact]
    public void Execute_WithBraceExpansionGlob_ScansMultipleDirectories()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "{Pages,Components}/**/*.razor";
            """);

        ws.WriteFile("Pages/Index.razor", """
            <div class="container mx-auto">
                <h1 class="text-3xl font-bold">Home</h1>
            </div>
            """);
        ws.WriteFile("Pages/About.razor", """
            <div class="p-4 bg-gray-100">
                <p class="text-lg">About us</p>
            </div>
            """);
        ws.WriteFile("Components/Button.razor", """
            <button class="bg-blue-500 text-white px-4 py-2 rounded">
                Click me
            </button>
            """);

        // File in a directory the glob shouldn't match.
        ws.WriteFile("Layouts/Main.razor", """
            <main class="min-h-screen">
                @Body
            </main>
            """);

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain("container");
        output.ShouldContain("mx-auto");
        output.ShouldContain("text-3xl");
        output.ShouldContain("bg-gray-100");
        output.ShouldContain("bg-blue-500");
        output.ShouldContain("text-white");
        output.ShouldContain("rounded");
        output.ShouldNotContain("min-h-screen");
    }

    [Fact]
    public void Execute_WithMultipleExtensionGlob_ScansAllMatchingExtensions()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "Pages/**/*.{razor,razor.cs}";
            """);

        ws.WriteFile("Pages/Index.razor", """
            <div class="p-4 bg-white">
                <h1 class="text-xl font-bold">Home</h1>
            </div>
            """);
        ws.WriteFile("Pages/Index.razor.cs", """
            public partial class Index
            {
                private string containerClass = @"class='flex items-center justify-center'";
            }
            """);

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain("p-4");
        output.ShouldContain("bg-white");
        output.ShouldContain("text-xl");
        output.ShouldContain("font-bold");
        output.ShouldContain("flex");
        output.ShouldContain("items-center");
        output.ShouldContain("justify-center");
    }

    [Fact]
    public void Execute_WithRecursiveGlob_ScansNestedDirectories()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "src/**/*.html";
            """);

        ws.WriteFile("src/index.html", """<div class="p-4 bg-gray-50">Home</div>""");
        ws.WriteFile("src/pages/about.html", """<div class="text-lg font-semibold">About</div>""");
        ws.WriteFile("src/pages/docs/guide.html", """<div class="bg-blue-100 rounded-lg">Guide</div>""");

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain("p-4");
        output.ShouldContain("bg-gray-50");
        output.ShouldContain("text-lg");
        output.ShouldContain("font-semibold");
        output.ShouldContain("bg-blue-100");
        output.ShouldContain("rounded-lg");
    }

    [Fact]
    public void Execute_WithGlobExcludingBinObj_IgnoresThoseDirectories()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "**/*.cs";
            """);

        ws.WriteFile("src/App.cs", """
            public class App
            {
                public string Class => "class='flex flex-col gap-4'";
            }
            """);
        // bin/ and obj/ trees should be ignored by the default exclude list.
        ws.WriteFile("bin/Debug/App.cs", """
            public class App
            {
                public string Class => "class='bg-red-500'";
            }
            """);
        ws.WriteFile("obj/Debug/Temp.cs", """
            public class Temp
            {
                public string Class => "class='text-yellow-500'";
            }
            """);

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain("flex");
        output.ShouldContain("flex-col");
        output.ShouldContain("gap-4");
        output.ShouldNotContain("bg-red-500");
        output.ShouldNotContain("text-yellow-500");
    }

    [Fact]
    public void Execute_WithMixedGlobAndLiteralPaths_ProcessesBoth()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "Components/**/*.razor";
            @source "Pages/Index.razor";
            """);

        ws.WriteFile("Components/Button.razor", """
            <button class="bg-green-500 text-sm font-medium">Click</button>
            """);
        ws.WriteFile("Pages/Index.razor", """
            <div class="max-w-4xl mx-auto p-6">Home</div>
            """);

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain("bg-green-500");
        output.ShouldContain("text-sm");
        output.ShouldContain("font-medium");
        output.ShouldContain("max-w-4xl");
        output.ShouldContain("mx-auto");
        output.ShouldContain("p-6");
    }

    [Fact]
    public void Execute_WithComplexBraceAndExtensionGlob_MatchesCorrectFiles()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "{Pages,Components}/**/*.{razor,razor.cs}";
            """);

        ws.WriteFile("Pages/Index.razor", """
            <div class="grid grid-cols-2 gap-6">Index</div>
            """);
        ws.WriteFile("Pages/Index.razor.cs", """
            public partial class Index { string css = "class='border-2 border-gray-300'"; }
            """);
        ws.WriteFile("Components/Button.razor", """
            <button class="rounded-md shadow-sm">Click</button>
            """);
        // Should NOT match — wrong directory.
        ws.WriteFile("Layouts/Main.razor", """
            <div class="w-full h-full">Layout</div>
            """);
        // Should NOT match — wrong extension.
        ws.WriteFile("Pages/test.html", """
            <div class="bg-purple-500">Test</div>
            """);

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldContain("grid");
        output.ShouldContain("grid-cols-2");
        output.ShouldContain("gap-6");
        output.ShouldContain("border-2");
        output.ShouldContain("border-gray-300");
        output.ShouldContain("rounded-md");
        output.ShouldContain("shadow-sm");
        output.ShouldNotContain("w-full");
        output.ShouldNotContain("h-full");
        output.ShouldNotContain("bg-purple-500");
    }

    [Fact]
    public void Execute_WithGlobNoMatches_GeneratesOutputWithoutUtilities()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source "nonexistent/**/*.razor";
            """);

        var outputPath = ws.PathFor("wwwroot/app.css");
        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.Exists(outputPath).ShouldBeTrue();
        // No discovered utilities + no @apply rules means the framework short-circuits to empty
        // CSS (see CssFramework.ProcessWithDetails). We just want the task to complete cleanly
        // with no exceptions and an output file that exists.
    }

    [Fact]
    public void Execute_WhenOutputIsUpToDate_SkipsRegeneration()
    {
        using var ws = new TestWorkspace();

        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source inline("bg-blue-500 text-white");
            """);
        var outputPath = ws.WriteFile("wwwroot/app.css", "/* existing CSS */");

        // Force the output's mtime to be newer than the input so the up-to-date check fires.
        File.SetLastWriteTimeUtc(outputPath, File.GetLastWriteTimeUtc(inputPath).AddSeconds(10));

        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();
        File.ReadAllText(outputPath).ShouldBe("/* existing CSS */");
    }

    [Fact]
    public void Execute_WhenInputIsNewer_RegeneratesOutput()
    {
        using var ws = new TestWorkspace();

        var outputPath = ws.WriteFile("wwwroot/app.css", "/* old CSS */");
        var inputPath = ws.WriteFile("app.css", """
            @import "tailwindcss" source(none);
            @source inline("bg-blue-500 text-white");
            """);

        File.SetLastWriteTimeUtc(inputPath, File.GetLastWriteTimeUtc(outputPath).AddSeconds(10));

        var task = ws.CreateTask(inputPath, outputPath);

        task.Execute().ShouldBeTrue();

        var output = File.ReadAllText(outputPath);
        output.ShouldNotBe("/* old CSS */");
        output.ShouldContain("bg-blue-500");
        output.ShouldContain("text-white");
    }
}
