---
title: Merging Classes
description: Resolve conflicting utility classes in a class list with CssFramework.Merge
order: 4
uid: merging-classes
tags: [merging, conflicts, tailwind-merge, components, overrides]
---

The moment you build reusable components, you hit class conflicts. A button has its own padding, but a caller wants to override it: `class="px-4 py-2 ... px-8"`. Both `px-4` and `px-8` end up in the class list, and which one wins now depends on CSS source order rather than the caller's intent. `CssFramework.Merge` resolves that the way [tailwind-merge](https://github.com/dcastil/tailwind-merge) does in the JavaScript world &mdash; later classes win, conflicting earlier ones are dropped:

```csharp
var framework = new CssFramework();

framework.Merge("px-2 p-4 bg-red-500 hover:p-2 bg-blue-500");
// → "p-4 hover:p-2 bg-blue-500"
```

`px-2` loses to the later `p-4` (a shorthand that covers it), `bg-red-500` loses to `bg-blue-500`, and `hover:p-2` survives because a variant scopes its own conflicts. Surviving classes keep their original relative order.

## Calling Merge

`Merge` is a method on `CssFramework` &mdash; the same object you already use for `Process`. Results are computed against the framework's theme and registered utilities, cached for its lifetime, and safe to call from multiple threads, so reuse the singleton you build at startup:

```csharp
// Program.cs
builder.Services.AddSingleton<CssFramework>(_ => new CssFramework());

// anywhere you compose class lists
public string Classes => _framework.Merge(baseClasses, overrideClasses);
```

Because merging runs against the same framework, it sees the same theme and the same registered utilities &mdash; including any [custom utilities](xref:custom-utilities) you added.

## Merging caller overrides

The most common use is letting a component expose an "extra classes" parameter that can override its defaults. The `params` overload joins several lists into one, with later lists winning:

```csharp
string Button(string? extra = null) =>
    _framework.Merge("px-4 py-2 rounded-lg bg-blue-500 text-white", extra);

Button();                  // "px-4 py-2 rounded-lg bg-blue-500 text-white"
Button("bg-green-600");    // "px-4 py-2 rounded-lg text-white bg-green-600"
Button("px-8 px-2");       // "py-2 rounded-lg bg-blue-500 text-white px-2"
```

`null` and blank entries are skipped, and whitespace between classes is normalized, so you can pass conditional fragments straight through without pre-cleaning them.

## How conflicts are resolved

This is where MonorailCSS differs from tailwind-merge under the hood. tailwind-merge ships a large hand-maintained table of class groups; MonorailCSS instead **compiles each class and compares what it actually writes**. Two classes conflict only when they target the same variant chain, and a later class removes an earlier one when it writes (or explicitly covers) every CSS property the earlier one writes.

Because conflicts come from real compiled output, the behavior falls out naturally:

```csharp
// Shorthands beat their longhands — but never the reverse.
framework.Merge("px-2 p-4");   // → "p-4"
framework.Merge("p-4 px-2");   // → "p-4 px-2"   (px-2 only narrows one axis)

// Variants isolate their own conflicts.
framework.Merge("hover:p-2 p-4");        // → "hover:p-2 p-4"
framework.Merge("hover:p-2 hover:p-4");  // → "hover:p-4"

// Variant order is normalized, so equivalent chains still conflict.
framework.Merge("hover:focus:p-2 focus:hover:p-4"); // → "focus:hover:p-4"

// `!important` is part of the conflict key.
framework.Merge("p-4! p-2");   // → "p-4! p-2"   (different importance, both kept)
framework.Merge("p-4! p-2!");  // → "p-2!"

// Arbitrary values and arbitrary properties participate too.
framework.Merge("w-4 w-[32px]");        // → "w-[32px]"
framework.Merge("p-4 [padding:1rem]");  // → "[padding:1rem]"
```

Composable utilities &mdash; the ones that build a value out of several `--tw-*` custom properties &mdash; are handled correctly because the merger ignores the shared scaffolding declaration and compares only the variable each class owns:

```csharp
framework.Merge("touch-pan-x touch-pan-y");  // → "touch-pan-x touch-pan-y"  (different axes)
framework.Merge("touch-pan-x touch-none");   // → "touch-none"               (reset wins)
framework.Merge("blur-sm grayscale");        // → "blur-sm grayscale"        (independent filters)
```

## What passes through untouched

Anything the merger can't model as a utility is left exactly where it is:

- **Unknown classes** &mdash; your own hand-written CSS classes, third-party classes &mdash; are never dropped.
- **Component-layer classes** such as `prose` and `container` don't conflict with utilities.

```csharp
framework.Merge("my-widget p-4 my-widget"); // → "my-widget p-4 my-widget"
framework.Merge("prose text-lg");           // → "prose text-lg"
```

## Custom utilities and reset semantics

[Custom utilities](xref:custom-utilities) you register participate in merging automatically &mdash; there's no separate config to keep in sync. The only case that needs a hint is a *reset* utility: one that replaces a value its composable siblings build up through `--tw-*` variables it doesn't itself declare (the way `touch-none` overrides `touch-pan-x`). Such a utility implements `IUtility.GetMergeInfo` to declare the keys it covers:

```csharp
MergeConflictInfo? IUtility.GetMergeInfo(Candidate candidate, Theme.Theme theme) =>
    MergeConflictInfo.CoversKeys("--tw-pan-x", "--tw-pan-y", "--tw-pinch-zoom");
```

`MergeConflictInfo.WritesKeys(...)` replaces the derived keys entirely; `CoversKeys(...)` supplements them. Ordinary utilities never need this.

## Acknowledgment

The class-merging feature is inspired by [TailwindMerge](https://github.com/Zettersten/TailwindMerge), a .NET take on the original [tailwind-merge](https://github.com/dcastil/tailwind-merge) JavaScript library. MonorailCSS keeps the same "later class wins" semantics but derives conflicts from compiled utility output rather than a hand-maintained class-group config, so new and custom utilities participate without extra configuration.

## Next steps

- Bundle recurring utility sets into named selectors with [component classes](xref:component-classes).
- Add your own utilities &mdash; they merge automatically &mdash; with [custom utilities](xref:custom-utilities).
