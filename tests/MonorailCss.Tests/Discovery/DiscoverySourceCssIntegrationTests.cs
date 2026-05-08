using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

/// <summary>
/// End-to-end coverage of the source-CSS pipeline integrated into <see cref="ClassDiscoveryService"/>.
/// Drops a Tailwind v4-style CSS file on disk, runs the service through <c>StartAsync</c>, and
/// verifies the framework picks up theme tokens, custom utilities, and custom variants from the
/// CSS without any other configuration.
/// </summary>
public class DiscoverySourceCssIntegrationTests
{
    [Fact]
    public async Task SourceCssPath_Loads_Theme_Tokens_And_Compiles_Custom_Color_Class()
    {
        var dir = TempDir();
        try
        {
            var cssPath = Path.Combine(dir, "app.css");
            File.WriteAllText(cssPath, """
                                       @theme {
                                           --color-brand: oklch(60% 0.2 250);
                                       }
                                       """);

            var (service, options) = CreateService(o => o.SourceCssPath = cssPath);
            using (service)
            {
                await service.StartAsync(CancellationToken.None);

                // The service swapped in a fresh framework built from the parsed CSS.
                // Verify by checking that bg-brand is now a valid candidate.
                options.Framework.TryValidateCandidate("bg-brand").ShouldBeTrue();
                var generated = options.Framework.Process(["bg-brand"]);
                generated.ShouldContain("--color-brand");

                await service.StopAsync(CancellationToken.None);
            }
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task SourceCssPath_Follows_Imports_And_Picks_Up_Custom_Utility()
    {
        var dir = TempDir();
        try
        {
            var importedPath = Path.Combine(dir, "_extras.css");
            File.WriteAllText(importedPath, """
                                            @utility scrollbar-hide {
                                                scrollbar-width: none;
                                            }
                                            """);

            var entryPath = Path.Combine(dir, "app.css");
            File.WriteAllText(entryPath, """
                                         @import "./_extras.css";
                                         @theme {
                                             --color-primary: red;
                                         }
                                         """);

            var (service, options) = CreateService(o =>
            {
                o.SourceCssPath = entryPath;
                o.ExtraSafelist.Add("scrollbar-hide");
                o.ExtraSafelist.Add("bg-primary");
            });
            using (service)
            {
                await service.StartAsync(CancellationToken.None);

                var css = service.Css;
                css.ShouldContain("scrollbar-width: none");
                css.ShouldContain("--color-primary");

                await service.StopAsync(CancellationToken.None);
            }
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task SourceCss_Inline_Adds_Custom_Variant_To_Framework()
    {
        // Inline mode: no SourceCssPath, only SourceCss. CssSourceProcessor.ProcessSource runs
        // without a base path so @import is recorded but not loaded.
        var (service, options) = CreateService(o =>
        {
            o.SourceCss = """
                          @custom-variant scrollbar (&::-webkit-scrollbar);
                          """;
        });
        using (service)
        {
            await service.StartAsync(CancellationToken.None);

            options.Framework.Settings.CustomVariants.ShouldContain(v => v.Name == "scrollbar");
            options.Framework.TryValidateCandidate("scrollbar:bg-red-500").ShouldBeTrue();

            await service.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task RawCss_Residue_Is_Prepended_To_Generated_Css()
    {
        var dir = TempDir();
        try
        {
            var cssPath = Path.Combine(dir, "app.css");
            File.WriteAllText(cssPath, """
                                       @theme {
                                           --color-brand: blue;
                                       }
                                       /* This selector is not consumed by the framework — it should flow through. */
                                       :root {
                                           --my-runtime-token: hotpink;
                                       }
                                       """);

            var (service, _) = CreateService(o => o.SourceCssPath = cssPath);
            using (service)
            {
                await service.StartAsync(CancellationToken.None);

                var css = service.Css;
                css.ShouldContain("--my-runtime-token");
                css.ShouldContain("hotpink");

                // @theme was consumed; the raw block doesn't carry the @theme directive forward.
                css.ShouldNotContain("@theme {");

                await service.StopAsync(CancellationToken.None);
            }
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task SourceCssPath_User_Keyframes_Are_Emitted_When_Animate_Variable_Is_Used()
    {
        var dir = TempDir();
        try
        {
            var cssPath = Path.Combine(dir, "app.css");
            File.WriteAllText(cssPath, """
                                       @theme static inline {
                                           --animate-enter: enter 150ms ease-out normal both;

                                           @keyframes enter {
                                               0% { opacity: 0; transform: scale(0.95); }
                                               100% { opacity: 1; transform: scale(1); }
                                           }
                                       }
                                       """);

            var (service, _) = CreateService(o =>
            {
                o.SourceCssPath = cssPath;
                o.ExtraSafelist.Add("animate-enter");
            });
            using (service)
            {
                await service.StartAsync(CancellationToken.None);

                var css = service.Css;

                // The @keyframes block should be in the output, top-level.
                css.ShouldContain("@keyframes enter");
                css.ShouldContain("opacity: 0");
                css.ShouldContain("scale(0.95)");

                // And the .animate-enter class should reference the variable.
                css.ShouldContain("--animate-enter");

                await service.StopAsync(CancellationToken.None);
            }
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static (ClassDiscoveryService Service, MonorailDiscoveryOptions Options) CreateService(Action<MonorailDiscoveryOptions>? configure = null)
    {
        var options = new MonorailDiscoveryOptions();
        configure?.Invoke(options);
        var wrapped = Options.Create(options);
        var service = new ClassDiscoveryService(wrapped, NullLogger<ClassDiscoveryService>.Instance);
        return (service, options);
    }

    private static string TempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "monorail-discovery-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
