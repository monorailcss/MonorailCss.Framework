﻿namespace MonorailCss.Tests.Plugins.Svg;

public class StrokeTests
{
    [Fact]
    public void Stroke_works()
    {
        var framework =  new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var r =framework.Process(["stroke-1"]);
        r.ShouldBeCss("""

                      .stroke-1 {
                        stroke-width:1;
                      }

                      """);
    }
}