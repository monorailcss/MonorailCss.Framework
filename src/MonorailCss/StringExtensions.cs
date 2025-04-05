using System.Text;

namespace MonorailCss;

internal static class StringExtensions
{

    public static string ToKebabCase(this string str)
    {

            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var stringBuilder = new StringBuilder();

            // Estimate capacity to avoid resizing
            if (stringBuilder.Capacity < str.Length + 8)
            {
                stringBuilder.Capacity = str.Length + 8;
            }

            // Process first character - never need a dash before it
            stringBuilder.Append(char.ToLowerInvariant(str[0]));

            // Process remaining char
            for (var i = 1; i < str.Length; i++)
            {
                var c = str[i];

                if (char.IsUpper(c))
                {
                    stringBuilder.Append('-');
                    stringBuilder.Append(char.ToLowerInvariant(c));
                }
                else if (char.IsWhiteSpace(c) || c == '_')
                {
                    stringBuilder.Append('-');
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
    }
}