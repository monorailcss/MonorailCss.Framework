---
title: ASP.NET Integration
description: Wire MonorailCSS into an ASP.NET app at build time, at runtime, or both
order: 3
uid: aspnet-integration
tags: [aspnet, integration, discovery, build-tasks, blazor, middleware]
---

MonorailCSS is a JIT compiler &mdash; it only emits CSS for the classes your app actually uses. Two packages bring that into ASP.NET:

- **`MonorailCss.Build.Tasks`** scans during `dotnet build` and writes CSS to disk. The output ships as a static asset; no work happens at runtime.
- **`MonorailCss.Discovery`** scans at startup, watches your source tree, and serves CSS from middleware. CSS regenerates as you edit.

You can pick either. You can also [wire both](#hybrid-build-time--dotnet-watch) so the build task produces the deployable file and Discovery keeps it fresh while you're running `dotnet watch`. Both packages read the same `app.css` and produce equivalent output from equivalent inputs.

## The shared input: `app.css`

Both packages read a Tailwind v4-style CSS file. The same directives work in either:

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

`@import` follows imports recursively, `@theme` defines design tokens, `@utility` registers custom utilities, `@custom-variant` registers custom variants, `@apply` resolves utility composition, and plain CSS passes through. Build.Tasks adds `@source`, `@source not`, and `@source inline(...)` for explicit source configuration.

The CSS-file-as-config angle has a second benefit: editor tooling like the [Tailwind CSS IntelliSense extension](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss) parses the same file and gives you autocomplete, hover previews, and color squares for every theme token in your `.razor` and `.cs` source.

## `MonorailCss.Build.Tasks` &mdash; build time

Install:

```bash
dotnet add package MonorailCss.Build.Tasks
```

The package auto-imports its targets. Add to your `.csproj`:

```xml
<PropertyGroup>
  <MonorailCssEnabled>true</MonorailCssEnabled>
</PropertyGroup>

<ItemGroup>
  <MonorailCss Include="wwwroot/app.css" />
</ItemGroup>
```

The default `OutputFile` is `wwwroot/css/%(Filename).css`, so `wwwroot/app.css` becomes `wwwroot/css/app.css`. `UseStaticFiles` serves it at `/css/app.css`:

```html
<link rel="stylesheet" href="/css/app.css" />
```

The task runs during `dotnet build`, scans the same source files, DLLs, and package static web assets Discovery does, and writes the file before content packaging. Rebuilds are incremental: unchanged inputs don't get rescanned. The `Clean` target removes the generated file.

> **Note:** `dotnet watch` does not re-trigger MSBuild targets. Build.Tasks alone won't keep your CSS fresh during a watch session &mdash; see the [hybrid section](#hybrid-build-time--dotnet-watch) for how to combine it with Discovery.

### Driving source configuration from CSS

The build task picks up source paths from `@source` and `@import` directives in your CSS:

```css
/* Widen auto-detection from wwwroot/ (where this file lives) up to the project root. */
@import "tailwindcss" source("..");

/* Disable auto-detection; only explicit @source directives below get scanned. */
@import "tailwindcss" source(none);
@source "../Components";
@source not "../Components/Legacy";

/* Scan a referenced library's DLL. $(...) syntax avoids clashing with glob {Pages,Components}. */
@source "../bin/$(Configuration)/$(TargetFramework)/MyComponentLibrary.dll";

/* Safelist runtime-built classes. Brace expansion supported. */
@source inline("bg-{red,blue}-{500,600}");
```

The placeholders inside `@source` paths (`$(Configuration)`, `$(TargetFramework)`, `$(RuntimeIdentifier)`) are resolved at build time from MSBuild properties.

### MSBuild configuration

| Property/Item | Purpose |
|---|---|
| `<MonorailCssEnabled>` | On/off; default `true`. Gate on `'$(Configuration)' == 'Release'` if you're combining with Discovery. |
| `<MonorailCssExcludeAssemblies>` | Semicolon-delimited assembly names to skip (e.g. `FluentValidation;BadIdeas.Icons.FontAwesome`). |
| `<MonorailCssScanStaticWebAssets>` | On/off; default `true`. Scans `.js`/`.mjs` shipped by referenced packages as static web assets (e.g. `_content/Pennington.UI/scripts.js`). Razor/Web SDK projects only. |
| `<MonorailCss>` `Include` | The entry CSS file. Multiple items produce multiple outputs. |
| `<MonorailCss>` `OutputFile` metadata | Override the default `wwwroot/css/%(Filename).css`. |

Framework assemblies (`MonorailCss`, `MonorailCss.Build.Tasks`, `MonorailCss.Discovery`) and the BCL are excluded automatically. You only need to list libraries that bake class-shaped strings into their IL.

## `MonorailCss.Discovery` &mdash; runtime

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
- **`CssEndpoint`** &mdash; the URL the middleware serves CSS at, default `/_monorail/app.css`. Change it to share a URL with a build-time static file (see hybrid below).
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

## Hybrid: build time + `dotnet watch`

Build.Tasks produces a deployable static file but doesn't see edits during a `dotnet watch` session. Discovery does see those edits but adds a hosted service and a startup IL scan to your app. You can wire both: Discovery only in Development, Build.Tasks only in Release. One `<link>` URL in your layout, two pipelines feeding it depending on environment.

**`Program.cs`** &mdash; register Discovery and the endpoint only in Development. In Production the request falls through to `UseStaticFiles`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddMonorailClassDiscovery(opt =>
    {
        opt.ExcludeAssemblies.Add("BadIdeas.Icons.FontAwesome");
    });
}

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/css/app.css", (IClassRegistry registry) =>
        Results.Text(registry.Css, "text/css", Encoding.UTF8));
}

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

