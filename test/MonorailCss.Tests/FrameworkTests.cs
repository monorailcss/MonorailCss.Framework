using MonorailCss.Plugins.Transforms;
using MonorailCss.Tests.Plugins;
using Shouldly;

namespace MonorailCss.Tests;

public class FrameworkTests
{
    [Fact]
    public void Missing_cssclass_is_reported()
    {
        var framework = new CssFramework(new CssFrameworkSettings
            { CssResetOverride = string.Empty});
        var r = framework.ProcessSplitWithWarnings([
            "missing", "block", "missing", "another-missing", "bg-red-101",
        ]);

        r.Warnings.ShouldBe(["missing", "another-missing", "bg-red-101"], ignoreOrder: true);
    }

    [Fact]
    public void Can_rewrite_negative_margins()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "-my-4",
        ]);
        r.ShouldBeCss("""
                      .-my-4 {
                        margin-bottom:calc(var(--monorail-spacing) * -4);
                        margin-top:calc(var(--monorail-spacing) * -4);
                      }
                      """);
    }

    [Fact]
    public void Can_do_pseudo_variants()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "first:text-red-100",
        ]);

        r.ShouldBeCss("""


                      .first\:text-red-100:first-child {
                        --monorail-text-opacity:1;
                        color:oklch(0.936 0.032 17.717 / var(--monorail-text-opacity));
                      }

                      """);
    }

    [Fact]
    public void Smoke_Test()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "bg-blue-100",
            "dark:bg-blue-50",
            "hover:bg-blue-200",
            "hover:sm:bg-blue-300",
            "sm:bg-blue-400",
            "dark:sm:bg-blue-500",
            "prose-h1:bg-blue-200",
        ]);
        r.ShouldBeCss("""
                      .bg-blue-100 {
                        --monorail-bg-opacity:1;
                        background-color:oklch(0.932 0.032 255.585 / var(--monorail-bg-opacity));
                      }
                      .dark .dark\:bg-blue-50 {
                        --monorail-bg-opacity:1;
                        background-color:oklch(0.97 0.014 254.604 / var(--monorail-bg-opacity));
                      }
                      .hover\:bg-blue-200:hover {
                        --monorail-bg-opacity:1;
                        background-color:oklch(0.882 0.059 254.128 / var(--monorail-bg-opacity));
                      }
                      .prose h1 .prose-h1\:bg-blue-200 {
                        --monorail-bg-opacity:1;
                        background-color:oklch(0.882 0.059 254.128 / var(--monorail-bg-opacity));
                      }
                      @media (min-width:640px) {
                        .hover\:sm\:bg-blue-300:hover {
                          --monorail-bg-opacity:1;
                          background-color:oklch(0.809 0.105 251.813 / var(--monorail-bg-opacity));
                        }
                        .sm\:bg-blue-400 {
                          --monorail-bg-opacity:1;
                          background-color:oklch(0.707 0.165 254.624 / var(--monorail-bg-opacity));
                        }
                        .dark .dark\:sm\:bg-blue-500 {
                          --monorail-bg-opacity:1;
                          background-color:oklch(0.623 0.214 259.815 / var(--monorail-bg-opacity));
                        }
                      }



                      """);
    }

    [Fact]
    public void Can_get_all_rules()
    {
        var framework = new CssFramework();
        var rules = framework.GetAllRules();
        rules.ShouldNotBeEmpty();
    }

    [Fact]
    public void Can_do_arbitrary_properties()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process([
            "[mask-type:luminance]",
        ]);
        r.ShouldBeCss("""

                      .\[mask-type\:luminance\] {
                        mask-type: luminance;
                      }

                      """);
    }


    [Fact]
    public void Transform_PropertyValue_With_Variables_Should_Be_Ok()
    {
        Transform.TransformValue.ShouldBe("translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y))");
    }

    [Fact]
    public void Can_Do_Apply()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty, Applies = new Dictionary<string, string>()
            {
                {"body", "font-sans mb-2"},
            }});
        var r = framework.Process([]);
        r.ShouldBeCss("""

                      body {
                        font-family:-apple-system, BlinkMacSystemFont, avenir next, avenir, segoe ui, helvetica neue, helvetica, Ubuntu, roboto, noto, arial, sans-serif;
                        margin-bottom:calc(var(--monorail-spacing) * 2);
                      }

                      """);
    }

    [Fact]
    public void Unknown_variants_are_swallowed()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );

        var r = framework.Process(["group-hover:font-xl", "test"]);
        r.ShouldBeCss("""


                      """);
    }

    [Fact]
    public void Importance_is_respected()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );

        var r = framework.Process(["m-4", "my-3", "mb-2"]);
        r.Trim().ReplaceLineEndings().ShouldBe("""
                                               :root {
                                                 --monorail-spacing:0.25rem;
                                               }
                                               .m-4 {
                                                 margin:calc(var(--monorail-spacing) * 4);
                                               }
                                               .my-3 {
                                                 margin-bottom:calc(var(--monorail-spacing) * 3);
                                                 margin-top:calc(var(--monorail-spacing) * 3);
                                               }
                                               .mb-2 {
                                                 margin-bottom:calc(var(--monorail-spacing) * 2);
                                               }
                                               """.Trim().ReplaceLineEndings());

        // now check if we send them in the opposite way. output ordering should remain equal.
        var r2 = framework.Process(["mb-2", "my-3", "m-4"]);
        r2.Trim().ReplaceLineEndings().ShouldBe("""

                                                :root {
                                                  --monorail-spacing:0.25rem;
                                                }
                                                .m-4 {
                                                  margin:calc(var(--monorail-spacing) * 4);
                                                }
                                                .my-3 {
                                                  margin-bottom:calc(var(--monorail-spacing) * 3);
                                                  margin-top:calc(var(--monorail-spacing) * 3);
                                                }
                                                .mb-2 {
                                                  margin-bottom:calc(var(--monorail-spacing) * 2);
                                                }

                                                """.Trim().ReplaceLineEndings());

        var r3 = framework.Process(["lg:rounded-none", "lg:rounded-l-lg"]);
        r3.Trim().ReplaceLineEndings().ShouldBe("""

                                                :root {
                                                  --monorail-spacing:0.25rem;
                                                }
                                                @media (min-width:1024px) {
                                                  .lg\:rounded-none {
                                                    border-radius:0px;
                                                  }
                                                  .lg\:rounded-l-lg {
                                                    border-bottom-left-radius:0.5rem;
                                                    border-top-left-radius:0.5rem;
                                                  }
                                                }
                                                """.Trim().ReplaceLineEndings());

        var r4 = framework.Process(["lg:rounded-l-lg", "lg:rounded-none"]);
        r4.Trim().ReplaceLineEndings().ShouldBe("""

                                                :root {
                                                  --monorail-spacing:0.25rem;
                                                }
                                                @media (min-width:1024px) {
                                                  .lg\:rounded-none {
                                                    border-radius:0px;
                                                  }
                                                  .lg\:rounded-l-lg {
                                                    border-bottom-left-radius:0.5rem;
                                                    border-top-left-radius:0.5rem;
                                                  }
                                                }

                                                """.Trim().ReplaceLineEndings());
    }

    [Fact]
    public void Placeholder_variant_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var r = framework.Process(["placeholder:text-gray-200", "md:hover:placeholder:text-gray-400"]);
        r.ShouldBeCss("""

                      .placeholder\:text-gray-200::placeholder {
                        --monorail-text-opacity:1;
                        color:oklch(0.928 0.006 264.531 / var(--monorail-text-opacity));
                      }
                      @media (min-width:768px) {
                        .md\:hover\:placeholder\:text-gray-400:hover::placeholder {
                          --monorail-text-opacity:1;
                          color:oklch(0.707 0.022 261.325 / var(--monorail-text-opacity));
                        }
                      }


                      """);
    }

    [Fact]
    public void Lots_of_classes()
    {
        var classes = new[]
        {
            "dark:text-gray-300",
            "text-blue-700",
            "badge-light",
            "max-w-5xl",
            "roslyn-identifier",
            "transition-colors",
            "mt-2",
            "lg:prose-lg",
            "roslyn-string---escape-character",
            "roslyn-enum-name",
            "p-4",
            "text-white",
            "text-gray-900",
            "roslyn-class-name",
            "w-full",
            "bg-sky-700/80",
            "md:self-end",
            "prose",
            "text-sky-700/90",
            "md:w-12",
            "dark:text-sky-500/90",
            "py-12",
            "py-4",
            "dark:text-gray-200",
            "symbol",
            "dark:text-gray-100",
            "to-sky-500",
            "language-bash",
            "comment",
            "flex",
            "hover:text-gray-800",
            "roslyn-punctuation",
            "max-w-full",
            "z-50",
            "border-sky-400/50",
            "lg:py-16",
            "bg-gray-700",
            "tracking-wider",
            "lg:pb-8",
            "pl-3",
            "md:mb-2",
            "font-extrabold",
            "pb-8",
            "focus:ring-white",
            "top-0",
            "left-2",
            "dark:border-gray-700/50",
            "h-6",
            "md:grid-cols-2",
            "text-gray-400",
            "title.class",
            "min-w-0",
            "max-w-7xl",
            "lg:flex",
            "w-4",
            "language-sql",
            "roslyn-property-name",
            "text-gray-900",
            "dark:text-blue-300",
            "roslyn-local-name",
            "roslyn-keyword---control",
            "property",
            "punctuation",
            "appearance-none",
            "sm:flex",
            "text-gray-400",
            "bg-gradient-to-br",
            "xl:px-0",
            "pr-10",
            "text-gray-300",
            "prose-sm",
            "md:prose-base",
            "text-3xl",
            "roslyn-keyword",
            "md:w-1/4",
            "roslyn-parameter-name",
            "rounded-full",
            "w-10",
            "h-4",
            "dark:shadow-lg",
            "pointer-events-none",
            "placeholder-gray-500",
            "justify-center",
            "bg-gray-300/10",
            "language-powershell",
            "language-json",
            "mb-4",
            "md:py-4",
            "border-gray-700",
            "fill-current",
            "space-x-6",
            "roslyn-struct-name",
            "token",
            "sm:text-sm",
            "hover:bg-gray-100",
            "roslyn-record-class-name",
            "dark:to-sky-300",
            "dark:border-gray-50/10",
            "md:items-baseline",
            "language-shell",
            "mt-3",
            "mr-6",
            "roslyn-operator",
            "border-sky-600/80",
            "roslyn-extension-method-name",
            "px-2",
            "md:items-center",
            "md:mt-12",
            "space-x-1",
            "md:text-4xl",
            "lg:justify-between",
            "grid",
            "bg-sky-200/50",
            "md:text-sm",
            "sm:max-w-md",
            "border-gray-900/20",
            "dark:bg-sky-900/25",
            "md:rounded-md",
            "roslyn-xml-doc-comment---text",
            "text-2xl",
            "sticky",
            "roslyn-string",
            "lg:px-8",
            "font-semibold",
            "roslyn-number",
            "lg:leading-loose",
            "dark:bg-gray-700",
            "ml-auto",
            "text-gray-600",
            "text-xl",
            "md:h-12",
            "focus:ring-offset-gray-800",
            "bg-gray-50",
            "sm:max-w-xs",
            "md:py-2",
            "hover:text-white",
            "text-base",
            "items-center",
            "relative",
            "operator",
            "focus:ring-indigo-500",
            "xl:col-span-4",
            "sm:px-6",
            "border-t",
            "roslyn-xml-doc-comment---delimiter",
            "bg-white",
            "backdrop-blur",
            "language-xml",
            "lg:items-center",
            "focus:border-white",
            "dark:from-sky-500",
            "dark:border-sky-800",
            "focus:placeholder-gray-400",
            "dark:border-gray-800",
            "focus:ring-2",
            "md:px-4",
            "language-text",
            "lg:mb-8",
            "text-gray-700",
            "uppercase",
            "dark:hover:bg-gray-900",
            "space-y-4",
            "bg-none",
            "md:ml-auto",
            "pb-4",
            "mt-4",
            "md:flex-row",
            "rounded",
            "dark:prose-invert",
            "w-6",
            "border-b",
            "lg:text-4xl",
            "px-4",
            "md:space-x-2",
            "md:order-3",
            "text-center",
            "stroke-1",
            "md:gap-8",
            "sm:ml-3",
            "md:flex",
            "roslyn-comment",
            "xl:mt-0",
            "shadow-md",
            "inset-y-0",
            "xl:grid-cols-5",
            "sm:mt-0",
            "bg-gray-800",
            "text-gray-800",
            "hover:border-gray-300",
            "md:px-8",
            "xl:grid",
            "variable",
            "bg-gray-50",
            "antialiased",
            "bg-indigo-500",
            "hover:text-gray-300",
            "dark:bg-gray-800/80",
            "pt-8",
            "lg:my-4",
            "md:mt-0",
            "transition-all",
            "overflow-hidden",
            "mt-12",
            "text-sky-900/75",
            "dark:bg-gray-800",
            "sr-only",
            "text-sky-100",
            "from-sky-800",
            "text-sm",
            "border-transparent",
            "mt-6",
            "flex-col",
            "md:mb-16",
            "border-gray-200",
            "dark:text-sky-300",
            "bg-gray-100/80",
            "mb-8",
            "grid-cols-2",
            "absolute",
            "block",
            "justify-between",
            "flex-row",
            "mb-1",
            "focus:ring-offset-2",
            "md:w-3/4",
            "badge",
            "gap-8",
            "roslyn-xml-doc-comment---name",
            "font-light",
            "dark:text-sky-300/75",
            "opacity-80",
            "py-2",
            "keyword",
            "mt-8",
            "roslyn-type-parameter-name",
            "mx-auto",
            "space-x-4",
            "rounded-md",
            "border-gray-200/50",
            "lg:mt-0",
            "hover:scale-105",
            "title.function",
            "font-bold",
            "max-w-4xl",
            "break-all",
            "dark:text-yellow-300",
            "hover:bg-indigo-600",
            "md:flex-wrap",
            "mt-1.5",
            "number",
            "roslyn-string---verbatim",
            "md:order-2",
            "mb-2",
            "roslyn-method-name",
            "mr-4",
            "ml-4",
            "min-h-full",
            "string",
            "roslyn-enum-member-name",
            "font-normal",
            "border",
            "text-xs",
            "py-1",
            "md:order-1",
            "font-medium",
            "dark:text-gray-400",
            "language-c#",
            "xl:gap-8",
            "sm:flex-shrink-0",
            "md:grid",
            "focus:outline-none",
            "hover:text-blue-400",
            "md:justify-between",
            "md:text-2xl",
            "h-10",
            "roslyn-field-name",
            "lg:font-extrabold",
            "dark:text-gray-200",
            "inline-block",
            "right-0",
            "dark:bg-sky-900/75",
        };

        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(classes);
        r.ShouldNotBeEmpty();
    }
}