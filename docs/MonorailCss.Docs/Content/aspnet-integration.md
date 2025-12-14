---
title: ASP.NET Integration
description: Different approaches for integrating MonorailCSS with ASP.NET applications
order: 3
category: Basics
tags: [ aspnet, integration, sourcegen, msbuild, middleware ]
---

Integrating a JIT CSS compiler like MonorailCSS with ASP.NET presents an interesting challenge: MonorailCSS needs to
know which CSS classes your application uses to generate the appropriate styles. Since it's a utility-first system, you
could potentially use thousands of different class combinations, but you only want to ship CSS for the classes you
actually use.

There are a few different approaches to solving this problem, each with their own trade-offs.

## The Core Problem

Unlike traditional CSS frameworks that ship everything upfront, MonorailCSS generates CSS on-demand based on the classes
you use. This is great for bundle size, but it means we need a way to discover which classes are actually in use in your
application.

For ASP.NET applications, your CSS classes are typically scattered across:

- Razor pages and components
- View files
- Component libraries
- Dynamically generated markup

## Approach 1: Source Generator Discovery

The [MonorailCss.SourceGen.AspNet](https://github.com/monorailcss/MonorailCss.SourceGen.AspNet) project takes a
compile-time approach. It's a source generator that scans your Razor files during compilation and creates a list of all
the CSS classes it finds.

**The idea:** During build, walk through your Razor files, extract class names from the static markup, and generate code
that returns this list. At runtime, your application can feed this list to MonorailCSS to generate the stylesheet.

**Trade-offs:**

- This produces a string array of CSS classes being used that can be consumed at runtime. Producing the actual CSS and
  serving still needs to be done.
- Fast at runtime since class discovery happens at compile time
- Only sees static class names (things like `class="bg-blue-500"`)
- Misses dynamically constructed classes (like `class="bg-@color-500"` where `color` is a variable)
- Best for applications with mostly static utility usage

## Approach 2: MSBuild Task Generation

The [MonorailCss.Build.Tasks](https://github.com/monorailcss/MonorailCss.Build.Tasks) project goes a step further and
generates the actual CSS file at build time.

**The idea:** Similar to the source generator, but instead of just discovering classes, it runs MonorailCSS during your
build process and outputs a static CSS file that you can reference normally.

**Trade-offs:**

- Zero runtime overhead - it's just a static CSS file
- CSS is generated once at build time, so you get the full output immediately
- Looks closest to tailwind, things like VS Code and Rider intellisense seems to wire up okay.
- Can't adapt to dynamic content
- Perfect for SSG (static site generation) scenarios

## Approach 3: Runtime Middleware Discovery

A more dynamic approach involves using middleware to intercept HTML responses, parse out CSS classes, and generate
styles on-demand.

**The idea:** Hook into the ASP.NET pipeline, capture rendered HTML, extract class names with regex or HTML parsing, and
maintain a running collection of discovered classes. Generate and serve CSS based on this collection.

**Trade-offs:**

- Works with dynamic classes and runtime-generated content
- Slower first request (needs to parse HTML and generate CSS)
- Each page will have its own collection of CSS classes, making exposing a single CSS file for the site tricky
- More complex to implement correctly
- Best for highly dynamic applications where class usage can't be determined at compile time

## Which Approach Should You Use?

It really depends on your application:

**Use the source generator** if your application mostly uses static utility classes in Razor files. This gives you a
nice balance of performance and convenience.

**Use the MSBuild task** if you're building a static site or an application where you know all possible classes upfront.
This gives you the best runtime performance since everything is pre-generated.

**Use runtime middleware** if your application heavily uses dynamic class generation, or if you can control page load
order. [MyLittleContent](https://github.com/phil-scott-78/MyLittleContentEngine) engine uses this approach, but it also owns the full stack including publishing.

You could even combine approaches - use the source generator or build task for your known classes, and supplement with
runtime discovery for dynamic content.
 
## Rolling Your Own

MonorailCSS's core API is straightforward enough that you can build your own integration approach if none of these fit
your needs:

```csharp
var framework = new CssFramework();
var classes = GetClassesSomehow();
var css = framework.Process(classes);
```

The hard part isn't calling MonorailCSS - it's figuring out how to reliably discover which classes your application
uses. That's the problem each of these approaches tries to solve in different ways.
