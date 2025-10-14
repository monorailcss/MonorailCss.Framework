---
title: Customization
description: Learn how to customize MonorailCSS themes and create component classes
order: 2
category: Basics
tags: [theme, customization, components]
---

MonorailCSS provides powerful customization options through themes and component classes.

## Customizing the Theme

The theme system uses CSS custom properties and can be customized to match your design system:

```csharp
using System.Collections.Immutable;

// Start with the default theme and customize it
var customTheme = new Theme()
    .AddColorPalette("brand", new Dictionary<string, string>
    {
        { "50", "#eff6ff" },
        { "100", "#dbeafe" },
        { "200", "#bfdbfe" },
        { "300", "#93c5fd" },
        { "400", "#60a5fa" },
        { "500", "#3b82f6" },
        { "600", "#2563eb" },
        { "700", "#1d4ed8" },
        { "800", "#1e40af" },
        { "900", "#1e3a8a" },
        { "950", "#172554" }
    }.ToImmutableDictionary())
    .MapColorPalette("sky", "primary")  // Map 'sky' palette to 'primary'
    .AddFontFamily("display", "'Inter', sans-serif");

var framework = new CssFramework(new CssFrameworkSettings
{
    Theme = customTheme
});

// Now you can use: bg-brand-500, text-primary-600, font-display
```

## Component Classes (Apply)

You can create component classes by applying utility classes to selectors:

```csharp
using System.Collections.Immutable;

var settings = new CssFrameworkSettings
{
    IncludePreflight = false,
    Applies = new Dictionary<string, string>
    {
        { "body", "font-sans text-gray-900" },
        { ".btn", "px-4 py-2 rounded-lg font-semibold" },
        { ".btn-primary", "bg-blue-500 text-white hover:bg-blue-600" },
        { ".card", "bg-white shadow-lg rounded-xl p-6" }
    }.ToImmutableDictionary()
};

var framework = new CssFramework(settings);
var css = framework.Process("btn btn-primary");
```

## Preflight CSS

Control whether to include base/reset styles:

```csharp
var framework = new CssFramework(new CssFrameworkSettings
{
    IncludePreflight = true  // Default is true
});
```
