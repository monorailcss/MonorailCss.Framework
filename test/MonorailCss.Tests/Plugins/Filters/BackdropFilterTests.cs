using MonorailCss;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins.Filters;

public class BackdropFilterTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { CssResetOverride = string.Empty });

    [Fact]
    public void BackdropBlur_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-blur", "backdrop-blur-md", "backdrop-blur-none"]);
        result.ShouldBeCss(@".backdrop-blur {
  backdrop-filter:blur(8px);
}
.backdrop-blur-md {
  backdrop-filter:blur(12px);
}
.backdrop-blur-none {
  backdrop-filter:blur(0);
}");
    }

    [Fact]
    public void BackdropBrightness_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-brightness-50", "backdrop-brightness-100", "backdrop-brightness-150"]);
        result.ShouldBeCss(@".backdrop-brightness-50 {
  backdrop-filter:brightness(.5);
}
.backdrop-brightness-100 {
  backdrop-filter:brightness(1);
}
.backdrop-brightness-150 {
  backdrop-filter:brightness(1.5);
}");
    }

    [Fact]
    public void BackdropContrast_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-contrast-0", "backdrop-contrast-100", "backdrop-contrast-200"]);
        result.ShouldBeCss(@".backdrop-contrast-0 {
  backdrop-filter:contrast(0);
}
.backdrop-contrast-100 {
  backdrop-filter:contrast(1);
}
.backdrop-contrast-200 {
  backdrop-filter:contrast(2);
}");
    }

    [Fact]
    public void BackdropGrayscale_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-grayscale-0", "backdrop-grayscale"]);
        result.ShouldBeCss(@".backdrop-grayscale-0 {
  backdrop-filter:grayscale(0);
}
.backdrop-grayscale {
  backdrop-filter:grayscale(1);
}");
    }

    [Fact]
    public void BackdropHueRotate_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-hue-rotate-0", "backdrop-hue-rotate-90", "backdrop-hue-rotate-180"]);
        result.ShouldBeCss(@".backdrop-hue-rotate-0 {
  backdrop-filter:hue-rotate(0deg);
}
.backdrop-hue-rotate-90 {
  backdrop-filter:hue-rotate(90deg);
}
.backdrop-hue-rotate-180 {
  backdrop-filter:hue-rotate(180deg);
}");
    }

    [Fact]
    public void BackdropInvert_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-invert-0", "backdrop-invert"]);
        result.ShouldBeCss(@".backdrop-invert-0 {
  backdrop-filter:invert(0);
}
.backdrop-invert {
  backdrop-filter:invert(1);
}");
    }

    [Fact]
    public void BackdropOpacity_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-opacity-0", "backdrop-opacity-50", "backdrop-opacity-100"]);
        result.ShouldBeCss(@".backdrop-opacity-0 {
  backdrop-filter:opacity(0);
}
.backdrop-opacity-50 {
  backdrop-filter:opacity(0.5);
}
.backdrop-opacity-100 {
  backdrop-filter:opacity(1);
}");
    }

    [Fact]
    public void BackdropSaturate_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-saturate-0", "backdrop-saturate-100", "backdrop-saturate-200"]);
        result.ShouldBeCss(@".backdrop-saturate-0 {
  backdrop-filter:saturate(0);
}
.backdrop-saturate-100 {
  backdrop-filter:saturate(1);
}
.backdrop-saturate-200 {
  backdrop-filter:saturate(2);
}");
    }

    [Fact]
    public void BackdropSepia_outputs_correct_css()
    {
        var result = _framework.Process(["backdrop-sepia-0", "backdrop-sepia"]);
        result.ShouldBeCss(@".backdrop-sepia-0 {
  backdrop-filter:sepia(0);
}
.backdrop-sepia {
  backdrop-filter:sepia(1);
}");
    }
}