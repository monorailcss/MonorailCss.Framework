using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MonorailCss.Discovery;

/// <summary>
/// Serves the generated CSS at <see cref="MonorailDiscoveryOptions.CssEndpoint"/> plus a
/// diagnostics endpoint (the CSS endpoint with a <c>/diagnostics</c> suffix).
/// Honors <c>If-None-Match</c> so browsers cache between hot-reload bumps.
/// </summary>
internal sealed class MonorailCssMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly RequestDelegate _next;
    private readonly ClassDiscoveryService _service;
    private readonly MonorailDiscoveryOptions _options;

    public MonorailCssMiddleware(RequestDelegate next, ClassDiscoveryService service, IOptions<MonorailDiscoveryOptions> options)
    {
        _next = next;
        _service = service;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        var diagnosticsPath = TrimExtension(_options.CssEndpoint) + "/diagnostics";

        if (string.Equals(path, _options.CssEndpoint, StringComparison.OrdinalIgnoreCase))
        {
            await ServeCssAsync(context);
            return;
        }

        if (string.Equals(path, diagnosticsPath, StringComparison.OrdinalIgnoreCase))
        {
            await ServeDiagnosticsAsync(context);
            return;
        }

        await _next(context);
    }

    private async Task ServeCssAsync(HttpContext context)
    {
        var etag = _service.ETag;

        if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var inm) &&
            inm.ToString() == etag)
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
            context.Response.Headers[HeaderNames.ETag] = etag;
            return;
        }

        var css = _service.Css;
        context.Response.ContentType = "text/css; charset=utf-8";
        context.Response.Headers[HeaderNames.ETag] = etag;
        context.Response.Headers[HeaderNames.CacheControl] = "no-cache";

        if (HttpMethods.IsHead(context.Request.Method))
        {
            return;
        }

        await context.Response.WriteAsync(css);
    }

    private async Task ServeDiagnosticsAsync(HttpContext context)
    {
        var snapshot = _service.GetDiagnostics();
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers[HeaderNames.CacheControl] = "no-store";
        await context.Response.WriteAsync(JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    private static string TrimExtension(string endpoint)
    {
        var dot = endpoint.LastIndexOf('.');
        var slash = endpoint.LastIndexOf('/');
        return dot > slash ? endpoint[..dot] : endpoint;
    }
}
