# MonorailCss Documentation Site

This documentation site is built using [MyLittleContentEngine](https://phil-scott-78.github.io/MyLittleContentEngine/), a static content generator written in .NET, with styling powered by MonorailCss.

## Running the Documentation Site

### Development Mode

To run the site in development mode with live reload:

```bash
cd docs/MonorailCss.Docs
dotnet watch
```

The site will be available at `http://localhost:5000` (or another port if 5000 is in use).

### Building Static Output

To generate static HTML output for deployment:

```bash
cd docs/MonorailCss.Docs
dotnet run -- build
```

This will generate the static site in the `output` directory.

## Adding Documentation

Documentation is written in Markdown files located in the `Content` directory.

### Creating a New Documentation Page

1. Create a new `.md` file in the `Content` directory
2. Add YAML front matter at the top of the file:

```markdown
---
title: Your Page Title
description: A brief description of the page
order: 1
category: Basics
date: 2025-10-12
tags: [tag1, tag2]
---

# Your Page Title

Your content goes here...
```

### Front Matter Properties

- `title` (required): The page title
- `description`: A brief description shown on the home page
- `order`: Numerical order for sorting (lower numbers appear first)
- `category`: Category for grouping related pages
- `date`: Publication date
- `tags`: Array of tags for categorization
- `isDraft`: Set to `true` to hide the page from the site

### Example

See the existing documentation files in the `Content` directory for examples.

## Customization

### Styling

The site uses MonorailCss for styling. Utility classes can be used directly in Razor components or markdown content.

### Layout

- `Components/Layout/MainLayout.razor`: Main site layout
- `Components/Pages/Home.razor`: Homepage displaying all documentation
- `Components/Pages/Doc.razor`: Individual documentation page template

### Configuration

Site configuration is in `Program.cs`:
- Site title and description
- Content paths
- MonorailCss integration

## Deployment

The generated static site can be deployed to:
- GitHub Pages
- Azure Static Web Apps
- Netlify
- Any static hosting service

Simply build the site using `dotnet run -- build` and deploy the contents of the `output` directory.
