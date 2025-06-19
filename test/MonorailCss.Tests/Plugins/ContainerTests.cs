using System.Collections.Immutable;
using MonorailCss.Plugins;
using MonorailCss.Plugins.Layout;

namespace MonorailCss.Tests.Plugins;

public class ContainerTests
{
    [Fact]
    public void Container_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["container"]);
        result.ShouldBeCss("""

                           .container {
                             width:100%;
                           }

                           """);
    }

    [Fact]
    public void Container_with_center()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings =  ImmutableList.Create<ISettings>( new Container.Settings { Center = true }),
        });

        var result = framework.Process(["container"]);
        result.ShouldBeCss("""

                           .container {
                             width:100%;

                             margin-left: auto;
                             margin-right: auto;
                           }

                           """);
    }

    [Fact]
    public void Container_with_padding()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Container.Settings
            {
                Center = true,
                Padding = new Dictionary<string, string> { { "DEFAULT", "4px" } }.ToImmutableDictionary(),
            }),
        });

        var result = framework.Process(["container"]);
        result.ShouldBeCss("""

                           .container {
                             width:100%;
                             margin-left: auto;
                             margin-right: auto;
                             padding-left: 4px;
                             padding-right: 4px;
                           }

                           """);
    }

    [Fact]
    public void Container_with_padding_and_variants()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty,
            PluginSettings = ImmutableList.Create<ISettings>(new Container.Settings
            {
                Center = true,
                Padding = new Dictionary<string, string> { { "DEFAULT", "4px" }, { "xl", "8px" }, }
                    .ToImmutableDictionary()
            }),
        });

        var result = framework.Process(["container", "xl:container-xl"]);
        result.ShouldBeCss("""

                           .container {
                             margin-left:auto;
                             margin-right:auto;
                             padding-left:4px;
                             padding-right:4px;
                             width:100%;
                           }
                           @media (min-width:1280px) {
                             .xl\:container-xl {
                               margin-left:auto;
                               margin-right:auto;
                               max-width:1280px;
                               padding-left:8px;
                               padding-right:8px;
                             }
                           }

                           """);
    }
}