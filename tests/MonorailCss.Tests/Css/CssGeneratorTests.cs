using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class CssGeneratorTests
{
    private readonly CssGenerator _generator = new();

    [Fact]
    public void ToCss_Declaration_ShouldGenerateCorrectCss()
    {
        var declaration = new Declaration("color", "red");
        var css = declaration.ToCss();

        css.ShouldBe("color: red;");
    }

    [Fact]
    public void ToCss_Declaration_WithImportant_ShouldGenerateCorrectCss()
    {
        var declaration = new Declaration("color", "red", true);
        var css = declaration.ToCss();

        css.ShouldBe("color: red !important;");
    }

    [Fact]
    public void ToCss_Declaration_WithIndentation_ShouldGenerateCorrectCss()
    {
        var declaration = new Declaration("color", "red");
        var css = declaration.ToCss(1);

        css.ShouldBe("  color: red;");
    }

    [Fact]
    public void ToCss_StyleRule_ShouldGenerateCorrectCss()
    {
        var rule = new StyleRule(".test", ImmutableList.Create<AstNode>(
            new Declaration("color", "red"),
            new Declaration("background", "blue")
        ));

        var css = rule.ToCss();
        var expected = """
                       .test {
                         color: red;
                         background: blue;
                       }
                       """;

        css.ShouldBe(expected);
    }

    [Fact]
    public void ToCss_NestedStyleRule_ShouldGenerateCorrectIndentation()
    {
        var innerRule = new StyleRule("&:hover", ImmutableList.Create<AstNode>(
            new Declaration("color", "green")
        ));

        var outerRule = new StyleRule(".button", ImmutableList.Create<AstNode>(
            new Declaration("color", "red"),
            innerRule
        ));

        var css = outerRule.ToCss();
        var expected = """
                       .button {
                         color: red;
                         &:hover {
                           color: green;
                         }
                       }
                       """;

        css.ShouldBe(expected);
    }

    [Fact]
    public void ToCss_AtRule_WithoutNodes_ShouldGenerateCorrectCss()
    {
        var atRule = new AtRule("import", "'styles.css'", ImmutableList<AstNode>.Empty);
        var css = atRule.ToCss();

        css.ShouldBe("@import 'styles.css';");
    }

    [Fact]
    public void ToCss_AtRule_WithNodes_ShouldGenerateCorrectCss()
    {
        var atRule = new AtRule("media", "(min-width: 768px)", ImmutableList.Create<AstNode>(
            new StyleRule(".responsive", ImmutableList.Create<AstNode>(
                new Declaration("width", "100%")
            ))
        ));

        var css = atRule.ToCss();
        var expected = """
                       @media (min-width: 768px) {
                         .responsive {
                           width: 100%;
                         }
                       }
                       """;

        css.ShouldBe(expected);
    }

    [Fact]
    public void ToCss_Comment_ShouldGenerateCorrectCss()
    {
        var comment = new Comment("This is a comment");
        var css = comment.ToCss();

        css.ShouldBe("/* This is a comment */");
    }

    [Fact]
    public void ToCss_Comment_WithIndentation_ShouldGenerateCorrectCss()
    {
        var comment = new Comment("Indented comment");
        var css = comment.ToCss(2);

        css.ShouldBe("    /* Indented comment */");
    }

    [Fact]
    public void ToCss_Context_ShouldGenerateAllNodes()
    {
        var context = new Context(
            ImmutableDictionary<string, object>.Empty,
            ImmutableList.Create<AstNode>(
                new Declaration("color", "red"),
                new Declaration("background", "blue")
            )
        );

        var css = context.ToCss();
        var expected = """
                       color: red;
                       background: blue;
                       """;

        css.ShouldBe(expected);
    }

    [Fact]
    public void GenerateCss_ComplexAstTree_ShouldGenerateCorrectCss()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new StyleRule(".container", ImmutableList.Create<AstNode>(
                new Declaration("max-width", "1200px"),
                new Declaration("margin", "0 auto"),
                new AtRule("media", "(max-width: 768px)", ImmutableList.Create<AstNode>(
                    new Declaration("max-width", "100%"),
                    new Declaration("padding", "0 20px")
                ))
            ))
        );

        var css = _generator.GenerateCss(nodes);
        var expected = """
                       @layer theme, base, components, utilities;

                       @layer utilities {
                         .container {
                           max-width: 1200px;
                           margin: 0 auto;
                           @media (max-width: 768px) {
                             max-width: 100%;
                             padding: 0 20px;
                           }
                         }
                       }
                       """;

        css.ShouldBe(expected);
    }

    [Fact]
    public void GenerateCss_WithLayers_ShouldOrganizeCorrectly()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new AtRule("layer", "theme", ImmutableList.Create<AstNode>(
                new Declaration("--color-primary", "#007bff")
            )),
            new AtRule("layer", "base", ImmutableList.Create<AstNode>(
                new StyleRule("body", ImmutableList.Create<AstNode>(
                    new Declaration("margin", "0")
                ))
            )),
            new AtRule("layer", "utilities", ImmutableList.Create<AstNode>(
                new StyleRule(".text-center", ImmutableList.Create<AstNode>(
                    new Declaration("text-align", "center")
                ))
            ))
        );

        var css = _generator.GenerateCss(nodes);
        var expected = """
                       @layer theme, base, components, utilities;

                       @layer theme {
                         --color-primary: #007bff;
                       }

                       @layer base {
                         body {
                           margin: 0;
                         }
                       }

                       @layer utilities {
                         .text-center {
                           text-align: center;
                         }
                       }
                       """;

        css.ShouldBe(expected);
    }

    [Fact]
    public void GenerateCss_EmptyNodes_ShouldReturnEmptyString()
    {
        var css = _generator.GenerateCss(ImmutableList<AstNode>.Empty);
        css.ShouldBe(string.Empty);
    }

    [Fact]
    public void GenerateCss_WithCommentsExcluded_ShouldNotIncludeComments()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Comment("This should be excluded"),
            new Declaration("color", "red")
        );

        var css = _generator.GenerateCss(nodes);

        css.ShouldNotContain("This should be excluded");
        css.ShouldContain("color: red");
    }

    [Fact]
    public void GenerateCss_WithCommentsIncluded_ShouldIncludeComments()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Comment("This should be included"),
            new Declaration("color", "red")
        );

        var css = _generator.GenerateCss(nodes, includeComments: true);

        css.ShouldContain("/* This should be included */");
        css.ShouldContain("color: red");
    }
}