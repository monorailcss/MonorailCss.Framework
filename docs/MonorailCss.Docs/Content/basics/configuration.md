---
title: Configuration
description: The framework-wide toggles every project decides up front
order: 2
uid: configuration
tags: [important, preflight, settings, configuration]
---

`CssFrameworkSettings` carries every knob MonorailCSS exposes. Most of them &mdash; theme, custom utilities, applies, prose &mdash; are rich data structures covered later. Two are simple booleans you'll usually decide once, on day one.

## Important

`Important = true` emits every generated declaration with `!important`. The use case is narrow: MonorailCSS is being layered on top of another stylesheet that already wins specificity wars, and you can't reorder the cascade. In greenfield projects keep this `false` (the default) and let normal cascade rules do the work.

```csharp:xmldocid
M:MonorailCss.Docs.Samples.Settings.Important.Build
```

If you only need `!important` on a few utilities, use Tailwind's per-class `!` syntax instead &mdash; either trailing (`bg-red-500!`) or leading (`!bg-red-500`).

## Preflight

`IncludePreflight` controls whether MonorailCSS emits its base reset styles &mdash; the rules that normalize browser defaults for headings, lists, form elements, and so on. The default is `true`, which is what you want for a standalone application.

Switch it off when MonorailCSS is generating just a slice of CSS that ships alongside another framework's reset (Bootstrap, an existing design system, a CMS theme), so you don't double-reset margins and form controls.

```csharp:xmldocid
M:MonorailCss.Docs.Samples.Settings.Preflight.Build
```

## Next step

Wire MonorailCSS into your app with [ASP.NET integration](xref:aspnet-integration).
