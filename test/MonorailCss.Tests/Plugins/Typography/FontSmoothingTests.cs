using MonorailCss;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins.Typography;

public class FontSmoothingTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { CssResetOverride = string.Empty });

    [Fact]
    public void Antialiased_outputs_correct_css()
    {
        var result = _framework.Process(["antialiased"]);
        result.ShouldBeCss(@".antialiased {
  -webkit-font-smoothing:antialiased;
  -moz-osx-font-smoothing:grayscale;
}");
    }

    [Fact]
    public void SubpixelAntialiased_outputs_correct_css()
    {
        var result = _framework.Process(["subpixel-antialiased"]);
        result.ShouldBeCss(@".subpixel-antialiased {
  -webkit-font-smoothing:auto;
  -moz-osx-font-smoothing:auto;
}");
    }

    [Fact]
    public void Multiple_smoothing_utilities_output_correct_css()
    {
        var result = _framework.Process(["antialiased", "subpixel-antialiased"]);
        result.ShouldBeCss(@".antialiased {
  -webkit-font-smoothing:antialiased;
  -moz-osx-font-smoothing:grayscale;
}
.subpixel-antialiased {
  -webkit-font-smoothing:auto;
  -moz-osx-font-smoothing:auto;
}");
    }
}