# MonorailCSS.Build.Tasks

[![NuGet](https://img.shields.io/nuget/vpre/MonorailCss.Build.Tasks)](https://www.nuget.org/packages/MonorailCss.Build.Tasks/)

MSBuild tasks that compile [MonorailCSS](https://www.nuget.org/packages/MonorailCss/) output at **build time**. The task scans your markup and referenced assemblies for utility classes, parses your input CSS for theme variables, custom utilities, and component rules, and writes an optimized static stylesheet. No Node toolchain required.

For **development-time / runtime** generation with hot reload, use [`MonorailCss.Discovery`](https://www.nuget.org/packages/MonorailCss.Discovery/) instead. The two share the same scanning + compilation core, so they extract the same classes from the same inputs.

## Usage

Add the package, then declare an input CSS file and an output path:

```xml
<ItemGroup>
  <MonorailCss Include="app.css">
    <OutputFile>wwwroot/css/app.css</OutputFile>
  </MonorailCss>
</ItemGroup>
```

The CSS is regenerated during build (with timestamp-based incremental skipping). Source scanning is driven entirely by Tailwind v4-style `@source` and `@import` directives in your input CSS:

```css
@import "tailwindcss";
@source "../components";
@source inline("bg-red-{50,100,200}");

@theme {
  --color-brand-500: #3b82f6;
}

@utility bordered-link {
  @apply font-semibold border-b border-current;
}
```

## Excluding assemblies from scanning

Every referenced assembly flows through the IL scanner. Skip ones whose IL embeds class-shaped strings (icon packs, validation libraries) to keep the candidate set focused:

```xml
<PropertyGroup>
  <MonorailCssExcludeAssemblies>FluentValidation;BadIdeas.Icons.FontAwesome</MonorailCssExcludeAssemblies>
</PropertyGroup>
```

Assemblies marked `[assembly: MonorailCssNoScan]` and framework assemblies (`Microsoft.*`, `System.*`, …) are excluded automatically.

See the [MonorailCSS documentation](https://github.com/monorailcss/MonorailCss.Framework) for the full directive reference, path placeholders, and configuration options.
