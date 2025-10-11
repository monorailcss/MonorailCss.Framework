using Microsoft.Build.Framework;
using MonorailCss.Build.Tasks.Parsing;
using Shouldly;
using System.IO.Abstractions.TestingHelpers;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

namespace MonorailCss.Build.Tasks.Tests;

public class CssImportProcessorTests
{
    [Fact]
    public void ProcessImports_WithSingleFile_ReturnsBasicResult()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "tailwindcss";

            @theme {
                --color-brand: #ff0000;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.ShouldNotBeNull();
        result.ImportedFiles.Count.ShouldBe(1);
        result.ImportedFiles[0].ShouldBe(fileSystem.Path.GetFullPath(mainCss));
        result.ThemeVariables.Count.ShouldBe(1);
        result.ThemeVariables["--color-brand"].ShouldBe("#ff0000");
    }

    [Fact]
    public void ProcessImports_WithNestedImports_ProcessesAllFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var themeCss = XFS.Path(@"C:\project\theme.css");
        var colorsCss = XFS.Path(@"C:\project\colors.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./theme.css";

            @theme {
                --spacing-base: 1rem;
            }
            """));

        fileSystem.AddFile(themeCss, new MockFileData("""
            @import "./colors.css";

            @theme {
                --font-sans: system-ui;
            }
            """));

        fileSystem.AddFile(colorsCss, new MockFileData("""
            @theme {
                --color-primary: #3b82f6;
                --color-secondary: #8b5cf6;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.ImportedFiles.Count.ShouldBe(3);
        result.ThemeVariables.Count.ShouldBe(4);
        result.ThemeVariables["--spacing-base"].ShouldBe("1rem");
        result.ThemeVariables["--font-sans"].ShouldBe("system-ui");
        result.ThemeVariables["--color-primary"].ShouldBe("#3b82f6");
        result.ThemeVariables["--color-secondary"].ShouldBe("#8b5cf6");
    }

    [Fact]
    public void ProcessImports_WithCircularDependency_DoesNotInfiniteLoop()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var fileA = XFS.Path(@"C:\project\a.css");
        var fileB = XFS.Path(@"C:\project\b.css");

        fileSystem.AddFile(fileA, new MockFileData("""
            @import "./b.css";

            @theme {
                --color-a: red;
            }
            """));

        fileSystem.AddFile(fileB, new MockFileData("""
            @import "./a.css";

            @theme {
                --color-b: blue;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(fileA);

        // Assert - Should process both files exactly once
        result.ImportedFiles.Count.ShouldBe(2);
        result.ThemeVariables.Count.ShouldBe(2);
        result.ThemeVariables["--color-a"].ShouldBe("red");
        result.ThemeVariables["--color-b"].ShouldBe("blue");
    }

    [Fact]
    public void ProcessImports_WithMissingImport_ContinuesProcessing()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./missing.css";

            @theme {
                --color-primary: #3b82f6;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert - Should process main file even though import is missing
        result.ImportedFiles.Count.ShouldBe(1);
        result.ThemeVariables.Count.ShouldBe(1);
        result.ThemeVariables["--color-primary"].ShouldBe("#3b82f6");
    }

    [Fact]
    public void ProcessImports_WithFontFace_ExtractsRawCss()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var fontsCss = XFS.Path(@"C:\project\fonts.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./fonts.css";
            """));

        fileSystem.AddFile(fontsCss, new MockFileData("""
            @font-face {
                font-family: InterVariable;
                font-weight: 300 900;
                font-display: swap;
                src: url(/fonts/Inter.woff2) format("woff2");
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.RawCssRules.Count.ShouldBeGreaterThan(0);
        var rawCss = string.Join("\n", result.RawCssRules.Select(r => r.Content));
        rawCss.ShouldContain("@font-face");
        rawCss.ShouldContain("InterVariable");
    }

    [Fact]
    public void ProcessImports_WithKeyframes_ExtractsThemeVariable()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var animationsCss = XFS.Path(@"C:\project\animations.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./animations.css";
            """));

        fileSystem.AddFile(animationsCss, new MockFileData("""
            @keyframes spin {
                from { transform: rotate(0deg); }
                to { transform: rotate(360deg); }
            }

            @theme {
                --animate-spin: spin 1s linear infinite;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.ThemeVariables["--animate-spin"].ShouldBe("spin 1s linear infinite");

        // @keyframes should be in raw CSS
        result.RawCssRules.Count.ShouldBeGreaterThan(0);
        var rawCss = string.Join("\n", result.RawCssRules.Select(r => r.Content));
        rawCss.ShouldContain("@keyframes");
        rawCss.ShouldContain("spin");
    }

    [Fact]
    public void ProcessImports_WithRootSelector_ExtractsRawCss()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var layoutCss = XFS.Path(@"C:\project\layout.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./layout.css";
            """));

        fileSystem.AddFile(layoutCss, new MockFileData("""
            :root {
                --base-size: 1rem;
                --max-width: 1200px;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.RawCssRules.Count.ShouldBeGreaterThan(0);
        var rawCss = string.Join("\n", result.RawCssRules.Select(r => r.Content));
        rawCss.ShouldContain(":root");
        rawCss.ShouldContain("--base-size");
        rawCss.ShouldContain("--max-width");
    }

    [Fact]
    public void ProcessImports_WithDarkModeSelector_ExtractsRawCss()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var darkCss = XFS.Path(@"C:\project\dark.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./dark.css";
            """));

        fileSystem.AddFile(darkCss, new MockFileData("""
            .dark {
                --color-bg: #000000;
                --color-text: #ffffff;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.RawCssRules.Count.ShouldBeGreaterThan(0);
        var rawCss = string.Join("\n", result.RawCssRules.Select(r => r.Content));
        rawCss.ShouldContain(".dark");
        rawCss.ShouldContain("--color-bg");
    }

    [Fact]
    public void ProcessImports_WithInlineThemeVariables_ResolvesReferences()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @theme {
                --color-blue-500: #3b82f6;
            }

            @theme inline {
                --color-primary: var(--color-blue-500);
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.ThemeVariables["--color-blue-500"].ShouldBe("#3b82f6");
        result.InlineThemeVariables["--color-primary"].ShouldBe("var(--color-blue-500)");
    }

    [Fact]
    public void ProcessImports_WithStaticThemeVariables_MergesCorrectly()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @theme static {
                --color-brand: #ff0000;
                --spacing-unit: 0.25rem;
            }

            @theme static inline {
                --spacing-base: calc(var(--spacing-unit) * 4);
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.StaticThemeVariables.Count.ShouldBe(2);
        result.StaticThemeVariables["--color-brand"].ShouldBe("#ff0000");
        result.StaticThemeVariables["--spacing-unit"].ShouldBe("0.25rem");
        result.StaticInlineThemeVariables.Count.ShouldBe(1);
        result.StaticInlineThemeVariables["--spacing-base"].ShouldBe("calc(var(--spacing-unit) * 4)");
    }

    [Fact]
    public void ProcessImports_WithComponentRules_MergesFromAllFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var componentsCss = XFS.Path(@"C:\project\components.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./components.css";

            .btn-primary {
                @apply bg-blue-500 text-white px-4 py-2;
            }
            """));

        fileSystem.AddFile(componentsCss, new MockFileData("""
            .card {
                @apply rounded-lg shadow-md p-6;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.ComponentRules.Count.ShouldBe(2);
        result.ComponentRules[".btn-primary"].ShouldContain("bg-blue-500");
        result.ComponentRules[".card"].ShouldContain("rounded-lg");
    }

    [Fact]
    public void ProcessImports_WithCustomUtilities_MergesFromAllFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var utilitiesCss = XFS.Path(@"C:\project\utilities.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./utilities.css";

            @utility scrollbar-hide {
                scrollbar-width: none;
                -ms-overflow-style: none;

                &::-webkit-scrollbar {
                    display: none;
                }
            }
            """));

        fileSystem.AddFile(utilitiesCss, new MockFileData("""
            @utility text-balance {
                text-wrap: balance;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.UtilityDefinitions.Count.ShouldBe(2);
        result.UtilityDefinitions.Any(u => u.Pattern == "scrollbar-hide").ShouldBeTrue();
        result.UtilityDefinitions.Any(u => u.Pattern == "text-balance").ShouldBeTrue();
    }

    [Fact]
    public void ProcessImports_WithCustomVariants_MergesFromAllFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var variantsCss = XFS.Path(@"C:\project\variants.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./variants.css";

            @custom-variant scrollbar (&::-webkit-scrollbar);
            """));

        fileSystem.AddFile(variantsCss, new MockFileData("""
            @custom-variant dark (&:where(.dark, .dark *));
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.SourceConfiguration.CustomVariants.Count.ShouldBe(2);
        result.SourceConfiguration.CustomVariants.Any(v => v.Name == "scrollbar").ShouldBeTrue();
        result.SourceConfiguration.CustomVariants.Any(v => v.Name == "dark").ShouldBeTrue();
    }

    [Fact]
    public void ProcessImports_WithLayerBlock_PreservesInRawCss()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var baseCss = XFS.Path(@"C:\project\base.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./base.css";
            """));

        fileSystem.AddFile(baseCss, new MockFileData("""
            @layer base {
                body {
                    @apply text-foreground bg-background;
                }
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.RawCssRules.Count.ShouldBeGreaterThan(0);
        var rawCss = string.Join("\n", result.RawCssRules.Select(r => r.Content));
        rawCss.ShouldContain("@layer");
    }

    [Fact]
    public void ProcessImports_LumexUIStyleStructure_ProcessesCorrectly()
    {
        // Arrange - Simulate LumexUI structure
        var fileSystem = new MockFileSystem();
        var globalsCss = XFS.Path(@"C:\project\styles\globals.css");
        var themeCss = XFS.Path(@"C:\project\bin\lumexui\_theme.css");
        var layoutCss = XFS.Path(@"C:\project\bin\lumexui\_layout.css");
        var lightCss = XFS.Path(@"C:\project\bin\lumexui\_light.css");
        var darkCss = XFS.Path(@"C:\project\bin\lumexui\_dark.css");

        fileSystem.AddFile(globalsCss, new MockFileData("""
            @import "tailwindcss" theme(static);
            @import "../bin/lumexui/_theme";

            @theme {
                --font-sans: InterVariable, system-ui;
            }
            """));

        fileSystem.AddFile(themeCss, new MockFileData("""
            @import "./_layout";
            @import "./_light";
            @import "./_dark";

            @custom-variant dark (&:where(.dark, .dark *));

            @theme static inline {
                --color-background: var(--lumex-background);
                --color-foreground: var(--lumex-foreground);
            }
            """));

        fileSystem.AddFile(layoutCss, new MockFileData("""
            :root {
                --lumex-radius-small: 0.375rem;
                --lumex-radius-medium: 0.625rem;
            }
            """));

        fileSystem.AddFile(lightCss, new MockFileData("""
            :root, .light {
                color-scheme: light;
                --lumex-background: var(--color-white);
                --lumex-foreground: var(--color-zinc-900);
            }
            """));

        fileSystem.AddFile(darkCss, new MockFileData("""
            .dark {
                color-scheme: dark;
                --lumex-background: var(--color-black);
                --lumex-foreground: var(--color-zinc-100);
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(globalsCss);

        // Assert
        result.ImportedFiles.Count.ShouldBe(5); // globals, _theme, _layout, _light, _dark

        // Theme variables from globals.css
        result.ThemeVariables["--font-sans"].ShouldBe("InterVariable, system-ui");

        // Static inline theme variables from _theme.css
        result.StaticInlineThemeVariables.Count.ShouldBeGreaterThan(0);
        result.StaticInlineThemeVariables["--color-background"].ShouldBe("var(--lumex-background)");

        // Custom variants
        result.SourceConfiguration.CustomVariants.Any(v => v.Name == "dark").ShouldBeTrue();

        // Raw CSS from _layout.css, _light.css, _dark.css
        result.RawCssRules.Count.ShouldBeGreaterThan(0);
        var rawCss = string.Join("\n", result.RawCssRules.Select(r => r.Content));
        rawCss.ShouldContain(":root");
        rawCss.ShouldContain(".light");
        rawCss.ShouldContain(".dark");
        rawCss.ShouldContain("--lumex-background");
        rawCss.ShouldContain("--lumex-foreground");
        rawCss.ShouldContain("--lumex-radius-small");
    }

    [Fact]
    public void ProcessImports_WithRelativePaths_ResolvesCorrectly()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\src\styles\main.css");
        var themeCss = XFS.Path(@"C:\project\theme\colors.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "../../theme/colors.css";

            @theme {
                --spacing: 1rem;
            }
            """));

        fileSystem.AddFile(themeCss, new MockFileData("""
            @theme {
                --color-primary: #3b82f6;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.ImportedFiles.Count.ShouldBe(2);
        result.ThemeVariables.Count.ShouldBe(2);
        result.ThemeVariables["--spacing"].ShouldBe("1rem");
        result.ThemeVariables["--color-primary"].ShouldBe("#3b82f6");
    }

    [Fact]
    public void ProcessImports_WithoutFileExtension_AddsExtensionAutomatically()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var mainCss = XFS.Path(@"C:\project\main.css");
        var themeCss = XFS.Path(@"C:\project\theme.css");

        fileSystem.AddFile(mainCss, new MockFileData("""
            @import "./theme";
            """));

        fileSystem.AddFile(themeCss, new MockFileData("""
            @theme {
                --color-primary: #3b82f6;
            }
            """));

        var processor = new CssImportProcessor(fileSystem);

        // Act
        var result = processor.ProcessImports(mainCss);

        // Assert
        result.ImportedFiles.Count.ShouldBe(2);
        result.ThemeVariables["--color-primary"].ShouldBe("#3b82f6");
    }
}
