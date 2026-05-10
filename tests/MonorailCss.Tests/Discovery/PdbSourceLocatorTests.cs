using System.Reflection;
using System.Reflection.Emit;
using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

public class PdbSourceLocatorTests
{
    [Fact]
    public void TryGetSourceRoot_Resolves_To_Local_Source_For_Assembly_With_Reachable_Pdb()
    {
        var asm = typeof(PdbSourceLocatorTests).Assembly;

        var ok = PdbSourceLocator.TryGetSourceRoot(asm, out var root);

        ok.ShouldBeTrue();
        Directory.Exists(root).ShouldBeTrue();

        // The resolved root must contain at least this very source file. We don't pin to an
        // exact path — checkout roots vary across machines — but the locator must be able to
        // reach the test sources from whatever it returned.
        var thisFile = Directory.EnumerateFiles(root, "PdbSourceLocatorTests.cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        thisFile.ShouldNotBeNull();
    }

    [Fact]
    public void TryGetSourceRoot_Returns_False_For_Dynamic_Assembly()
    {
        var dynamicAsm = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("MonorailCss.Tests.DynamicProbe"),
            AssemblyBuilderAccess.Run);

        var ok = PdbSourceLocator.TryGetSourceRoot(dynamicAsm, out var root);

        ok.ShouldBeFalse();
        root.ShouldBeEmpty();
    }
}
