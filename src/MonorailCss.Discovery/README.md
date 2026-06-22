# MonorailCSS.Discovery

[![NuGet](https://img.shields.io/nuget/vpre/MonorailCss.Discovery)](https://www.nuget.org/packages/MonorailCss.Discovery/)

Runtime CSS class discovery for [MonorailCSS](https://www.nuget.org/packages/MonorailCss/). It scans the IL of loaded assemblies — plus any source files you point it at — to find the utility classes your app actually uses, then compiles them with MonorailCSS. No source generators, no MSBuild tasks, no Node toolchain.

This is the companion package for **development-time** and **runtime** CSS generation (e.g. a Blazor app serving `/css/app.css` live with hot reload). For **build-time** CSS generation that ships a static stylesheet, use [`MonorailCss.Build.Tasks`](https://www.nuget.org/packages/MonorailCss.Build.Tasks/) instead.

## What it scans

- **Loaded assemblies** via in-memory metadata (`Assembly.TryGetRawMetadata`) — class-shaped string literals baked into `.razor`/`.cs` compiled output, including those in referenced NuGet packages.
- **Source files** (`.razor`, `.cshtml`, `.cs`, `.html`, …) for live class extraction during development. Under `dotnet watch` this reaches across project boundaries: the source directories of referenced, locally-built projects (located from their build PDBs) are watched too, so editing a component in a referenced library regenerates CSS even though its source lives outside the running app.
- **Static web assets** (`.js`/`.mjs` shipped by Razor Class Libraries under `_content/`), so component libraries that build markup at runtime are covered.

Assemblies marked `[assembly: MonorailCssNoScan]` — including the MonorailCSS framework assemblies — are skipped automatically. Framework assemblies (`Microsoft.*`, `System.*`, …) are excluded by default. You can exclude additional assemblies (e.g. icon packs whose IL is full of class-shaped strings) via options.

## Basic usage (ASP.NET Core / Blazor)

```csharp
builder.Services.AddMonorailCssDiscovery(options =>
{
    options.SourceCssPath = "wwwroot/app.css"; // @theme / @apply / @utility / @source directives
});

// ...
app.MapMonorailCss("/css/app.css"); // serves generated CSS, regenerates on change in development
```

In your markup, reference the endpoint exactly as you would a static stylesheet:

```html
<link rel="stylesheet" href="/css/app.css" />
```

A common pattern is **hybrid**: run Discovery in Development for instant feedback, and let `MonorailCss.Build.Tasks` emit a static `app.css` for Release so production serves a plain file with no scanning at startup.

See the [MonorailCSS documentation](https://github.com/monorailcss/MonorailCss.Framework) for configuration, theming, and the full directive reference.
