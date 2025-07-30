namespace MonorailCss.Tests.Plugins.Borders;

public class BorderWidthTests
{
    [Fact]
    public void Border_width_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["border", "border-2", "border-b-4", "border-t"]);
        result.ShouldBeCss("""

                           .border-b-4 {
                             border-bottom-width:4px;
                           }
                           .border {
                             border-width: 1px;
                           }
                           .border-t {
                             border-top-width: 1px;
                           }

                           .border-2 {
                             border-width: 2px;
                           }

                           """);
    }

    [Fact]
    public void Can_do_arbitrary_numeric_border_widths()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });

        var r = framework.Process(["border-r-5", "border-l-32", "border-x-99", "border-y-10", "border-t-1", "border-b-25"]);
        r.ShouldBeCss("""

                      .border-r-5 {
                        border-right-width:5px;
                      }
                      .border-l-32 {
                        border-left-width:32px;
                      }
                      .border-x-99 {
                        border-left-width:99px;
                        border-right-width:99px;
                      }
                      .border-y-10 {
                        border-bottom-width:10px;
                        border-top-width:10px;
                      }
                      .border-t-1 {
                        border-top-width:1px;
                      }
                      .border-b-25 {
                        border-bottom-width:25px;
                      }

                      """);
    }

    [Fact]
    public void Can_do_arbitrary_value_border_widths()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });

        var r = framework.Process(["border-r-[4em]", "border-l-[2rem]", "border-x-[10%]", "border-y-[5vh]", "border-t-[3px]", "border-b-[1.5rem]"]);
        r.ShouldBeCss("""

                      .border-r-\[4em\] {
                        border-right-width:4em;
                      }
                      .border-l-\[2rem\] {
                        border-left-width:2rem;
                      }
                      .border-x-\[10%\] {
                        border-left-width:10%;
                        border-right-width:10%;
                      }
                      .border-y-\[5vh\] {
                        border-bottom-width:5vh;
                        border-top-width:5vh;
                      }
                      .border-t-\[3px\] {
                        border-top-width:3px;
                      }
                      .border-b-\[1\.5rem\] {
                        border-bottom-width:1.5rem;
                      }

                      """);
    }

    [Fact]
    public void Can_do_zero_and_negative_border_widths()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });

        var r = framework.Process(["border-r-0", "border-l-[-2px]", "border-12"]);
        r.ShouldBeCss("""

                      .border-r-0 {
                        border-right-width:0px;
                      }
                      .border-l-\[-2px\] {
                        border-left-width:-2px;
                      }
                      .border-12 {
                        border-width:12px;
                      }

                      """);
    }
}