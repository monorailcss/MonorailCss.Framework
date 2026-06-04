using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

public class StaticWebAssetManifestTests
{
    // Mirrors the real {App}.staticwebassets.runtime.json shape: a ContentRoots table plus a
    // trie whose leaves carry {ContentRootIndex, SubPath}. Forward-slash roots keep the JSON
    // free of backslash escaping and round-trip identically through Path.Combine on both OSes.
    private const string SampleManifest =
        """
        {
          "ContentRoots": [
            "/nuget/aspnetcore/_framework/",
            "/proj/obj/compressed/",
            "/nuget/pennington.ui/0.1.0/staticwebassets/"
          ],
          "Root": {
            "Children": {
              "_framework": {
                "Children": {
                  "blazor.web.js": {
                    "Children": null,
                    "Asset": { "ContentRootIndex": 0, "SubPath": "blazor.web.js" },
                    "Patterns": null
                  }
                },
                "Asset": null,
                "Patterns": null
              },
              "_content": {
                "Children": {
                  "Pennington.UI": {
                    "Children": {
                      "scripts.js": {
                        "Children": null,
                        "Asset": { "ContentRootIndex": 2, "SubPath": "scripts.js" },
                        "Patterns": null
                      },
                      "scripts.js.gz": {
                        "Children": null,
                        "Asset": { "ContentRootIndex": 1, "SubPath": "aaov-{0}-7cod.gz" },
                        "Patterns": null
                      }
                    },
                    "Asset": null,
                    "Patterns": null
                  }
                },
                "Asset": null,
                "Patterns": null
              }
            },
            "Asset": null,
            "Patterns": null
          }
        }
        """;

    [Fact]
    public void Resolve_Maps_Content_Asset_To_Physical_Nuget_Path()
    {
        var byUrl = StaticWebAssetManifest.Resolve(SampleManifest)
            .ToDictionary(a => a.UrlPath, a => a.PhysicalPath);

        byUrl.ShouldContainKey("_content/Pennington.UI/scripts.js");
        byUrl["_content/Pennington.UI/scripts.js"]
            .ShouldBe(Path.Combine("/nuget/pennington.ui/0.1.0/staticwebassets/", "scripts.js"));
    }

    [Fact]
    public void Resolve_Builds_Url_Path_From_Trie_Keys()
    {
        var urls = StaticWebAssetManifest.Resolve(SampleManifest).Select(a => a.UrlPath).ToList();

        urls.ShouldContain("_framework/blazor.web.js");
        urls.ShouldContain("_content/Pennington.UI/scripts.js");
        urls.ShouldContain("_content/Pennington.UI/scripts.js.gz");
    }

    [Fact]
    public void Resolve_Indexes_The_Correct_Content_Root_Per_Asset()
    {
        var byUrl = StaticWebAssetManifest.Resolve(SampleManifest)
            .ToDictionary(a => a.UrlPath, a => a.PhysicalPath);

        // The compressed sibling points at a different content root (the obj/compressed dir),
        // so a naive "all assets share one root" assumption would resolve it wrongly.
        byUrl["_content/Pennington.UI/scripts.js.gz"]
            .ShouldBe(Path.Combine("/proj/obj/compressed/", "aaov-{0}-7cod.gz"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{ "ContentRoots": [], "Root": null }""")]
    [InlineData("""{ "Root": { "Children": null, "Asset": null } }""")]
    public void Resolve_Returns_Empty_On_Unusable_Input(string json)
    {
        StaticWebAssetManifest.Resolve(json).ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_Skips_Assets_With_Out_Of_Range_Content_Root_Index()
    {
        const string json =
            """
            {
              "ContentRoots": ["/only/one/"],
              "Root": { "Children": {
                "ok.js": { "Children": null, "Asset": { "ContentRootIndex": 0, "SubPath": "ok.js" } },
                "bad.js": { "Children": null, "Asset": { "ContentRootIndex": 9, "SubPath": "bad.js" } }
              }, "Asset": null }
            }
            """;

        var urls = StaticWebAssetManifest.Resolve(json).Select(a => a.UrlPath).ToList();

        urls.ShouldContain("ok.js");
        urls.ShouldNotContain("bad.js");
    }

    [Fact]
    public void GetManifestPath_Sits_Next_To_The_Assembly()
    {
        var assembly = typeof(StaticWebAssetManifest).Assembly;

        var path = StaticWebAssetManifest.GetManifestPath(assembly);

        path.ShouldNotBeNull();
        path.ShouldEndWith(".staticwebassets.runtime.json");
        Path.GetDirectoryName(path).ShouldBe(Path.GetDirectoryName(assembly.Location));
        Path.GetFileName(path).ShouldBe("MonorailCss.Discovery.staticwebassets.runtime.json");
    }
}
