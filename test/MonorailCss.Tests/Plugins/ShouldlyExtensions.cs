using System.Reflection;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using JetBrains.Annotations;
using Shouldly;

namespace MonorailCss.Tests.Plugins;

[ShouldlyMethods]
public static class ShouldlyExtensions
{
    public static void ShouldContainElementWithCssProperty(this string value, string element, string property,
        string propertyValue)
    {
        var parser = new CssParser(new CssParserOptions()
        {
            IsIncludingUnknownDeclarations =  true,
            IsToleratingInvalidSelectors = true,
            IsIncludingUnknownRules = true,
        });
        var styleSheet = parser.ParseStyleSheet(value);
        var styleRules = styleSheet.Rules.OfType<ICssStyleRule>().ToArray();
        if (styleRules.Length == 0)
        {
            return;
        }

        styleRules.ShouldContain(i => i.SelectorText.Equals(element));
        styleRules
            .First(i => i.SelectorText == element)
            .Style.ShouldSatisfyAllConditions(
                i => i.ShouldContain(prop => prop.Name == property),
                i => i.ShouldContain(prop => prop.Name == property && prop.Value == propertyValue)
            );
    }

    public static void ShouldBeCss(this string value, [LanguageInjection(InjectedLanguage.CSS)] string expected)
    {
        var parser = new CssParser(new CssParserOptions
        {
            IsIncludingUnknownDeclarations = true,
            IsToleratingInvalidSelectors = true,
            IsIncludingUnknownRules = true
        });

        var originalSheet = parser.ParseStyleSheet(value);
        originalSheet.ShouldNotBeNull("Could not parse original stylesheet.");

        var expectedSheet = parser.ParseStyleSheet(expected);
        expectedSheet.ShouldNotBeNull("Could not parse expected stylesheet.");

        try
        {
            // First check basic CSS rules
            RulesShouldBeEqual(originalSheet.Rules, expectedSheet.Rules);

            // Then check media queries
            var originalMediaRules = originalSheet.Rules.OfType<ICssMediaRule>().ToArray();
            var expectedMediaRules = expectedSheet.Rules.OfType<ICssMediaRule>().ToArray();

            originalMediaRules.Select(i => i.ConditionText).ShouldBe(
                expectedMediaRules.Select(i => i.ConditionText),
                customMessage: "Media query conditions don't match");

            foreach (var originalMediaRule in originalMediaRules)
            {
                var expectedMediaRule = expectedMediaRules.FirstOrDefault(i => i.ConditionText == originalMediaRule.ConditionText);
                expectedMediaRule.ShouldNotBeNull($"Expected media rule with condition '{originalMediaRule.ConditionText}' was not found");
                RulesShouldBeEqual(originalMediaRule.Rules, expectedMediaRule.Rules);
            }
        }
        catch (Exception ex)
        {
            throw new ShouldAssertException(
                $"CSS does not match expected value.\n\nActual:\n{value.Trim()}\n\nExpected:\n{expected.Trim()}\n\nError: {ex.Message}");
        }
    }

    private static void RulesShouldBeEqual(ICssRuleList originalRuleList, ICssRuleList expectedRuleList)
    {
        // Process different rule types separately
        RulesShouldBeEqual(originalRuleList.OfType<ICssStyleRule>().ToArray(), expectedRuleList.OfType<ICssStyleRule>().ToArray());
        RulesShouldBeEqual(originalRuleList.OfType<ICssKeyframesRule>().ToArray(), expectedRuleList.OfType<ICssKeyframesRule>().ToArray());

        // Add support for other rule types as needed...
        var otherOriginalRules = originalRuleList.Where(r => !(r is ICssStyleRule || r is ICssKeyframesRule || r is ICssMediaRule)).ToArray();
        var otherExpectedRules = expectedRuleList.Where(r => !(r is ICssStyleRule || r is ICssKeyframesRule || r is ICssMediaRule)).ToArray();

        if (otherOriginalRules.Length > 0 || otherExpectedRules.Length > 0)
        {
            // If we have any other rule types, just check that the counts match
            otherOriginalRules.Length.ShouldBe(otherExpectedRules.Length,
                "Different number of unsupported rule types between original and expected CSS");
        }
    }

    private static void RulesShouldBeEqual(ICssKeyframesRule[] originalRules, ICssKeyframesRule[] expectedRules)
    {
        var originalSelectors = originalRules.Select(i => i.Name).ToArray();
        var expectedSelectors = expectedRules.Select(i => i.Name).ToArray();

        originalSelectors.ShouldBe(expectedSelectors,
            customMessage: "Missing keyframe in style sheet",
            ignoreOrder: true);

        // for each rule in the original sheet, there should be a rule in the expected sheet with the same properties
        foreach (var originalRule in originalRules)
        {
            var expectedRule = expectedRules.FirstOrDefault(i => i.Name == originalRule.Name);
            expectedRule.ShouldNotBeNull($"Expected keyframe rule '{originalRule.Name}' not found");
            RulesShouldBeEqual(originalRule.Rules.OfType<ICssKeyframeRule>().ToArray(), expectedRule.Rules.OfType<ICssKeyframeRule>().ToArray());
        }
    }

