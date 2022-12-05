using static MonorailCss.CssFramework;

namespace MonorailCss.Plugins.Transforms;

internal static class Transform
{
    internal static readonly string TransformValue =
        $"translate({GetCssVariableWithPrefix("translate-x")}, {GetCssVariableWithPrefix("translate-y")}) rotate({GetCssVariableWithPrefix("rotate")}) skewX({GetCssVariableWithPrefix("skew-x")}) skewY({GetCssVariableWithPrefix("skew-y")}) scaleX({GetCssVariableWithPrefix("scale-x")}) scaleY({GetCssVariableWithPrefix("scale-y")})";
}