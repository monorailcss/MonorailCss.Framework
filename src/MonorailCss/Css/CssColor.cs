namespace MonorailCss.Css;

/// <summary>
/// Represents a color to be used in the design system.
/// </summary>
public readonly struct CssColor
{
    private readonly int _r;
    private readonly int _g;
    private readonly int _b;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssColor"/> struct.
    /// </summary>
    /// <param name="hex">A three or six character hex color.</param>
    public CssColor(string hex)
    {
        var offset = 0;
        if (hex.StartsWith("#"))
        {
            offset = 1;
        }

        if (hex.Length == 6 + offset)
        {
            _r = int.Parse(hex[offset..(offset + 2)], System.Globalization.NumberStyles.HexNumber);
            _g = int.Parse(hex[(offset + 2)..(offset + 4)], System.Globalization.NumberStyles.HexNumber);
            _b = int.Parse(hex[(offset + 4)..(offset + 6)], System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 3 + offset)
        {
            _r = HexToInt(hex[0 + offset]);
            _g = HexToInt(hex[1 + offset]);
            _b = HexToInt(hex[2 + offset]);

            _r = (_r * 16) + _r;
            _g = (_g * 16) + _g;
            _b = (_b * 16) + _b;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(hex), "Length should be three or six hex characters.");
        }
    }

    /// <summary>
    /// Returns the color in the format of rgb(r g b).
    /// </summary>
    /// <returns>Color in the format of rgb( r g b).</returns>
    public string AsRgb()
    {
        return $"rgba({_r}, {_g}, {_b}, 1)";
    }

    /// <summary>
    /// Returns the color in the format of rgb(r g b / opacity).
    /// </summary>
    /// <param name="opacity">A number of variable name of the opacity.</param>
    /// <returns>The color in the format of rgb(r g b / opacity).</returns>
    public string AsRgbWithOpacity(string opacity)
    {
        return $"rgba({_r}, {_g}, {_b}, {opacity})";
    }
    
    private static int HexToInt(char hexChar)
    {
        hexChar = char.ToUpper(hexChar);  // may not be necessary

        if (hexChar < 'A')
        {
            return hexChar - '0';
        }
        else
        {
            return 10 + (hexChar - 'A');
        }
    }
}