namespace MonorailCss.Tests.Plugins.Sizing;

public class AspectRatioTests
{
    [Fact]
    public void Aspect_auto_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });

        var result = framework.Process(["aspect-auto"]);
        result.ShouldBeCss("""
                           .aspect-auto {
                             aspect-ratio:auto;
                           }
                           """);
    }

    [Fact]
    public void Aspect_square_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });

        var result = framework.Process(["aspect-square"]);
        result.ShouldBeCss("""
                           .aspect-square {
                             aspect-ratio:1 / 1;
                           }
                           """);
    }

    [Fact]
    public void Aspect_video_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });

        var result = framework.Process(["aspect-video"]);
        result.ShouldBeCss("""
                           .aspect-video {
                             aspect-ratio:16 / 9;
                           }
                           """);
    }

    [Fact]
    public void Aspect_arbitrary_value_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });

        var result = framework.Process(["aspect-[3/2]"]);
        result.ShouldBeCss("""
                           .aspect-\[3\/2\] {
                             aspect-ratio:3/2;
                           }
                           """);
    }

    [Fact]
    public void Aspect_arbitrary_value_with_spaces_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });

        var result = framework.Process(["aspect-[4_/_3]"]);
        result.ShouldBeCss("""
                           .aspect-\[4_\/_3\] {
                             aspect-ratio:4 / 3;
                           }
                           """);
    }
}