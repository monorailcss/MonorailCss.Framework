using MonorailCss.Tests.Plugins;

namespace MonorailCss.Tests;

public class VariantTests
{
    [Fact]
    public void Arbitrary_values_respect_built_in_suffixes()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "2xl:w-80",
        ]);

        r.ShouldBeCss("""

                      :root {
                        --monorail-spacing:0.25rem;
                      }
                      @media (min-width:1536px) {
                        .\32 xl\:w-80 {
                          width:calc(var(--monorail-spacing) * 80);
                        }
                      }


                      """);
    }

}