namespace MonorailCss.Tests;

/// <summary>
/// Reusable default CssFramework to share between tests.
/// </summary>
public class CssFrameworkFixture
{
    public readonly CssFramework CssFramework = new(new CssFrameworkSettings()
    {
        IncludePreflight = false
    });
}