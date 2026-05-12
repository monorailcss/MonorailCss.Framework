using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using MonorailCss.Discovery;
using TryMonorail.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Hybrid integration: in Development, register runtime Discovery so dotnet watch gets hot
// reload over EnC deltas, source file edits, and app.css changes. In Production, Discovery is
// not registered — the build task pre-generated wwwroot/css/app.css during the Release build,
// and UseStaticFiles serves it. See docs/.../aspnet-integration.md for the full pattern.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddMonorailClassDiscovery(opt =>
    {
        // The Monorail framework itself ships template strings ("bg-{color}-500" etc.) in its
        // utilities; the FontAwesome icon pack bakes thousands of class-shaped strings into its
        // metadata. Neither contributes utilities the playground chrome actually uses. The same
        // exclusion list lives in TryMonorail.csproj's MonorailCssExcludeAssemblies so the
        // build-time scan converges on the same candidate set.
        opt.ExcludeAssemblies.Add("MonorailCss");
        opt.ExcludeAssemblies.Add("BadIdeas.Icons.FontAwesome");
    });
}

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

if (app.Environment.IsDevelopment())
{
    app.MapMethods("/css/app.css", ["GET", "HEAD"], (HttpContext ctx, IClassRegistry registry) =>
    {
        // IClassRegistry.Version is already wrapped in quotes per RFC 7232.
        var etag = registry.Version;
        ctx.Response.Headers[HeaderNames.ETag] = etag;
        ctx.Response.Headers[HeaderNames.CacheControl] = "no-cache";

        if (ctx.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var inm) && inm.ToString() == etag)
        {
            return Results.StatusCode(StatusCodes.Status304NotModified);
        }

        if (HttpMethods.IsHead(ctx.Request.Method))
        {
            ctx.Response.ContentType = "text/css; charset=utf-8";
            return Results.Empty;
        }

        // registry.Css is the cached SourceCss + generated utilities the discovery service
        // already built — same content the built-in middleware serves.
        return Results.Text(registry.Css, "text/css", System.Text.Encoding.UTF8);
    });
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
