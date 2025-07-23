using MonorailCss;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins.Typography;

public class WordBreakTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { CssResetOverride = string.Empty });

    [Fact]
    public void BreakNormal_outputs_correct_css()
    {
        var result = _framework.Process(["break-normal"]);
        result.ShouldBeCss(@".break-normal {
  overflow-wrap:normal;
  break-words:normal;
}");
    }

    [Fact]
    public void BreakWords_outputs_correct_css()
    {
        var result = _framework.Process(["break-words"]);
        result.ShouldBeCss(@".break-words {
  overflow-wrap:break-word;
}");
    }

    [Fact]
    public void BreakAll_outputs_correct_css()
    {
        var result = _framework.Process(["break-all"]);
        result.ShouldBeCss(@".break-all {
  word-break:break-all;
}");
    }

    [Fact]
    public void Multiple_break_utilities_output_correct_css()
    {
        var result = _framework.Process(["break-normal", "break-words", "break-all"]);
        result.ShouldBeCss(@".break-normal {
  overflow-wrap:normal;
  break-words:normal;
}
.break-words {
  overflow-wrap:break-word;
}
.break-all {
  word-break:break-all;
}");
    }
}