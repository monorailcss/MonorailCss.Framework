using MonorailCss;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins.Backgrounds;

public class BackgroundClipTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { CssResetOverride = string.Empty });

    [Fact]
    public void BgClipBorder_outputs_correct_css()
    {
        var result = _framework.Process(["bg-clip-border"]);
        result.ShouldBeCss(@".bg-clip-border {
  background-clip:border-box;
}");
    }

    [Fact]
    public void BgClipPadding_outputs_correct_css()
    {
        var result = _framework.Process(["bg-clip-padding"]);
        result.ShouldBeCss(@".bg-clip-padding {
  background-clip:padding-box;
}");
    }

    [Fact]
    public void BgClipContent_outputs_correct_css()
    {
        var result = _framework.Process(["bg-clip-content"]);
        result.ShouldBeCss(@".bg-clip-content {
  background-clip:content-box;
}");
    }

    [Fact]
    public void BgClipText_outputs_correct_css()
    {
        var result = _framework.Process(["bg-clip-text"]);
        result.ShouldBeCss(@".bg-clip-text {
  background-clip:text;
}");
    }

    [Fact]
    public void Multiple_background_clip_utilities_output_correct_css()
    {
        var result = _framework.Process(["bg-clip-border", "bg-clip-padding", "bg-clip-content", "bg-clip-text"]);
        result.ShouldBeCss(@".bg-clip-border {
  background-clip:border-box;
}
.bg-clip-padding {
  background-clip:padding-box;
}
.bg-clip-content {
  background-clip:content-box;
}
.bg-clip-text {
  background-clip:text;
}");
    }

    [Fact]
    public void Invalid_suffix_returns_no_css()
    {
        var result = _framework.Process(["bg-clip-invalid"]);
        result.ShouldNotContain("background-clip");
    }
}