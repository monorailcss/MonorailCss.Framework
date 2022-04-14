﻿using System.Text;
using MonorailCss.Css;

namespace MonorailCss.CssWriter;

internal static class CssWriter
{
    public static void AppendCssRules(CssStylesheet stylesheet, StringBuilder stringBuilder)
    {
        // need to order the media rules so that no rules get overriden by items with a rule, and that
        // size related variants are ordered properly with larger sizes coming after smaller.
        var mediaRulesOrdered = stylesheet.MediaRules.OrderBy(i => i.Features.Sum(feature => feature.Priority));

        foreach (var mediaRule in mediaRulesOrdered)
        {
            var indent = 0;
            var hasModifiers = mediaRule.Features.Count > 0;

            if (hasModifiers)
            {
                stringBuilder.AppendLine($"@media {string.Join(" and ", mediaRule.Features.Select(i => i.Feature))} {{");
                indent = 2;
            }

            foreach (var groupedRuleSet in mediaRule.RuleSets.OrderBy(i => i.Importance).GroupBy(i => i.Selector.Selector, set => set))
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

    public static void AppendCssRules(CssDeclarationList defaultVariableDeclarationList, StringBuilder stringBuilder)
    {
        if (defaultVariableDeclarationList.Count == 0)
        {
            return;
        }

        stringBuilder.AppendLine("body, ::before, ::after {");
        foreach (var declaration in defaultVariableDeclarationList.OrderBy(i => i.Property))
        {
            stringBuilder.AppendLine($"  {declaration.Property}:{declaration.Value};");
        }

        stringBuilder.AppendLine("}");
    }
}