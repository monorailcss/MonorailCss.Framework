using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

/// <summary>
/// Covers <c>[assembly: MonorailCssNoScan]</c> opt-out: the MonorailCss framework assembly
/// applies it to itself, so the scanner must skip it entirely rather than harvest the
/// utility-class-shaped template strings baked into its IL <c>#US</c> heap.
/// </summary>
public class MonorailCssNoScanTests
{
    [Fact]
    public void Scan_SkipsAssemblyMarkedWithMonorailCssNoScan()
    {
        var scanner = new AssemblyClassScanner(new ValidationCache(new CssFramework()));
        var output = new HashSet<string>(StringComparer.Ordinal);

        // typeof(CssFramework).Assembly is the MonorailCss assembly, which carries
        // [assembly: MonorailCssNoScan].
        var scanned = scanner.Scan(typeof(CssFramework).Assembly, output);

        scanned.ShouldBeFalse();
        output.ShouldBeEmpty();
    }
}
