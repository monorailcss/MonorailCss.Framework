using MonorailCss.Docs.Components.Shared;
using Pennington.DocSite.Services;
using Pennington.Islands;
using Pennington.Routing;

namespace MonorailCss.Docs.Services;

public class ContentIslandRenderer(
    ComponentRenderer renderer,
    ContentResolver contentResolver,
    UtilityContentService utilityService)
    : RazorIslandRenderer<ContentIsland>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var url = route.CanonicalPath.Value;

        if (url == "/" || string.IsNullOrEmpty(url))
            return null;

        var markdown = await contentResolver.GetContentByUrlAsync(url);
        if (markdown is not null)
            return new Dictionary<string, object?> { [nameof(ContentIsland.Url)] = url };

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
