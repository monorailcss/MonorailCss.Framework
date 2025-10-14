---
title: Advanced Features
description: Explore custom variants, arbitrary values, and other advanced features
order: 3
category: Advanced
tags: [variants, arbitrary-values, advanced]
---

MonorailCSS includes several advanced features for more complex use cases.

## Custom Variants

Create custom pseudo-classes or selector modifiers:

```csharp
var settings = new CssFrameworkSettings
{
    CustomVariants = new List<CustomVariantDefinition>
    {
        new() { Name = "scrollbar", Selector = "&::-webkit-scrollbar" },
        new() { Name = "scrollbar-track", Selector = "&::-webkit-scrollbar-track" }
    }.ToImmutableList()
};

var framework = new CssFramework(settings);
// Use: scrollbar:w-2 scrollbar-track:bg-gray-100
```

## Arbitrary Values

MonorailCSS supports arbitrary values in square brackets:

```csharp
var framework = new CssFramework();
var css = framework.Process("bg-[#1da1f2] text-[14px] w-[500px]");
```

This allows you to use custom values without extending the theme, perfect for one-off designs or rapid prototyping.

## Theme Variable Tracking

MonorailCSS includes a theme optimization system that tracks which CSS custom properties are actually used in your application, allowing you to generate minimal theme CSS files.
