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

        var sb = new StringBuilder(str.Length + 2);

        foreach (var c in str)
        {
            if (char.IsUpper(c))
            {
                sb.Append('-');
                sb.Append(char.ToLower(c));
            }
            else if (char.IsWhiteSpace(c) || c == '_')
            {
                sb.Append('-');
            }
            else
            {
                sb.Append(c);
            }
        }

        // Remove the potentially leading '-' character
        return sb.Length > 0 && sb[0] == '-' ? sb.ToString(1, sb.Length - 1) : sb.ToString();
    }
}