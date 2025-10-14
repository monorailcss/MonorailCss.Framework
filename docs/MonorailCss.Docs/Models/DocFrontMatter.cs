using MyLittleContentEngine.Models;

namespace MonorailCss.Docs.Models;

public class DocFrontMatter : IFrontMatter
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Uid { get; init; } = null;
    public string[] Tags { get; init; } = [];
    public bool IsDraft { get; init; }
    public int Order { get; init; } = 0;
    public string Category { get; init; } = string.Empty;
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }

    public Metadata AsMetadata()
    {
        return new Metadata
        {
            Title = Title,
            Description = Description,
            Order = Order,
            LastMod = DateTime.MaxValue
        };
    }
}