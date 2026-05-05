using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

public class DiscoveryTokenizationTests
{
    [Fact]
    public void SourceFileScanner_Should_Extract_DigitPrefixed_Breakpoint_Variants()
    {
        var framework = new CssFramework();
        var validationCache = new ValidationCache(framework);
        var scanner = new SourceFileScanner(validationCache);

        var path = WriteTempFile(
            ".razor",
            """<div class="sticky 2xl:w-1/4 3xl:p-4 hover:bg-red-500"></div>""");

        try
        {
            var bucket = new HashSet<string>(StringComparer.Ordinal);
            scanner.ScanFile(path, bucket);

            bucket.ShouldContain("2xl:w-1/4");
            bucket.ShouldContain("3xl:p-4");
            bucket.ShouldContain("hover:bg-red-500");
            bucket.ShouldContain("sticky");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PreFilter_Should_Accept_DigitLeading_Tokens()
    {
        var framework = new CssFramework();
        var preFilter = new PreFilter(framework);

        preFilter.IsPlausible("2xl:w-92").ShouldBeTrue();
        preFilter.IsPlausible("3xl:grid-cols-4").ShouldBeTrue();
        preFilter.IsPlausible("2xs:hidden").ShouldBeTrue();
    }

    [Fact]
    public void CssFramework_Should_Generate_Css_For_DigitPrefixed_Breakpoint_Variant()
    {
        var framework = new CssFramework();

        var css = framework.Process(["2xl:w-1/4"]);

        css.ShouldContain("min-width: 1536px");
        css.ShouldContain("calc(1/4 * 100%)");
    }

    private static string WriteTempFile(string extension, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"monorail-discovery-{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, content);
        return path;
    }
}
