---
title: Getting Started
description: Learn how to use MonorailCSS in your .NET projects
order: 1
category: Basics
tags: [getting-started, basics]
---

MonorailCSS is a utility-first CSS library inspired heavily by Tailwind CSS. It's a JIT CSS compiler written in .NET that aims to be Tailwind CSS 4.1 compatible.

## Installation

Install MonorailCSS from NuGet:

```bash
dotnet add package MonorailCss
```

## Basic Usage

Given a list of CSS classes, MonorailCSS will produce optimized CSS output.

```csharp
var framework = new CssFramework();
var css = framework.Process("my-4 mx-4 text-red-500");
```

Will produce:

```css
.mx-4 {
  margin-left: 1rem;
  margin-right: 1rem;
}
.my-4 {
  margin-bottom: 1rem;
  margin-top: 1rem;
}
.text-red-500 {
  color: var(--color-red-500);
}
```

## Processing Collections

You can also process collections of classes:

```csharp
var classes = new[] { "bg-blue-500", "text-white", "p-4", "rounded-lg" };
var css = framework.Process(classes);
```

## Next Steps

- Learn about [customizing the theme](customization)
- Explore [advanced features](advanced-features)
- Understand the [architecture](architecture)
