using MonorailCss;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins.Filters;

public class FilterTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { CssResetOverride = string.Empty });

    [Fact]
    public void Blur_outputs_correct_css()
    {
        var result = _framework.Process(["blur", "blur-md", "blur-none"]);
        result.ShouldBeCss(@".blur {
  filter:blur(8px);
}
.blur-md {
  filter:blur(12px);
}
.blur-none {
  filter:;
}");
    }

    [Fact]
    public void Brightness_outputs_correct_css()
    {
        var result = _framework.Process(["brightness-50", "brightness-100", "brightness-150"]);
        result.ShouldBeCss(@".brightness-50 {
  filter:brightness(.5);
}
.brightness-100 {
  filter:brightness(1);
}
.brightness-150 {
  filter:brightness(1.5);
}");
    }

    [Fact]
    public void Contrast_outputs_correct_css()
    {
        var result = _framework.Process(["contrast-0", "contrast-100", "contrast-200"]);
        result.ShouldBeCss(@".contrast-0 {
  filter:contrast(0);
}
.contrast-100 {
  filter:contrast(1);
}
.contrast-200 {
  filter:contrast(2);
}");
    }

    [Fact]
    public void DropShadow_outputs_correct_css()
    {
        var result = _framework.Process(["drop-shadow", "drop-shadow-lg", "drop-shadow-none"]);
        result.ShouldBeCss(@".drop-shadow {
  filter:drop-shadow(0 1px 2px rgb(0 0 0 / 0.1)) drop-shadow(0 1px 1px rgb(0 0 0 / 0.06));
}
.drop-shadow-lg {
  filter:drop-shadow(0 10px 8px rgb(0 0 0 / 0.04)) drop-shadow(0 4px 3px rgb(0 0 0 / 0.1));
}
.drop-shadow-none {
  filter:drop-shadow(0 0 #0000);
}");
    }

    [Fact]
    public void Grayscale_outputs_correct_css()
    {
        var result = _framework.Process(["grayscale-0", "grayscale"]);
        result.ShouldBeCss(@".grayscale-0 {
  filter:grayscale(0);
}
.grayscale {
  filter:grayscale(1);
}");
    }

    [Fact]
    public void HueRotate_outputs_correct_css()
    {
        var result = _framework.Process(["hue-rotate-0", "hue-rotate-90", "hue-rotate-180"]);
        result.ShouldBeCss(@".hue-rotate-0 {
  filter:hue-rotate(0deg);
}
.hue-rotate-90 {
  filter:hue-rotate(90deg);
}
.hue-rotate-180 {
  filter:hue-rotate(180deg);
}");
    }

    [Fact]
    public void Invert_outputs_correct_css()
    {
        var result = _framework.Process(["invert-0", "invert"]);
        result.ShouldBeCss(@".invert-0 {
  filter:invert(0);
}
.invert {
  filter:invert(1);
}");
    }

    [Fact]
    public void Saturate_outputs_correct_css()
    {
        var result = _framework.Process(["saturate-0", "saturate-100", "saturate-200"]);
        result.ShouldBeCss(@".saturate-0 {
  filter:saturate(0);
}
.saturate-100 {
  filter:saturate(1);
}
.saturate-200 {
  filter:saturate(2);
}");
    }

    [Fact]
    public void Sepia_outputs_correct_css()
    {
        var result = _framework.Process(["sepia-0", "sepia"]);
        result.ShouldBeCss(@".sepia-0 {
  filter:sepia(0);
}
.sepia {
  filter:sepia(1);
}");
    }
}