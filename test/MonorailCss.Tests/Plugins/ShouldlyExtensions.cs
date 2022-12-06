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
        var parser = new CssParser();
        var styleSheet = parser.ParseStyleSheet(value);
        styleSheet.Rules.OfType<ICssStyleRule>().ShouldContain(i => i.SelectorText.Equals(element));
        styleSheet.Rules.OfType<ICssStyleRule>()
            .First(i => i.SelectorText == element)
            .Style.ShouldSatisfyAllConditions(
                i => i.ShouldContain(prop => prop.Name == property),
                i => i.ShouldContain(prop => prop.Name == property && prop.Value == propertyValue)
            );
    }

    public static void ShouldBeCss(this string value, [LanguageInjection(InjectedLanguage.CSS)] string expected)
    {
        var parser = new CssParser();
        var originalSheet = parser.ParseStyleSheet(value);
        var expectedSheet = parser.ParseStyleSheet(expected);

        RulesShouldBeEqual(originalSheet.Rules, expectedSheet.Rules);

        var originalMediaRules = originalSheet.Rules.OfType<ICssMediaRule>().ToArray();
        var expectedMediaRules = expectedSheet.Rules.OfType<ICssMediaRule>().ToArray();

        originalMediaRules.Select(i => i.ConditionText).ShouldBe(expectedMediaRules.Select(i => i.ConditionText));

        foreach (var originalMediaRule in originalMediaRules)
        {
            var expectedMediaRule = expectedMediaRules.First(i => i.ConditionText == originalMediaRule.ConditionText);
            RulesShouldBeEqual(originalMediaRule.Rules, expectedMediaRule.Rules);
        }
    }

    private static void RulesShouldBeEqual(ICssRuleList originalRuleList, ICssRuleList expectedRuleList)
    {
        RulesShouldBeEqual(originalRuleList.OfType<ICssStyleRule>().ToArray(), expectedRuleList.OfType<ICssStyleRule>().ToArray());
        RulesShouldBeEqual(originalRuleList.OfType<ICssKeyframesRule>().ToArray(), expectedRuleList.OfType<ICssKeyframesRule>().ToArray());
    }

    private static void RulesShouldBeEqual(ICssKeyframesRule[] originalRules, ICssKeyframesRule[] expectedRules)
    {
        var originalSelectors = originalRules.Select(i => i.Name).ToArray();
        var expectedSelectors = expectedRules.Select(i => i.Name).ToArray();

        originalSelectors.ShouldBe(expectedSelectors, customMessage: "Missing keyframe in style sheet",
            ignoreOrder: true);

        // for each rule in the original sheet, there should be a rule in the expected sheet with the same properties
        foreach (var originalRule in originalRules)
        {
            var expectedRule = expectedRules.First(i => i.Name == originalRule.Name);
            RulesShouldBeEqual(originalRule.Rules.OfType<ICssKeyframeRule>().ToArray(), expectedRule.Rules.OfType<ICssKeyframeRule>().ToArray());
        }
    }

    private static void RulesShouldBeEqual(ICssKeyframeRule[] originalRules, ICssKeyframeRule[] expectedRules)
    {
        // foreach property in original rules there should be a matching property in the expected rules
        foreach (var originalRule in originalRules)
        {
            var expectedRule = expectedRules.First(i => i.KeyText == originalRule.KeyText);

            var originalPropertiesAndValues = originalRule.Style.Select(i => (i.Name, i.Value)).OrderBy(i => i.Name).ToArray();
            var expectedPropertiesAndValues = expectedRule.Style.Select(i => (i.Name, i.Value)).OrderBy(i => i.Name).ToArray();
            originalPropertiesAndValues.ShouldBe(expectedPropertiesAndValues, ignoreOrder: true);
        }
    }

    private static void RulesShouldBeEqual(ICssStyleRule[] originalRules, ICssStyleRule[] expectedRules)
    {
        var originalSelectors = originalRules.Select(i => i.SelectorText);
        var expectedSelectors = expectedRules.Select(i => i.SelectorText);

        originalSelectors.ShouldBe(expectedSelectors, customMessage: "Missing selectors in style sheet",
            ignoreOrder: true);

        foreach (var originalRule in originalRules)
        {
            var expectedRule = expectedRules.First(i => i.SelectorText == originalRule.SelectorText);
            var originalPropertiesAndValues =
                originalRule.Style.Select(i => (i.Name, i.Value)).OrderBy(i => i.Name).ToArray();
            var expectedPropertiesAndValues =
                expectedRule.Style.Select(i => (i.Name, i.Value)).OrderBy(i => i.Name).ToArray();
            originalPropertiesAndValues.ShouldBe(expectedPropertiesAndValues,
                customMessage: $"{originalRule.Friendly()} rules mismatch.", ignoreOrder: true);
        }
    }

    private static string Friendly(this ICssStyleRule rule) => rule.Parent is not ICssMediaRule mediaRule
        ? rule.SelectorText
        : $"@media {mediaRule.ConditionText} {{ {rule.SelectorText}...";
}