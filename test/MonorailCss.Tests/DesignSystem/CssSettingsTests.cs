﻿using MonorailCss.Css;
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
            Css =
            [
                (CssProperties.BorderWidth, "1"),
                (CssProperties.Float, "left"),
            ],
            ChildRules =
            [
                new("a", [(CssProperties.Height, "4px"), (CssProperties.Width, "2px")]),

            ],
        };

        var setting2 = new CssSettings
        {
            Css =
            [
                (CssProperties.BorderWidth, "4"), (CssProperties.Display, "block"),
            ],
            ChildRules =
            [
                new("a",
                    [(CssProperties.Height, "2px"), (CssProperties.Margin, "2px")]),
            ]
        };

        var newSettings = setting1 + setting2;
        newSettings.Css.Count.ShouldBe(3);
    }
}