**`.csproj`** &mdash; gate the build task on Release, point the output at the URL the dev endpoint serves on:

```xml
<PropertyGroup>
  <MonorailCssEnabled Condition="'$(Configuration)' == 'Release'">true</MonorailCssEnabled>
  <MonorailCssExcludeAssemblies>BadIdeas.Icons.FontAwesome</MonorailCssExcludeAssemblies>
</PropertyGroup>

<ItemGroup>
  <MonorailCss Include="wwwroot/app.css">
    <OutputFile>$(MSBuildProjectDirectory)/wwwroot/css/app.css</OutputFile>
  </MonorailCss>
</ItemGroup>
```

**Layout** &mdash; one stylesheet link, no environment branching:

```html
<link rel="stylesheet" href="/css/app.css" />
```

How a request for `/css/app.css` resolves:

- **Development.** Build task is off, so `wwwroot/css/app.css` doesn't exist. `UseStaticFiles` finds nothing, the request falls through to the routing endpoint, and the live `IClassRegistry` serves it.
- **Production.** Discovery isn't registered. The Release build produced `wwwroot/css/app.css`, which `UseStaticFiles` serves directly with its own `ETag`.

A couple of things to watch out for:

- **Keep the exclusion list in both places synced.** Both pipelines walk the same assembly set; an icon pack that inflates one inflates the other.
- **`dotnet clean` after switching configurations.** If you build `-c Release` and then go back to Debug, the static file persists in `wwwroot/css/` and shadows the dev endpoint. `dotnet clean` removes it.
- **Components that inject `IClassRegistry` will throw in Production.** Resolve it via `IServiceProvider.GetService<IClassRegistry>()` instead of `@inject` so a null in Production is a no-op.

The `TryMonorail` project in this repo is a worked example.

## Rolling your own

Both packages are wrappers around `CssFramework.Process`. If neither scanning model fits &mdash; you have your own class collector, you're generating CSS in a non-ASP.NET host, you're driving everything from a build step that produces inputs by some other route &mdash; call the framework directly:

```csharp
var framework = new CssFramework();
var classes = GetClassesSomehow();
var css = framework.Process(classes);
```

See [getting started](xref:getting-started) for the bare-API walkthrough. The hard part isn't calling MonorailCSS &mdash; it's reliably discovering which classes your application uses. Discovery and Build.Tasks solve that for the ASP.NET case.
