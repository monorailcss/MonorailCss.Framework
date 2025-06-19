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
        cssSheet.ShouldContain("""
                               .prose :where(h2 code):not(:where([class~="not-prose"],[class~="not-prose"] *)) {
                                 color:inherit;
                                 font-size:0.875em;
                               }
                               """);
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
}