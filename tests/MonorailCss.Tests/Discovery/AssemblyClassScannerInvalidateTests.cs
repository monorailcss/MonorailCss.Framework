using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

public class AssemblyClassScannerInvalidateTests
{
    [Fact]
    public void Invalidate_Forces_Next_Scan_To_Miss_Cache()
    {
        var framework = new CssFramework();
        var validationCache = new ValidationCache(framework);
        var scanner = new AssemblyClassScanner(validationCache);

        // Pick any non-BCL assembly that has IL metadata; the test project itself is fine.
        var asm = typeof(AssemblyClassScannerInvalidateTests).Assembly;

        var first = new HashSet<string>(StringComparer.Ordinal);
        scanner.Scan(asm, first).ShouldBeTrue();
        scanner.MvidCacheMisses.ShouldBe(1);
        scanner.MvidCacheHits.ShouldBe(0);

        var second = new HashSet<string>(StringComparer.Ordinal);
        scanner.Scan(asm, second).ShouldBeTrue();
        scanner.MvidCacheMisses.ShouldBe(1);
        scanner.MvidCacheHits.ShouldBe(1);

        scanner.Invalidate(asm.ManifestModule.ModuleVersionId);

        var third = new HashSet<string>(StringComparer.Ordinal);
        scanner.Scan(asm, third).ShouldBeTrue();
        scanner.MvidCacheMisses.ShouldBe(2);
        scanner.MvidCacheHits.ShouldBe(1);
    }
}
