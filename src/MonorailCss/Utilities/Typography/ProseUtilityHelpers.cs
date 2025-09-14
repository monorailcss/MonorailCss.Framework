namespace MonorailCss.Utilities.Typography;

internal static class ProseUtilityHelpers
{
    /// <summary>
    /// Converts pixel value to rem units based on a 16px base.
    /// </summary>
    /// <param name="px">The pixel value to be converted.</param>
    /// <returns>The converted value in rem units as a string.</returns>
    public static string Rem(double px)
    {
        var value = Math.Round(px / 16.0, 7);
        return $"{value}rem".Replace(".0rem", "rem");
    }

    /// <summary>
    /// Converts pixel value to em units based on a specified base size.
    /// </summary>
    /// <param name="px">The pixel value to be converted.</param>
    /// <param name="baseSize">The base size to be used for the conversion.</param>
    /// <returns>The converted value in em units as a string.</returns>
    public static string Em(double px, double baseSize)
    {
        var value = Math.Round(px / baseSize, 7);
        return $"{value}em".Replace(".0em", "em");
    }

    /// <summary>
    /// Rounds the given number to 7 decimal places and removes any trailing zeros.
    /// </summary>
    /// <param name="num">The number to be rounded.</param>
    /// <returns>A string representation of the rounded number without trailing zeros.</returns>
    public static string Round(double num)
    {
        return Math.Round(num, 7).ToString("0.#######");
    }

    /// <summary>
    /// Converts a hexadecimal color code to an RGB space-separated string format.
    /// </summary>
    /// <param name="hex">The hexadecimal color code, with or without a leading '#' character.</param>
    /// <returns>A string representing the color in "R G B" format. Returns "0 0 0" if the conversion fails or the input is invalid.</returns>
    public static string HexToRgb(string hex)
    {
        try
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            if (hex.Length != 6)
            {
                return "0 0 0"; // Return black as fallback
            }

            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);

            return $"{r} {g} {b}";
        }
        catch
        {
            return "0 0 0"; // Return black as fallback for any conversion errors
        }
    }
}