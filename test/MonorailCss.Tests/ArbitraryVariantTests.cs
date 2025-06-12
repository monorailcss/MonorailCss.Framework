using MonorailCss.Tests.Plugins;
using Shouldly;

namespace MonorailCss.Tests;

public class ArbitraryVariantTests
{
    [Fact]
    public void Data_arbitrary_variant_with_value_should_generate_correct_css()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "data-[mobile-menu-open=true]:bg-red-500",
        ]);
        
        r.ShouldBeCss("""
                      .data-\[mobile-menu-open\=true\]\:bg-red-500[data-mobile-menu-open="true"] {
                        --monorail-bg-opacity:1;
                        background-color:oklch(0.637 0.237 25.331 / var(--monorail-bg-opacity));
                      }
                      """);
    }

    [Fact]
    public void Data_arbitrary_variant_without_value_should_generate_correct_css()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "data-[loading]:opacity-50",
        ]);
        
        r.ShouldBeCss("""
                      .data-\[loading\]\:opacity-50[data-loading] {
                        opacity:0.5;
                      }
                      """);
    }

    [Fact]
    public void Aria_arbitrary_variant_with_value_should_generate_correct_css()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "aria-[expanded=true]:rotate-180",
        ]);
        
        r.ShouldBeCss("""
                      :root {
                        --monorail-rotate:0;
                        --monorail-scale-x:1;
                        --monorail-scale-y:1;
                        --monorail-skew-x:0;
                        --monorail-skew-y:0;
                        --monorail-spacing:0.25rem;
                        --monorail-translate-x:0;
                        --monorail-translate-y:0;
                      }
                      .aria-\[expanded\=true\]\:rotate-180[aria-expanded="true"] {
                        --monorail-rotate:180deg;
                        transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
                      }
                      """);
    }

    [Fact]
    public void Aria_arbitrary_variant_without_value_should_generate_correct_css()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "aria-[disabled]:text-gray-400",
        ]);
        
        r.ShouldBeCss("""
                      .aria-\[disabled\]\:text-gray-400[aria-disabled] {
                        --monorail-text-opacity:1;
                        color:oklch(0.707 0.022 261.325 / var(--monorail-text-opacity));
                      }
                      """);
    }

    [Fact]
    public void Multiple_data_and_aria_variants_should_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "data-[state=open]:block",
            "aria-[selected=true]:bg-blue-100",
            "data-[loading]:animate-spin",
        ]);
        
        r.ShouldBeCss("""
                      .data-\[state\=open\]\:block[data-state="open"] {
                        display:block;
                      }
                      .aria-\[selected\=true\]\:bg-blue-100[aria-selected="true"] {
                        --monorail-bg-opacity:1;
                        background-color:oklch(0.932 0.032 255.585 / var(--monorail-bg-opacity));
                      }
                      @keyframes spin {
                        to {
                          transform:rotate(360deg);
                        }
                      }
                      .data-\[loading\]\:animate-spin[data-loading] {
                        animation:spin 1s linear infinite;
                      }
                      """);
    }

    [Fact]
    public void Data_and_aria_variants_with_responsive_modifiers_should_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "md:data-[state=closed]:hidden",
            "lg:aria-[expanded=false]:text-sm",
        ]);
        
        r.ShouldBeCss("""
                      @media (min-width:768px) {
                        .md\:data-\[state\=closed\]\:hidden[data-state="closed"] {
                          display:none;
                        }
                      }
                      @media (min-width:1024px) {
                        .lg\:aria-\[expanded\=false\]\:text-sm[aria-expanded="false"] {
                          font-size:0.875rem;
                          line-height:1.25rem;
                        }
                      }
                      """);
    }

    [Fact]
    public void Data_and_aria_variants_with_pseudo_modifiers_should_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "hover:data-[state=active]:bg-green-500",
            "focus:aria-[invalid=true]:border-red-500",
        ]);
        
        r.ShouldBeCss("""
                      .hover\:data-\[state\=active\]\:bg-green-500:hover[data-state="active"] {
                        --monorail-bg-opacity:1;
                        background-color:oklch(0.723 0.219 149.579 / var(--monorail-bg-opacity));
                      }
                      .focus\:aria-\[invalid\=true\]\:border-red-500:focus[aria-invalid="true"] {
                        border-color:oklch(0.637 0.237 25.331);
                      }
                      """);
    }
}