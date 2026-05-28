namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for setting the scrollbar track color via scrollbar-color.
/// </summary>
internal class ScrollbarTrackColorUtility : BaseScrollbarColorUtility
{
    protected override string Pattern => "scrollbar-track";

    protected override string TwVariable => "--tw-scrollbar-track";
}