    private static void RulesShouldBeEqual(ICssKeyframeRule[] originalRules, ICssKeyframeRule[] expectedRules)
    {
        // Verify keyframe rules match by key text (e.g., "from", "to", "50%")
        var originalKeyTexts = originalRules.Select(i => i.KeyText).OrderBy(t => t).ToArray();
        var expectedKeyTexts = expectedRules.Select(i => i.KeyText).OrderBy(t => t).ToArray();

        originalKeyTexts.ShouldBe(expectedKeyTexts,
            customMessage: "Keyframe rule entry points don't match",
            ignoreOrder: false); // Order is important for keyframes

        // Compare properties for each keyframe point
        foreach (var originalRule in originalRules)
        {
            var expectedRule = expectedRules.FirstOrDefault(i => i.KeyText == originalRule.KeyText);
            expectedRule.ShouldNotBeNull($"Expected keyframe point '{originalRule.KeyText}' not found");

            var originalPropertiesAndValues = GetNormalizedProperties(originalRule.Style);
            var expectedPropertiesAndValues = GetNormalizedProperties(expectedRule.Style);

            originalPropertiesAndValues.ShouldBe(expectedPropertiesAndValues,
                customMessage: $"Keyframe '{originalRule.KeyText}' properties don't match",
                ignoreOrder: true);
        }
    }

    private static void RulesShouldBeEqual(ICssStyleRule[] originalRules, ICssStyleRule[] expectedRules)
    {
        var originalSelectors = originalRules.Select(NormalizeSelector).ToArray();
        var expectedSelectors = expectedRules.Select(NormalizeSelector).ToArray();

        try
        {
            originalSelectors.ShouldBe(expectedSelectors,
                customMessage: "Missing selectors in style sheet",
                ignoreOrder: true);
        }
        catch (Exception)
        {
            // More detailed error message for selector mismatch
            var missingInExpected = originalSelectors.Except(expectedSelectors).ToArray();
            var missingInActual = expectedSelectors.Except(originalSelectors).ToArray();

            if (missingInExpected.Any())
                throw new ShouldAssertException($"Selectors in actual but missing from expected: {string.Join(", ", missingInExpected)}");
            if (missingInActual.Any())
                throw new ShouldAssertException($"Selectors in expected but missing from actual: {string.Join(", ", missingInActual)}");

            throw; // If we can't determine a more specific error, rethrow the original
        }

        foreach (var originalRule in originalRules)
        {
            // Find the matching rule with the same normalized selector
            string normalizedOriginalSelector = NormalizeSelector(originalRule);
            var matchingRules = expectedRules.Where(r => NormalizeSelector(r) == normalizedOriginalSelector).ToArray();

            matchingRules.Length.ShouldNotBe(0,
                $"No matching rule found for selector '{originalRule.SelectorText}' (normalized: '{normalizedOriginalSelector}')");

            // Only compare to the first matching rule for simplicity
            var expectedRule = matchingRules[0];

            try
            {
                var originalPropertiesAndValues = GetNormalizedProperties(originalRule.Style);
                var expectedPropertiesAndValues = GetNormalizedProperties(expectedRule.Style);

                originalPropertiesAndValues.ShouldBe(expectedPropertiesAndValues,
                    customMessage: $"{originalRule.Friendly()} rules don't match.",
                    ignoreOrder: true);
            }
            catch (Exception ex)
            {
                // Provide more detailed error for property mismatches
                var originalProps = GetNormalizedProperties(originalRule.Style);
                var expectedProps = GetNormalizedProperties(expectedRule.Style);

                var missingInExpected = originalProps.Except(expectedProps).ToArray();
                var missingInActual = expectedProps.Except(originalProps).ToArray();

                var errorMsg = $"CSS property mismatch for selector '{originalRule.SelectorText}':\n";

                if (missingInExpected.Any())
                    errorMsg += $"Properties in actual but missing from expected: {string.Join(", ", missingInExpected.Select(p => $"{p.Name}: {p.Value}"))}\n";
                if (missingInActual.Any())
                    errorMsg += $"Properties in expected but missing from actual: {string.Join(", ", missingInActual.Select(p => $"{p.Name}: {p.Value}"))}\n";

                throw new ShouldAssertException(errorMsg, ex);
            }
        }
    }

    private static string NormalizeSelector(ICssStyleRule rule)
    {
        // Normalize whitespace, handle nested CSS, and normalize casing
        return rule.SelectorText
            .Replace(" ", "")
            .Replace("\t", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .ToLowerInvariant();
    }

    private static (string Name, string Value)[] GetNormalizedProperties(ICssStyleDeclaration style)
    {
        return style.Select(i => (
            Name: i.Name.Trim().ToLowerInvariant(),
            Value: i.Value.Trim().Replace(" ", "").ToLowerInvariant()
        )).OrderBy(i => i.Name).ToArray();
    }

    private static string Friendly(this ICssStyleRule rule) => rule.Parent is not ICssMediaRule mediaRule
        ? rule.SelectorText
        : $"@media {mediaRule.ConditionText} {{ {rule.SelectorText}...";
}