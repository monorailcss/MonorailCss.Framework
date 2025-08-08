namespace MonorailCss.Tests.Plugins.Interactivity;

public class ScrollbarTests
{
    [Fact]
    public void ScrollbarWidth_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-thin"]);
        result.ShouldBeCss("""

                           .scrollbar-thin {
                             scrollbar-width:thin;
                           }

                           """);
    }

    [Fact]
    public void ScrollbarWidth_auto_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-auto"]);
        result.ShouldBeCss("""

                           .scrollbar-auto {
                             scrollbar-width:auto;
                           }

                           """);
    }

    [Fact]
    public void ScrollbarWidth_none_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-none"]);
        result.ShouldBeCss("""

                           .scrollbar-none {
                             scrollbar-width:none;
                           }

                           """);
    }

    [Fact]
    public void ScrollbarThumb_color_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-thumb-blue-500"]);
        result.ShouldBeCss("""
                           :root {
                             --monorail-spacing:0.25rem;
                           }
                           
                           .scrollbar-thumb-blue-500 {
                             --monorail-scrollbar-thumb:oklch(0.623 0.214 259.815);
                             scrollbar-color:var(--monorail-scrollbar-thumb) var(--monorail-scrollbar-track, initial);
                           }

                           """);
    }

    [Fact]
    public void ScrollbarTrack_color_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-track-gray-200"]);
        result.ShouldBeCss("""
                           :root {
                             --monorail-spacing:0.25rem;
                           }
                           
                           .scrollbar-track-gray-200 {
                             --monorail-scrollbar-track:oklch(0.928 0.006 264.531);
                             scrollbar-color:var(--monorail-scrollbar-thumb, initial) var(--monorail-scrollbar-track);
                           }

                           """);
    }

    [Fact]
    public void ScrollbarThumb_arbitrary_value_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-thumb-[#ff0000]"]);
        result.ShouldBeCss(@":root {
  --monorail-spacing:0.25rem;
}
.scrollbar-thumb-\[\#ff0000\] {
  --monorail-scrollbar-thumb:rgb(255, 0, 0);
  scrollbar-color:var(--monorail-scrollbar-thumb) var(--monorail-scrollbar-track, initial);
}");
    }

    [Fact]
    public void ScrollbarTrack_arbitrary_value_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-track-[rgba(0,0,0,0.1)]"]);
        result.ShouldBeCss(@":root {
  --monorail-spacing:0.25rem;
}
.scrollbar-track-\[rgba\(0\2c 0\2c 0\2c 0\.1\)\] {
  --monorail-scrollbar-track:rgba(0, 0, 0, 0.1);
  scrollbar-color:var(--monorail-scrollbar-thumb, initial) var(--monorail-scrollbar-track);
}");
    }

    [Fact]
    public void Combined_scrollbar_classes_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["scrollbar-thin", "scrollbar-thumb-blue-500", "scrollbar-track-gray-200"]);
        result.ShouldBeCss("""
                           :root {
                             --monorail-spacing:0.25rem;
                           }
                           
                           .scrollbar-thin {
                             scrollbar-width:thin;
                           }
                           
                           .scrollbar-thumb-blue-500 {
                             --monorail-scrollbar-thumb:oklch(0.623 0.214 259.815);
                             scrollbar-color:var(--monorail-scrollbar-thumb) var(--monorail-scrollbar-track, initial);
                           }
                           
                           .scrollbar-track-gray-200 {
                             --monorail-scrollbar-track:oklch(0.928 0.006 264.531);
                             scrollbar-color:var(--monorail-scrollbar-thumb, initial) var(--monorail-scrollbar-track);
                           }

                           """);
    }
}