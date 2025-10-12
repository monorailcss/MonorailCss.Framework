# MonorailCSS

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/MonorailCss)](https://www.nuget.org/packages/MonorailCss/)

MonorailCSS is a utility-first CSS library inspired heavily by Tailwind CSS. It's a JIT CSS compiler written in .NET that aims to be Tailwind CSS 4.1 compatible.

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

You can also process collections of classes:

```csharp
var classes = new[] { "bg-blue-500", "text-white", "p-4", "rounded-lg" };
var css = framework.Process(classes);
```

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

## Advanced Features

### Custom Variants

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

### Arbitrary Values

MonorailCSS supports arbitrary values in square brackets:

```csharp
var framework = new CssFramework();
var css = framework.Process("bg-[#1da1f2] text-[14px] w-[500px]");
```

### Preflight CSS

Control whether to include base/reset styles:

```csharp
var framework = new CssFramework(new CssFrameworkSettings
{
    IncludePreflight = true  // Default is true
});
```        
