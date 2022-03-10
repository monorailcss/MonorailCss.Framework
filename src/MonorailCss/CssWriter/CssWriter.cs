using System.Text;
using MonorailCss.Css;

namespace MonorailCss.CssWriter;

internal class CssWriter
{
    public static void AppendCssRules(CssStylesheet stylesheet, StringBuilder stringBuilder)
    {
        // make sure the rules without a media feature are listed first
        var mediaRulesOrdered = stylesheet.MediaRules.OrderBy(i => i.Features.Count);

        foreach (var mediaRule in mediaRulesOrdered)
        {
            var indent = 0;
            var hasModifiers = mediaRule.Features.Count > 0;

            if (hasModifiers)
            {
                stringBuilder.AppendLine($"@media {string.Join(" and ", mediaRule.Features)} {{");
                indent = 2;
            }

            foreach (var groupedRuleSet in mediaRule.RuleSets.GroupBy(i => i.Selector.Selector, set => set).OrderBy(i => i.Key))
            {
                var selectorIndent = new string(' ', indent);
                var propIndent = new string(' ', indent + 2);

                stringBuilder.AppendLine($"{selectorIndent}{groupedRuleSet.Key} {{");

                foreach (var grouping in groupedRuleSet)
                {
                    foreach (var declaration in grouping.DeclarationList.OrderBy(i => i.Property))
                    {
                        stringBuilder.AppendLine($"{propIndent}{declaration.Property}:{declaration.Value};");
                    }
                }

                stringBuilder.AppendLine($"{selectorIndent}}}");
            }

            if (hasModifiers)
            {
                stringBuilder.AppendLine("}");
            }
        }
    }
}