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
        hex = hex.Replace("#", string.Empty);

        if (hex.Length == 6)
        {
            _r = int.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
            _g = int.Parse(hex[2..4], System.Globalization.NumberStyles.HexNumber);
            _b = int.Parse(hex[4..6], System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 3)
        {
            _r = int.Parse(hex[1..1], System.Globalization.NumberStyles.HexNumber);
            _g = int.Parse(hex[2..2], System.Globalization.NumberStyles.HexNumber);
            _b = int.Parse(hex[3..3], System.Globalization.NumberStyles.HexNumber);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(hex), "Length should be three or six hex characters.");
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CssColor"/> struct.
    /// </summary>
    /// <param name="r">Red.</param>
    /// <param name="g">Green.</param>
    /// <param name="b">Blue.</param>
    public CssColor(int r, int g, int b)
    {
        _r = r;
        _g = g;
        _b = b;
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
}