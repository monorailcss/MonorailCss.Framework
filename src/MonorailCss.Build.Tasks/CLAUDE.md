# MonorailCss.Build.Tasks

MSBuild task library for integrating MonorailCss into .NET build pipelines. Provides build-time CSS generation by scanning project files for utility classes.

## Purpose

This project packages MonorailCss as an MSBuild task that:
1. Scans content files (Razor, HTML, JSX, etc.) for utility class names
2. Parses input CSS files for theme variables, custom utilities, and component rules
3. Supports Tailwind v4 `@source` and `@import` directives for flexible source configuration
4. Generates optimized CSS output using the MonorailCss framework
5. Runs automatically during build, ensuring CSS stays synchronized with markup

## Key Components

### ProcessCssTask (`ProcessCssTask.cs`)
Main MSBuild task that orchestrates the build process. Accepts:
- `InputFile`: CSS file with theme definitions, custom utilities, and source directives (e.g., `app.css`)
- `OutputFile`: Generated CSS output path

Source scanning is controlled entirely through `@source` and `@import` directives in the input CSS file.

Uses regex patterns to extract class names from various frameworks:
- `class="..."` - HTML/Razor
- `className="..."` - React/JSX
- `:class="..."` - Vue
- `@class(...)` - Blazor
- `classList={...}` - Solid/object syntax

### Parsing (`Parsing/`)

**CssThemeParser.cs**: Parses input CSS files for:
- `@theme` blocks containing CSS custom properties
- Component rules with `@apply` directives
- `@utility` blocks for custom utility definitions
- `@import` and `@source` directives for source configuration

**CssSourceParser.cs**: Parses Tailwind v4 source directives:
- `@import "tailwindcss" source("path")` - Set base path for auto-detection
- `@import "tailwindcss" source(none)` - Disable auto-detection
- `@source "path"` - Explicitly include a file or directory
- `@source not "path"` - Exclude a file or directory
- `@source inline("pattern")` - Safelist utilities with brace/variant expansion

**CustomUtilityCssParser.cs**: Extracts custom utility definitions from `@utility` directives, supporting:
- Static utilities (e.g., `@utility scrollbar-none`)
- Wildcard patterns (e.g., `@utility scrollbar-thumb-*`)
- Nested selectors with `&` references
- Automatic custom property dependency tracking

**SourceConfiguration.cs**: DTOs representing parsed source directives including include/exclude paths, inline utilities, and base path configuration.

### Scanning (`Scanning/`)

**DllScanner.cs**: Placeholder implementation for scanning .NET assemblies (DLLs) for utility classes. Currently logs a warning and returns empty set. Future implementation will use reflection to scan embedded resources and custom attributes.

## Usage

Typically consumed via NuGet package. The task is automatically invoked during build when configured in the project file:

```xml
<ItemGroup>
  <MonorailCss Include="app.css" Output="wwwroot/css/app.css" />
</ItemGroup>
```

### Source Configuration Examples

**Basic auto-detection** (default behavior):
```css
@import "tailwindcss";
/* Automatically scans current directory for .razor, .html, etc. */
```

**Custom base path**:
```css
@import "tailwindcss" source("../src");
/* Scans ../src directory instead of current directory */
```

**Explicit sources only**:
```css
@import "tailwindcss" source(none);
@source "../components";
@source "../pages";
/* Only scans explicitly listed directories */
```

**Excluding paths**:
```css
@import "tailwindcss";
@source not "../src/legacy";
/* Auto-detect, but skip legacy folder */
```

**DLL scanning** (placeholder):
```css
@import "tailwindcss" source(none);
@source "../dist/MyComponentLibrary.dll";
/* Scans DLL for utilities (not yet implemented) */
```

**Safelisting utilities**:
```css
@import "tailwindcss";
@source inline("underline");
@source inline("bg-red-{50,100,200}");
@source inline("{hover:,focus:,}underline");
/* Forces generation of specific utilities */
```

## Testing

Run the interactive project to verify the task output:
```bash
dotnet build
```

The generated CSS will be in the specified output location.
