﻿namespace MonorailCss.Tests.Plugins.Borders;

public class OutlineTests
{
    [Fact]
    public void Outline_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["outline"]);
        result.ShouldBeCss("""

                           .outline {
                             outline-style: solid;
                           }

                           """);
    }

    [Fact]
    public void Outline_none_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["outline-none"]);
        result.ShouldBeCss("""

                           .outline-none {
                             outline:2px solid transparent;
                             outline-offset:2px;
                           }

                           """);
    }


    [Fact]
    public void Outline_color_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["outline outline-pink-500 "]);
        result.ShouldBeCss("""

                           .outline {
                             outline-style:solid;
                           }
                           .outline-pink-500 {
                             outline-color:oklch(0.656 0.241 354.308);
                           }


                           """);
    }


    [Fact]
    public void Outline_offset_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["outline outline-offset-0"]);
        result.ShouldBeCss("""

                           .outline {
                             outline-style:solid;
                           }
                           .outline-offset-0 {
                             outline-offset:0px;
                           }

                           """);
    }

    [Fact]
    public void Outline_offset_2_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["outline outline-offset-2"]);
        result.ShouldBeCss("""

                           .outline {
                             outline-style:solid;
                           }
                           .outline-offset-2 {
                             outline-offset:2px;
                           }

                           """);
    }

    [Fact]
    public void Outline_dashed_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["outline-dashed"]);
        result.ShouldBeCss("""

                           .outline-dashed {
                             outline-style:dashed;
                           }

                           """);
    }
}