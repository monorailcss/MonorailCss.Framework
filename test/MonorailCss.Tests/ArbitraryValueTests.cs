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
    public void Arbitrary_values_respect_built_in_suffixes()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "h-screen",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .h-screen {
                        height:100vh;
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

    [Fact]
    public void CSS_selector_escapes_question_mark_and_equals()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "bg-[url('/path/to/image.jpg?v=1&size=large')]",
        ]);

        // Verify that ?, =, and & characters are properly escaped in the CSS selector
        r.ShouldContain("""
                        .bg-\[url\(\'\/path\/to\/image\.jpg\?v\=1\&size\=large\'\)\]
                        """);
        r.ShouldBeCss("""

                      .bg-\[url\(\'\/path\/to\/image\.jpg\?v\=1\&size\=large\'\)\] {
                        background-image:url('/path/to/image.jpg?v=1&size=large');
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_with_px_unit()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "p-[10px]",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .p-\[10px\] {
                        padding:10px;
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_with_vh_unit()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "p-[10vh]",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .p-\[10vh\] {
                        padding:10vh;
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_with_em_unit()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "w-[2.5em]",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .w-\[2\.5em\] {
                        width:2.5em;
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_with_rem_unit()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "h-[1.5rem]",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .h-\[1\.5rem\] {
                        height:1.5rem;
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_with_percentage_unit()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "w-[50%]",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .w-\[50%\] {
                        width:50%;
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_with_vw_unit()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "w-[100vw]",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .w-\[100vw\] {
                        width:100vw;
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_with_negative_px_unit()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "m-[-10px]",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      .m-\[-10px\] {
                        margin:-10px;
                      }

                      """);
    }
}