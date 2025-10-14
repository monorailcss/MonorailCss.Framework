using MyLittleContentEngine.Models;

namespace MonorailCss.Docs.Models;

public class UtilityFrontMatter : IFrontMatter
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Uid { get; init; } = null;
    public string[] Tags { get; init; } = [];
    public bool IsDraft { get; init; }
    public string? Section { get; init; }
    public string? RedirectUrl { get; init; }

    public Metadata AsMetadata()
    {
        return new Metadata
        {
            Title = Title,
            Description = Description,
        };
    }
}