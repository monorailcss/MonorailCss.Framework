namespace MonorailCss.Css;

/// <summary>
/// Represents a color to be used in the design system.
/// </summary>
public readonly struct CssColor : IEquatable<CssColor>
{
    private readonly ColorModel? _colorModel;
    private readonly string _originalString;
    private readonly bool _isValid;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssColor"/> struct.
    /// </summary>
    /// <param name="colorString">A three or six character hex color, RGB/RGBA color, or OKLCH color.</param>
    public CssColor(string colorString)
    {
        _originalString = colorString.Trim();
        _isValid = false;
        _colorModel = null;

        try
        {
            // Try to parse as RGB/RGBA
            _colorModel = RgbColorModel.TryParse(_originalString, out var isValidRgb);

            // If not valid RGB, try OKLCH
            if (_colorModel == null || !isValidRgb)
            {
                _colorModel = OklchColorModel.TryParse(_originalString, out var isValidOklch);
                _isValid = isValidOklch;
            }
            else
            {
                _isValid = isValidRgb;
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    /// Returns the color in its original format (RGB or OKLCH).
    /// </summary>
    /// <returns>Color in the original format it was provided.</returns>
    public string AsString()
    {
        if (!_isValid || _colorModel == null)
        {
            return _originalString;
        }

        return _colorModel.AsString();
    }

    /// <summary>
    /// Returns whether the color has an alpha channel value.
    /// </summary>
    /// <returns>True if so, false if not.</returns>
    public bool HasAlpha() => _colorModel?.HasAlpha() ?? false;

    /// <summary>
    /// Returns the color with custom opacity.
    /// </summary>
    /// <param name="opacity">A number or variable name of the opacity.</param>
    /// <returns>The color with custom opacity in its original format.</returns>
    public string AsStringWithOpacity(string opacity)
    {
        if (!_isValid || _colorModel == null)
        {
            return _originalString;
        }

        return _colorModel.AsStringWithOpacity(opacity);
    }

    /// <summary>
    /// Determines if the color is valid.
    /// </summary>
    /// <returns>True if the color is valid, otherwise false.</returns>
    internal bool IsValid() => _isValid;

    /// <summary>
    /// Determines if the color is in OKLCH format.
    /// </summary>
    /// <returns>True if the color is in OKLCH format, otherwise false.</returns>
    public bool IsOklch() => _colorModel is OklchColorModel;

    /// <inheritdoc />
    public bool Equals(CssColor other)
    {
        if (!_isValid || !other._isValid || _colorModel == null || other._colorModel == null)
        {
            return false;
        }

        return _colorModel.Equals(other._colorModel);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CssColor other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _colorModel?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Determines whether two CssColor instances are equal.
    /// </summary>
    /// <param name="left">The first CssColor to compare.</param>
    /// <param name="right">The second CssColor to compare.</param>
    /// <returns>True if the two CssColor instances are equal; otherwise, false.</returns>
    public static bool operator ==(CssColor left, CssColor right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two CssColor instances are not equal.
    /// </summary>
    /// <param name="left">The first CssColor to compare.</param>
    /// <param name="right">The second CssColor to compare.</param>
    /// <returns>True if the two CssColor instances are not equal; otherwise, false.</returns>
    public static bool operator !=(CssColor left, CssColor right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Base abstract class for different color models.
    /// </summary>
    private abstract class ColorModel : IEquatable<ColorModel>
    {
        public abstract string AsString();
        public abstract bool HasAlpha();
        public abstract string AsStringWithOpacity(string opacity);
        public abstract bool Equals(ColorModel? other);
        public override abstract int GetHashCode();

        public override bool Equals(object? obj)
        {
            return Equals(obj as ColorModel);
        }
    }

    /// <summary>
    /// Represents an RGB/RGBA color model.
    /// </summary>
    private sealed class RgbColorModel : ColorModel
    {
        private readonly int _r;
        private readonly int _g;
        private readonly int _b;
        private readonly decimal? _a;

        private RgbColorModel(int r, int g, int b, decimal? a)
        {
            _r = r;
            _g = g;
            _b = b;
            _a = a;
        }

        /// <summary>
        /// Attempts to parse a string as an RGB color model.
        /// </summary>
        /// <param name="colorString">The color string to parse.</param>
        /// <param name="isValid">Whether the parsing was successful.</param>
        /// <returns>An RgbColorModel if successful, null otherwise.</returns>
        public static ColorModel? TryParse(string colorString, out bool isValid)
        {
            isValid = false;

            // Try RGB format
            if (colorString.StartsWith("rgb(") && colorString.EndsWith(")"))
            {
                var split = colorString[4..^1].Split(',');
                if (split.Length != 3)
                {
                    return null;
                }

                if (!int.TryParse(split[0].Trim(), out var r) ||
                    !int.TryParse(split[1].Trim(), out var g) ||
                    !int.TryParse(split[2].Trim(), out var b))
                {
                    return null;
                }

                isValid = true;
                return new RgbColorModel(r, g, b, null);
            }

            // Try RGBA format
            if (colorString.StartsWith("rgba(") && colorString.EndsWith(")"))
            {
                var split = colorString[5..^1].Split(',');
                if (split.Length != 4)
                {
                    return null;
                }

                if (!int.TryParse(split[0].Trim(), out var r) ||
                    !int.TryParse(split[1].Trim(), out var g) ||
                    !int.TryParse(split[2].Trim(), out var b) ||
                    !decimal.TryParse(split[3].Trim(), out var a))
                {
                    return null;
                }

                isValid = true;
                return new RgbColorModel(r, g, b, a);
            }

            // Try HEX format
            if (!colorString.StartsWith('#') || colorString.Length is not (4 or 7))
            {
                return null;
            }

            {
                try
                {
                    var (r, g, b) = HexToRgba(colorString[1..]);
                    isValid = true;
                    return new RgbColorModel(r, g, b, null);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Converts a hex color string to RGB values.
        /// </summary>
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

        public override string AsString() => _a.HasValue
            ? $"rgba({_r}, {_g}, {_b}, {_a})"
            : $"rgb({_r}, {_g}, {_b})";

        public override bool HasAlpha() => _a.HasValue;

        public override string AsStringWithOpacity(string opacity) => $"rgba({_r}, {_g}, {_b}, {opacity})";

        public override bool Equals(ColorModel? other)
        {
            return other is RgbColorModel rgb &&
                   _r == rgb._r &&
                   _g == rgb._g &&
                   _b == rgb._b &&
                   _a == rgb._a;
        }

        public override int GetHashCode() => HashCode.Combine(_r, _g, _b, _a);
    }

    /// <summary>
    /// Represents an OKLCH color model.
    /// </summary>
    private sealed class OklchColorModel : ColorModel
    {
        private readonly decimal _l; // lightness
        private readonly decimal _c; // chroma
        private readonly decimal _h; // hue
        private readonly decimal _alpha; // alpha

        private OklchColorModel(decimal l, decimal c, decimal h, decimal alpha)
        {
            _l = l;
            _c = c;
            _h = h;
            _alpha = alpha;
        }

        /// <summary>
        /// Attempts to parse a string as an OKLCH color model.
        /// </summary>
        /// <param name="colorString">The color string to parse.</param>
        /// <param name="isValid">Whether the parsing was successful.</param>
        /// <returns>An OklchColorModel if successful, null otherwise.</returns>
        public static ColorModel? TryParse(string colorString, out bool isValid)
        {
            isValid = false;

            if (!colorString.StartsWith("oklch(") || !colorString.EndsWith(")"))
            {
                return null;
            }

            var content = colorString[6..^1].Trim();

            // Check if there's a separate alpha notation with "/"
            var alphaSplit = content.Split('/', StringSplitOptions.TrimEntries);
            var alpha = 1m;

            if (alphaSplit.Length > 2)
            {
                return null; // Invalid format with multiple "/"
            }

            if (alphaSplit.Length == 2)
            {
                if (!decimal.TryParse(alphaSplit[1].Trim(), out alpha))
                {
                    return null;
                }

                content = alphaSplit[0].Trim();
            }

            var split = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 3)
            {
                return null;
            }

            if (!decimal.TryParse(split[0].Trim(), out var l) ||
                !decimal.TryParse(split[1].Trim(), out var c) ||
                !decimal.TryParse(split[2].Trim(), out var h))
            {
                return null;
            }

            isValid = true;
            return new OklchColorModel(l, c, h, alpha);
        }

        public override string AsString()
        {
            return _alpha == 1m
                ? $"oklch({_l} {_c} {_h})"
                : $"oklch({_l} {_c} {_h} / {_alpha})";
        }

        public override bool HasAlpha() => _alpha != 1m;

        public override string AsStringWithOpacity(string opacity) => $"oklch({_l} {_c} {_h} / {opacity})";

        public override bool Equals(ColorModel? other)
        {
            return other is OklchColorModel oklch &&
                   _l == oklch._l &&
                   _c == oklch._c &&
                   _h == oklch._h &&
                   _alpha == oklch._alpha;
        }

        public override int GetHashCode() => HashCode.Combine(_l, _c, _h, _alpha);
    }
}