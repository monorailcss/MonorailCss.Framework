# MonorailCSS

MonorailCSS is a utility-first CSS library inspired heavily by Tailwind.

Give a design system and a list of CSS classes it will produce a CSS file.

```csharp
var framework = new CssFramework(MonorailCss.DesignSystem.Default);
var result = framework.Process(new[] { "my-4", "mx-4", "text-red-300" });
```

Will produce.

```css
.mx-4 {
  margin-left:1rem;
  margin-right:1rem;
}
.my-4- {
  margin-bottom:-1rem;
  margin-top:-1rem;
}
.text-red-400 {
  color:rgba(248, 113, 113, 1);
}
```

The design system can be customized, and extra CSS can be included using the design system in case you need to extend.

```csharp

var allTheCssClassesInOurProject = GatherThemUpSomehow();

var framework = new CssFramework(DesignSystem.Default with
    {
        Colors = DesignSystem.Default.Colors.AddRange(
            new Dictionary<string, ImmutableDictionary<string, CssColor>>()
            {
                { "primary", DesignSystem.Default.Colors[ColorNames.Sky] },
                { "base", DesignSystem.Default.Colors[ColorNames.Gray] },
            })
    })
    .Apply("body", "font-sans")
    .Apply(
        ".token.comment,.token.prolog,.token.doctype,.token.cdata,.token.punctuation,.token.selector,.token.tag",
        "text-gray-300")
    .Apply(".token.boolean,.token.number,.token.constant,.token.attr-name,.token.deleted", "text-blue-300")
    .Apply(".token.string,.token.char,.token.attr-value,.token.builtin,.token.inserted", "text-green-300")
    .Apply(
        ".token.operator,.token.entity,.token.url,.token.symbol,.token.class-name,.language-css .token.string,.style .token.string",
        "text-cyan-300")
    .Apply(".token.atrule,.token.keyword", "text-indigo-300")
    .Apply(".token.property,.token.function", "text-orange-300")
    .Apply(".token.regex,.token.important", "text-red-300");

var style = framework.Process(allTheCssClassesInOurProject);
```        
