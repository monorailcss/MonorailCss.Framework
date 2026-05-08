using MonorailCss.Parser.SourceCss;
using Shouldly;

namespace MonorailCss.Tests.Parser.SourceCss;

public class CssSourceProcessorTests
{
    [Fact]
    public void Process_Source_Adds_Theme_Variables_To_Framework_Settings()
    {
        const string css = """
                           @theme {
                               --color-brand: oklch(60% 0.2 250);
                               --shadow-card: 0 1px 3px rgba(0,0,0,0.1);
                           }
                           """;

        var processor = new CssSourceProcessor();
        var result = processor.ProcessSource(css);

        result.Settings.Theme.ContainsKey("--color-brand").ShouldBeTrue();
        result.Settings.Theme.ContainsKey("--shadow-card").ShouldBeTrue();
    }

    [Fact]
    public void Process_Source_Resolves_Bg_Class_Using_Theme_Variable()
    {
        const string css = """
                           @theme {
                               --color-brand: oklch(60% 0.2 250);
                           }
                           """;

        var processor = new CssSourceProcessor();
        var result = processor.ProcessSource(css);
        var framework = new CssFramework(result.Settings);

        var generated = framework.Process(["bg-brand"]);

        generated.ShouldContain("--color-brand");
        generated.ShouldContain("background-color");
    }

    [Fact]
    public void Process_Source_Resolves_Theme_Inline_Variables_Through_Add_Inline()
    {
        // `var(--color-zinc-500)` is a known Tailwind default; AddInline should resolve it
        // to its concrete value rather than leaving the var() intact.
        const string css = """
                           @theme inline {
                               --color-foreground: var(--color-zinc-500);
                           }
                           """;

        var baseSettings = new CssFrameworkSettings
        {
            Theme = MonorailCss.Theme.Theme.CreateWithDefaults(),
        };

        var processor = new CssSourceProcessor();
        var result = processor.ProcessSource(css, basePath: null, baseSettings);

        var resolved = result.Settings.Theme.ResolveValue("--color-foreground", []);
        resolved.ShouldNotBeNull();
        // Resolved value should be a concrete oklch token (Tailwind 4 zinc-500), not the
        // var() reference.
        resolved.ShouldContain("oklch");
    }

    [Fact]
    public void Process_Source_Picks_Up_Custom_Variant_With_Nested_Parens()
    {
        const string css = """
                           @custom-variant dark (&:where(.dark, .dark *));
                           """;

        var processor = new CssSourceProcessor();
        var result = processor.ProcessSource(css);

        result.Settings.CustomVariants.Count.ShouldBe(1);
        result.Settings.CustomVariants[0].Name.ShouldBe("dark");
        result.Settings.CustomVariants[0].Selector.ShouldBe("&:where(.dark, .dark *)");
    }

    [Fact]
    public void Process_Source_Picks_Up_Apply_Inside_Layer_Base()
    {
        const string css = """
                           @layer base {
                               body { @apply text-foreground bg-background; }
                           }
                           """;

        var processor = new CssSourceProcessor();
        var result = processor.ProcessSource(css);

        result.Settings.Applies.ShouldContainKey("body");
        result.Settings.Applies["body"].ShouldBe("text-foreground bg-background");
    }

    [Fact]
    public void Process_Source_Captures_Pass_Through_Css_As_Raw()
    {
        const string css = """
                           @theme {
                               --color-brand: blue;
                           }
                           :root {
                               --my-runtime-token: red;
                           }
                           @keyframes spin {
                               to { transform: rotate(360deg); }
                           }
                           """;

        var processor = new CssSourceProcessor();
        var result = processor.ProcessSource(css);

        // @theme block was consumed → not in raw
        result.RawCss.ShouldNotContain("@theme");

        // :root and @keyframes flow through verbatim
        result.RawCss.ShouldContain(":root");
        result.RawCss.ShouldContain("--my-runtime-token");
        result.RawCss.ShouldContain("@keyframes spin");
    }

    [Fact]
    public void Process_File_Follows_Imports_Recursively()
    {
        var dir = Path.Combine(Path.GetTempPath(), "monorail-source-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            var importedPath = Path.Combine(dir, "_palette.css");
            File.WriteAllText(importedPath, """
                                            @theme {
                                                --color-import: red;
                                            }
                                            """);

            var entryPath = Path.Combine(dir, "app.css");
            File.WriteAllText(entryPath, """
                                         @import "./_palette.css";
                                         @theme {
                                             --color-entry: green;
                                         }
                                         """);

            var processor = new CssSourceProcessor();
            var result = processor.ProcessFile(entryPath);

            result.Settings.Theme.ContainsKey("--color-import").ShouldBeTrue();
            result.Settings.Theme.ContainsKey("--color-entry").ShouldBeTrue();

            result.ImportedFiles.Count.ShouldBe(2);
            result.ImportedFiles.ShouldContain(p => Path.GetFileName(p) == "app.css");
            result.ImportedFiles.ShouldContain(p => Path.GetFileName(p) == "_palette.css");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Process_File_Wires_Custom_Utility_Apply_Through_Settings()
    {
        var path = Path.Combine(Path.GetTempPath(), "monorail-source-test-" + Guid.NewGuid().ToString("N") + ".css");
        File.WriteAllText(path, """
                                @utility scrollbar-hide {
                                    scrollbar-width: none;
                                }
                                """);

        try
        {
            var processor = new CssSourceProcessor();
            var result = processor.ProcessFile(path);

            result.Settings.CustomUtilities.Count.ShouldBe(1);
            result.Settings.CustomUtilities[0].Pattern.ShouldBe("scrollbar-hide");

            // The utility should now be compilable through a framework built from the result.
            var framework = new CssFramework(result.Settings);
            var generated = framework.Process(["scrollbar-hide"]);
            generated.ShouldContain("scrollbar-width: none");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
