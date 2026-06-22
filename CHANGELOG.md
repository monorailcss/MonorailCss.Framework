# Changelog

All notable changes to MonorailCSS are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

While the version is `0.x`, the public API may change between minor releases. The
recommended extension surface is the `BaseStaticUtility` / `BaseFunctionalUtility`
base classes and `CssFrameworkSettings`; the lower-level `IUtility` / `IVariant` AST
contract may still shift.

## [Unreleased]

### Added

- **Cross-project source watching under `dotnet watch`.** Discovery now watches the source
  directories of *referenced* projects, not just the running app's content root, so editing a
  `.razor`/`.cs` file in a referenced library (e.g. running a docs host while editing a
  component library it references) regenerates CSS live. Referenced project roots are located
  by reading each locally-built assembly's portable-PDB document table and walking up to the
  owning project directory; assemblies with no local source (Release builds, NuGet-cached RCLs,
  single-file/AOT images) contribute nothing. Enabled automatically when the `DOTNET_WATCH`
  environment variable is set, and controllable via the new
  `MonorailDiscoveryOptions.WatchReferencedProjectSources` (`null` = auto, `true`/`false` =
  force). The discovery diagnostics endpoint now reports the resolved `WatchSourceDirectories`.
- **Live watching of static-web-asset scripts.** The development source watcher now also watches
  the script extensions it scans (`.js`/`.mjs` by default, following `StaticWebAssetExtensions` and
  the `ScanStaticWebAssets` toggle), so editing a component script that builds markup at runtime
  regenerates CSS live. Previously these files were scanned once at startup but live edits to them
  were missed until the next process start.

## [0.1.0]

First release out of alpha. MonorailCSS now ships as three packages —
`MonorailCss` (the JIT compiler), `MonorailCss.Discovery` (runtime/dev-time class
discovery), and `MonorailCss.Build.Tasks` (build-time CSS generation).

### Added

- **Class conflict merging.** New `CssFramework.Merge` API (backed by `ClassMerger`)
  resolves conflicting utility classes the way tailwind-merge does — later classes win,
  conflicts are derived from each class's compiled declarations rather than a
  hand-maintained config, and partially-overridden shorthands are decomposed into the
  surviving longhands (`my-4 mt-6` → `mb-4 mt-6`). Results are cached and concurrency-safe.
- **`MonorailCss.Discovery` package.** Runtime CSS class discovery that scans loaded
  assembly IL, source files, and Razor Class Library static web assets to extract the
  utilities an app uses, with incremental rescans, debounced assembly-load handling, and
  hot-reload integration. Supports `[assembly: MonorailCssNoScan]` opt-out and a
  Tailwind-style ignore list.
- **`MonorailCss.Build.Tasks` package.** MSBuild task for build-time CSS generation with a
  persistent on-disk scan cache, `dotnet watch` integration, and a
  `MonorailCssExcludeAssemblies` property (plus automatic framework self-exclusion).
- **Animations.** `@keyframes` are now emitted for default and user-defined animations.
- **Typography / prose** aligned with Tailwind v4.2, including size variants.
- **Variants:** `min-*` / `max-*` breakpoints, `*:` / `**:` child variants, arbitrary
  `group-[selector]` / `peer-[selector]` forms, `open` / `not-open` / `popover-open` /
  `starting-style`, and `content: var(--tw-content)` injection for `before:` / `after:`.
- **Utilities:** broad Tailwind v4.2/4.3 coverage added — logical-property utilities
  (`block-size`, `inline-size`, `inset-s/e/bs/be`, `border-bs/be-*`, `mbs/mbe-*`,
  `pbs/pbe-*`, scroll block utilities), scrollbar utilities
  (`scrollbar-width` / `-gutter` / `-thumb` / `-track`), container-query sizing with
  `/name` modifiers, `zoom`, `tab-size`, additional color palettes (mauve, olive, mist,
  taupe), and more.
- **Documentation site** organized by CSS property, with live syntax-highlighted CSS
  output and per-utility examples.

### Changed

- **Utility discovery is now compile-time.** A Roslyn source generator emits an explicit
  utility registry, replacing reflection-based discovery. This makes the library
  trim-safe / AOT-compatible (`IsAotCompatible`) and removes the per-construction
  assembly scan.
- Logging migrated to `LoggerMessage`-defined partial methods.

### Fixed

- `@property` leak and assorted per-compile allocation reductions.
- `--tw-content` registered with an initial value so `before:` / `after:` render.
- `@plugin` directives are stripped instead of leaking into output.
- Gradient functions emitted as `background-image` for arbitrary `bg-[…]` values.
- `sr-only` emits `clip-path: inset(50%)` to match Tailwind v4.x.
- Numerous Tailwind-canonical alignment fixes across `text-`, `font-`, `leading-`,
  `border-`, shadow, mask, and placeholder utilities.

### Performance

- Parallelized discovery scanning with reduced allocations; incremental rescan that
  skips source-file scanning on late-load and hot-reload events.

For the complete history, see the
[commit log](https://github.com/monorailcss/MonorailCss.Framework/commits/main).

[Unreleased]: https://github.com/monorailcss/MonorailCss.Framework/compare/0.1.0...HEAD
[0.1.0]: https://github.com/monorailcss/MonorailCss.Framework/compare/0.0.4...0.1.0
