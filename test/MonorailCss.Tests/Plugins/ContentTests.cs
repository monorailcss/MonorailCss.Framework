using MonorailCss;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins;

public class ContentTests
{
    private readonly CssFramework _framework = new(new CssFrameworkSettings { CssResetOverride = string.Empty });

    [Fact]
    public void ContentNone_outputs_correct_css()
    {
        var result = _framework.Process(["content-none"]);
        result.ShouldBeCss(@".content-none {
  content:none;
}");
    }

    [Fact]
    public void Content_with_arbitrary_string_outputs_correct_css()
    {
        var result = _framework.Process(["content-['Hello_World']"]);
        result.ShouldBeCss("""
                           .content-\[\'Hello_World\'\] {
                             content:'Hello World';
                           }
                           """);
    }

    [Fact]
    public void Content_with_arbitrary_empty_string_outputs_correct_css()
    {
        var result = _framework.Process(["content-['']"]);
        result.ShouldBeCss(@".content-\[\'\'\] {
  content:'';
}");
    }

    [Fact]
    public void Content_with_arbitrary_counter_outputs_correct_css()
    {
        var result = _framework.Process(["content-[counter(my-counter)]"]);
        result.ShouldBeCss(@".content-\[counter\(my-counter\)\] {
  content:counter(my-counter);
}");
    }

    [Fact]
    public void Content_with_arbitrary_attr_outputs_correct_css()
    {
        var result = _framework.Process(["content-[attr(data-title)]"]);
        result.ShouldBeCss(@".content-\[attr\(data-title\)\] {
  content:attr(data-title);
}");
    }

    [Fact]
    public void Content_with_custom_property_outputs_correct_css()
    {
        var result = _framework.Process(["content-(--my-content)", "content-(--icon)"]);
        result.ShouldBeCss(@".content-\(--my-content\) {
  content:var(--my-content);
}
.content-\(--icon\) {
  content:var(--icon);
}");
    }

    [Fact]
    public void Content_with_multiple_values_outputs_correct_css()
    {
        var result = _framework.Process(["content-none", "content-['Test']", "content-(--custom)"]);
        result.ShouldBeCss(@".content-none {
  content:none;
}
.content-\[\'Test\'\] {
  content:'Test';
}
.content-\(--custom\) {
  content:var(--custom);
}");
    }
}