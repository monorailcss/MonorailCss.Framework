---
title: Prose Customization
description: Customize the typography plugin to match your design system
order: 5
uid: prose
tags: [prose, typography, content]
---

The `prose` utility styles long-form content &mdash; blog posts, documentation, anything generated from Markdown. It comes with sensible defaults out of the box, but you'll often want to tune individual elements (links, blockquotes, code blocks) to match your design system. That's what `ProseCustomization` is for.

## Adding Element Rules

`ProseCustomization` takes a function that receives the current theme and returns a per-modifier dictionary of element rules. The modifier keys are `"DEFAULT"`, `"base"`, `"sm"`, `"lg"`, `"xl"`, `"2xl"`, and `"invert"` &mdash; the same set you reach for in markup with `prose-lg`, `prose-invert`, etc.

```csharp:symbol
Prose/BoldLinks.cs > BoldLinks.Build
```

Two things are happening here: every link inside a `prose` block becomes bold and underlined (in `DEFAULT`), and blockquotes get a heavier font-weight when the user opts into `prose-lg` (in `lg`).

## Element Rules in Detail

Each `ProseElementRule` has:

- **`Selector`** &mdash; any CSS selector. `"a"`, `"blockquote"`, `"pre > code"`, `"h2 + p"` all work.
- **`Declarations`** &mdash; the properties to apply.
- **`UseWhereWrapper`** (default `true`) &mdash; wraps the selector in `:where()` so it doesn't compete with utility specificity.
- **`ExcludeClass`** (default `"not-prose"`) &mdash; gives users an escape hatch: anything inside `<div class="not-prose">` is exempt.

## Pulling From the Theme

The customization function receives the theme so you can reference design tokens:

```csharp
Customization = theme => /* ... */
```

Inside your `ProseDeclaration` values you can use `var(--color-brand-500)` directly, knowing those CSS variables are emitted alongside the prose rules.

## Next steps

- Add the brand colors prose pulls from in [theme & colors](xref:theme).
- See every customization point assembled in one [full example](xref:full-example).
