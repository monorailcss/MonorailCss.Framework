using MonorailCss.Tests.Plugins;
using Shouldly;

namespace MonorailCss.Tests;

public class PeerTests
{
    [Fact]
    public void Can_do_named_peers()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "peer-checked/tab2:block",
        ]);

        r.Trim().ShouldBe("""
                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .peer-checked\/tab2\:block {
                        &:is(:where(.peer\/tab2):checked ~ *) {
                          display:block;
                        }
                      }
                      """.Trim(), StringCompareShould.IgnoreLineEndings);
    }

    [Fact]
    public void Can_do_regular_peers()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "peer-checked:block",
        ]);

        r.ShouldBeCss("""

                      .peer-checked\:block {
                        &:is(:where(.peer):checked ~ *) {
                          display:block;
                        }
                      }

                      """);
    }
}

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
    public void Can_do_arbitrary_value_with_calc()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "h-[calc(100vh-4.75rem)]",
        ]);
        r.ShouldContain("100vh - 4.75rem");
        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .h-\[calc\(100vh-4\.75rem\)\] {
                        height:calc(100vh - 4.75rem);
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