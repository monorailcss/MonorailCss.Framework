using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

/// <summary>
/// Covers the input-unchanged short-circuit in <see cref="MonorailCssGenerator"/> — two
/// calls with identical inputs return the same record instance without re-running the
/// full pipeline; mutating the input forces a fresh generation.
/// </summary>
public class MonorailCssGeneratorCacheTests
{
    [Fact]
    public void Generate_With_Identical_Inputs_Returns_Same_Result_Instance()
    {
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var request = new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4", "hover:underline" },
        };

        var first = generator.Generate(request);
        var second = generator.Generate(request);

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void Generate_With_Changed_Safelist_Returns_New_Result()
    {
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500" },
        });

        var second = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4" },
        });

        ReferenceEquals(first, second).ShouldBeFalse();
        second.Classes.ShouldContain("p-4");
        second.Css.ShouldContain("padding");
    }

    [Fact]
    public void Generate_With_Same_Set_Different_Order_Returns_Cached_Result()
    {
        // Candidate set is order-independent — the second call should hit the cache.
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4", "m-2" },
        });
        var second = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "m-2", "bg-red-500", "p-4" },
        });

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void Generate_With_Same_Set_Reverting_To_Earlier_Inputs_Reuses_Cache()
    {
        // After a change, reverting to the original safelist re-runs generation (the cache
        // holds only the most recent result) but still produces stable output.
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500" },
        });
        generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4" },
        });
        var third = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500" },
        });

        third.ETag.ShouldBe(first.ETag);
        third.Css.ShouldBe(first.Css);
    }

    [Fact]
    public void Generate_With_Changed_SourceCss_Bypasses_Cache()
    {
        // A different SourceCss yields a different framework instance, which clears the cache.
        var generator = new MonorailCssGenerator();
        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            SourceCss = "@theme { --color-brand: red; }",
            ExtraSafelist = new[] { "bg-brand" },
        });
        var second = generator.Generate(new MonorailCssGenerationRequest
        {
            SourceCss = "@theme { --color-brand: blue; }",
            ExtraSafelist = new[] { "bg-brand" },
        });

        ReferenceEquals(first, second).ShouldBeFalse();
        first.Css.ShouldContain("red");
        second.Css.ShouldContain("blue");
    }

    [Fact]
    public void Generate_With_Empty_ChangedSourceFiles_Returns_Cached_Result()
    {
        var generator = new MonorailCssGenerator();
        var framework = new CssFramework();

        var first = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4" },
        });

        // Empty (non-null) changed-file list means "nothing changed since last call".
        var second = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = framework,
            ExtraSafelist = new[] { "bg-red-500", "p-4" },
            ChangedSourceFiles = Array.Empty<string>(),
        });

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void Generate_With_ChangedSourceFiles_Only_Rescans_Listed_Files()
    {
        var dir = TempDir();
        try
        {
            var fileA = Path.Combine(dir, "a.razor");
            var fileB = Path.Combine(dir, "b.razor");
            File.WriteAllText(fileA, """<div class="bg-red-500"></div>""");
            File.WriteAllText(fileB, """<div class="p-4"></div>""");

            var generator = new MonorailCssGenerator();
            var framework = new CssFramework();

            // First pass: full scan picks up both files.
            var first = generator.Generate(new MonorailCssGenerationRequest
            {
                BaseFramework = framework,
                SourceFiles = new[] { fileA, fileB },
            });
            first.Classes.ShouldContain("bg-red-500");
            first.Classes.ShouldContain("p-4");

            // Modify file A only and rebuild via the incremental path (ChangedSourceFiles=[fileA]).
            // Bump mtime via timestamps to guarantee the per-file cache invalidates even when the
            // write completes within the same filesystem-time tick.
            File.WriteAllText(fileA, """<div class="bg-red-500 rotate-12"></div>""");
            File.SetLastWriteTimeUtc(fileA, DateTime.UtcNow.AddSeconds(1));

            var second = generator.Generate(new MonorailCssGenerationRequest
            {
                BaseFramework = framework,
                ChangedSourceFiles = new[] { fileA },
            });

            second.Classes.ShouldContain("rotate-12"); // new token from A
            second.Classes.ShouldContain("bg-red-500"); // still there
            second.Classes.ShouldContain("p-4"); // B not rescanned, but still in cache
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Generate_With_ChangedSourceFiles_Retains_Tokens_From_Previous_File_Version()
    {
        // Matches Tailwind's documented trade-off (packages/@tailwindcss-cli/.../build/index.ts:478-480):
        // tokens contributed by a previous version of a file are NOT removed when the file
        // is rescanned. A class that was deleted from the source lingers in the output until
        // a full rescan (startup, css-watcher) flushes the cache.
        var dir = TempDir();
        try
        {
            var file = Path.Combine(dir, "page.razor");
            File.WriteAllText(file, """<div class="rotate-12"></div>""");

            var generator = new MonorailCssGenerator();
            var framework = new CssFramework();

            var first = generator.Generate(new MonorailCssGenerationRequest
            {
                BaseFramework = framework,
                SourceFiles = new[] { file },
            });
            first.Classes.ShouldContain("rotate-12");

            // Remove the class from the file and trigger an incremental rescan.
            File.WriteAllText(file, """<div></div>""");
            File.SetLastWriteTimeUtc(file, DateTime.UtcNow.AddSeconds(1));

            var second = generator.Generate(new MonorailCssGenerationRequest
            {
                BaseFramework = framework,
                ChangedSourceFiles = new[] { file },
            });

            // Token from the previous version is preserved — this is the trade-off, not a bug.
            second.Classes.ShouldContain("rotate-12");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static string TempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "monorail-generator-cache-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
