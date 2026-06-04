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
    public void SourceFileScanner_Should_Extract_Classes_From_JavaScript()
    {
        // A .js file falls through to the generic CandidateLexer strategy, so utility classes
        // survive regardless of how the script applies them — className assignment, classList
        // calls, or a bare space-delimited string literal (the shape Pennington.UI's
        // scripts.js uses to build its modal: 'w-3 h-3').
        var framework = new CssFramework();
        var scanner = new SourceFileScanner(new ValidationCache(framework));

        var path = WriteTempFile(
            ".js",
            """
            export function modal() {
              const el = document.createElement('div');
              el.className = 'fixed inset-0 flex items-center justify-center';
              const icon = document.createElement('span');
              icon.classList.add('w-3', 'h-3');
              el.innerHTML = `<button class="rounded-md px-4 py-2">close</button>`;
              return el;
            }
            """);

        try
        {
            var bucket = new HashSet<string>(StringComparer.Ordinal);
            scanner.ScanFile(path, bucket);

            bucket.ShouldContain("w-3");
            bucket.ShouldContain("h-3");
            bucket.ShouldContain("fixed");
            bucket.ShouldContain("inset-0");
            bucket.ShouldContain("items-center");
            bucket.ShouldContain("rounded-md");
            bucket.ShouldContain("px-4");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void CandidateLexer_Should_Emit_DigitLeading_Tokens()
    {
        var tokens = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in CandidateLexer.Tokenize("foo 2xl:w-92 3xl:grid-cols-4 2xs:hidden bar"))
        {
            tokens.Add(token);
        }

        tokens.ShouldContain("2xl:w-92");
        tokens.ShouldContain("3xl:grid-cols-4");
        tokens.ShouldContain("2xs:hidden");
    }

    [Fact]
    public void CssFramework_Should_Generate_Css_For_DigitPrefixed_Breakpoint_Variant()
    {
        var framework = new CssFramework();

        var css = framework.Process(["2xl:w-1/4"]);

        css.ShouldContain("min-width: 1536px");
        css.ShouldContain("calc(1/4 * 100%)");
    }

    [Fact]
    public void PreFilter_Should_Not_Drop_Any_Token_The_Parser_Accepts()
    {
        var framework = new CssFramework();
        var cache = new ValidationCache(framework);

        var corpus = string.Join(
            ' ',
            "flex block hidden grid underline italic uppercase",            // bare static utils
            "bg-red-500 hover:text-white p-4 -mt-2 w-1/2 sm:grid-cols-3",    // structured
            "bg-[#abcdef] opacity-[0.5] p-0.5 text-blue-700/25 !font-bold",  // arbitrary/modifier/important
            "the quick brown fox jumps lazily over content section render"); // prose noise

        var output = new HashSet<string>(StringComparer.Ordinal);
        cache.CollectValid(corpus, output);

        // The pre-filter must be conservative: every lexer token the parser accepts survives it.
        foreach (var token in CandidateLexer.Tokenize(corpus))
        {
            if (framework.TryValidateCandidate(token))
            {
                output.ShouldContain(token);
            }
        }

        // And the natural-language prose is dropped. Each word is first confirmed to be a
        // non-utility so the assertion can't be invalidated by a word that is secretly valid.
        foreach (var word in new[] { "quick", "brown", "jumps", "lazily" })
        {
            framework.TryValidateCandidate(word).ShouldBeFalse($"'{word}' should not be a utility");
            output.ShouldNotContain(word);
        }
    }

    [Fact]
    public void PreFilter_Should_Keep_Every_Bare_Static_Utility()
    {
        var framework = new CssFramework();
        var cache = new ValidationCache(framework);

        var staticNames = framework.GetStaticUtilityNames();
        var output = new HashSet<string>(StringComparer.Ordinal);
        cache.CollectValid(string.Join(' ', staticNames), output);

        foreach (var name in staticNames)
        {
            // The lexer's length window is 2..96; names below it are out of scope here.
            if (name.Length >= 2)
            {
                output.ShouldContain(name);
            }
        }
    }

    private static string WriteTempFile(string extension, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"monorail-discovery-{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, content);
        return path;
    }
}
