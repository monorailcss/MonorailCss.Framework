﻿using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests;

public class VariantNameTests
{
    public static IEnumerable<object[]> Data()
    {
        yield return new object[]
        {
            "dark:hover:bg-blue-100",
            new IVariant[] { new SelectorVariant(".dark"), new PseudoClassVariant(":hover") },
            ".dark .dark\\:hover\\:bg-blue-100:hover"
        };
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void Extract(string original, IVariant[] variants, string expected)
    {
        ClassHelper.GetSelectorSyntax(original, variants).ShouldBe(expected);
    }
}

public class ExtractorTests
{
    [Theory]
    [InlineData("invisible", new string[0], "invisible", "", ':')]
    [InlineData("dark:sm:invisible", new[] { "dark", "sm" }, "invisible", "", ':')]
    [InlineData("line-through", new string[0], "line-through", "", ':')]
    [InlineData("dark:sm:line-through", new[] { "dark", "sm" }, "line-through", "", ':')]
    [InlineData("dark_sm_mono-line-through", new[] { "dark", "sm" }, "line-through", "mono-", '_')]
    [InlineData("prose-headings:underline", new[] { "prose-headings"}, "underline", "", ':')]
    public void Can_extract_utilities(string className, string[] variants, string utility, string prefix,
        char separator)
    {
        var r = ClassHelper.Extract(className, new []{ "bg", "text", "flex" }, prefix, separator) as UtilitySyntax;
        r.ShouldNotBeNull();
        r.ShouldSatisfyAllConditions(
            i => i.Modifiers.ShouldBe(variants, ignoreOrder:true),
            i => i.Name.ShouldBe(utility)
        );
    }

    [Theory]
    [InlineData("ml-2", new string[0], "ml", "2", "", ':')]
    [InlineData("max-w-prose", new string[0], "max-w","prose",  "", ':')]
    [InlineData("bg-blue-100", new string[0], "bg", "blue-100",  "", ':')]
    [InlineData("flex",  new string[0], "flex", null,  "", ':')]
    [InlineData("flex-none",  new string[0], "flex", "none",  "", ':')]
    [InlineData("md:bg-blue-100", new[] {"md"}, "bg", "blue-100", "", ':')]
    [InlineData("dark:md:bg-blue-100", new [] {"dark", "md"}, "bg", "blue-100","", ':')]
    [InlineData("dark_md_bg-blue-100", new [] {"dark", "md"}, "bg", "blue-100", "", '_')]
    [InlineData("dark_md_mono-bg-blue-100", new [] {"dark", "md"}, "bg", "blue-100", "mono-", '_')]
    [InlineData("bg-blue-100/50", new string[0], "bg", "blue-100/50", "", ':')]
    [InlineData("dark:text-blue-100/50", new[] {"dark"}, "text", "blue-100/50", "", ':')]
    [InlineData("mono-bg-blue-100", new string[0], "bg", "blue-100", "mono-", ':')]
    [InlineData("mono-bg-blue-100", new string[0], "bg", "blue-100", "mono-", '_')]
    [InlineData("border-b", new string[0], "border-b", null, "", ':')]
    public void Can_extract_namespace_utilities(string className, string[] variants, string ns, string suffix, string prefix, char separator)
    {
        var r = ClassHelper.Extract(className, new[] { "bg", "text", "border", "border-b", "flex", "m", "ml", "mx", "max-w" }, prefix, separator) as NamespaceSyntax;
        r.ShouldNotBeNull();
        r.ShouldSatisfyAllConditions(
            i => i.Modifiers.ShouldBe(variants, ignoreOrder:true),
            i => i.Namespace.ShouldBe(ns),
            i => i.Suffix.ShouldBe(suffix)
        );
    }

    [Theory]
    [InlineData("bg-[#ccc]", new string[0], "bg", "#ccc", "", ':')]
    [InlineData("w-[300px]", new string[0], "w", "300px", "", ':')]
    [InlineData("md:bg-[#ccc]", new[] {"md"}, "bg", "#ccc", "", ':')]
    [InlineData("dark:md:bg-[#ccc]", new [] {"dark", "md"}, "bg", "#ccc", "", ':')]
    [InlineData("dark_md_bg-[#ccc]", new [] {"dark", "md"}, "bg", "#ccc", "", '_')]
    [InlineData("bg-[#ccc/50]", new string[0], "bg", "#ccc/50", "", ':')]
    [InlineData("mono-bg-[#ccc]", new string[0], "bg", "#ccc", "mono-", ':')]
    [InlineData("mono-bg-[#ccc]", new string[0], "bg", "#ccc", "mono-", '_')]
    public void Can_extract_arbitrary_utilities(string className, string[]? variants, string ns, string arbitraryValue, string prefix, char separator)
    {
        var r = ClassHelper.Extract(className, new[] { "bg", "w" }, prefix, separator) as ArbitraryValueSyntax;
        r.ShouldNotBeNull();
        r.ShouldSatisfyAllConditions(
            i => i.Modifiers.ShouldBe(variants, ignoreOrder:true),
            i => i.Namespace.ShouldBe(ns),
            i => i.ArbitraryValue.ShouldBe(arbitraryValue)
        );
    }

    [Theory]
    [InlineData("[my-item:12px]", new string[0], "my-item", "12px", "", ':')]
    [InlineData("dark:md:[my-item:12px]", new [] {"dark", "md"}, "my-item", "12px", "", ':')]
    [InlineData("dark_md_[my-item:12px]", new [] {"dark", "md"}, "my-item", "12px", "", '_')]
    [InlineData("dark_md_mono-[my-item:12px]", new [] {"dark", "md"}, "my-item", "12px", "mono-", '_')]
    [InlineData("dark:md:mono-[my-item:12px]", new [] {"dark", "md"}, "my-item", "12px", "mono-", ':')]
    public void Can_extract_arbitrary_values(string className, string[]? variants, string property, string value, string prefix, char separator)
    {
        var r = ClassHelper.Extract(className, new[] { "bg" }, prefix, separator) as ArbitraryPropertySyntax;
        r.ShouldNotBeNull();
        r.ShouldSatisfyAllConditions(
            i => i.Modifiers.ShouldBe(variants, ignoreOrder:true),
            i => i.ArbitraryValue.ShouldBe(value),
            i => i.PropertyName.ShouldBe(property)
        );
    }

    [Theory]
    [InlineData("bg-")]
    [InlineData("bg-[#ccc")]
    [InlineData("sm:")]
    [InlineData("[prop:")]
    [InlineData("[prop:]")]
    [InlineData("[:value]")]
    public void Return_null_for_invalid_classnames(string className)
    {
        var r = ClassHelper.Extract(className, new []{ "bg" }, string.Empty, ':');
        r.ShouldBeNull();
    }
}