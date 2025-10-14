---
title: Customization
description: Learn how to customize MonorailCSS themes, create component classes, and add custom utilities
order: 2
category: Basics
tags: [theme, customization, components, utilities, variants]
---

MonorailCSS provides powerful customization options through themes, component classes, custom utilities, and custom variants.

## Customizing the Theme

The theme system uses CSS custom properties and can be customized to match your design system:

```csharp
using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Theme;

// Start with the default theme and customize it
var customTheme = Theme.CreateWithDefaults()
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

You can create component classes by applying utility classes to selectors. Note that you can use variant prefixes (like `hover:`, `focus:`) within the utility class strings:

```csharp
using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Theme;

var settings = new CssFrameworkSettings
{
    Theme = Theme.CreateWithDefaults(),
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

## Custom Utilities

You can define custom utilities to extend MonorailCSS with your own utility classes. This is useful for adding utilities that aren't built-in, like scrollbar styling:

```csharp
using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Parser.Custom;
using MonorailCss.Theme;

var customUtilities = ImmutableList.Create(
    // Static utility: scrollbar-none
    new UtilityDefinition
    {
        Pattern = "scrollbar-none",
        IsWildcard = false,
        Declarations = ImmutableList.Create(
            new CssDeclaration("scrollbar-width", "none")
        ),
        NestedSelectors = ImmutableList.Create(
            new NestedSelector("&::-webkit-scrollbar", ImmutableList.Create(
                new CssDeclaration("display", "none")
            ))
        )
    },
    // Dynamic utility: scrollbar-thumb-* (accepts color values)
    new UtilityDefinition
    {
        Pattern = "scrollbar-thumb-*",
        IsWildcard = true,
        Declarations = ImmutableList.Create(
            new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")
        )
    }
);

var framework = new CssFramework(new CssFrameworkSettings
{
    Theme = Theme.CreateWithDefaults(),
    CustomUtilities = customUtilities
});

// Now you can use: scrollbar-none, scrollbar-thumb-red-500, etc.
```

### Custom Utility Features

- **Static utilities**: Fixed pattern like `scrollbar-none`
- **Dynamic utilities**: Wildcard pattern like `scrollbar-thumb-*` that accepts values
- **Nested selectors**: Use `&` to reference parent selector (e.g., `&::-webkit-scrollbar`)
- **Theme integration**: Use `--value(--color-*)` to reference theme colors
- **Variant support**: Custom utilities work with all built-in variants (hover, focus, responsive, etc.)

## Custom Variants

You can define custom variants to extend the built-in variants (hover, focus, etc.) with your own selectors:

```csharp
using System.Collections.Immutable;
using MonorailCss;

var customVariants = ImmutableList.Create(
    new CustomVariantDefinition
    {
        Name = "scrollbar",
        Selector = "&::-webkit-scrollbar",
        Weight = 490  // Controls ordering in output
    },
    new CustomVariantDefinition
    {
        Name = "scrollbar-thumb",
        Selector = "&::-webkit-scrollbar-thumb",
        Weight = 491
    }
);

var framework = new CssFramework(new CssFrameworkSettings
{
    Theme = Theme.CreateWithDefaults(),
    CustomVariants = customVariants
});

// Now you can use: scrollbar:bg-gray-200, scrollbar-thumb:bg-blue-500, etc.
```

## Preflight CSS

Control whether to include base/reset styles:

```csharp
var framework = new CssFramework(new CssFrameworkSettings
{
    Theme = Theme.CreateWithDefaults(),
    IncludePreflight = true  // Default is true
});
```
