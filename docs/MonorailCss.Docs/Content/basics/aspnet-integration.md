---
title: ASP.NET Integration
description: Wire MonorailCSS into an ASP.NET app with runtime class discovery
order: 3
uid: aspnet-integration
tags: [aspnet, integration, discovery, blazor, middleware]
---

MonorailCSS is a JIT compiler &mdash; it only emits CSS for the classes your app actually uses. That works in any framework, but it leaves an open question for ASP.NET: how do you tell MonorailCSS which classes that is, and how do you wire up the theme?

`MonorailCss.Discovery` answers both. At startup it walks the IL of every loaded assembly &mdash; your app, your Razor class libraries, every component package on NuGet &mdash; and pulls out the strings that look like utility classes. It also reads a Tailwind v4-style CSS file (your `app.css`) and feeds its `@theme`, `@utility`, `@apply`, `@custom-variant`, and `@import` directives into the framework. In development it watches your `.razor`, `.cshtml`, and `.cs` files for class changes and your CSS file (plus everything it imports) for theme changes &mdash; both flow through hot-reload the moment you save. No source generators, no MSBuild targets, no per-request HTML parsing.

The CSS-file-as-config angle has a second benefit: editor tooling like the [Tailwind CSS IntelliSense extension](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss) parses the same file and gives you autocomplete, hover previews, and color squares for every theme token in your `.razor` and `.cs` source. One file, two consumers.

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

With no configuration `AddMonorailCss` registers a default `CssFramework`, scans every non-BCL referenced assembly, watches your project's source directory in development, and auto-detects `wwwroot/app.css` as the entry CSS file (recursively following its `@import`s). `UseMonorailCss` mounts the CSS endpoint at `/_monorail/app.css`.

## CSS-driven configuration

Most projects don't need a custom `CssFramework` at all &mdash; the theme, custom utilities, custom variants, and component classes all live in `wwwroot/app.css`, exactly the same shape Tailwind v4 expects:

```css
/* wwwroot/app.css */
@import "tailwindcss";

@theme {
    --color-brand: oklch(60% 0.2 250);
    --shadow-card: 0 1px 3px rgba(0, 0, 0, 0.1);
    --radius-card: 0.625rem;
}

@custom-variant dark (&:where(.dark, .dark *));

@utility scrollbar-hide {
    scrollbar-width: none;
}

@layer base {
    body {
        @apply text-foreground bg-background;
    }
}
```

That's it &mdash; `bg-brand`, `shadow-card`, `rounded-card`, `dark:bg-card`, `scrollbar-hide` all resolve. The framework follows `@import` recursively; in development a file watcher rebuilds the framework whenever you save any of the imported files.

## Configuration

Pass a callback to `AddMonorailCss` to override the auto-detected defaults. The options most projects reach for:

```csharp
builder.Services.AddMonorailCss(opt =>
{
    opt.SourceCssPath = "wwwroot/app.css";          // explicit path; auto-detected when omitted
    opt.ExcludeAssemblies.Add("MonorailCss");
    opt.ExcludeAssemblies.Add("BadIdeas.Icons.FontAwesome");
    opt.ExtraSafelist.Add("bg-red-500");
    opt.CssEndpoint = "/css/app.css";
});
```

- **`SourceCssPath`** &mdash; path to the entry CSS file. The framework follows `@import` recursively, picks up `@theme`/`@utility`/`@custom-variant`/`@apply` directives, and watches every imported file in development. Auto-detected as `wwwroot/app.css` when both this and `SourceCss` are unset.
- **`SourceCss`** &mdash; an in-memory CSS string, for cases where you compose the CSS programmatically rather than putting it on disk. Same parsing as `SourceCssPath`. Can be combined with `SourceCssPath` (the inline content layers on top of the file-derived settings).
- **`Framework`** &mdash; supply a pre-configured `CssFramework` when you need to seed custom prose configuration or register utilities programmatically. The CSS-file processing layers on top of these settings; you don't have to choose. See [configuration](xref:configuration) for the full settings surface.
- **`ExcludeAssemblies`** &mdash; skip assemblies whose IL strings are noise rather than real utilities. The MonorailCSS framework itself ships template strings like `"bg-{color}-500"` in its utilities; icon packs like `BadIdeas.Icons.FontAwesome` bake thousands of class-shaped tokens into their metadata. Excluding these makes the difference between scanning tens of thousands of phantom classes and scanning the hundred or so your app actually uses. BCL assemblies (`System.*`, `Microsoft.*`) are skipped automatically.
- **`ExtraSafelist`** &mdash; force-include classes that static scanning can't reconstruct, e.g. anything built at runtime via `$"bg-{color}-500"`.
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
