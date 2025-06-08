namespace MonorailCss.Tests.Plugins.Backgrounds;

public class BackgroundImageTests
{
    [Fact]
    public void BackgroundAttachment_fixed_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-fixed"]);
        result.ShouldBeCss("""

                           .bg-fixed {
                             background-attachment: fixed;
                           }

                           """);
    }

    [Fact]
    public void BackgroundAttachment_local_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-local"]);
        result.ShouldBeCss("""

                           .bg-local {
                             background-attachment: local;
                           }

                           """);
    }

    [Fact]
    public void BackgroundSize_cover_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-cover"]);
        result.ShouldBeCss("""

                           .bg-cover {
                             background-size: cover;
                           }

                           """);
    }

    [Fact]
    public void BackgroundSize_contain_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-contain"]);
        result.ShouldBeCss("""

                           .bg-contain {
                             background-size: contain;
                           }

                           """);
    }

    [Fact]
    public void BackgroundPosition_center_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-center"]);
        result.ShouldBeCss("""

                           .bg-center {
                             background-position: center;
                           }

                           """);
    }

    [Fact]
    public void BackgroundPosition_left_top_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-left-top"]);
        result.ShouldBeCss("""

                           .bg-left-top {
                             background-position: left top;
                           }

                           """);
    }

    [Fact]
    public void BackgroundRepeat_no_repeat_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-no-repeat"]);
        result.ShouldBeCss("""

                           .bg-no-repeat {
                             background-repeat: no-repeat;
                           }

                           """);
    }

    [Fact]
    public void BackgroundRepeat_repeat_x_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-repeat-x"]);
        result.ShouldBeCss("""

                           .bg-repeat-x {
                             background-repeat: repeat-x;
                           }

                           """);
    }

    [Fact]
    public void BackgroundRepeat_round_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-repeat-round"]);
        result.ShouldBeCss("""

                           .bg-repeat-round {
                             background-repeat: round;
                           }

                           """);
    }

    [Fact]
    public void BackgroundImage_arbitrary_url_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-[url('/path/to/image.jpg')]"]);
        result.ShouldBeCss("""

                           .bg-\[url\('\/path\/to\/image\.jpg'\)\] {
                             background-image:url('/path/to/image.jpg');
                           }

                           """);
    }

    [Fact]
    public void BackgroundImage_arbitrary_linear_gradient_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-[linear-gradient(45deg,red,blue)]"]);
        result.ShouldBeCss("""

                           .bg-\[linear-gradient\(45deg\,red\,blue\)\] {
                             background-image: linear-gradient(45deg,red,blue);
                           }

                           """);
    }

    [Fact]
    public void BackgroundImage_none_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["bg-none"]);
        result.ShouldBeCss("""

                           .bg-none {
                             background-image: none;
                           }

                           """);
    }
}