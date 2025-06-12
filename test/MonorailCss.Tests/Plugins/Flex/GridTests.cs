namespace MonorailCss.Tests.Plugins.Flex;

public class GridTests
{
    [Fact]
    public void Can_do_grid_columns()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["grid-cols-3", "grid-cols-none", "grid-cols-subgrid"]);
        r.ShouldBeCss("""

                      .grid-cols-3 {
                        grid-template-columns:repeat(3, minmax(0, 1fr));
                      }
                      .grid-cols-none {
                        grid-template-columns:none;
                      }
                      .grid-cols-subgrid {
                        grid-template-columns:subgrid;
                      }

                      """);
    }

    [Fact]
    public void Can_do_grid_rows()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["grid-rows-4", "grid-rows-none", "grid-rows-subgrid"]);
        r.ShouldBeCss("""

                      .grid-rows-4 {
                        grid-template-rows:repeat(4, minmax(0, 1fr));
                      }
                      .grid-rows-none {
                        grid-template-rows:none;
                      }
                      .grid-rows-subgrid {
                        grid-template-rows:subgrid;
                      }

                      """);
    }

    [Fact]
    public void Can_do_grid_columns_arbitrary_values()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["grid-cols-[200px_minmax(900px,1fr)_100px]", "grid-cols-[1fr_2fr]"]);
        r.ShouldBeCss("""

                      .grid-cols-\[200px_minmax\(900px\,1fr\)_100px\] {
                        grid-template-columns:200px minmax(900px,1fr) 100px;
                      }
                      .grid-cols-\[1fr_2fr\] {
                        grid-template-columns:1fr 2fr;
                      }

                      """);
    }

    [Fact]
    public void Can_do_grid_rows_arbitrary_values()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["grid-rows-[200px_minmax(900px,1fr)_100px]", "grid-rows-[auto_1fr_auto]"]);
        r.ShouldBeCss("""

                      .grid-rows-\[200px_minmax\(900px\,1fr\)_100px\] {
                        grid-template-rows:200px minmax(900px,1fr) 100px;
                      }
                      .grid-rows-\[auto_1fr_auto\] {
                        grid-template-rows:auto 1fr auto;
                      }

                      """);
    }

    [Fact]
    public void Can_do_grid_with_responsive_variants()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["grid-cols-1", "md:grid-cols-2", "lg:grid-cols-3"]);
        r.ShouldBeCss("""

                      .grid-cols-1 {
                        grid-template-columns:repeat(1, minmax(0, 1fr));
                      }
                      @media (min-width:768px) {
                        .md\:grid-cols-2 {
                          grid-template-columns:repeat(2, minmax(0, 1fr));
                        }
                      }
                      @media (min-width:1024px) {
                        .lg\:grid-cols-3 {
                          grid-template-columns:repeat(3, minmax(0, 1fr));
                        }
                      }

                      """);
    }
}