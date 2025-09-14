using MonorailCss.Utilities;
using Shouldly;

namespace MonorailCss.Tests;

public class UtilityDiscoveryTests
{
    [Fact]
    public void UtilityDiscovery_Should_Find_All_Utilities()
    {
        // Act
        var utilities = UtilityDiscovery.DiscoverAllUtilities().ToList();

        // Assert - Should find all utilities implementing IUtility
        utilities.ShouldNotBeEmpty();
        utilities.Count.ShouldBeGreaterThan(5); // We have at least 5+ utilities

        // Check some expected utilities are found
        utilities.ShouldContain(u => u.GetType().Name == "DisplayUtility");
        utilities.ShouldContain(u => u.GetType().Name == "BackgroundColorUtility");
        utilities.ShouldContain(u => u.GetType().Name == "TextUtility");
        utilities.ShouldContain(u => u.GetType().Name == "PaddingUtility");
        utilities.ShouldContain(u => u.GetType().Name == "MarginUtility");
    }

    [Fact]
    public void UtilityDiscovery_Should_Order_By_Priority()
    {
        // Act
        var utilities = UtilityDiscovery.DiscoverAllUtilities().ToList();

        // Assert - Utilities should be ordered by priority
        for (var i = 1; i < utilities.Count; i++)
        {
            var current = utilities[i];
            var previous = utilities[i - 1];

            // Current priority should be >= previous priority (sorted ascending)
            ((int)current.Priority).ShouldBeGreaterThanOrEqualTo((int)previous.Priority);
        }

        // Static utilities (Priority = ExactStatic = 0) should come first
        var firstUtility = utilities.First();
        firstUtility.Priority.ShouldBe(UtilityPriority.ExactStatic);
    }

    [Fact]
    public void UtilityDiscovery_Should_Only_Find_Concrete_Classes()
    {
        // Act
        var utilities = UtilityDiscovery.DiscoverAllUtilities().ToList();

        // Assert - Should not include abstract base classes
        utilities.ShouldNotContain(u => u.GetType().Name == "BaseColorUtility");
        utilities.ShouldNotContain(u => u.GetType().Name == "BaseSpacingUtility");
        utilities.ShouldNotContain(u => u.GetType().Name == "BaseStaticUtility");

        // All discovered utilities should be concrete
        foreach (var utility in utilities)
        {
            utility.GetType().IsAbstract.ShouldBeFalse();
            utility.GetType().IsClass.ShouldBeTrue();
        }
    }
}