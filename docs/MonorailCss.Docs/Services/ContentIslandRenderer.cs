using MonorailCss.Docs.Components.Shared;
using MonorailCss.Docs.Models;
using MyLittleContentEngine.Services.Content;
using MyLittleContentEngine.Services.Spa;

namespace MonorailCss.Docs.Services;

public class ContentIslandRenderer(
    ComponentRenderer renderer,
    IMarkdownContentService<DocFrontMatter> contentService,
    UtilityContentService utilityService)
    : RazorIslandRenderer<ContentIsland>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(string url)
    {
        // Home page has its own component (Home.razor), not handled by ContentIsland
        if (url == "/")
            return null;

        // Check markdown content
        var markdownResult = await contentService.GetRenderedContentPageByUrlOrDefault(url);
        if (markdownResult is not null)
            return new Dictionary<string, object?> { [nameof(ContentIsland.Url)] = url };

        // Check utility content (/{category}/{property} pattern)
        var segments = url.Trim('/').Split('/');
        if (segments.Length == 2)
        {
            var categoryName = await utilityService.GetCategoryNameFromSlugAsync(segments[0]);
            var propertyUtilities = await utilityService.GetUtilitiesForPropertyAsync(segments[0], segments[1]);

            if (categoryName is not null && propertyUtilities is not null)
                return new Dictionary<string, object?> { [nameof(ContentIsland.Url)] = url };
        }

        return null;
    }
}
