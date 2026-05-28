---
title: Component Classes
description: Bundle utility classes into reusable selectors with the Applies setting
order: 2
uid: component-classes
tags: [components, applies, btn, card]
---

Utility classes are great until you find yourself typing `px-4 py-2 rounded-lg font-semibold bg-blue-500 text-white hover:bg-blue-600` for the tenth time. The `Applies` setting bundles those utilities into a named selector you can reuse from markup.

## Defining Components

Each entry in `Applies` maps a CSS selector to a space-separated list of utilities. Variants like `hover:`, `focus:`, and `md:` are honored inside that list.

```csharp:symbol
Settings/Applies.cs > Applies.Build
```

After this configuration, you can write `<button class="btn btn-primary">Save</button>` and the framework expands those classes into the right declarations &mdash; including the `:hover` rule.

## Where do they end up?

Component rules land in the `@layer components` bucket. Plain utility classes live in `@layer utilities`, which has higher specificity in MonorailCSS's output order, so a utility applied directly to an element still wins over a component class. That means you can override a single property on a component without rewriting the whole thing:

```html
<button class="btn btn-primary p-6">Bigger Save</button>
```

The `p-6` utility wins over the `px-4 py-2` baked into `.btn`.

## Body and other element selectors

`Applies` keys are real CSS selectors, not just class names &mdash; so you can use them to set page-wide defaults too:

```csharp
{ "body", "font-sans text-gray-900 antialiased" }
```

This is a clean way to set base typography without writing a separate stylesheet.

## Next steps

- Build truly novel utilities (not just bundled ones) with [custom utilities](xref:custom-utilities).
- Customize the [theme](xref:theme) so component classes pull from your brand colors.
