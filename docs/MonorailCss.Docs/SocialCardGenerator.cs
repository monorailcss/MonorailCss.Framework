using Ashcroft;
using Pennington.SocialCards;

namespace MonorailCss.Docs;

public static class SocialCardGenerator
{
    // Social-card assets live in the project's SocialCardAssets folder. Resolved against the
    // content root, so the paths are correct under both `dotnet run` and `dotnet run -- build`
    // (the static-build crawler fetches /social-cards/*.png from the in-memory host).
    public static Task<byte[]?> Build(SocialCardRequest socialCardRequest, IWebHostEnvironment environment)
    {
        var socialCardAssetsPath = Path.Combine(environment.ContentRootPath, "SocialCardAssets");
        var cardBackgroundPath = Path.Combine(environment.ContentRootPath, "MonorailCss-social-card.png");

        var card = SocialCard.Create(socialCardRequest.Width, socialCardRequest.Height)
            .Background(cardBackgroundPath)
            .At(Anchor.MiddleLeft, stack =>
            {
                stack.MaxWidth(960);

                stack.Title(socialCardRequest.Title);
                if (socialCardRequest.Description != null)
                {
                    stack.Subtitle(socialCardRequest.Description);
                }
            })
            .At(Anchor.BottomLeft, stack =>
            {
                stack.Meta(socialCardRequest.SiteTitle, color: "#c6753f");
            });

        return Task.FromResult<byte[]?>(card.ToBytes());
    }
}