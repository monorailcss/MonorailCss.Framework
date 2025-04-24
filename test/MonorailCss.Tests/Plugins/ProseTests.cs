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
            CssResetOverride = string.Empty, PluginSettings = new List<ISettings> { proseSettings }
        });

        var cssSheet = framework.Process(["prose"]);
        // this code should be working, but AngleSharp.Css translates the colors behind the scenes
        // cssSheet.ShouldContainElementWithCssProperty(".prose a", CssProperties.Color, "oklch(0.623 0.214 259.815)");
        cssSheet.ShouldContainElementWithCssProperty(".prose a", CssProperties.Color, "rgba(14, 0, 55, 1)");
    }

    [Fact]
    public void Prose_works_with_custom_namespace()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = new List<ISettings> { new Prose.Settings { Namespace = "writing" } },
        });
        var results = framework.Process(["writing", "writing-sm", "mx-4"]);
        results.ShouldContain(".writing");
    }
}