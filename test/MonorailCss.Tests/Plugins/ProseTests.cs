using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Plugins;
using MonorailCss.Plugins.Prose;
using Shouldly;

namespace MonorailCss.Tests.Plugins;

public class ProseTests
{
    [Fact]
    public void Prose_works()
    {
        var proseSettings = new Prose.Settings
        {
            CustomSettings = designSystem => new Dictionary<string, CssSettings>
            {
                {
                    "DEFAULT", new CssSettings
                    {
                        ChildRules =
                        [
                            new("a",
                            [
                                (CssProperties.FontWeight, "inherit"),
                                (CssProperties.TextDecoration, "none"),
                                (CssProperties.BorderBottomWidth, "1px"),
                                (CssProperties.Color, designSystem.Colors[ColorNames.Blue][ColorLevels._500].AsString()),
                            ]),
                        ]
                    }
                }
            }.ToImmutableDictionary()
        };
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty, PluginSettings = ImmutableList.Create<ISettings>(proseSettings),
        });

        var cssSheet = framework.Process(["prose"]);
        cssSheet.ShouldContain(".prose :where(h2 code):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))");
        cssSheet.ShouldContain("color:inherit");
        cssSheet.ShouldContain("font-size:0.875em");
    }

    [Fact]
    public void Prose_works_with_custom_namespace()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Prose.Settings { Namespace = "writing" }),
        });
        var results = framework.Process(["writing", "writing-sm", "mx-4"]);
        results.ShouldContain(".writing");
    }

    [Fact]
    public void Prose_fluid_base_class_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Prose.Settings()),
        });
        var results = framework.Process(["prose-fluid"]);
        results.ShouldContain(".prose-fluid");
        results.ShouldContain("clamp(");
        results.ShouldContain("--prose-fluid-");
    }

    [Fact]
    public void Prose_fluid_breakpoint_classes_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Prose.Settings()),
        });
        var results = framework.Process(["prose-fluid-starting-md", "prose-fluid-ending-lg"]);
        results.ShouldContain(".prose-fluid-starting-md");
        results.ShouldContain(".prose-fluid-ending-lg");
        results.ShouldContain("--prose-fluid-bp-min:768");
        results.ShouldContain("--prose-fluid-bp-max:1024");
    }

    [Fact]
    public void Prose_fluid_size_classes_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Prose.Settings()),
        });
        var results = framework.Process(["prose-fluid-from-sm", "prose-fluid-to-xl"]);
        results.ShouldContain(".prose-fluid-from-sm");
        results.ShouldContain(".prose-fluid-to-xl");
        results.ShouldContain("--prose-fluid-font-size-min");
        results.ShouldContain("--prose-fluid-font-size-max");
    }

    [Fact]
    public void Prose_fluid_combined_usage_example()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Prose.Settings()),
        });
        
        // Example usage: prose-fluid prose-fluid-starting-md prose-fluid-ending-lg prose-fluid-from-sm prose-fluid-to-2xl
        var results = framework.Process([
            "prose-fluid", 
            "prose-fluid-starting-md", 
            "prose-fluid-ending-lg", 
            "prose-fluid-from-sm", 
            "prose-fluid-to-2xl"
        ]);
        
        // Should contain all the classes
        results.ShouldContain(".prose-fluid");
        results.ShouldContain(".prose-fluid-starting-md");
        results.ShouldContain(".prose-fluid-ending-lg");
        results.ShouldContain(".prose-fluid-from-sm");
        results.ShouldContain(".prose-fluid-to-2xl");
        
        // Should contain CSS variables
        results.ShouldContain("--prose-fluid-bp-min");
        results.ShouldContain("--prose-fluid-bp-max");
        results.ShouldContain("--prose-fluid-font-size-min");
        results.ShouldContain("--prose-fluid-font-size-max");
    }

    [Fact]
    public void Prose_fluid_generates_css_variables()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Prose.Settings()),
        });
        var results = framework.Process(["prose-fluid", "prose-fluid-from-base", "prose-fluid-to-lg"]);
        
        // Should contain CSS variable-based clamp calculations
        results.ShouldContain("clamp(var(--prose-fluid-font-size-min)");
        results.ShouldContain("var(--prose-fluid-font-size-max)");
        results.ShouldContain("calc(");
        results.ShouldContain("100vw");
        
        // Should contain the variable definitions (default ones from prose-fluid)
        results.ShouldContain("--prose-fluid-font-size-min:1rem");
        results.ShouldContain("--prose-fluid-font-size-max:1.125rem");
    }
}