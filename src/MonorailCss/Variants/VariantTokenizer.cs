using System.Collections.Immutable;
using MonorailCss.Parser;

namespace MonorailCss.Variants;

/// <summary>
/// Tokenizes variant strings from class names, handling complex nesting and escaping.
/// </summary>
internal class VariantTokenizer
{
    /// <summary>
    /// Parses a class string to extract variant tokens and the base utility.
    /// </summary>
    /// <param name="input">The full class string (e.g., "hover:focus:bg-red-500").</param>
    /// <returns>A tuple of variant tokens and the base utility string.</returns>
    public (ImmutableList<VariantToken> Variants, string BaseUtility) Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return (ImmutableList<VariantToken>.Empty, input);
        }

        // Use SegmentHelper to properly handle nested brackets and parentheses
        var segments = SegmentHelper.Segment(input, ':');

        if (segments.Count <= 1)
        {
            // No variants, just the utility
            return (ImmutableList<VariantToken>.Empty, input);
        }

        // Last segment is the utility, everything else are variants
        var utilitySegment = segments[^1];
        var variantSegments = segments.GetRange(0, segments.Count - 1);

        var variants = ImmutableList.CreateBuilder<VariantToken>();

        foreach (var segment in variantSegments)
        {
            var token = ParseVariantToken(segment);
            if (token.HasValue)
            {
                variants.Add(token.Value);
            }
        }

        return (variants.ToImmutable(), utilitySegment);
    }

    /// <summary>
    /// Parses a single variant segment into a token.
    /// </summary>
    private VariantToken? ParseVariantToken(string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return null;
        }

        // Check for arbitrary variant [...]
        if (segment.StartsWith('[') && segment.EndsWith(']'))
        {
            var content = segment[1..^1];
            return VariantToken.Arbitrary(content);
        }

        // Check for compound variants with modifier (group/name-hover)
        var slashIndex = segment.IndexOf('/');
        if (slashIndex > 0)
        {
            var root = segment[..slashIndex];
            var rest = segment[(slashIndex + 1)..];

            // Check if rest contains a dash (e.g., name-hover)
            var dashIndex = rest.IndexOf('-');
            if (dashIndex > 0)
            {
                var modifier = rest[..dashIndex];
                var subVariant = rest[(dashIndex + 1)..];
                return VariantToken.Compound(root, subVariant, modifier);
            }
        }

        // Check for compound variants without modifier (group-hover, peer-focus)
        if (IsCompoundVariant(segment, out var compoundRoot, out var compoundSub))
        {
            return VariantToken.Compound(compoundRoot, compoundSub);
        }

        // Check for functional variants (aria-[...], data-[...], has-[...])
        if (IsFunctionalVariant(segment, out var funcRoot, out var funcValue))
        {
            return VariantToken.Functional(funcRoot, funcValue);
        }

        // Check for container query variants (@, @min, @max)
        if (IsContainerQueryVariant(segment, out var containerRoot, out var containerValue))
        {
            if (containerValue != null)
            {
                return VariantToken.Functional(containerRoot, containerValue);
            }

            return VariantToken.Static(containerRoot);
        }

        // Check for media query breakpoints
        if (IsBreakpointVariant(segment))
        {
            return VariantToken.Static(segment);
        }

        // Default to static variant
        return VariantToken.Static(segment);
    }

    /// <summary>
    /// Checks if a segment is a compound variant and extracts its parts.
    /// </summary>
    private bool IsCompoundVariant(string segment, out string root, out string subVariant)
    {
        root = string.Empty;
        subVariant = string.Empty;

        // Known compound prefixes
        var compoundPrefixes = new[] { "group", "peer" };

        foreach (var prefix in compoundPrefixes)
        {
            if (segment.StartsWith($"{prefix}-"))
            {
                root = prefix;
                subVariant = segment[(prefix.Length + 1)..];
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a segment is a functional variant and extracts its parts.
    /// </summary>
    private bool IsFunctionalVariant(string segment, out string root, out string value)
    {
        root = string.Empty;
        value = string.Empty;

        // Known functional prefixes
        var functionalPrefixes = new[] { "aria", "data", "has", "where", "is", "not", "supports" };

        foreach (var prefix in functionalPrefixes)
        {
            if (segment.StartsWith($"{prefix}-"))
            {
                root = prefix;
                var rawValue = segment[(prefix.Length + 1)..];

                // Strip brackets if present (e.g., "[checked]" -> "checked")
                if (rawValue.StartsWith('[') && rawValue.EndsWith(']'))
                {
                    value = rawValue[1..^1];
                }
                else
                {
                    value = rawValue;
                }

                return true;
            }
        }

        // Check for [@media ...], [@container ...], [@supports ...]
        if (segment.StartsWith("[@"))
        {
            root = "arbitrary";
            value = segment;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a segment is a container query variant and extracts its parts.
    /// </summary>
    private bool IsContainerQueryVariant(string segment, out string root, out string? value)
    {
        root = string.Empty;
        value = null;

        // Handle @min-*, @max-* functional variants
        if (segment.StartsWith("@min-"))
        {
            root = "@min";
            value = segment[5..]; // Remove "@min-" prefix
            return true;
        }

        if (segment.StartsWith("@max-"))
        {
            root = "@max";
            value = segment[5..]; // Remove "@max-" prefix
            return true;
        }

        // Handle static @ variants (@sm, @md, @lg, etc.)
        if (segment.StartsWith('@') && segment.Length > 1)
        {
            var containerName = segment[1..]; // Remove @ prefix

            // Check if it's a known container size
            if (IsKnownContainerSize(containerName))
            {
                root = "@" + containerName;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a name corresponds to a known container size.
    /// </summary>
    private bool IsKnownContainerSize(string name)
    {
        return name is "3xs" or "2xs" or "xs" or "sm" or "md" or "lg" or "xl" or "2xl" or "3xl" or "4xl" or "5xl" or "6xl" or "7xl";
    }

    /// <summary>
    /// Checks if a segment is a responsive breakpoint variant.
    /// </summary>
    private bool IsBreakpointVariant(string segment)
    {
        return segment is "sm" or "md" or "lg" or "xl" or "2xl" or
               "min" or "max" or "portrait" or "landscape";
    }
}