namespace MonorailCss.Tests.Plugins;

public class MarginTests
{
    [Fact]
    public void Margins_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["my-4-", "mx-4"]);
        result.ShouldBeCss("""

                           .my-4- {
                             margin-block:calc(var(--monorail-spacing) * -4);
                           }
                           .mx-4 {
                             margin-inline:calc(var(--monorail-spacing) * 4);
                           }

                           """);
    }

    [Fact]
    public void Supports_Auto_Values()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["mx-auto"]);
        result.ShouldBeCss("""

                           .mx-auto {
                             margin-inline:auto;
                           }

                           """);
    }

    [Fact]
    public void Logical_Properties_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["ms-4", "me-2"]);
        result.ShouldBeCss("""

                           .me-2 {
                             margin-inline-end:calc(var(--monorail-spacing) * 2);
                           }
                           .ms-4 {
                             margin-inline-start:calc(var(--monorail-spacing) * 4);
                           }

                           """);
    }

    [Fact]
    public void Traditional_Properties_Still_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["ml-3", "mr-6", "mt-2", "mb-8"]);
        result.ShouldBeCss("""

                           .mb-8 {
                             margin-bottom:calc(var(--monorail-spacing) * 8);
                           }
                           .ml-3 {
                             margin-left:calc(var(--monorail-spacing) * 3);
                           }
                           .mr-6 {
                             margin-right:calc(var(--monorail-spacing) * 6);
                           }
                           .mt-2 {
                             margin-top:calc(var(--monorail-spacing) * 2);
                           }

                           """);
    }
}