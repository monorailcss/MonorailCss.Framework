---
title: ASP.NET Integration
description: Wire MonorailCSS into an ASP.NET app with runtime class discovery
order: 3
uid: aspnet-integration
tags: [aspnet, integration, discovery, blazor, middleware]
---

MonorailCSS is a JIT compiler &mdash; it only emits CSS for the classes your app actually uses. That works in any framework, but it leaves an open question for ASP.NET: how do you tell MonorailCSS which classes that is?

`MonorailCss.Discovery` answers that. At startup it walks the IL of every loaded assembly &mdash; your app, your Razor class libraries, every component package on NuGet &mdash; and pulls out the strings that look like utility classes. In development it also watches your `.razor`, `.cshtml`, and `.cs` files so hot-reload edits show up the moment you save them. No source generators, no MSBuild targets, no per-request HTML parsing.

## Installation

```bash
dotnet add package MonorailCss.Discovery
```

The package targets `net9.0` and `net10.0` and references `Microsoft.AspNetCore.App`.

## Quickstart

Two lines in `Program.cs`:

```csharp
using MonorailCss.Discovery;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMonorailCss();

var app = builder.Build();

app.UseStaticFiles();
app.UseMonorailCss();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

Then point your layout at the served stylesheet:

```html
<link rel="stylesheet" href="/_monorail/app.css" />
```

With no configuration `AddMonorailCss` registers a default `CssFramework`, scans every non-BCL referenced assembly, watches your project's source directory in development, and uses `wwwroot/app.css` as the source CSS prefix when it exists. `UseMonorailCss` mounts the CSS endpoint at `/_monorail/app.css`.

## Configuration

Pass a callback to `AddMonorailCss` to override the auto-detected defaults. The options most projects reach for:

```csharp
var framework = new CssFramework(new CssFrameworkSettings
{
    Theme = MonorailCss.Theme.Theme.CreateWithDefaults(),
});

builder.Services.AddSingleton(framework);
builder.Services.AddMonorailCss(opt =>
{
    opt.Framework = framework;
    opt.ExcludeAssemblies.Add("MonorailCss");
    opt.ExcludeAssemblies.Add("BadIdeas.Icons.FontAwesome");
    opt.ExtraSafelist.Add("bg-red-500");
    opt.SourceCss = File.ReadAllText("wwwroot/app.css");
    opt.CssEndpoint = "/css/app.css";
});
```

- **`Framework`** &mdash; supply a configured `CssFramework` when you have a custom theme, prose config, or registered utilities. See [configuration](xref:configuration) for the full settings surface.
- **`ExcludeAssemblies`** &mdash; skip assemblies whose IL strings are noise rather than real utilities. The MonorailCSS framework itself ships template strings like `"bg-{color}-500"` in its utilities; icon packs like `BadIdeas.Icons.FontAwesome` bake thousands of class-shaped tokens into their metadata. Excluding these makes the difference between scanning tens of thousands of phantom classes and scanning the hundred or so your app actually uses. BCL assemblies (`System.*`, `Microsoft.*`) are skipped automatically.
- **`ExtraSafelist`** &mdash; force-include classes that static scanning can't reconstruct, e.g. anything built at runtime via `$"bg-{color}-500"`.
- **`SourceCss`** &mdash; the equivalent of `app.css`: `@theme` blocks, `@apply` components, plain CSS. The discovered utilities are appended to whatever you put here. Defaults to the contents of `wwwroot/app.css` when present.
- **`CssEndpoint`** &mdash; the URL the middleware serves CSS at. Defaults to `/_monorail/app.css`; change it if that path collides with something else.

## Owning the endpoint

The built-in middleware handles caching headers, `If-None-Match`, and `HEAD` requests. When that's not enough &mdash; you want auth in front of the CSS, custom cache directives, or to mirror the output to disk &mdash; register the discovery service without the middleware and inject `IClassRegistry` into your own endpoint:

```csharp
builder.Services.AddMonorailClassDiscovery(opt =>
{
    opt.Framework = framework;
    opt.ExcludeAssemblies.Add("MonorailCss");
});

var app = builder.Build();

app.MapMethods("/css/app.css", ["GET", "HEAD"], (HttpContext ctx, IClassRegistry registry) =>
{
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

    return Results.Text(registry.Css, "text/css", Encoding.UTF8);
});
```

`IClassRegistry` exposes three things:

- **`GetClasses()`** &mdash; an immutable snapshot of every class the discovery pipeline has validated.
- **`Version`** &mdash; an opaque, content-derived ETag token. It changes whenever the class set or source CSS changes, but reverting to a prior class set yields the prior token, so an `If-None-Match` round-trip works the way browsers expect. The string is already wrapped in quotes per RFC 7232.
- **`Css`** &mdash; the fully assembled CSS (your `SourceCss` prefix plus the generated utilities), recomputed only when the class set changes. Same content the built-in middleware serves.

Bust the browser cache between hot-reloads by pinning the `Version` token onto the `<link>` href:

```razor
@inject IClassRegistry Registry
<link rel="stylesheet" href="/css/app.css?v=@Registry.Version" />
```

## Rolling your own

Discovery is a wrapper around `CssFramework.Process`. If its scanning model doesn't fit &mdash; you have your own class collector, you're generating CSS in a non-ASP.NET host, you want to drive everything from a build step &mdash; you can call the framework directly:

```csharp
var framework = new CssFramework();
var classes = GetClassesSomehow();
var css = framework.Process(classes);
```

See [getting started](xref:getting-started) for the bare-API walkthrough. The hard part isn't calling MonorailCSS &mdash; it's reliably discovering which classes your application uses. Discovery solves that for the ASP.NET case; outside it, you're on your own.
