---
title: Custom Utilities
description: Extend MonorailCSS with your own utility classes, including wildcard patterns and nested selectors
order: 3
uid: custom-utilities
tags: [utilities, custom, wildcard]
---

When MonorailCSS doesn't already cover a property you need &mdash; mask gradients, view transitions, anchor positioning &mdash; you can add the utility yourself. Custom utilities show up alongside built-ins, support every variant (`hover:`, `md:`, `focus-visible:`, custom variants), and live in the same generated CSS.

## A Static Utility

The simplest case: a utility that emits a fixed declaration.

```csharp:xmldocid
M:MonorailCss.Docs.Samples.Custom.ScrollbarUtilities.Build
```

That single block actually defines four utilities at once. Three of them are explained below. (MonorailCSS now ships scrollbar utilities out of the box; the example uses a `my-scrollbar-*` prefix to demonstrate the recipe without shadowing the built-ins.)

## Wildcard Utilities

When the trailing portion of a class name is dynamic &mdash; a color, a size, a number &mdash; use `IsWildcard = true` and a `*` in the pattern. The captured value is available inside declarations via `--value(...)`.

In the example above:

```csharp
new UtilityDefinition
{
    Pattern = "my-scrollbar-thumb-*",
    IsWildcard = true,
    Declarations = ImmutableList.Create(
        new CssDeclaration("--my-scrollbar-thumb-color", "--value(--color-*)")),
}
```

`--value(--color-*)` tells MonorailCSS to look up the captured suffix inside the `--color-*` namespace. So `my-scrollbar-thumb-blue-500` resolves to `var(--color-blue-500)`. Other namespace patterns work the same way: `--value(--spacing-*)`, `--value(--font-*)`, etc.

## Composing Utilities

Notice that `my-scrollbar-color` in the example doesn't take a value &mdash; it just reads the two custom properties set by `my-scrollbar-thumb-*` and `my-scrollbar-track-*`. This is the recommended pattern for utilities that share related state: each piece sets a CSS variable, and a final utility reads them and emits the actual property. It mirrors how MonorailCSS's built-in gradient and ring utilities (and the new scrollbar-thumb/track ones) work internally.

## Nested Selectors

Sometimes a utility needs to target a pseudo-element on the same element. Use `NestedSelectors` with `&` standing in for the parent:

```csharp:xmldocid
M:MonorailCss.Docs.Samples.Custom.ScrollbarUtilitiesNested.Build
```

This emits both `.scrollbar-hide { scrollbar-width: none; }` and `.scrollbar-hide::-webkit-scrollbar { display: none; }`, hiding the scrollbar in both Firefox and WebKit-based browsers. (The built-in `scrollbar-none` covers the Firefox half; pair it with this custom utility when you also need to kill the WebKit pseudo-element.)

## Next steps

- Make pseudo-elements addressable by any utility with [custom variants](xref:custom-variants).
- Wire utilities into reusable component classes via [component classes](xref:component-classes).
