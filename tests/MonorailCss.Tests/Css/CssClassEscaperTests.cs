using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class CssClassEscaperTests
{
    [Theory]
    [InlineData("simple-class", "simple-class")]
    [InlineData("hover:bg-red-500", "hover\\:bg-red-500")]
    [InlineData("w-1/2", "w-1\\/2")]
    [InlineData("[&>*]:underline", "\\[\\&\\>\\*\\]\\:underline")]
    [InlineData("bg-(--my-color)", "bg-\\(--my-color\\)")]
    [InlineData("!important", "\\!important")]
    [InlineData("group/menu:hover", "group\\/menu\\:hover")]
    [InlineData("sm:md:lg:text-center", "sm\\:md\\:lg\\:text-center")]
    [InlineData("[@media(min-width:768px)]:flex", "\\[\\@media\\(min-width\\:768px\\)\\]\\:flex")]
    [InlineData("text-[#123456]", "text-\\[\\#123456\\]")]
    [InlineData("w-[calc(100%-1rem)]", "w-\\[calc\\(100\\%-1rem\\)\\]")]
    [InlineData("before:content-['']", "before\\:content-\\[\\'\\'\\]")]
    [InlineData("peer-checked:bg-blue-500", "peer-checked\\:bg-blue-500")]
    [InlineData("*:rounded", "\\*\\:rounded")]
    [InlineData("even:bg-gray-100", "even\\:bg-gray-100")]
    [InlineData("data-[state=open]:bg-white", "data-\\[state\\=open\\]\\:bg-white")]
    [InlineData("aria-[pressed=true]:bg-gray-300", "aria-\\[pressed\\=true\\]\\:bg-gray-300")]
    [InlineData("50%", "\\35 0\\%")]
    [InlineData("text-2.5xl", "text-2\\.5xl")]
    public void Escape_ShouldEscapeSpecialCharacters(string input, string expected)
    {
        var result = CssClassEscaper.Escape(input);
        result.ShouldBe(expected);
    }

    [Fact]
    public void Escape_ShouldHandleEmptyString()
    {
        CssClassEscaper.Escape("").ShouldBe("");
    }

    [Fact]
    public void Escape_ShouldHandleNull()
    {
        CssClassEscaper.Escape(null!).ShouldBe(null);
    }

    [Fact]
    public void Escape_ShouldCacheResults()
    {
        CssClassEscaper.ClearCache();

        var input = "hover:bg-red-500";
        var result1 = CssClassEscaper.Escape(input);
        var result2 = CssClassEscaper.Escape(input);

        result1.ShouldBe(result2);
        ReferenceEquals(result1, result2).ShouldBeTrue();
    }

    [Theory]
    [InlineData("has-[:checked]:bg-blue-600", "has-\\[\\:checked\\]\\:bg-blue-600")]
    [InlineData("group-has-[:focus]:ring-2", "group-has-\\[\\:focus\\]\\:ring-2")]
    [InlineData("[@supports(display:grid)]:grid", "\\[\\@supports\\(display\\:grid\\)\\]\\:grid")]
    [InlineData("[@container(min-width:768px)]:flex", "\\[\\@container\\(min-width\\:768px\\)\\]\\:flex")]
    public void Escape_ShouldHandleComplexSelectors(string input, string expected)
    {
        var result = CssClassEscaper.Escape(input);
        result.ShouldBe(expected);
    }

    [Fact]
    public void ClearCache_ShouldClearCache()
    {
        var input = "test:class";
        var result1 = CssClassEscaper.Escape(input);

        CssClassEscaper.ClearCache();

        var result2 = CssClassEscaper.Escape(input);

        result1.ShouldBe(result2);
        ReferenceEquals(result1, result2).ShouldBeFalse();
    }

    [Theory]
    [InlineData("2xl:bg-red-500", "\\32 xl\\:bg-red-500")]
    [InlineData("3xl:text-center", "\\33 xl\\:text-center")]
    [InlineData("4xl:p-4", "\\34 xl\\:p-4")]
    [InlineData("5xl:flex", "\\35 xl\\:flex")]
    [InlineData("6xl:grid", "\\36 xl\\:grid")]
    [InlineData("7xl:hidden", "\\37 xl\\:hidden")]
    [InlineData("8xl:block", "\\38 xl\\:block")]
    [InlineData("9xl:inline", "\\39 xl\\:inline")]
    [InlineData("2xl:hover:bg-blue-600", "\\32 xl\\:hover\\:bg-blue-600")]
    [InlineData("0:margin-0", "\\30 \\:margin-0")]
    [InlineData("1/2", "\\31 \\/2")]
    [InlineData("100%", "\\31 00\\%")]
    public void Escape_ShouldEscapeLeadingDigits(string input, string expected)
    {
        var result = CssClassEscaper.Escape(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("-2xl:margin", "-\\32 xl\\:margin")]
    [InlineData("-3xl:p-4", "-\\33 xl\\:p-4")]
    [InlineData("-1/2", "-\\31 \\/2")]
    [InlineData("-100px", "-\\31 00px")]
    [InlineData("-0:margin", "-\\30 \\:margin")]
    public void Escape_ShouldEscapeHyphenFollowedByDigit(string input, string expected)
    {
        var result = CssClassEscaper.Escape(input);
        result.ShouldBe(expected);
    }
}