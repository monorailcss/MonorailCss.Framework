﻿namespace MonorailCss.Tests.Plugins;

public class MarginTests
{
    [Fact]
    public void Margins_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["my-4-", "mx-4"]);
        result.ShouldBeCss("""

                           .my-4- {
                             margin-bottom:calc(var(--monorail-spacing) * -4);
                             margin-top:calc(var(--monorail-spacing) * -4);
                           }
                           .mx-4 {
                             margin-left:calc(var(--monorail-spacing) * 4);
                             margin-right:calc(var(--monorail-spacing) * 4);
                           }

                           """);
    }

    [Fact]
    public void Supports_Negative_Auto()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["mx-auto"]);
        result.ShouldBeCss("""

                           .mx-auto {
                             margin-right:auto;
                             margin-left:auto;
                           }

                           """);
    }
}