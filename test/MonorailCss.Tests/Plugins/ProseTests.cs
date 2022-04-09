using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Plugins;
using MonorailCss.Plugins.Borders;
using Shouldly;

namespace MonorailCss.Tests.Plugins;

public class ProseTests
{
    [Fact]
    public void Prose_works()
    {
        var proseSettings = new Prose.Settings()
        {
            CustomSettings = designSystem =>  new Dictionary<string, CssSettings>()
            {
                { "DEFAULT", new CssSettings() { ChildRules = new CssRuleSetList()
                {
                    new("a", new CssDeclarationList()
                    {
                        new(CssProperties.FontWeight, "inherit"),
                        new(CssProperties.TextDecoration, "none"),
                        new(CssProperties.BorderBottomWidth, "1px"),
                        new(CssProperties.Color, designSystem.Colors[ColorNames.Blue][ColorLevels._500].AsRgb())
                    })
                } } }
            }.ToImmutableDictionary()
        };
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty)
            .WithSettings(proseSettings);
        var cssSheet = framework.Process(new[] { "prose" });
        cssSheet.ShouldContainElementWithCssProperty(".prose a", CssProperties.Color, "rgba(59, 130, 246, 1)");
    }

    [Fact]
    public void Prose_works_with_custom_namespace()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        framework = framework.WithSettings(new Prose.Settings { Namespace = "writing" });
        var results = framework.Process(new[] { "writing", "writing-sm", "mx-4" });
        results.ShouldContain(".writing");
    }
}