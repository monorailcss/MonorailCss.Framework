using MonorailCss.Plugins;
using Shouldly;

namespace MonorailCss.Tests.Plugins;

public class ProseTests
{
    [Fact]
    public void Prose_works()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        var cssSheet = framework.Process(new[] { "prose", "prose-sm" });
    }

    [Fact]
    public void Prose_works_with_custom_namespace()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        framework = framework.WithSettings(new Prose.Settings { Namespace = "writing" });
        var results = framework.Process(new[] { "writing", "writing-sm", "mx-4" });
        results.ShouldContain(".writing");
    }
}