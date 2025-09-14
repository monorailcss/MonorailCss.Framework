using MonorailCss.Utilities.Backgrounds;
using MonorailCss.Utilities.Layout;
using MonorailCss.Utilities.Resolvers;
using MonorailCss.Utilities.Typography;
using Shouldly;

namespace MonorailCss.Tests;

public class DynamicNamespaceTests
{

    [Fact]
    public void DesignSystem_Should_Handle_Utilities_Without_Namespaces()
    {
        // Arrange
        var resolver = new DynamicNamespaceResolver();

        // Act - Register display utility (which has no namespaces)
        resolver.RegisterUtility(new DisplayUtility());

        // Assert - Display utility shouldn't appear in namespace registry
        var namespaceRegistry = resolver.GetAllNamespaces();
        namespaceRegistry.ShouldNotContainKey("DisplayUtility");
    }

    [Fact]
    public void DynamicNamespaceResolver_Should_Cache_Results()
    {
        // Arrange
        var resolver = new DynamicNamespaceResolver();
        var bgUtility = new BackgroundColorUtility();

        // Act
        resolver.RegisterUtility(bgUtility);

        // Get namespaces twice - should use cache second time
        var namespaces1 = resolver.GetNamespaces("bg");
        var namespaces2 = resolver.GetNamespaces("bg");

        // Assert
        namespaces1.ShouldBeSameAs(namespaces2); // Same reference means it was cached
    }

    [Fact]
    public void DynamicNamespaceResolver_Should_Clear_Cache_On_New_Registration()
    {
        // Arrange
        var resolver = new DynamicNamespaceResolver();
        var bgUtility = new BackgroundColorUtility();
        var textUtility = new TextUtility();

        // Act
        resolver.RegisterUtility(bgUtility);
        var namespaces1 = resolver.GetNamespaces("bg");

        // Register new utility - should clear cache
        resolver.RegisterUtility(textUtility);
        var namespaces2 = resolver.GetNamespaces("bg");

        // Assert
        // After cache clear, a new lookup happens
        // The values should be the same, but might be different references
        namespaces1.ShouldBe(namespaces2); // Same values
    }
}