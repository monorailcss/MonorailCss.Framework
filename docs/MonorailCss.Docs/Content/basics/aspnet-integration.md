---
title: ASP.NET Integration
description: Wire MonorailCSS into an ASP.NET app with the Discovery package or the CssFramework API
order: 3
uid: aspnet-integration
tags: [aspnet, integration, discovery, blazor, middleware]
---

MonorailCSS is a JIT compiler &mdash; it only emits CSS for the classes your app actually uses. The **`MonorailCss.Discovery`** package brings that into ASP.NET: it scans at startup, watches your source tree, and serves CSS from middleware. CSS regenerates as you edit. If its scanning model doesn't fit, you can call `CssFramework` directly instead &mdash; see [rolling your own](#rolling-your-own) at the end.

The shape is simple: every place a class name can hide is scanned, the discovered names are compiled against your theme by `CssFramework`, and the result is a single stylesheet. The diagram below traces that flow &mdash; the four class sources at the top fan into the scan, `app.css` configures the engine, and one CSS file comes out.

```beck
meta:
  title: From source to stylesheet
  subtitle: How discovery feeds CssFramework
  direction: TB
nodes:
  - { id: razor,     title: Razor & C#,            subtitle: .razor · .cs,                 kind: external, icon: code,      accent: neutral, rank: 0 }
  - { id: dll,       title: Referenced assemblies, subtitle: class strings in IL,           kind: external, icon: container, accent: neutral, rank: 0 }
  - { id: assets,    title: Static web assets,     subtitle: _content/**/*.js,              kind: external, icon: function,  accent: neutral, rank: 0 }
  - { id: scan,      title: Discovery scan,        subtitle: collect + validate,            kind: gateway,  icon: search,    accent: primary, rank: 1, order: 0 }
  - { id: appcss,    title: app.css,               subtitle: "@theme · @utility · @apply",  kind: gateway,  icon: file,      accent: primary, rank: 1, order: 1 }
  - { id: framework, title: CssFramework,          subtitle: JIT compile,                   kind: service,  icon: bolt,      accent: primary, rank: 2 }
  - { id: css,       title: app.css,               subtitle: served at /_monorail/app.css,  kind: service,  icon: globe,     accent: primary, rank: 3 }
groups:
  - { id: sources, label: Class sources, members: [razor, dll, assets], accent: neutral }
edges:
  - { from: razor,     to: scan }
  - { from: dll,       to: scan }
  - { from: assets,    to: scan }
  - { from: scan,      to: framework, label: candidate classes }
  - { from: appcss,    to: framework, label: theme + utilities }
  - { from: framework, to: css,       label: compiled CSS }
```

## The input: `app.css`

Discovery reads a Tailwind v4-style CSS file:

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

`@import` follows imports recursively, `@theme` defines design tokens, `@utility` registers custom utilities, `@custom-variant` registers custom variants, `@apply` resolves utility composition, and plain CSS passes through.

The CSS-file-as-config angle has a second benefit: editor tooling like the [Tailwind CSS IntelliSense extension](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss) parses the same file and gives you autocomplete, hover previews, and color squares for every theme token in your `.razor` and `.cs` source.

## `MonorailCss.Discovery`

Install:

```bash
dotnet add package MonorailCss.Discovery
```

Wire it up in `Program.cs`:

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

Point your layout at the served stylesheet:

```html
<link rel="stylesheet" href="/_monorail/app.css" />
```

With no configuration `AddMonorailCss` auto-detects `wwwroot/app.css`, scans every non-BCL referenced assembly for class strings, scans JavaScript shipped by component packages as static web assets, watches your source tree for changes in Development, and serves the result at `/_monorail/app.css`. Edit a class in a `.razor` file under `dotnet watch` and the browser sees the new CSS on the next HEAD poll.

### Configuration

Pass a callback to `AddMonorailCss` to override defaults:

```csharp
builder.Services.AddMonorailCss(opt =>
{
    opt.ExcludeAssemblies.Add("BadIdeas.Icons.FontAwesome");
    opt.ExtraSafelist.Add("bg-red-500");
    opt.CssEndpoint = "/css/app.css";
});
```

The options you'll actually reach for:

- **`ExcludeAssemblies`** &mdash; skip libraries whose IL strings would inflate the candidate set without contributing real utilities. Icon packs that bake thousands of class-shaped tokens into metadata are the usual culprits. MonorailCSS itself and BCL assemblies (`System.*`, `Microsoft.*`) are excluded automatically.
- **`ExtraSafelist`** &mdash; force-include classes static scanning can't reconstruct, e.g. anything built at runtime via `$"bg-{color}-500"`.
- **`ScanStaticWebAssets`** &mdash; on by default; reads classes out of JavaScript that referenced packages/RCLs ship under `_content/<Package>/`. Those files live in the NuGet cache &mdash; outside your source tree and the assembly IL &mdash; so nothing else reaches them; a component whose modal markup is built in `scripts.js` needs this. Narrow what's read with `StaticWebAssetExtensions` (default `.js`, `.mjs`), or suppress a package's assets by adding it to `ExcludeAssemblies`.
- **`SourceCssPath`** &mdash; path to your entry CSS file. Auto-detected as `wwwroot/app.css` when unset.
- **`CssEndpoint`** &mdash; the URL the middleware serves CSS at, default `/_monorail/app.css`.
- **`Framework`** &mdash; supply a pre-configured `CssFramework` when you need to seed prose configuration or register utilities programmatically. See [configuration](xref:configuration). The CSS file processing layers on top.

There are a couple of less-common options (`SourceCss` for in-memory CSS, `WriteToFile` to mirror the output to disk, `WatchSourceDirectories` for non-standard layouts) on `MonorailDiscoveryOptions`; the defaults are right for most projects.

### Owning the endpoint

The built-in middleware handles `ETag`, `If-None-Match`, and `HEAD`, and exposes a JSON diagnostics view at `{CssEndpoint}/diagnostics` (handy when a class isn't appearing and you want to confirm whether it failed to discover or failed to compile).

When the built-in middleware isn't enough &mdash; you want auth in front of the CSS, custom cache directives, or to mirror to a CDN &mdash; register discovery without the middleware and inject `IClassRegistry` into your own endpoint:

```csharp
builder.Services.AddMonorailClassDiscovery(opt =>
{
    opt.ExcludeAssemblies.Add("BadIdeas.Icons.FontAwesome");
});

var app = builder.Build();

app.MapGet("/css/app.css", (IClassRegistry registry) =>
    Results.Text(registry.Css, "text/css", Encoding.UTF8));
```

`IClassRegistry` exposes `Css` (the assembled stylesheet), `Version` (a content-derived ETag, already wrapped in quotes per RFC 7232 &mdash; don't re-quote it), and `GetClasses()` (the validated class set).

## Rolling your own

The Discovery package is a wrapper around `CssFramework.Process`. If its scanning model doesn't fit &mdash; you have your own class collector, you're generating CSS in a non-ASP.NET host, you're driving everything from a build step that produces inputs by some other route &mdash; call the framework directly:

```csharp
var framework = new CssFramework();
var classes = GetClassesSomehow();
var css = framework.Process(classes);
```

See [getting started](xref:getting-started) for the bare-API walkthrough. The hard part isn't calling MonorailCSS &mdash; it's reliably discovering which classes your application uses. Discovery solves that for the ASP.NET case.
