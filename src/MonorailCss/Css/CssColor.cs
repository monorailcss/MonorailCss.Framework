namespace MonorailCss.Css;

/// <summary>
/// Represents a color to be used in the design system.
/// </summary>
public readonly struct CssColor
{
    private readonly int? _r;
    private readonly int? _g;
    private readonly int? _b;
    private readonly decimal? _a;
    private readonly string _originalString;
    private readonly bool _isValid;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssColor"/> struct.
    /// </summary>
    /// <param name="colorString">A three or six character hex color.</param>
    public CssColor(string colorString)
    {
        _originalString = colorString.Trim();
        _isValid = false;
        _r = null;
        _g = null;
        _b = null;
        _a = null;
        try
        {
            if (_originalString.StartsWith("rgb(") && _originalString.EndsWith(")"))
            {
                var split = _originalString[4..^1].Split(',');
                if (split.Length != 3)
                {
                    return;
                }

                _r = int.Parse(split[0]);
                _g = int.Parse(split[1]);
                _b = int.Parse(split[2]);
                _a = null;
                _isValid = true;
            }
            else if (colorString.StartsWith("rgba"))
            {
                var split = _originalString[5..^1].Split(',');
                if (split.Length != 4)
                {
                    return;
                }

                _r = int.Parse(split[0]);
                _g = int.Parse(split[1]);
                _b = int.Parse(split[2]);
                _a = decimal.Parse(split[3]);
                _isValid = true;
            }
            else if (colorString.StartsWith("#") && colorString.Length is 4 or 7)
            {
                (_r, _g, _b) = HexToRgba(colorString[1..]);
                _isValid = true;
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    /// Returns the color in the format of rgb(r g b).
    /// </summary>
    /// <returns>Color in the format of rgb( r g b).</returns>
    public string AsRgb() => _isValid
            ? $"rgba({_r}, {_g}, {_b}, {_a ?? 1})"
            : _originalString;

    /// <summary>
    /// Returns whether the color has an alpha channel value.
    /// </summary>
    /// <returns>True if so, false if not.</returns>
    public bool HasAlpha() => _a != null;

    /// <summary>
    /// Returns the color in the format of rgb(r g b / opacity).
    /// </summary>
    /// <param name="opacity">A number of variable name of the opacity.</param>
    /// <returns>The color in the format of rgb(r g b / opacity).</returns>
    public string AsRgbWithOpacity(string opacity)
    {
        return $"rgba({_r}, {_g}, {_b}, {opacity})";
    }

    private static (int R, int G, int B) HexToRgba(string hexColor)
    {
        // Check the length of the hex color
        if (hexColor.Length == 3)
        {
            // If the length is 3, we need to expand it to 6 characters
            // by repeating each character
            var expandedHexColor = string.Empty + hexColor[0] + hexColor[0] + hexColor[1] + hexColor[1] + hexColor[2] + hexColor[2];
            hexColor = expandedHexColor;
        }

        // Convert the hex color to r, g, and b values
        var r = Convert.ToInt32(hexColor[..2], 16);
        var g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
        var b = Convert.ToInt32(hexColor.Substring(4, 2), 16);

        return (r, g, b);
    }

    internal bool IsValid() => _isValid;
}