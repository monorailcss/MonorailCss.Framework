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
}