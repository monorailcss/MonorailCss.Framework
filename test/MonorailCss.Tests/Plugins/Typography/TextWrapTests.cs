using MonorailCss;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins.Typography;

public class TextWrapTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { CssResetOverride = string.Empty });

    [Fact]
    public void TextWrap_outputs_correct_css()
    {
        var result = _framework.Process(["text-wrap"]);
        result.ShouldBeCss(@".text-wrap {
  text-wrap:wrap;
}");
    }

    [Fact]
    public void TextNowrap_outputs_correct_css()
    {
        var result = _framework.Process(["text-nowrap"]);
        result.ShouldBeCss(@".text-nowrap {
  text-wrap:nowrap;
}");
    }

    [Fact]
    public void TextBalance_outputs_correct_css()
    {
        var result = _framework.Process(["text-balance"]);
        result.ShouldBeCss(@".text-balance {
  text-wrap:balance;
}");
    }

    [Fact]
    public void TextPretty_outputs_correct_css()
    {
        var result = _framework.Process(["text-pretty"]);
        result.ShouldBeCss(@".text-pretty {
  text-wrap:pretty;
}");
    }

    [Fact]
    public void Multiple_text_wrap_utilities_output_correct_css()
    {
        var result = _framework.Process(["text-wrap", "text-nowrap", "text-balance", "text-pretty"]);
        result.ShouldBeCss(@".text-wrap {
  text-wrap:wrap;
}
.text-nowrap {
  text-wrap:nowrap;
}
.text-balance {
  text-wrap:balance;
}
.text-pretty {
  text-wrap:pretty;
}");
    }
}