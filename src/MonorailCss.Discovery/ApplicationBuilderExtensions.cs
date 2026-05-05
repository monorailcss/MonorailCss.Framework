using Microsoft.AspNetCore.Builder;

namespace MonorailCss.Discovery;

/// <summary>
/// <see cref="IApplicationBuilder"/> extensions that register the MonorailCss CSS endpoint middleware.
/// </summary>
public static class MonorailCssApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the middleware that serves discovered CSS at the configured endpoint
    /// (default <c>/_monorail/app.css</c>).
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder for chaining.</returns>
    public static IApplicationBuilder UseMonorailCss(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MonorailCssMiddleware>();
    }
}
