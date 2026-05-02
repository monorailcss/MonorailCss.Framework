---
title: Full Example
description: A worked configuration that wires up every customization point in one place
order: 6
uid: full-example
tags: [example, customization, settings]
---

Real projects rarely use a single customization in isolation &mdash; you typically introduce a brand palette, alias it to semantic names, register a couple of component classes, and add a custom utility or two. This page shows all of that wired up at once so you can see how the pieces compose.

```csharp:xmldocid
M:MonorailCss.Docs.Samples.Combined.FullSetup.Build
```

That's the full customization surface in one place: a brand palette, an aliased "primary" name, a custom font, component classes, a custom utility, and a custom variant. Everything else MonorailCSS does &mdash; the variant system, the utility pipeline, theme resolution &mdash; runs on top of these settings.

## Next steps

- Wire MonorailCSS into your app via [ASP.NET integration](xref:aspnet-integration).
- Read about the framework's internal pipeline in [architecture](xref:architecture).
