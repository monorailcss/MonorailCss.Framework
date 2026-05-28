namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for setting the scrollbar thumb color via scrollbar-color.
/// </summary>
internal class ScrollbarThumbColorUtility : BaseScrollbarColorUtility
{
    protected override string Pattern => "scrollbar-thumb";

    protected override string TwVariable => "--tw-scrollbar-thumb";
}
