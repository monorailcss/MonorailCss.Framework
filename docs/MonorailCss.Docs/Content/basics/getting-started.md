---
title: Getting Started
description: Install MonorailCSS and generate your first CSS
order: 1
uid: getting-started
tags: [getting-started, basics]
---

MonorailCSS is a Tailwind 4.3-compatible JIT CSS compiler for .NET. Hand it a list of class names; it returns the CSS those classes need.

> **API surface:** this page walks through usage by example. For full method signatures, configuration types, and xmldoc-sourced descriptions of every public field, see the **[API reference](/api-reference)**.

## Installation

```bash
dotnet add package MonorailCss
```

## Your first stylesheet

```csharp
using MonorailCss;

var framework = new CssFramework();
var css = framework.Process("my-4 mx-4 text-red-500");
```

`framework.Process` returns a CSS string built from the classes you passed in. If you used five classes, only those five classes &mdash; plus the theme variables and preflight reset they depend on &mdash; appear in the output.

## The Tailwind features you expect

Variants, breakpoints, dark mode, arbitrary values, and opacity modifiers all work the same way they do in Tailwind 4. One example covers them all:

```csharp
var css = framework.Process(
    "hover:bg-blue-500 md:p-8 dark:text-white bg-red-500/50 bg-[#1da1f2] text-white!");
```

Produces:

```css
.bg-\[\#1da1f2\] {
  background-color: #1da1f2;
}
.bg-red-500\/50 {
  background-color: color-mix(in oklab, var(--color-red-500) 50%, transparent);
}
.text-white\! {
  color: var(--color-white) !important;
}
.hover\:bg-blue-500:hover {
  background-color: var(--color-blue-500);
}
@media (min-width: 768px) {
  .md\:p-8 {
    padding: calc(var(--spacing) * 8);
  }
}
.dark\:text-white:where(.dark, .dark *) {
  color: var(--color-white);
}
```

A few things worth pointing out:

- **Dark mode is class-based** by default &mdash; a parent element with `class="dark"` activates `dark:*` utilities. Configure dark mode globally with a [custom variant](xref:custom-variants) if you need `prefers-color-scheme` instead.
- **Important** works as a trailing `!` (Tailwind 4 syntax) or a leading `!`. Both produce the same `!important` output.
- **Arbitrary values** in `[...]` are validated and emitted untouched.

## Processing collections

You'll usually pass a deduplicated set of classes harvested from your views, not a single string. `Process` accepts either:

```csharp
var classes = new[] { "bg-blue-500", "text-white", "p-4", "rounded-lg" };
var css = framework.Process(classes);
```

## Using the framework in an app

`CssFramework` does its expensive work &mdash; reflection-based utility discovery, variant registration, theme resolution &mdash; in its constructor. Build it once and reuse the instance for every `Process` call:

```csharp
// Program.cs
builder.Services.AddSingleton<CssFramework>(_ =>
    new CssFramework(new CssFrameworkSettings
    {
        Theme = MonorailCss.Theme.Theme.CreateWithDefaults(),
    }));
```

`CssFrameworkSettings` exposes the rest of the configuration surface &mdash; preflight, important, color emission, custom utilities, prose customization, and more. See the [API reference](/api-reference#CssFrameworkSettings) for every field, and the [configuration guide](xref:configuration) for the patterns most projects need first.

Because utilities are discovered via reflection at startup, MonorailCSS is not currently configured for trimming or NativeAOT. If you need either, file an issue with your scenario.

## Next step

Add your brand colors with [theme & colors](xref:theme) &mdash; the most common first customization.
