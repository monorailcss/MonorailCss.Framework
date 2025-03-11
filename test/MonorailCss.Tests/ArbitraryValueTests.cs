using MonorailCss.Tests.Plugins;

namespace MonorailCss.Tests;

public class ArbitraryValueTests
{
    [Fact]
    public void Can_do_arbitrary_value_in_short_hex()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "bg-[#123]",
        ]);
        r.ShouldBeCss("""

                      .bg-\[\#123\] {
                        --monorail-bg-opacity:1;
                        background-color:rgba(17, 34, 51, var(--monorail-bg-opacity));
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_in_long_hex()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "bg-[#010203]",
        ]);
        r.ShouldBeCss("""

                      .bg-\[\#010203\] {
                        --monorail-bg-opacity:1;
                        background-color:rgba(1, 2, 3, var(--monorail-bg-opacity));
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_in_rgb()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "bg-[rgb(17,34,51)]",
        ]);
        r.ShouldBeCss("""

                      .bg-\[rgb\(17\2c 34\2c 51\)\] {
                        --monorail-bg-opacity:1;
                        background-color:rgba(17, 34, 51, var(--monorail-bg-opacity));
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_in_rgba()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "bg-[rgba(17,34,51,.75)]",
        ]);
        r.ShouldBeCss("""

                      .bg-\[rgba\(17\2c 34\2c 51\2c \.75\)\] {
                        background-color:rgba(17, 34, 51, .75);
                      }

                      """);
    }


    [Fact]
    public void Can_do_arbitrary_value_in_named_color()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "bg-[orange]",
        ]);
        r.ShouldBeCss("""

                      .bg-\[orange\] {
                        background-color:orange;
                      }

                      """);
    }
}