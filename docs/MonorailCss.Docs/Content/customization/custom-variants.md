---
title: Custom Variants
description: Add your own variants for pseudo-classes, pseudo-elements, and other selector modifiers
order: 4
uid: custom-variants
tags: [variants, pseudo-elements, scrollbar]
---

Variants are the prefixes you stack in front of utilities &mdash; `hover:bg-blue-500`, `md:flex`, `dark:text-white`. Each one transforms the underlying selector. MonorailCSS ships with the full set of built-in pseudo-classes, breakpoints, and container queries, but you can add your own when you need to target a selector the framework doesn't know about.

## Defining a Variant

A variant has a name (the prefix users will type), a selector pattern (using `&` for the parent), and a weight that controls output order.

```csharp:xmldocid
M:MonorailCss.Docs.Samples.Custom.ScrollbarVariants.Build
```

With this in place, every utility in the framework can be retargeted at the scrollbar pseudo-elements:

```html
<div class="overflow-auto scrollbar-thumb:bg-blue-500 scrollbar-thumb:rounded-full scrollbar-track:bg-slate-100">
  ...
</div>
```

## The Weight

`Weight` controls where the variant's rules appear in the output. Lower numbers come first. The default is 490, which places custom variants just before MonorailCSS's built-in pseudo-element variants (which start at 500). Bump it higher to push custom variants after built-ins, or use distinct weights to control ordering between your own variants &mdash; useful when one pseudo-element should override another.

## Variants vs. Custom Utilities

The two features are orthogonal. A custom utility creates a new property/value combination (the *what*); a custom variant retargets the selector (the *where*). The scrollbar example shipping in the [custom utilities](xref:custom-utilities) page demonstrates both: utilities like `scrollbar-thumb-*` set CSS variables, and variants like `scrollbar-thumb:` let you apply *any* utility to the thumb pseudo-element.

## Next steps

- Pair this with [custom utilities](xref:custom-utilities) for full pseudo-element styling.
- Tweak the [theme](xref:theme) so variants pull from brand colors.
