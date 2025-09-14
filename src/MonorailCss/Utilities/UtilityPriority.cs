namespace MonorailCss.Utilities;

internal enum UtilityPriority
{
    ExactStatic = 0,
    ConstrainedFunctional = 100,
    NegativeVariant = 200,
    StandardFunctional = 300,
    NamespaceHandler = 400,
    ArbitraryHandler = 500,
    Fallback = 1000,
}