using System.Collections.Immutable;
using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

/// <summary>
/// Demonstrates how to use ThemeToCssConverter to export configurations.
/// </summary>
public class ThemeToCssConverterExampleTest
{
    [Fact]
    public void ExportConfigurationToCss_Example()
    {
        // Arrange - Create a configuration in C#
        var settings = new CssFrameworkSettings
        {
            Theme = MonorailCss.Theme.Theme.CreateEmpty()
                .Add("--color-brand-primary", "#0066cc")
                .Add("--color-brand-secondary", "#6633ff")
                .Add("--spacing-xl", "2.5rem")
                .Add("--font-heading", "'Montserrat', sans-serif"),
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn-primary", "bg-brand-primary text-white px-6 py-3 rounded-lg hover:opacity-90")
                .Add(".btn-secondary", "bg-brand-secondary text-white px-6 py-3 rounded-lg hover:opacity-90")
                .Add(".card", "bg-white shadow-lg rounded-xl p-6")
                .Add(".heading", "font-heading text-4xl font-bold")
        };

        // Act - Export to CSS
        var converter = new ThemeToCssConverter();
        var cssOutput = converter.ConvertSettings(settings);

        // Output for demonstration
        Console.WriteLine("=== Exported CSS Configuration ===");
        Console.WriteLine(cssOutput);
        Console.WriteLine("==================================");

        // Assert - Verify the export contains expected content
        cssOutput.ShouldContain("@import \"tailwindcss\";");
        cssOutput.ShouldContain("@theme {");
        cssOutput.ShouldContain("--color-brand-primary: #0066cc;");
        cssOutput.ShouldContain("--color-brand-secondary: #6633ff;");
        cssOutput.ShouldContain(".btn-primary {");
        cssOutput.ShouldContain("@apply bg-brand-primary text-white px-6 py-3 rounded-lg hover:opacity-90;");
    }

    [Fact]
    public void ShareConfigurationBetweenProjects_Example()
    {
        // Demonstrates exporting a design system configuration
        // that can be shared across multiple projects

        // Create a design system configuration
        var designSystemTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-primary-500", "#6366f1")
            .Add("--color-primary-600", "#4f46e5")
            .Add("--radius-default", "0.375rem")
            .Add("--shadow-default", "0 1px 3px 0 rgb(0 0 0 / 0.1)");

        var designSystemComponents = ImmutableDictionary<string, string>.Empty
            .Add(".ds-button", "px-4 py-2 rounded-default shadow-default font-semibold")
            .Add(".ds-card", "bg-white rounded-default shadow-default p-6")
            .Add(".ds-input", "border rounded-default px-3 py-2 focus:outline-none focus:ring-2");

        // Export to shareable CSS
        var converter = new ThemeToCssConverter();
        var sharedCss = converter.Convert(designSystemTheme, designSystemComponents);

        Console.WriteLine("=== Shareable Design System CSS ===");
        Console.WriteLine(sharedCss);
        Console.WriteLine("===================================");

        // This CSS can now be:
        // 1. Committed to version control
        // 2. Published as an npm package
        // 3. Shared via CDN
        // 4. Imported by any project using MonorailCss

        sharedCss.ShouldContain("@theme {");
        sharedCss.ShouldContain(".ds-button {");
        sharedCss.ShouldContain(".ds-card {");
        sharedCss.ShouldContain(".ds-input {");
    }
}