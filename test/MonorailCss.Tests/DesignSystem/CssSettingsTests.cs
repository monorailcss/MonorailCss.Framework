using MonorailCss.Css;
using MonorailCss.Plugins;
using Shouldly;

namespace MonorailCss.Tests.DesignSystem;

public class CssSettingsTests
{
    [Fact]
    public void Can_combine()
    {
        var setting1 = new CssSettings
        {
            Css = new CssDeclarationList { new(CssProperties.BorderWidth, "1"), new(CssProperties.Float, "left"), },
            ChildRules = new CssRuleSetList
            {
                new("a",
                    new CssDeclarationList { new(CssProperties.Height, "4px"), new(CssProperties.Width, "2px") })
            }
        };

        var setting2 = new CssSettings
        {
            Css = new CssDeclarationList
            {
                new(CssProperties.BorderWidth, "4"), new(CssProperties.Display, "block"),
            },
            ChildRules = new CssRuleSetList
            {
                new("a",
                    new CssDeclarationList { new(CssProperties.Height, "2px"), new(CssProperties.Margin, "2px") })
            }
        };

        var newSettings = setting1 + setting2;
        newSettings.Css.Count.ShouldBe(3);
    }
}