---
title: Theme & Colors
description: Customize MonorailCSS color palettes, font families, and other design tokens
order: 1
uid: theme
tags: [theme, colors, palette, fonts, design-tokens]
---

The theme is where your design tokens live. Colors, fonts, spacing, breakpoints &mdash; every value MonorailCSS knows about is keyed by a CSS custom property name (like `--color-blue-500` or `--font-display`) and emitted as a real CSS variable in the output. Customizing the theme means adding, replacing, or aliasing those keys.

## Adding a Color Palette

The most common customization: introduce a new palette so utilities like `bg-brand-500` and `text-brand-700` resolve to your brand colors.

```csharp:symbol
Theme/BrandPalette.cs > BrandPalette.Build
```

Once registered, every utility that consumes the `--color-*` namespace works with your new palette: `bg-brand-500`, `border-brand-200`, `ring-brand-300`, `from-brand-50`, etc.

### What about the shade keys?

The standard scale is `50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950` &mdash; the same shades the built-in palettes use. You don't have to provide every shade, but you do need to provide the ones you reference in markup. Missing shades simply won't generate utilities.

## Overriding a Single Shade

Need to nudge a single color without rebuilding a palette? Use `Theme.Add` directly with the fully-qualified key:

```csharp:symbol
Theme/OverrideExistingColor.cs > OverrideExistingColor.Build
```

This changes only `--color-blue-500` and `--color-blue-600`. The rest of the blue scale, and every other palette, are untouched.

## Aliasing a Palette

Aliases let you give a semantic name (`primary`, `accent`, `surface`) to an existing palette. Behind the scenes the alias becomes a thin layer of CSS variables pointing at the source palette, so swapping the source later is a one-line change.

```csharp:symbol
Theme/PaletteAlias.cs > PaletteAlias.Build
```

After this runs, `bg-primary-500` emits the same color as `bg-sky-500`, but the markup is in design-system terms.

## Starting From an Empty Theme

If you don't want any of the built-in defaults &mdash; no Tailwind palette, no preset spacing scale &mdash; start from `Theme.CreateEmpty()` and build up only what you need:

```csharp:symbol
Theme/EmptyTheme.cs > EmptyTheme.Build
```

This is useful for design systems that ship a constrained token set.

## Adding Font Families

Font families are stored under the `--font-*` namespace and surface as `font-{name}` utilities.

```csharp:symbol
Theme/FontFamily.cs > FontFamily.Build
```

Now `font-display` and `font-mono` are available alongside the built-in `font-sans` and `font-serif`.

## Next steps

- Bundle utilities into reusable selectors with [component classes](xref:component-classes).
- Add your own utility classes with [custom utilities](xref:custom-utilities).
