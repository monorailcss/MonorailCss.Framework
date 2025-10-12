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

**Incremental Build Support**: The task implements timestamp-based incremental builds. If the output file exists and is newer than the input file, CSS regeneration is skipped. This prevents duplicate file tracking errors from the Static Web Assets system and improves build performance.

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

**CssSourceParser.cs**: Parses Tailwind v4 source and import directives:
- `@import "path"` - Import CSS files (parsed but not yet processed)
- `@import "path" source("path")` - Set base path for auto-detection
- `@import "path" source(none)` - Disable auto-detection
- `@import "path" theme(static)` - Theme modifier (parsed but not yet implemented)
- `@import "path" layer(utilities)` - Layer modifier (parsed but not yet implemented)
- `@source "path"` - Explicitly include a file or directory
- `@source not "path"` - Exclude a file or directory
- `@source inline("pattern")` - Safelist utilities with brace/variant expansion
- `@custom-variant name (selector)` - Define custom variant with selector pattern

**CustomUtilityCssParser.cs**: Extracts custom utility definitions from `@utility` directives, supporting:
- Static utilities (e.g., `@utility scrollbar-none`)
- Wildcard patterns (e.g., `@utility scrollbar-thumb-*`)
- `@apply` directives within utilities (e.g., `@utility bordered-link { @apply font-semibold border-b; }`)
- Nested selectors with `&` references
- Automatic custom property dependency tracking

**SourceConfiguration.cs**: DTOs representing parsed source directives including include/exclude paths, inline utilities, and base path configuration.

**PathPlaceholderResolver.cs**: Resolves dynamic placeholders in file paths using MSBuild properties. Supports `{Configuration}`, `{TargetFramework}`, and `{RuntimeIdentifier}` placeholders with case-insensitive matching. Enables build-configuration-agnostic `@source` paths.

### Scanning (`Scanning/`)

**GlobScanner.cs**: Handles glob pattern matching for file system scanning. Supports:
- Wildcards (`*`) for single-level matching
- Recursive globs (`**`) for multi-level directory traversal
- Brace expansion (`{Pages,Components}` or `{razor,cs}`) for multiple alternatives
- Automatic exclusion of `bin` and `obj` directories
- Cross-platform path normalization

**DllScanner.cs**: Scans .NET assemblies (DLLs) for utility class strings embedded in string literals using PE metadata reader. Extracts string values from the #Strings heap in the metadata tables.

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

**DLL scanning with placeholders**:
```css
@import "tailwindcss" source(none);
@source "../bin/{Configuration}/{TargetFramework}/MyComponentLibrary.dll";
/* Scans DLL for utilities, with placeholders resolved at build time */
/* Placeholders: {Configuration}, {TargetFramework}, {RuntimeIdentifier} */
```

**Safelisting utilities**:
```css
@import "tailwindcss";
@source inline("underline");
@source inline("bg-red-{50,100,200}");
@source inline("{hover:,focus:,}underline");
/* Forces generation of specific utilities */
```

**Custom variants**:
```css
@custom-variant scrollbar (&::-webkit-scrollbar);
@custom-variant scrollbar-track (&::-webkit-scrollbar-track);
@custom-variant scrollbar-thumb (&::-webkit-scrollbar-thumb);

/* Use custom variants in markup */
/* class="scrollbar:w-2 scrollbar-track:bg-gray-100" */
```

**Import modifiers** (theme and layer are parsed but not yet implemented):
```css
@import "tailwindcss" theme(static);
@import "../bin/lumexui/_theme";
@import "./typography" layer(utilities);
/* Theme and layer modifiers are logged but not processed by framework */
```

**Custom utilities with @apply**:
```css
@utility bordered-link {
    @apply font-semibold leading-tight text-current border-b border-current hover:border-b-2;
}

/* Use in markup: class="bordered-link" */
```

### Path Placeholders

`@source` paths can contain placeholders that are resolved at build time using MSBuild properties. This eliminates the need to hardcode build-specific paths like `Debug` or `Release`.

**Supported Placeholders:**
- `{Configuration}` - Resolves to "Debug", "Release", or custom configuration
- `{TargetFramework}` - Resolves to "net9.0", "net8.0", etc.
- `{RuntimeIdentifier}` - Resolves to "win-x64", "linux-x64", etc. (optional)

**Example:**
```css
@source "../bin/{Configuration}/{TargetFramework}/LumexUI.dll";
```

At build time, this resolves to:
- Debug: `../bin/Debug/net9.0/LumexUI.dll`
- Release: `../bin/Release/net9.0/LumexUI.dll`

**Features:**
- Case-insensitive: `{configuration}`, `{Configuration}`, and `{CONFIGURATION}` all work
- Partial resolution: If a placeholder value is not available, it remains unchanged (a warning is logged)
- Works with all path types: DLL files, directories, and glob patterns
- Works with `@source not` exclusions as well

**Technical Details:**
The placeholders are resolved in `ProcessCssTask` using values passed from MSBuild via the `MonorailCss.Build.Tasks.targets` file. The resolution is handled by `PathPlaceholderResolver` before any path normalization or file system operations.

## Unsupported Directives

The following Tailwind v4 directives are **not yet parsed**:
- `@plugin` - Plugin loading (e.g., `@plugin "@tailwindcss/typography"`)
- CSS import processing - `@import` paths are not resolved or merged into output

These may be added in future versions.

## Testing

Run the interactive project to verify the task output:
```bash
dotnet build
```

The generated CSS will be in the specified output location.
