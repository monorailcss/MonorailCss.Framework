using System.Text;
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
                var nestedIndent = new string(' ', indent + 4);

                // Check if this is a conditional selector (contains a nested condition)
                if (groupedRuleSet.Key.Contains(" &:is("))
                {
                    // Extract the main selector and the condition
                    var parts = groupedRuleSet.Key.Split(new[] { " &:is(" }, StringSplitOptions.None);
                    var mainSelector = parts[0];
                    var condition = "&:is(" + parts[1];

                    stringBuilder.AppendLine($"{selectorIndent}{mainSelector} {{");
                    stringBuilder.AppendLine($"{propIndent}{condition} {{");

                    foreach (var grouping in groupedRuleSet)
                    {
                        foreach (var declaration in grouping.DeclarationList.OrderBy(i => i.Property))
                        {
                            var declarationLines = declaration.ToCssString().Split(Environment.NewLine);
                            foreach (var line in declarationLines)
                            {
                                stringBuilder.AppendLine($"{nestedIndent}{line}");
                            }
                        }
                    }

                    stringBuilder.AppendLine($"{propIndent}}}");
                    stringBuilder.AppendLine($"{selectorIndent}}}");
                }
                else
                {
                    // Normal non-conditional selector
                    stringBuilder.AppendLine($"{selectorIndent}{groupedRuleSet.Key} {{");

                    foreach (var grouping in groupedRuleSet)
                    {
                        foreach (var declaration in grouping.DeclarationList.OrderBy(i => i.Property))
                        {
                            var declarationLines = declaration.ToCssString().Split(Environment.NewLine);
                            foreach (var line in declarationLines)
                            {
                                stringBuilder.AppendLine($"{propIndent}{line}");
                            }
                        }
                    }

                    stringBuilder.AppendLine($"{selectorIndent}}}");
                }
            }

            if (hasModifiers)
            {
                stringBuilder.AppendLine("}");
            }
        }
    }

    public static void AppendDefaultCssRules(CssDeclarationList defaultVariableDeclarationList, StringBuilder stringBuilder)
    {
        if (defaultVariableDeclarationList.Count == 0)
        {
            return;
        }

        stringBuilder.AppendLine("body, ::before, ::after {");
        foreach (var declaration in defaultVariableDeclarationList.OrderBy(i => i.Property))
        {
            stringBuilder.AppendLine($"  {declaration.ToCssString()}");
        }

        stringBuilder.AppendLine("}");
    }
}
