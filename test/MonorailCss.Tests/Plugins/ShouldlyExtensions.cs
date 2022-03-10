using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using JetBrains.Annotations;
using Shouldly;

namespace MonorailCss.Tests.Plugins;

[ShouldlyMethods]
public static class ShouldlyExtensions
{
    public static void ShouldBeCss(this string value,[LanguageInjection(InjectedLanguage.CSS)] string expected)
    {
        var parser = new CssParser();
        var originalSheet = parser.ParseStyleSheet(value);
        var expectedSheet = parser.ParseStyleSheet(expected);

        RulesShouldBeEqual(
            originalSheet.Rules.OfType<ICssStyleRule>().ToArray(),
            expectedSheet.Rules.OfType<ICssStyleRule>().ToArray());

        var originalMediaRules = originalSheet.Rules.OfType<ICssMediaRule>().ToArray();
        var expectedMediaRules = expectedSheet.Rules.OfType<ICssMediaRule>().ToArray();

        originalMediaRules.Select(i => i.ConditionText).ShouldBe(expectedMediaRules.Select(i => i.ConditionText));

        foreach (var originalMediaRule in originalMediaRules)
        {
            var expectedMediaRule = expectedMediaRules.First(i => i.ConditionText == originalMediaRule.ConditionText);
            RulesShouldBeEqual(
                originalMediaRule.Rules.OfType<ICssStyleRule>().ToArray(),
                expectedMediaRule.Rules.OfType<ICssStyleRule>().ToArray());
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