using Shouldly;

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

/// <summary>
/// Comprehensive test for all implemented Tailwind CSS utilities following the guidelines
/// from TailwindUtilitiesImplementation.md. Tests framework-level behavior using one
/// large theory test with systematic coverage of all utility categories.
/// </summary>
public class AllUtilitiesIntegrationTest(CssFrameworkFixture fixture) : IClassFixture<CssFrameworkFixture>
{

    private readonly CssFramework _cssFramework = fixture.CssFramework;

    [Theory]
    [MemberData(nameof(AllUtilitiesTestData))]
    public void AllUtilities_ShouldGenerateAtLeastOneProperty(string className, string expectedProperty)
    {
        // Arrange
        // Act
        var result = _cssFramework.Process(className);

        Console.WriteLine(result);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    public static IEnumerable<object[]> AllUtilitiesTestData()
    {
        // =============================================================================
        // 1. Layout & Positioning Utilities
        // =============================================================================

        // Display utilities
        yield return ["block", "display: block"];
        yield return ["inline", "display: inline"];
        yield return ["inline-block", "display: inline-block"];
        yield return ["flex", "display: flex"];
        yield return ["inline-flex", "display: inline-flex"];
        yield return ["grid", "display: grid"];
        yield return ["inline-grid", "display: inline-grid"];
        yield return ["table", "display: table"];
        yield return ["inline-table", "display: inline-table"];
        yield return ["table-caption", "display: table-caption"];
        yield return ["table-cell", "display: table-cell"];
        yield return ["table-column", "display: table-column"];
        yield return ["table-column-group", "display: table-column-group"];
        yield return ["table-footer-group", "display: table-footer-group"];
        yield return ["table-header-group", "display: table-header-group"];
        yield return ["table-row", "display: table-row"];
        yield return ["table-row-group", "display: table-row-group"];
        yield return ["flow-root", "display: flow-root"];
        yield return ["contents", "display: contents"];
        yield return ["list-item", "display: list-item"];
        yield return ["none", "display: none"];
        yield return ["hidden", "display: none"];

        // Container utilities
        yield return ["container", "width: 100%"];
        yield return ["container", "@media (width >= 40rem)"];  // Verify media queries are generated

        // Position utilities
        yield return ["static", "position: static"];
        yield return ["fixed", "position: fixed"];
        yield return ["absolute", "position: absolute"];
        yield return ["relative", "position: relative"];
        yield return ["sticky", "position: sticky"];

        // Visibility utilities
        yield return ["visible", "visibility: visible"];
        yield return ["invisible", "visibility: hidden"];
        yield return ["collapse", "visibility: collapse"];

        // Isolation utilities
        yield return ["isolate", "isolation: isolate"];
        yield return ["isolation-auto", "isolation: auto"];

        // Float utilities
        yield return ["float-start", "float: inline-start"];
        yield return ["float-end", "float: inline-end"];
        yield return ["float-left", "float: left"];
        yield return ["float-right", "float: right"];
        yield return ["float-none", "float: none"];

        // Clear utilities
        yield return ["clear-start", "clear: inline-start"];
        yield return ["clear-end", "clear: inline-end"];
        yield return ["clear-left", "clear: left"];
        yield return ["clear-right", "clear: right"];
        yield return ["clear-both", "clear: both"];
        yield return ["clear-none", "clear: none"];

        // Box Sizing utilities
        yield return ["box-border", "box-sizing: border-box"];
        yield return ["box-content", "box-sizing: content-box"];

        // CSS Containment utilities
        yield return ["contain-none", "contain: none"];
        yield return ["contain-strict", "contain: strict"];
        yield return ["contain-content", "contain: content"];
        yield return ["contain-size", "contain: size"];
        yield return ["contain-layout", "contain: layout"];
        yield return ["contain-style", "contain: style"];
        yield return ["contain-paint", "contain: paint"];

        // Container Query utilities
        yield return ["@container", "container-type: inline-size"];

        // Overflow utilities
        yield return ["overflow-auto", "overflow: auto"];
        yield return ["overflow-hidden", "overflow: hidden"];
        yield return ["overflow-clip", "overflow: clip"];
        yield return ["overflow-visible", "overflow: visible"];
        yield return ["overflow-scroll", "overflow: scroll"];
        yield return ["overflow-x-auto", "overflow-x: auto"];
        yield return ["overflow-x-hidden", "overflow-x: hidden"];
        yield return ["overflow-x-clip", "overflow-x: clip"];
        yield return ["overflow-x-visible", "overflow-x: visible"];
        yield return ["overflow-x-scroll", "overflow-x: scroll"];
        yield return ["overflow-y-auto", "overflow-y: auto"];
        yield return ["overflow-y-hidden", "overflow-y: hidden"];
        yield return ["overflow-y-clip", "overflow-y: clip"];
        yield return ["overflow-y-visible", "overflow-y: visible"];
        yield return ["overflow-y-scroll", "overflow-y: scroll"];

        // =============================================================================
        // 2. Flexbox & Grid Utilities
        // =============================================================================

        // Flex Direction utilities
        yield return ["flex-row", "flex-direction: row"];
        yield return ["flex-row-reverse", "flex-direction: row-reverse"];
        yield return ["flex-col", "flex-direction: column"];
        yield return ["flex-col-reverse", "flex-direction: column-reverse"];

        // Flex Wrap utilities
        yield return ["flex-wrap", "flex-wrap: wrap"];
        yield return ["flex-nowrap", "flex-wrap: nowrap"];
        yield return ["flex-wrap-reverse", "flex-wrap: wrap-reverse"];

        // Justify Content utilities
        yield return ["justify-normal", "justify-content: normal"];
        yield return ["justify-center", "justify-content: center"];
        yield return ["justify-start", "justify-content: flex-start"];
        yield return ["justify-end", "justify-content: flex-end"];
        yield return ["justify-between", "justify-content: space-between"];
        yield return ["justify-around", "justify-content: space-around"];
        yield return ["justify-evenly", "justify-content: space-evenly"];
        yield return ["justify-stretch", "justify-content: stretch"];

        // Align Items utilities
        yield return ["items-center", "align-items: center"];
        yield return ["items-start", "align-items: flex-start"];
        yield return ["items-end", "align-items: flex-end"];
        yield return ["items-baseline", "align-items: baseline"];
        yield return ["items-stretch", "align-items: stretch"];

        // Align Self utilities
        yield return ["self-auto", "align-self: auto"];
        yield return ["self-center", "align-self: center"];
        yield return ["self-start", "align-self: flex-start"];
        yield return ["self-end", "align-self: flex-end"];
        yield return ["self-baseline", "align-self: baseline"];
        yield return ["self-stretch", "align-self: stretch"];

        // Align Content utilities
        yield return ["content-normal", "align-content: normal"];
        yield return ["content-center", "align-content: center"];
        yield return ["content-start", "align-content: flex-start"];
        yield return ["content-end", "align-content: flex-end"];
        yield return ["content-between", "align-content: space-between"];
        yield return ["content-around", "align-content: space-around"];
        yield return ["content-evenly", "align-content: space-evenly"];
        yield return ["content-baseline", "align-content: baseline"];
        yield return ["content-stretch", "align-content: stretch"];

        // Justify Items utilities
        yield return ["justify-items-start", "justify-items: start"];
        yield return ["justify-items-end", "justify-items: end"];
        yield return ["justify-items-center", "justify-items: center"];
        yield return ["justify-items-stretch", "justify-items: stretch"];

        // Justify Self utilities
        yield return ["justify-self-auto", "justify-self: auto"];
        yield return ["justify-self-start", "justify-self: start"];
        yield return ["justify-self-end", "justify-self: end"];
        yield return ["justify-self-center", "justify-self: center"];
        yield return ["justify-self-stretch", "justify-self: stretch"];

        // Place utilities
        yield return ["place-content-center", "place-content: center"];
        yield return ["place-content-start", "place-content: start"];
        yield return ["place-content-end", "place-content: end"];
        yield return ["place-content-between", "place-content: space-between"];
        yield return ["place-content-around", "place-content: space-around"];
        yield return ["place-content-evenly", "place-content: space-evenly"];
        yield return ["place-content-baseline", "place-content: baseline"];
        yield return ["place-content-stretch", "place-content: stretch"];
        yield return ["place-items-start", "place-items: start"];
        yield return ["place-items-end", "place-items: end"];
        yield return ["place-items-center", "place-items: center"];
        yield return ["place-items-baseline", "place-items: baseline"];
        yield return ["place-items-stretch", "place-items: stretch"];
        yield return ["place-self-auto", "place-self: auto"];
        yield return ["place-self-start", "place-self: start"];
        yield return ["place-self-end", "place-self: end"];
        yield return ["place-self-center", "place-self: center"];
        yield return ["place-self-stretch", "place-self: stretch"];

        // =============================================================================
        // 3. Layout Utilities (Additional)
        // =============================================================================

        // Overscroll Behavior utilities
        yield return ["overscroll-auto", "overscroll-behavior: auto"];
        yield return ["overscroll-contain", "overscroll-behavior: contain"];
        yield return ["overscroll-none", "overscroll-behavior: none"];
        yield return ["overscroll-x-auto", "overscroll-behavior-x: auto"];
        yield return ["overscroll-x-contain", "overscroll-behavior-x: contain"];
        yield return ["overscroll-x-none", "overscroll-behavior-x: none"];
        yield return ["overscroll-y-auto", "overscroll-behavior-y: auto"];
        yield return ["overscroll-y-contain", "overscroll-behavior-y: contain"];
        yield return ["overscroll-y-none", "overscroll-behavior-y: none"];

        // =============================================================================
        // 4. Spacing Utilities (Note: These use CSS variables/calc expressions)
        // =============================================================================

        // Padding utilities - test for property name since values use CSS variables
        yield return ["p-4", "padding:"];
        yield return ["px-2", "padding-inline:"];
        yield return ["py-8", "padding-block:"];
        yield return ["pt-1", "padding-top:"];
        yield return ["pr-6", "padding-right:"];
        yield return ["pb-12", "padding-bottom:"];
        yield return ["pl-3", "padding-left:"];
        yield return ["ps-5", "padding-inline-start:"];
        yield return ["pe-7", "padding-inline-end:"];

        // Margin utilities - test for property name since values use CSS variables
        yield return ["m-4", "margin:"];
        yield return ["m-auto", "margin: auto"];
        yield return ["mx-2", "margin-inline:"];
        yield return ["my-8", "margin-block:"];
        yield return ["mt-1", "margin-top:"];
        yield return ["mr-6", "margin-right:"];
        yield return ["mb-12", "margin-bottom:"];
        yield return ["ml-3", "margin-left:"];
        yield return ["ms-5", "margin-inline-start:"];
        yield return ["me-7", "margin-inline-end:"];

        // =============================================================================
        // 5. Typography Utilities
        // =============================================================================

        // Text Align utilities
        yield return ["text-left", "text-align: left"];
        yield return ["text-center", "text-align: center"];
        yield return ["text-right", "text-align: right"];
        yield return ["text-justify", "text-align: justify"];
        yield return ["text-start", "text-align: start"];
        yield return ["text-end", "text-align: end"];

        // Font Style utilities
        yield return ["italic", "font-style: italic"];
        yield return ["not-italic", "font-style: normal"];

        // Text Decoration utilities
        yield return ["underline", "text-decoration-line: underline"];
        yield return ["overline", "text-decoration-line: overline"];
        yield return ["line-through", "text-decoration-line: line-through"];
        yield return ["no-underline", "text-decoration-line: none"];

        // Text Transform utilities
        yield return ["uppercase", "text-transform: uppercase"];
        yield return ["lowercase", "text-transform: lowercase"];
        yield return ["capitalize", "text-transform: capitalize"];
        yield return ["normal-case", "text-transform: none"];

        // =============================================================================
        // 6. Background & Color Utilities
        // =============================================================================

        // Text Color utilities - test for property name since values use CSS variables
        yield return ["text-red-500", "color:"];
        yield return ["text-blue-600", "color:"];
        yield return ["text-green-400", "color:"];

        // Background Color utilities - test for property name since values use CSS variables
        yield return ["bg-red-500", "background-color:"];
        yield return ["bg-blue-600", "background-color:"];
        yield return ["bg-green-400", "background-color:"];

        // =============================================================================
        // 7. Additional Typography Utilities
        // =============================================================================

        // Text Decoration Style utilities
        yield return ["decoration-solid", "text-decoration-style: solid"];
        yield return ["decoration-double", "text-decoration-style: double"];
        yield return ["decoration-dotted", "text-decoration-style: dotted"];
        yield return ["decoration-dashed", "text-decoration-style: dashed"];
        yield return ["decoration-wavy", "text-decoration-style: wavy"];

        // Text Wrap utilities
        yield return ["text-wrap", "text-wrap: wrap"];
        yield return ["text-nowrap", "text-wrap: nowrap"];
        yield return ["text-balance", "text-wrap: balance"];
        yield return ["text-pretty", "text-wrap: pretty"];

        // Text Size Adjust utilities
        yield return ["text-size-adjust-none", "text-size-adjust: none"];
        yield return ["text-size-adjust-auto", "text-size-adjust: auto"];
        yield return ["text-size-adjust-[120%]", "text-size-adjust: 120%"];

        // Whitespace utilities
        yield return ["whitespace-normal", "white-space: normal"];
        yield return ["whitespace-nowrap", "white-space: nowrap"];
        yield return ["whitespace-pre", "white-space: pre"];
        yield return ["whitespace-pre-line", "white-space: pre-line"];
        yield return ["whitespace-pre-wrap", "white-space: pre-wrap"];
        yield return ["whitespace-break-spaces", "white-space: break-spaces"];

        // Word Break utilities
        yield return ["break-normal", "overflow-wrap: normal"]; // break-normal sets multiple properties
        yield return ["break-words", "overflow-wrap: break-word"];
        yield return ["break-all", "word-break: break-all"];
        yield return ["break-keep", "word-break: keep-all"];

        // Grid Flow utilities
        yield return ["grid-flow-row", "grid-auto-flow: row"];
        yield return ["grid-flow-col", "grid-auto-flow: column"];
        yield return ["grid-flow-dense", "grid-auto-flow: dense"];
        yield return ["grid-flow-row-dense", "grid-auto-flow: row dense"];
        yield return ["grid-flow-col-dense", "grid-auto-flow: column dense"];

        // Text Overflow utilities
        yield return ["truncate", "overflow: hidden"]; // truncate sets multiple properties
        yield return ["text-ellipsis", "text-overflow: ellipsis"];
        yield return ["text-clip", "text-overflow: clip"];

        // Hyphens utilities (with vendor prefixes)
        yield return ["hyphens-none", "-webkit-hyphens: none"];
        yield return ["hyphens-manual", "-webkit-hyphens: manual"];
        yield return ["hyphens-auto", "-webkit-hyphens: auto"];

        // Background Repeat utilities
        yield return ["bg-repeat", "background-repeat: repeat"];
        yield return ["bg-no-repeat", "background-repeat: no-repeat"];
        yield return ["bg-repeat-x", "background-repeat: repeat-x"];
        yield return ["bg-repeat-y", "background-repeat: repeat-y"];
        yield return ["bg-repeat-round", "background-repeat: round"];
        yield return ["bg-repeat-space", "background-repeat: space"];

        // Background Attachment utilities
        yield return ["bg-fixed", "background-attachment: fixed"];
        yield return ["bg-local", "background-attachment: local"];
        yield return ["bg-scroll", "background-attachment: scroll"];

        // Background Clip utilities
        yield return ["bg-clip-text", "background-clip: text"];
        yield return ["bg-clip-border", "background-clip: border-box"];
        yield return ["bg-clip-padding", "background-clip: padding-box"];
        yield return ["bg-clip-content", "background-clip: content-box"];

        // Background Origin utilities
        yield return ["bg-origin-border", "background-origin: border-box"];
        yield return ["bg-origin-padding", "background-origin: padding-box"];
        yield return ["bg-origin-content", "background-origin: content-box"];

        // Border Style utilities
        yield return ["border-solid", "border-style: solid"];
        yield return ["border-dashed", "border-style: dashed"];
        yield return ["border-dotted", "border-style: dotted"];
        yield return ["border-double", "border-style: double"];
        yield return ["border-hidden", "border-style: hidden"];
        yield return ["border-none", "border-style: none"];

        // Border Image utilities
        yield return ["border-image-none", "border-image: none"];
        yield return ["border-image-[url(image.png)]", "border-image: url(image.png)"];

        // Border Collapse utilities
        yield return ["border-collapse", "border-collapse: collapse"];
        yield return ["border-separate", "border-collapse: separate"];

        // =============================================================================
        // Divide Utilities (Child Element Borders)
        // =============================================================================

        // Divide Width utilities - test for child selector and properties
        yield return ["divide-x-0", ":where(& > :not(:last-child))"];
        yield return ["divide-x-0", "--tw-divide-x-reverse: 0"];
        yield return ["divide-x-0", "border-inline-style: var(--tw-border-style)"];
        yield return ["divide-x-0", "border-inline-start-width: calc(0px * var(--tw-divide-x-reverse))"];
        yield return ["divide-x-0", "border-inline-end-width: calc(0px * calc(1 - var(--tw-divide-x-reverse)))"];

        yield return ["divide-x-2", ":where(& > :not(:last-child))"];
        yield return ["divide-x-2", "--tw-divide-x-reverse: 0"];
        yield return ["divide-x-2", "border-inline-style: var(--tw-border-style)"];
        yield return ["divide-x-2", "border-inline-start-width: calc(2px * var(--tw-divide-x-reverse))"];
        yield return ["divide-x-2", "border-inline-end-width: calc(2px * calc(1 - var(--tw-divide-x-reverse)))"];

        yield return ["divide-x-4", ":where(& > :not(:last-child))"];
        yield return ["divide-x-4", "border-inline-start-width: calc(4px * var(--tw-divide-x-reverse))"];

        yield return ["divide-x-8", ":where(& > :not(:last-child))"];
        yield return ["divide-x-8", "border-inline-start-width: calc(8px * var(--tw-divide-x-reverse))"];

        yield return ["divide-y-0", ":where(& > :not(:last-child))"];
        yield return ["divide-y-0", "--tw-divide-y-reverse: 0"];
        yield return ["divide-y-0", "border-top-style: var(--tw-border-style)"];
        yield return ["divide-y-0", "border-bottom-style: var(--tw-border-style)"];
        yield return ["divide-y-0", "border-top-width: calc(0px * var(--tw-divide-y-reverse))"];
        yield return ["divide-y-0", "border-bottom-width: calc(0px * calc(1 - var(--tw-divide-y-reverse)))"];

        yield return ["divide-y-2", ":where(& > :not(:last-child))"];
        yield return ["divide-y-2", "border-top-width: calc(2px * var(--tw-divide-y-reverse))"];

        yield return ["divide-y-4", ":where(& > :not(:last-child))"];
        yield return ["divide-y-4", "border-top-width: calc(4px * var(--tw-divide-y-reverse))"];

        yield return ["divide-y-8", ":where(& > :not(:last-child))"];
        yield return ["divide-y-8", "border-top-width: calc(8px * var(--tw-divide-y-reverse))"];

        // Divide Color utilities
        yield return ["divide-transparent", ":where(& > :not(:last-child))"];
        yield return ["divide-transparent", "border-color: transparent"];

        yield return ["divide-current", ":where(& > :not(:last-child))"];
        yield return ["divide-current", "border-color: currentColor"];

        yield return ["divide-red-500", ":where(& > :not(:last-child))"];
        yield return ["divide-red-500", "border-color:"];  // Uses CSS variable

        yield return ["divide-blue-600", ":where(& > :not(:last-child))"];
        yield return ["divide-blue-600", "border-color:"];

        // Divide Color utilities with opacity - test for color-mix
        yield return ["divide-green-400/50", ":where(& > :not(:last-child))"];
        yield return ["divide-green-400/50", "color-mix(in oklab,"];

        yield return ["divide-gray-900/25", ":where(& > :not(:last-child))"];
        yield return ["divide-gray-900/25", "color-mix(in oklab,"];

        // Divide Style utilities
        yield return ["divide-solid", ":where(& > :not(:last-child))"];
        yield return ["divide-solid", "--tw-border-style: solid"];
        yield return ["divide-solid", "border-style: solid"];

        yield return ["divide-dashed", ":where(& > :not(:last-child))"];
        yield return ["divide-dashed", "--tw-border-style: dashed"];
        yield return ["divide-dashed", "border-style: dashed"];

        yield return ["divide-dotted", ":where(& > :not(:last-child))"];
        yield return ["divide-dotted", "--tw-border-style: dotted"];

        yield return ["divide-double", ":where(& > :not(:last-child))"];
        yield return ["divide-double", "--tw-border-style: double"];

        yield return ["divide-none", ":where(& > :not(:last-child))"];
        yield return ["divide-none", "--tw-border-style: none"];
        yield return ["divide-none", "border-style: none"];

        // Divide Reverse utilities
        yield return ["divide-x-reverse", ":where(& > :not(:last-child))"];
        yield return ["divide-x-reverse", "--tw-divide-x-reverse: 1"];

        yield return ["divide-y-reverse", ":where(& > :not(:last-child))"];
        yield return ["divide-y-reverse", "--tw-divide-y-reverse: 1"];

        // Mix Blend Mode utilities
        yield return ["mix-blend-normal", "mix-blend-mode: normal"];
        yield return ["mix-blend-multiply", "mix-blend-mode: multiply"];
        yield return ["mix-blend-screen", "mix-blend-mode: screen"];
        yield return ["mix-blend-overlay", "mix-blend-mode: overlay"];
        yield return ["mix-blend-darken", "mix-blend-mode: darken"];
        yield return ["mix-blend-lighten", "mix-blend-mode: lighten"];
        yield return ["mix-blend-color-dodge", "mix-blend-mode: color-dodge"];
        yield return ["mix-blend-color-burn", "mix-blend-mode: color-burn"];
        yield return ["mix-blend-hard-light", "mix-blend-mode: hard-light"];
        yield return ["mix-blend-soft-light", "mix-blend-mode: soft-light"];
        yield return ["mix-blend-difference", "mix-blend-mode: difference"];
        yield return ["mix-blend-exclusion", "mix-blend-mode: exclusion"];
        yield return ["mix-blend-hue", "mix-blend-mode: hue"];
        yield return ["mix-blend-saturation", "mix-blend-mode: saturation"];
        yield return ["mix-blend-color", "mix-blend-mode: color"];
        yield return ["mix-blend-luminosity", "mix-blend-mode: luminosity"];

        // Background Blend Mode utilities
        yield return ["bg-blend-normal", "background-blend-mode: normal"];
        yield return ["bg-blend-multiply", "background-blend-mode: multiply"];
        yield return ["bg-blend-screen", "background-blend-mode: screen"];
        yield return ["bg-blend-overlay", "background-blend-mode: overlay"];
        yield return ["bg-blend-darken", "background-blend-mode: darken"];
        yield return ["bg-blend-lighten", "background-blend-mode: lighten"];
        yield return ["bg-blend-color-dodge", "background-blend-mode: color-dodge"];
        yield return ["bg-blend-color-burn", "background-blend-mode: color-burn"];
        yield return ["bg-blend-hard-light", "background-blend-mode: hard-light"];
        yield return ["bg-blend-soft-light", "background-blend-mode: soft-light"];
        yield return ["bg-blend-difference", "background-blend-mode: difference"];
        yield return ["bg-blend-exclusion", "background-blend-mode: exclusion"];
        yield return ["bg-blend-hue", "background-blend-mode: hue"];
        yield return ["bg-blend-saturation", "background-blend-mode: saturation"];
        yield return ["bg-blend-color", "background-blend-mode: color"];
        yield return ["bg-blend-luminosity", "background-blend-mode: luminosity"];

        // Mask Image utilities
        yield return ["mask-none", "mask-image: none"];

        // Mask Size utilities
        yield return ["mask-auto", "mask-size: auto"];
        yield return ["mask-cover", "mask-size: cover"];
        yield return ["mask-contain", "mask-size: contain"];

        // Mask Repeat utilities
        yield return ["mask-repeat", "mask-repeat: repeat"];
        yield return ["mask-no-repeat", "mask-repeat: no-repeat"];
        yield return ["mask-repeat-x", "mask-repeat: repeat-x"];
        yield return ["mask-repeat-y", "mask-repeat: repeat-y"];
        yield return ["mask-repeat-round", "mask-repeat: round"];
        yield return ["mask-repeat-space", "mask-repeat: space"];

        // Mask Origin utilities
        yield return ["mask-origin-border", "mask-origin: border-box"];
        yield return ["mask-origin-padding", "mask-origin: padding-box"];
        yield return ["mask-origin-content", "mask-origin: content-box"];

        // Mask Clip utilities
        yield return ["mask-clip-border", "mask-clip: border-box"];
        yield return ["mask-clip-padding", "mask-clip: padding-box"];
        yield return ["mask-clip-content", "mask-clip: content-box"];

        // Mask Type utilities
        yield return ["mask-type-alpha", "mask-type: alpha"];
        yield return ["mask-type-luminance", "mask-type: luminance"];

        // Mask Position utilities
        yield return ["mask-top-left", "mask-position: left top"];
        yield return ["mask-top", "mask-position: top"];
        yield return ["mask-top-right", "mask-position: right top"];
        yield return ["mask-left", "mask-position: left"];
        yield return ["mask-center", "mask-position: center"];
        yield return ["mask-right", "mask-position: right"];
        yield return ["mask-bottom-left", "mask-position: left bottom"];
        yield return ["mask-bottom", "mask-position: bottom"];
        yield return ["mask-bottom-right", "mask-position: right bottom"];

        // Mask Composite utilities
        yield return ["mask-add", "mask-composite: add"];
        yield return ["mask-subtract", "mask-composite: subtract"];
        yield return ["mask-intersect", "mask-composite: intersect"];
        yield return ["mask-exclude", "mask-composite: exclude"];

        // Mask Mode utilities
        yield return ["mask-alpha", "mask-mode: alpha"];
        yield return ["mask-luminance", "mask-mode: luminance"];
        yield return ["mask-match", "mask-mode: match-source"];

        // =============================================================================
        // 8. Gap Utilities
        // =============================================================================

        // Gap utilities - test for property name since values use CSS variables
        yield return ["gap-0", "gap:"];
        yield return ["gap-4", "gap:"];
        yield return ["gap-px", "gap: 1px"];
        yield return ["gap-x-2", "column-gap:"];
        yield return ["gap-y-8", "row-gap:"];

        // =============================================================================
        // 9. Inset Utilities (Positioning)
        // =============================================================================

        // Inset utilities - single inset sets all four sides
        yield return ["inset-0", "top:"];
        yield return ["inset-4", "right:"];  // inset sets multiple properties, test for any one
        yield return ["inset-auto", "top: auto"];
        yield return ["inset-full", "top: 100%"];
        yield return ["inset-1/2", "top: calc(1/2 * 100%)"];
        yield return ["-inset-4", "top:"];  // negative values

        // Individual positioning utilities
        yield return ["top-0", "top:"];
        yield return ["top-4", "top:"];
        yield return ["top-auto", "top: auto"];
        yield return ["top-full", "top: 100%"];
        yield return ["top-1/2", "top: calc(1/2 * 100%)"];
        yield return ["-top-4", "top:"];  // negative values

        yield return ["right-0", "right:"];
        yield return ["right-4", "right:"];
        yield return ["right-auto", "right: auto"];
        yield return ["right-full", "right: 100%"];
        yield return ["-right-2", "right:"];

        yield return ["bottom-0", "bottom:"];
        yield return ["bottom-4", "bottom:"];
        yield return ["bottom-auto", "bottom: auto"];
        yield return ["bottom-full", "bottom: 100%"];
        yield return ["-bottom-2", "bottom:"];

        yield return ["left-0", "left:"];
        yield return ["left-4", "left:"];
        yield return ["left-auto", "left: auto"];
        yield return ["left-full", "left: 100%"];
        yield return ["-left-2", "left:"];

        // Logical positioning utilities
        yield return ["start-0", "inset-inline-start:"];
        yield return ["start-4", "inset-inline-start:"];
        yield return ["start-auto", "inset-inline-start: auto"];
        yield return ["-start-2", "inset-inline-start:"];

        yield return ["end-0", "inset-inline-end:"];
        yield return ["end-4", "inset-inline-end:"];
        yield return ["end-auto", "inset-inline-end: auto"];
        yield return ["-end-2", "inset-inline-end:"];

        // =============================================================================
        // 10. Size Utilities (Sets both width and height)
        // =============================================================================

        // Size utilities - set both width and height
        yield return ["size-auto", "width: auto"];  // size sets multiple properties, test for one
        yield return ["size-full", "width: 100%"];
        yield return ["size-min", "width: min-content"];
        yield return ["size-max", "width: max-content"];
        yield return ["size-fit", "width: fit-content"];
        yield return ["size-4", "width:"];  // Uses CSS variable
        yield return ["size-1/2", "width: calc(1/2 * 100%)"];
        yield return ["size-1/3", "width: calc(1/3 * 100%)"];
        yield return ["size-3/4", "width: calc(3/4 * 100%)"];

        // =============================================================================
        // 8. Sizing Utilities
        // =============================================================================

        // Width utilities
        yield return ["w-auto", "width: auto"];
        yield return ["w-full", "width: 100%"];
        yield return ["w-screen", "width: 100vw"];
        yield return ["w-svw", "width: 100svw"];
        yield return ["w-lvw", "width: 100lvw"];
        yield return ["w-dvw", "width: 100dvw"];
        yield return ["w-min", "width: min-content"];
        yield return ["w-max", "width: max-content"];
        yield return ["w-fit", "width: fit-content"];
        yield return ["w-4", "width:"];  // Uses CSS variable
        yield return ["w-1/2", "width: calc(1/2 * 100%)"];
        yield return ["w-1/3", "width: calc(1/3 * 100%)"];
        yield return ["w-2/3", "width: calc(2/3 * 100%)"];
        yield return ["w-1/4", "width: calc(1/4 * 100%)"];
        yield return ["w-3/4", "width: calc(3/4 * 100%)"];

        // Height utilities
        yield return ["h-auto", "height: auto"];
        yield return ["h-full", "height: 100%"];
        yield return ["h-screen", "height: 100vh"];
        yield return ["h-svh", "height: 100svh"];
        yield return ["h-lvh", "height: 100lvh"];
        yield return ["h-dvh", "height: 100dvh"];
        yield return ["h-min", "height: min-content"];
        yield return ["h-max", "height: max-content"];
        yield return ["h-fit", "height: fit-content"];
        yield return ["h-4", "height:"];  // Uses CSS variable
        yield return ["h-1/2", "height: calc(1/2 * 100%)"];
        yield return ["h-1/3", "height: calc(1/3 * 100%)"];
        yield return ["h-2/3", "height: calc(2/3 * 100%)"];
        yield return ["h-1/4", "height: calc(1/4 * 100%)"];
        yield return ["h-3/4", "height: calc(3/4 * 100%)"];

        // Min Width utilities
        yield return ["min-w-0", "min-width: 0"];
        yield return ["min-w-auto", "min-width: auto"];
        yield return ["min-w-full", "min-width: 100%"];
        yield return ["min-w-screen", "min-width: 100vw"];
        yield return ["min-w-svw", "min-width: 100svw"];
        yield return ["min-w-lvw", "min-width: 100lvw"];
        yield return ["min-w-dvw", "min-width: 100dvw"];
        yield return ["min-w-min", "min-width: min-content"];
        yield return ["min-w-max", "min-width: max-content"];
        yield return ["min-w-fit", "min-width: fit-content"];
        yield return ["min-w-4", "min-width:"];  // Uses CSS variable
        yield return ["min-w-1/2", "min-width: calc(1/2 * 100%)"];
        yield return ["min-w-1/3", "min-width: calc(1/3 * 100%)"];
        yield return ["min-w-3/4", "min-width: calc(3/4 * 100%)"];

        // Max Width utilities
        yield return ["max-w-none", "max-width: none"];
        yield return ["max-w-0", "max-width: 0"];
        yield return ["max-w-full", "max-width: 100%"];
        yield return ["max-w-screen", "max-width: 100vw"];
        yield return ["max-w-svw", "max-width: 100svw"];
        yield return ["max-w-lvw", "max-width: 100lvw"];
        yield return ["max-w-dvw", "max-width: 100dvw"];
        yield return ["max-w-min", "max-width: min-content"];
        yield return ["max-w-max", "max-width: max-content"];
        yield return ["max-w-fit", "max-width: fit-content"];
        yield return ["max-w-4", "max-width:"];  // Uses CSS variable
        yield return ["max-w-1/2", "max-width: calc(1/2 * 100%)"];
        yield return ["max-w-1/3", "max-width: calc(1/3 * 100%)"];
        yield return ["max-w-3/4", "max-width: calc(3/4 * 100%)"];

        // Min Height utilities
        yield return ["min-h-0", "min-height: 0"];
        yield return ["min-h-auto", "min-height: auto"];
        yield return ["min-h-full", "min-height: 100%"];
        yield return ["min-h-screen", "min-height: 100vh"];
        yield return ["min-h-svh", "min-height: 100svh"];
        yield return ["min-h-lvh", "min-height: 100lvh"];
        yield return ["min-h-dvh", "min-height: 100dvh"];
        yield return ["min-h-min", "min-height: min-content"];
        yield return ["min-h-max", "min-height: max-content"];
        yield return ["min-h-fit", "min-height: fit-content"];
        yield return ["min-h-4", "min-height:"];  // Uses CSS variable
        yield return ["min-h-1/2", "min-height: calc(1/2 * 100%)"];
        yield return ["min-h-1/3", "min-height: calc(1/3 * 100%)"];
        yield return ["min-h-3/4", "min-height: calc(3/4 * 100%)"];

        // Max Height utilities
        yield return ["max-h-none", "max-height: none"];
        yield return ["max-h-0", "max-height: 0"];
        yield return ["max-h-full", "max-height: 100%"];
        yield return ["max-h-screen", "max-height: 100vh"];
        yield return ["max-h-svh", "max-height: 100svh"];
        yield return ["max-h-lvh", "max-height: 100lvh"];
        yield return ["max-h-dvh", "max-height: 100dvh"];
        yield return ["max-h-min", "max-height: min-content"];
        yield return ["max-h-max", "max-height: max-content"];
        yield return ["max-h-fit", "max-height: fit-content"];
        yield return ["max-h-4", "max-height:"];  // Uses CSS variable
        yield return ["max-h-1/2", "max-height: calc(1/2 * 100%)"];
        yield return ["max-h-1/3", "max-height: calc(1/3 * 100%)"];
        yield return ["max-h-3/4", "max-height: calc(3/4 * 100%)"];

        // =============================================================================
        // 11. Effects Utilities (Opacity & Box Shadow)
        // =============================================================================

        // Opacity utilities - test for property and common values
        yield return ["opacity-0", "opacity: 0"];
        yield return ["opacity-5", "opacity: 0.05"];
        yield return ["opacity-10", "opacity: 0.1"];
        yield return ["opacity-20", "opacity: 0.2"];
        yield return ["opacity-25", "opacity: 0.25"];
        yield return ["opacity-30", "opacity: 0.3"];
        yield return ["opacity-40", "opacity: 0.4"];
        yield return ["opacity-50", "opacity: 0.5"];
        yield return ["opacity-60", "opacity: 0.6"];
        yield return ["opacity-70", "opacity: 0.7"];
        yield return ["opacity-75", "opacity: 0.75"];
        yield return ["opacity-80", "opacity: 0.8"];
        yield return ["opacity-90", "opacity: 0.9"];
        yield return ["opacity-95", "opacity: 0.95"];
        yield return ["opacity-100", "opacity: 1"];

        // Box Shadow utilities - test for both --tw-shadow CSS variable and composite box-shadow
        yield return ["shadow-none", "--tw-shadow: 0 0 #0000"];
        yield return ["shadow-none", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        yield return ["shadow-sm", "--tw-shadow: 0 1px 3px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 1px 2px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1))"];
        yield return ["shadow-sm", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        yield return ["shadow", "--tw-shadow: 0 1px 3px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 1px 2px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1))"];
        yield return ["shadow", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        yield return ["shadow-md", "--tw-shadow: 0 4px 6px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 2px 4px -2px var(--tw-shadow-color, rgb(0 0 0 / 0.1))"];
        yield return ["shadow-md", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        yield return ["shadow-lg", "--tw-shadow: 0 10px 15px -3px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 4px 6px -4px var(--tw-shadow-color, rgb(0 0 0 / 0.1))"];
        yield return ["shadow-lg", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        yield return ["shadow-xl", "--tw-shadow: 0 20px 25px -5px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 8px 10px -6px var(--tw-shadow-color, rgb(0 0 0 / 0.1))"];
        yield return ["shadow-xl", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        yield return ["shadow-2xl", "--tw-shadow: 0 25px 50px -12px var(--tw-shadow-color, rgb(0 0 0 / 0.25))"];
        yield return ["shadow-2xl", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        yield return ["shadow-inner", "--tw-shadow: inset 0 2px 4px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.05))"];
        yield return ["shadow-inner", "box-shadow: var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)"];

        // Shadow Color utilities - test for --tw-shadow-color CSS variable
        yield return ["shadow-red-500", "--tw-shadow-color: oklch(63.7% 0.237 25.331)"];
        yield return ["shadow-blue-600", "--tw-shadow-color: oklch(54.6% 0.245 262.881)"];
        yield return ["shadow-blue-600/50", "--tw-shadow-color: color-mix(in oklab, oklch(54.6% 0.245 262.881) 50%, transparent)"];
        yield return ["shadow-inherit", "--tw-shadow-color: inherit"];
        yield return ["shadow-current", "--tw-shadow-color: currentcolor"];
        yield return ["shadow-transparent", "--tw-shadow-color: transparent"];
        yield return ["shadow-black", "--tw-shadow-color: #000"];

        // Text Shadow utilities - test for text-shadow CSS property
        yield return ["text-shadow-sm", "text-shadow: 0px 1px 0px var(--tw-text-shadow-color, rgb(0 0 0 / 0.075)), 0px 1px 1px var(--tw-text-shadow-color, rgb(0 0 0 / 0.075)), 0px 2px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.075))"];
        yield return ["text-shadow-md", "text-shadow: 0px 1px 1px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 1px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 2px 4px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1))"];
        yield return ["text-shadow-lg", "text-shadow: 0px 1px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 3px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 4px 8px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1))"];
        yield return ["text-shadow-none", "text-shadow: none"];

        // Text Shadow Color utilities - test for --tw-text-shadow-color CSS variable
        yield return ["text-shadow-red-500", "--tw-text-shadow-color: oklch(63.7% 0.237 25.331)"];
        yield return ["text-shadow-blue-600", "--tw-text-shadow-color: oklch(54.6% 0.245 262.881)"];
        yield return ["text-shadow-blue-600/50", "--tw-text-shadow-color: color-mix(in oklab, oklch(54.6% 0.245 262.881) 50%, transparent)"];
        yield return ["text-shadow-inherit", "--tw-text-shadow-color: inherit"];
        yield return ["text-shadow-current", "--tw-text-shadow-color: currentcolor"];
        yield return ["text-shadow-transparent", "--tw-text-shadow-color: transparent"];
        yield return ["text-shadow-black", "--tw-text-shadow-color: #000"];

        // =============================================================================
        // 12. Z-Index Utilities
        // =============================================================================

        // Z-Index utilities - static and functional
        yield return ["z-auto", "z-index: auto"];
        yield return ["z-0", "z-index: 0"];
        yield return ["z-10", "z-index: 10"];
        yield return ["z-20", "z-index: 20"];
        yield return ["z-30", "z-index: 30"];
        yield return ["z-40", "z-index: 40"];
        yield return ["z-50", "z-index: 50"];
        yield return ["-z-10", "z-index: -10"];
        yield return ["-z-20", "z-index: -20"];
        yield return ["-z-50", "z-index: -50"];

        // =============================================================================
        // 13. Flexbox Order Utilities
        // =============================================================================

        // Order utilities - static and functional
        yield return ["order-first", "order: -9999"];
        yield return ["order-last", "order: 9999"];
        yield return ["order-none", "order: 0"];
        yield return ["order-1", "order: 1"];
        yield return ["order-2", "order: 2"];
        yield return ["order-3", "order: 3"];
        yield return ["order-6", "order: 6"];
        yield return ["order-12", "order: 12"];
        yield return ["-order-1", "order: -1"];
        yield return ["-order-2", "order: -2"];
        yield return ["-order-12", "order: -12"];

        // =============================================================================
        // 14. Flexbox Grow/Shrink Utilities
        // =============================================================================

        // Flex Grow utilities
        yield return ["grow", "flex-grow: 1"];        // Default value
        yield return ["flex-grow", "flex-grow: 1"];   // Alternative name
        yield return ["grow-0", "flex-grow: 0"];
        yield return ["flex-grow-0", "flex-grow: 0"];

        // Flex Shrink utilities
        yield return ["shrink", "flex-shrink: 1"];        // Default value
        yield return ["flex-shrink", "flex-shrink: 1"];   // Alternative name
        yield return ["shrink-0", "flex-shrink: 0"];
        yield return ["flex-shrink-0", "flex-shrink: 0"];

        // =============================================================================
        // 15. Flex Utilities (flex shorthand)
        // =============================================================================

        // Flex static utilities
        yield return ["flex-auto", "flex: auto"];
        yield return ["flex-initial", "flex: initial"];
        yield return ["flex-none", "flex: none"];

        // Flex numeric utilities
        yield return ["flex-1", "flex: 1"];
        yield return ["flex-2", "flex: 2"];
        yield return ["flex-3", "flex: 3"];

        // =============================================================================
        // 16. Flex Basis Utilities
        // =============================================================================

        // Flex Basis static utilities
        yield return ["basis-auto", "flex-basis: auto"];
        yield return ["basis-full", "flex-basis: 100%"];
        yield return ["basis-min", "flex-basis: min-content"];
        yield return ["basis-max", "flex-basis: max-content"];
        yield return ["basis-fit", "flex-basis: fit-content"];

        // Flex Basis spacing utilities - test for property name since values use CSS variables
        yield return ["basis-0", "flex-basis:"];
        yield return ["basis-4", "flex-basis:"];
        yield return ["basis-px", "flex-basis: 1px"];

        // Flex Basis fraction utilities
        yield return ["basis-1/2", "flex-basis: calc(1/2 * 100%)"];
        yield return ["basis-1/3", "flex-basis: calc(1/3 * 100%)"];
        yield return ["basis-2/3", "flex-basis: calc(2/3 * 100%)"];
        yield return ["basis-1/4", "flex-basis: calc(1/4 * 100%)"];
        yield return ["basis-3/4", "flex-basis: calc(3/4 * 100%)"];

        // =============================================================================
        // 17. Grid Template Columns Utilities
        // =============================================================================

        // Grid Template Columns static utilities
        yield return ["grid-cols-none", "grid-template-columns: none"];
        yield return ["grid-cols-subgrid", "grid-template-columns: subgrid"];

        // Grid Template Columns numeric utilities
        yield return ["grid-cols-1", "grid-template-columns: repeat(1, minmax(0, 1fr))"];
        yield return ["grid-cols-2", "grid-template-columns: repeat(2, minmax(0, 1fr))"];
        yield return ["grid-cols-3", "grid-template-columns: repeat(3, minmax(0, 1fr))"];
        yield return ["grid-cols-4", "grid-template-columns: repeat(4, minmax(0, 1fr))"];
        yield return ["grid-cols-6", "grid-template-columns: repeat(6, minmax(0, 1fr))"];
        yield return ["grid-cols-12", "grid-template-columns: repeat(12, minmax(0, 1fr))"];

        // =============================================================================
        // 18. Grid Template Rows Utilities
        // =============================================================================

        // Grid Template Rows static utilities
        yield return ["grid-rows-none", "grid-template-rows: none"];
        yield return ["grid-rows-subgrid", "grid-template-rows: subgrid"];

        // Grid Template Rows numeric utilities
        yield return ["grid-rows-1", "grid-template-rows: repeat(1, minmax(0, 1fr))"];
        yield return ["grid-rows-2", "grid-template-rows: repeat(2, minmax(0, 1fr))"];
        yield return ["grid-rows-3", "grid-template-rows: repeat(3, minmax(0, 1fr))"];
        yield return ["grid-rows-4", "grid-template-rows: repeat(4, minmax(0, 1fr))"];
        yield return ["grid-rows-6", "grid-template-rows: repeat(6, minmax(0, 1fr))"];

        // =============================================================================
        // 19. Typography Utilities
        // =============================================================================

        // Font Family utilities - test for property name since values use CSS variables
        yield return ["font-sans", "font-family:"];
        yield return ["font-serif", "font-family:"];
        yield return ["font-mono", "font-family:"];

        // Font Weight utilities
        yield return ["font-thin", "font-weight: 100"];
        yield return ["font-extralight", "font-weight: 200"];
        yield return ["font-light", "font-weight: 300"];
        yield return ["font-normal", "font-weight: 400"];
        yield return ["font-medium", "font-weight: 500"];
        yield return ["font-semibold", "font-weight: 600"];
        yield return ["font-bold", "font-weight: 700"];
        yield return ["font-extrabold", "font-weight: 800"];
        yield return ["font-black", "font-weight: 900"];

        // Font Size utilities - test for property name since values use CSS variables
        yield return ["text-xs", "font-size:"];
        yield return ["text-sm", "font-size:"];
        yield return ["text-base", "font-size:"];
        yield return ["text-lg", "font-size:"];
        yield return ["text-xl", "font-size:"];
        yield return ["text-2xl", "font-size:"];
        yield return ["text-3xl", "font-size:"];
        yield return ["text-4xl", "font-size:"];

        // Line Height utilities
        yield return ["leading-none", "line-height: 1"];
        yield return ["leading-tight", "line-height: 1.25"];
        yield return ["leading-snug", "line-height: 1.375"];
        yield return ["leading-normal", "line-height: 1.5"];
        yield return ["leading-relaxed", "line-height: 1.625"];
        yield return ["leading-loose", "line-height: 2"];
        yield return ["leading-3", "line-height: 3"];
        yield return ["leading-4", "line-height: 4"];

        // Letter Spacing utilities
        yield return ["tracking-tighter", "letter-spacing: -0.05em"];
        yield return ["tracking-tight", "letter-spacing: -0.025em"];
        yield return ["tracking-normal", "letter-spacing: 0em"];
        yield return ["tracking-wide", "letter-spacing: 0.025em"];
        yield return ["tracking-wider", "letter-spacing: 0.05em"];
        yield return ["tracking-widest", "letter-spacing: 0.1em"];

        // Font Smoothing utilities
        yield return ["antialiased", "-webkit-font-smoothing: antialiased"];
        yield return ["antialiased", "-moz-osx-font-smoothing: grayscale"];
        yield return ["subpixel-antialiased", "-webkit-font-smoothing: auto"];
        yield return ["subpixel-antialiased", "-moz-osx-font-smoothing: auto"];

        // Font Variant Numeric utilities
        yield return ["normal-nums", "font-variant-numeric: normal"];
        yield return ["ordinal", "--tw-ordinal: ordinal"];
        yield return ["ordinal", "font-variant-numeric: var(--tw-ordinal,) var(--tw-slashed-zero,) var(--tw-numeric-figure,) var(--tw-numeric-spacing,) var(--tw-numeric-fraction,)"];
        yield return ["slashed-zero", "--tw-slashed-zero: slashed-zero"];
        yield return ["slashed-zero", "font-variant-numeric: var(--tw-ordinal,) var(--tw-slashed-zero,) var(--tw-numeric-figure,) var(--tw-numeric-spacing,) var(--tw-numeric-fraction,)"];
        yield return ["lining-nums", "--tw-numeric-figure: lining-nums"];
        yield return ["lining-nums", "font-variant-numeric: var(--tw-ordinal,) var(--tw-slashed-zero,) var(--tw-numeric-figure,) var(--tw-numeric-spacing,) var(--tw-numeric-fraction,)"];
        yield return ["oldstyle-nums", "--tw-numeric-figure: oldstyle-nums"];
        yield return ["proportional-nums", "--tw-numeric-spacing: proportional-nums"];
        yield return ["tabular-nums", "--tw-numeric-spacing: tabular-nums"];
        yield return ["diagonal-fractions", "--tw-numeric-fraction: diagonal-fractions"];
        yield return ["stacked-fractions", "--tw-numeric-fraction: stacked-fractions"];

        // Text Underline Offset utilities
        yield return ["underline-offset-auto", "text-underline-offset: auto"];
        yield return ["underline-offset-0", "text-underline-offset: 0px"];
        yield return ["underline-offset-1", "text-underline-offset: 1px"];
        yield return ["underline-offset-2", "text-underline-offset: 2px"];
        yield return ["underline-offset-4", "text-underline-offset: 4px"];
        yield return ["underline-offset-8", "text-underline-offset: 8px"];

        // =============================================================================
        // 20. Grid Column Span Utilities
        // =============================================================================

        // Column Span utilities
        yield return ["col-span-1", "grid-column: span 1 / span 1"];
        yield return ["col-span-2", "grid-column: span 2 / span 2"];
        yield return ["col-span-3", "grid-column: span 3 / span 3"];
        yield return ["col-span-4", "grid-column: span 4 / span 4"];
        yield return ["col-span-5", "grid-column: span 5 / span 5"];
        yield return ["col-span-6", "grid-column: span 6 / span 6"];
        yield return ["col-span-7", "grid-column: span 7 / span 7"];
        yield return ["col-span-8", "grid-column: span 8 / span 8"];
        yield return ["col-span-9", "grid-column: span 9 / span 9"];
        yield return ["col-span-10", "grid-column: span 10 / span 10"];
        yield return ["col-span-11", "grid-column: span 11 / span 11"];
        yield return ["col-span-12", "grid-column: span 12 / span 12"];
        yield return ["col-span-full", "grid-column: 1 / -1"];

        // Column Auto utilities
        yield return ["col-auto", "grid-column: auto"];

        // Column Start utilities
        yield return ["col-start-1", "grid-column-start: 1"];
        yield return ["col-start-2", "grid-column-start: 2"];
        yield return ["col-start-3", "grid-column-start: 3"];
        yield return ["col-start-4", "grid-column-start: 4"];
        yield return ["col-start-5", "grid-column-start: 5"];
        yield return ["col-start-6", "grid-column-start: 6"];
        yield return ["col-start-7", "grid-column-start: 7"];
        yield return ["col-start-8", "grid-column-start: 8"];
        yield return ["col-start-9", "grid-column-start: 9"];
        yield return ["col-start-10", "grid-column-start: 10"];
        yield return ["col-start-11", "grid-column-start: 11"];
        yield return ["col-start-12", "grid-column-start: 12"];
        yield return ["col-start-13", "grid-column-start: 13"];
        yield return ["col-start-auto", "grid-column-start: auto"];

        // Column End utilities
        yield return ["col-end-1", "grid-column-end: 1"];
        yield return ["col-end-2", "grid-column-end: 2"];
        yield return ["col-end-3", "grid-column-end: 3"];
        yield return ["col-end-4", "grid-column-end: 4"];
        yield return ["col-end-5", "grid-column-end: 5"];
        yield return ["col-end-6", "grid-column-end: 6"];
        yield return ["col-end-7", "grid-column-end: 7"];
        yield return ["col-end-8", "grid-column-end: 8"];
        yield return ["col-end-9", "grid-column-end: 9"];
        yield return ["col-end-10", "grid-column-end: 10"];
        yield return ["col-end-11", "grid-column-end: 11"];
        yield return ["col-end-12", "grid-column-end: 12"];
        yield return ["col-end-13", "grid-column-end: 13"];
        yield return ["col-end-auto", "grid-column-end: auto"];

        // =============================================================================
        // 21. Grid Row Span Utilities
        // =============================================================================

        // Row Span utilities
        yield return ["row-span-1", "grid-row: span 1 / span 1"];
        yield return ["row-span-2", "grid-row: span 2 / span 2"];
        yield return ["row-span-3", "grid-row: span 3 / span 3"];
        yield return ["row-span-4", "grid-row: span 4 / span 4"];
        yield return ["row-span-5", "grid-row: span 5 / span 5"];
        yield return ["row-span-6", "grid-row: span 6 / span 6"];
        yield return ["row-span-full", "grid-row: 1 / -1"];

        // Row Auto utilities
        yield return ["row-auto", "grid-row: auto"];

        // Row Start utilities
        yield return ["row-start-1", "grid-row-start: 1"];
        yield return ["row-start-2", "grid-row-start: 2"];
        yield return ["row-start-3", "grid-row-start: 3"];
        yield return ["row-start-4", "grid-row-start: 4"];
        yield return ["row-start-5", "grid-row-start: 5"];
        yield return ["row-start-6", "grid-row-start: 6"];
        yield return ["row-start-7", "grid-row-start: 7"];
        yield return ["row-start-auto", "grid-row-start: auto"];

        // Row End utilities
        yield return ["row-end-1", "grid-row-end: 1"];
        yield return ["row-end-2", "grid-row-end: 2"];
        yield return ["row-end-3", "grid-row-end: 3"];
        yield return ["row-end-4", "grid-row-end: 4"];
        yield return ["row-end-5", "grid-row-end: 5"];
        yield return ["row-end-6", "grid-row-end: 6"];
        yield return ["row-end-7", "grid-row-end: 7"];
        yield return ["row-end-auto", "grid-row-end: auto"];

        // =============================================================================
        // 22. Grid Auto Columns/Rows Utilities
        // =============================================================================

        // Grid Auto Columns utilities
        yield return ["auto-cols-auto", "grid-auto-columns: auto"];
        yield return ["auto-cols-min", "grid-auto-columns: min-content"];
        yield return ["auto-cols-max", "grid-auto-columns: max-content"];
        yield return ["auto-cols-fr", "grid-auto-columns: minmax(0, 1fr)"];

        // Grid Auto Rows utilities
        yield return ["auto-rows-auto", "grid-auto-rows: auto"];
        yield return ["auto-rows-min", "grid-auto-rows: min-content"];
        yield return ["auto-rows-max", "grid-auto-rows: max-content"];
        yield return ["auto-rows-fr", "grid-auto-rows: minmax(0, 1fr)"];

        // =============================================================================
        // 23. Aspect Ratio Utilities
        // =============================================================================

        // Aspect Ratio utilities
        yield return ["aspect-auto", "aspect-ratio: auto"];
        yield return ["aspect-square", "aspect-ratio: 1 / 1"];
        yield return ["aspect-video", "aspect-ratio: 16 / 9"];

        // Enhanced Aspect Ratio (arbitrary values)
        yield return ["aspect-[4/5]", "aspect-ratio: 4/5"];
        yield return ["aspect-[21/9]", "aspect-ratio: 21/9"];
        yield return ["aspect-[1.618]", "aspect-ratio: 1.618"];

        // =============================================================================
        // 24. Line Clamp Utilities
        // =============================================================================

        // Line Clamp utilities - test for multiple properties being set
        yield return ["line-clamp-none", "overflow: visible"];  // line-clamp-none sets multiple properties
        yield return ["line-clamp-1", "overflow: hidden"];      // line-clamp-1 sets multiple properties
        yield return ["line-clamp-2", "-webkit-line-clamp: 2"]; // Check webkit property is set
        yield return ["line-clamp-3", "display: -webkit-box"];  // Check display property is set
        yield return ["line-clamp-6", "-webkit-box-orient: vertical"]; // Check box-orient is set

        // =============================================================================
        // 25. Text Decoration Color Utilities
        // =============================================================================

        // Text Decoration Color utilities
        yield return ["decoration-inherit", "text-decoration-color: inherit"];
        yield return ["decoration-current", "text-decoration-color: currentcolor"];
        yield return ["decoration-transparent", "text-decoration-color: transparent"];
        yield return ["decoration-red-500", "text-decoration-color:"];  // Uses CSS variable
        yield return ["decoration-blue-600", "text-decoration-color:"];  // Uses CSS variable

        // =============================================================================
        // 26. Text Decoration Thickness Utilities
        // =============================================================================

        // Text Decoration Thickness utilities
        yield return ["decoration-auto", "text-decoration-thickness: auto"];
        yield return ["decoration-from-font", "text-decoration-thickness: from-font"];
        yield return ["decoration-0", "text-decoration-thickness: 0px"];
        yield return ["decoration-1", "text-decoration-thickness: 1px"];
        yield return ["decoration-2", "text-decoration-thickness: 2px"];
        yield return ["decoration-4", "text-decoration-thickness: 4px"];
        yield return ["decoration-8", "text-decoration-thickness: 8px"];

        // =============================================================================
        // 27. Text Indent Utilities
        // =============================================================================

        // Text Indent utilities - test for property name since values use CSS variables/calc
        yield return ["indent-0", "text-indent:"];
        yield return ["indent-px", "text-indent: 1px"];
        yield return ["indent-1", "text-indent:"];
        yield return ["indent-2", "text-indent:"];
        yield return ["indent-4", "text-indent:"];
        yield return ["-indent-4", "text-indent:"];  // Negative values

        // =============================================================================
        // 28. Vertical Align Utilities
        // =============================================================================

        // Vertical Align utilities
        yield return ["align-baseline", "vertical-align: baseline"];
        yield return ["align-top", "vertical-align: top"];
        yield return ["align-middle", "vertical-align: middle"];
        yield return ["align-bottom", "vertical-align: bottom"];
        yield return ["align-text-top", "vertical-align: text-top"];
        yield return ["align-text-bottom", "vertical-align: text-bottom"];
        yield return ["align-sub", "vertical-align: sub"];
        yield return ["align-super", "vertical-align: super"];

        // =============================================================================
        // 29. Font Stretch Utilities
        // =============================================================================

        // Font Stretch utilities
        yield return ["font-stretch-normal", "font-stretch: normal"];
        yield return ["font-stretch-ultra-condensed", "font-stretch: ultra-condensed"];
        yield return ["font-stretch-extra-condensed", "font-stretch: extra-condensed"];
        yield return ["font-stretch-condensed", "font-stretch: condensed"];
        yield return ["font-stretch-semi-condensed", "font-stretch: semi-condensed"];
        yield return ["font-stretch-semi-expanded", "font-stretch: semi-expanded"];
        yield return ["font-stretch-expanded", "font-stretch: expanded"];
        yield return ["font-stretch-extra-expanded", "font-stretch: extra-expanded"];
        yield return ["font-stretch-ultra-expanded", "font-stretch: ultra-expanded"];
        yield return ["font-stretch-[75%]", "font-stretch: 75%"];

        // =============================================================================
        // 31. Background Image Utilities
        // =============================================================================

        // Background Image utilities
        yield return ["bg-none", "background-image: none"];
        yield return ["bg-gradient-to-r", "--tw-gradient-position: to right in oklab"];      // Check gradient position CSS variable
        yield return ["bg-gradient-to-r", "background-image: linear-gradient(var(--tw-gradient-stops))"]; // Check gradient background
        yield return ["bg-gradient-to-l", "--tw-gradient-position: to left in oklab"];
        yield return ["bg-gradient-to-t", "--tw-gradient-position: to top in oklab"];
        yield return ["bg-gradient-to-b", "--tw-gradient-position: to bottom in oklab"];
        yield return ["bg-gradient-to-tr", "--tw-gradient-position: to top right in oklab"];
        yield return ["bg-gradient-to-bl", "--tw-gradient-position: to bottom left in oklab"];

        // Radial gradients
        yield return ["bg-radial", "--tw-gradient-position: in oklab"];
        yield return ["bg-radial", "background-image: radial-gradient(var(--tw-gradient-stops))"];

        // Conic gradients
        yield return ["bg-conic", "--tw-gradient-position: in oklab"];
        yield return ["bg-conic", "background-image: conic-gradient(var(--tw-gradient-stops))"];

        // =============================================================================
        // 31. Background Size Utilities
        // =============================================================================

        // Background Size utilities
        yield return ["bg-auto", "background-size: auto"];
        yield return ["bg-cover", "background-size: cover"];
        yield return ["bg-contain", "background-size: contain"];

        // =============================================================================
        // 32. Background Position Utilities
        // =============================================================================

        // Background Position utilities
        yield return ["bg-top", "background-position: top"];
        yield return ["bg-center", "background-position: center"];
        yield return ["bg-bottom", "background-position: bottom"];
        yield return ["bg-left", "background-position: left"];
        yield return ["bg-right", "background-position: right"];
        yield return ["bg-top-left", "background-position: left top"];
        yield return ["bg-top-right", "background-position: right top"];
        yield return ["bg-bottom-left", "background-position: left bottom"];
        yield return ["bg-bottom-right", "background-position: right bottom"];

        // =============================================================================
        // 33. Border Width Utilities
        // =============================================================================

        // Basic border width utilities
        yield return ["border", "border-width: 1px"];  // Default border
        yield return ["border-0", "border-width:"];    // Uses CSS variable
        yield return ["border-2", "border-width:"];    // Uses CSS variable
        yield return ["border-4", "border-width:"];    // Uses CSS variable
        yield return ["border-8", "border-width:"];    // Uses CSS variable

        // Horizontal border width utilities (using logical properties)
        yield return ["border-x", "border-inline-width: 1px"];     // Default 1px
        yield return ["border-x-0", "border-inline-width:"];       // Uses CSS variable
        yield return ["border-x-2", "border-inline-width:"];       // Uses CSS variable
        yield return ["border-x-4", "border-inline-width:"];       // Uses CSS variable
        yield return ["border-x-8", "border-inline-width:"];       // Uses CSS variable

        // Vertical border width utilities (using logical properties)
        yield return ["border-y", "border-block-width: 1px"];      // Default 1px
        yield return ["border-y-0", "border-block-width:"];        // Uses CSS variable
        yield return ["border-y-2", "border-block-width:"];        // Uses CSS variable
        yield return ["border-y-4", "border-block-width:"];        // Uses CSS variable
        yield return ["border-y-8", "border-block-width:"];        // Uses CSS variable

        // Individual side border width utilities
        yield return ["border-t", "border-top-width: 1px"];        // Default 1px
        yield return ["border-t-0", "border-top-width:"];          // Uses CSS variable
        yield return ["border-t-1", "border-top-width:"];          // Uses CSS variable
        yield return ["border-t-2", "border-top-width:"];          // Uses CSS variable
        yield return ["border-t-4", "border-top-width:"];          // Uses CSS variable
        yield return ["border-t-8", "border-top-width:"];          // Uses CSS variable

        yield return ["border-r", "border-right-width: 1px"];      // Default 1px
        yield return ["border-r-0", "border-right-width:"];        // Uses CSS variable
        yield return ["border-r-1", "border-right-width:"];        // Uses CSS variable
        yield return ["border-r-2", "border-right-width:"];        // Uses CSS variable
        yield return ["border-r-4", "border-right-width:"];        // Uses CSS variable
        yield return ["border-r-8", "border-right-width:"];        // Uses CSS variable

        yield return ["border-b", "border-bottom-width: 1px"];     // Default 1px
        yield return ["border-b-0", "border-bottom-width:"];       // Uses CSS variable
        yield return ["border-b-1", "border-bottom-width:"];       // Uses CSS variable
        yield return ["border-b-2", "border-bottom-width:"];       // Uses CSS variable
        yield return ["border-b-4", "border-bottom-width:"];       // Uses CSS variable
        yield return ["border-b-8", "border-bottom-width:"];       // Uses CSS variable

        yield return ["border-l", "border-left-width: 1px"];       // Default 1px
        yield return ["border-l-0", "border-left-width:"];         // Uses CSS variable
        yield return ["border-l-1", "border-left-width:"];         // Uses CSS variable
        yield return ["border-l-2", "border-left-width:"];         // Uses CSS variable
        yield return ["border-l-4", "border-left-width:"];         // Uses CSS variable
        yield return ["border-l-8", "border-left-width:"];         // Uses CSS variable

        // Logical property border width utilities
        yield return ["border-s", "border-inline-start-width: 1px"];  // Default 1px
        yield return ["border-s-0", "border-inline-start-width:"];    // Uses CSS variable
        yield return ["border-s-1", "border-inline-start-width:"];    // Uses CSS variable
        yield return ["border-s-2", "border-inline-start-width:"];    // Uses CSS variable
        yield return ["border-s-4", "border-inline-start-width:"];    // Uses CSS variable

        yield return ["border-e", "border-inline-end-width: 1px"];    // Default 1px
        yield return ["border-e-0", "border-inline-end-width:"];      // Uses CSS variable
        yield return ["border-e-1", "border-inline-end-width:"];      // Uses CSS variable
        yield return ["border-e-2", "border-inline-end-width:"];      // Uses CSS variable
        yield return ["border-e-4", "border-inline-end-width:"];      // Uses CSS variable

        // =============================================================================
        // 34. Border Color Utilities
        // =============================================================================

        // Basic border color utilities
        yield return ["border-transparent", "border-color: transparent"];
        yield return ["border-current", "border-color: currentColor"];
        yield return ["border-inherit", "border-color: inherit"];
        yield return ["border-black", "border-color:"];     // Uses CSS variable
        yield return ["border-white", "border-color:"];     // Uses CSS variable
        yield return ["border-red-500", "border-color:"];   // Uses CSS variable
        yield return ["border-blue-600", "border-color:"];  // Uses CSS variable
        yield return ["border-green-400", "border-color:"]; // Uses CSS variable

        // Horizontal border color utilities (using logical properties)
        yield return ["border-x-transparent", "border-inline-color: transparent"];
        yield return ["border-x-current", "border-inline-color: currentColor"];
        yield return ["border-x-inherit", "border-inline-color: inherit"];
        yield return ["border-x-red-500", "border-inline-color:"];   // Uses CSS variable
        yield return ["border-x-blue-600", "border-inline-color:"];  // Uses CSS variable

        // Vertical border color utilities (using logical properties)
        yield return ["border-y-transparent", "border-block-color: transparent"];
        yield return ["border-y-current", "border-block-color: currentColor"];
        yield return ["border-y-inherit", "border-block-color: inherit"];
        yield return ["border-y-red-500", "border-block-color:"];    // Uses CSS variable
        yield return ["border-y-blue-600", "border-block-color:"];   // Uses CSS variable

        // Individual side border color utilities
        yield return ["border-t-transparent", "border-top-color: transparent"];
        yield return ["border-t-current", "border-top-color: currentColor"];
        yield return ["border-t-inherit", "border-top-color: inherit"];
        yield return ["border-t-red-500", "border-top-color:"];      // Uses CSS variable
        yield return ["border-t-blue-600", "border-top-color:"];     // Uses CSS variable

        yield return ["border-r-transparent", "border-right-color: transparent"];
        yield return ["border-r-current", "border-right-color: currentColor"];
        yield return ["border-r-inherit", "border-right-color: inherit"];
        yield return ["border-r-red-500", "border-right-color:"];    // Uses CSS variable
        yield return ["border-r-blue-600", "border-right-color:"];   // Uses CSS variable

        yield return ["border-b-transparent", "border-bottom-color: transparent"];
        yield return ["border-b-current", "border-bottom-color: currentColor"];
        yield return ["border-b-inherit", "border-bottom-color: inherit"];
        yield return ["border-b-red-500", "border-bottom-color:"];   // Uses CSS variable
        yield return ["border-b-blue-600", "border-bottom-color:"];  // Uses CSS variable

        yield return ["border-l-transparent", "border-left-color: transparent"];
        yield return ["border-l-current", "border-left-color: currentColor"];
        yield return ["border-l-inherit", "border-left-color: inherit"];
        yield return ["border-l-red-500", "border-left-color:"];     // Uses CSS variable
        yield return ["border-l-blue-600", "border-left-color:"];    // Uses CSS variable

        // Logical property border color utilities
        yield return ["border-s-transparent", "border-inline-start-color: transparent"];
        yield return ["border-s-current", "border-inline-start-color: currentColor"];
        yield return ["border-s-inherit", "border-inline-start-color: inherit"];
        yield return ["border-s-red-500", "border-inline-start-color:"];  // Uses CSS variable
        yield return ["border-s-blue-600", "border-inline-start-color:"]; // Uses CSS variable

        yield return ["border-e-transparent", "border-inline-end-color: transparent"];
        yield return ["border-e-current", "border-inline-end-color: currentColor"];
        yield return ["border-e-inherit", "border-inline-end-color: inherit"];
        yield return ["border-e-red-500", "border-inline-end-color:"];    // Uses CSS variable
        yield return ["border-e-blue-600", "border-inline-end-color:"];   // Uses CSS variable

        // =============================================================================
        // 35. Outline Utilities
        // =============================================================================

        // Outline width utilities
        yield return ["outline-0", "outline-style: var(--tw-outline-style)"];
        yield return ["outline-0", "outline-width:"];    // Uses CSS variable
        yield return ["outline-1", "outline-style: var(--tw-outline-style)"];
        yield return ["outline-1", "outline-width:"];    // Uses CSS variable
        yield return ["outline-2", "outline-style: var(--tw-outline-style)"];
        yield return ["outline-2", "outline-width:"];    // Uses CSS variable calc(var(--spacing) * 2)
        yield return ["outline-4", "outline-style: var(--tw-outline-style)"];
        yield return ["outline-4", "outline-width:"];    // Uses CSS variable
        yield return ["outline-8", "outline-style: var(--tw-outline-style)"];
        yield return ["outline-8", "outline-width:"];    // Uses CSS variable

        // Outline color utilities
        yield return ["outline-transparent", "outline-color: transparent"];
        yield return ["outline-current", "outline-color: currentColor"];
        yield return ["outline-inherit", "outline-color: inherit"];
        yield return ["outline-red-500", "outline-color:"];   // Uses CSS variable
        yield return ["outline-blue-600", "outline-color:"];  // Uses CSS variable

        // Outline style utilities
        yield return ["outline-none", "--tw-outline-style: none"];
        yield return ["outline-none", "outline-style: none"];
        yield return ["outline-dashed", "--tw-outline-style: dashed"];
        yield return ["outline-dashed", "outline-style: dashed"];
        yield return ["outline-dotted", "--tw-outline-style: dotted"];
        yield return ["outline-dotted", "outline-style: dotted"];
        yield return ["outline-double", "--tw-outline-style: double"];
        yield return ["outline-double", "outline-style: double"];

        // Outline offset utilities
        yield return ["outline-offset-0", "outline-offset:"];    // Uses CSS variable
        yield return ["outline-offset-1", "outline-offset:"];    // Uses CSS variable
        yield return ["outline-offset-2", "outline-offset:"];    // Uses CSS variable calc(var(--spacing) * 2)
        yield return ["outline-offset-4", "outline-offset:"];    // Uses CSS variable
        yield return ["outline-offset-8", "outline-offset:"];    // Uses CSS variable
        yield return ["-outline-offset-1", "outline-offset:"];   // Uses CSS variable (negative)
        yield return ["-outline-offset-2", "outline-offset:"];   // Uses CSS variable (negative)

        // =============================================================================
        // 36. Border Spacing Utilities (Table-specific)
        // =============================================================================

        // Basic border spacing utilities - sets both horizontal and vertical
        yield return ["border-spacing-0", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-px", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-1", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-2", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-4", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-8", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];

        // Horizontal border spacing utilities
        yield return ["border-spacing-x-0", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-x-px", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-x-1", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-x-2", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-x-4", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];

        // Vertical border spacing utilities
        yield return ["border-spacing-y-0", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-y-px", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-y-1", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-y-2", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];
        yield return ["border-spacing-y-4", "border-spacing: var(--tw-border-spacing-x) var(--tw-border-spacing-y)"];

        // Additional tests to verify CSS variable setting
        yield return ["border-spacing-2", "--tw-border-spacing-x:"];  // Verify CSS variable is set
        yield return ["border-spacing-2", "--tw-border-spacing-y:"];  // Verify CSS variable is set
        yield return ["border-spacing-x-4", "--tw-border-spacing-x:"]; // Verify X variable is set
        yield return ["border-spacing-y-4", "--tw-border-spacing-y:"]; // Verify Y variable is set

        // =============================================================================
        // 36. Border Radius Utilities
        // =============================================================================

        // Basic border radius utilities
        yield return ["rounded", "border-radius: 0.25rem"];
        yield return ["rounded-none", "border-radius: 0"];
        yield return ["rounded-sm", "border-radius:"];        // Uses CSS variable
        yield return ["rounded-md", "border-radius:"];        // Uses CSS variable
        yield return ["rounded-lg", "border-radius:"];        // Uses CSS variable
        yield return ["rounded-xl", "border-radius:"];        // Uses CSS variable
        yield return ["rounded-2xl", "border-radius:"];       // Uses CSS variable
        yield return ["rounded-3xl", "border-radius:"];       // Uses CSS variable
        yield return ["rounded-full", "border-radius: calc(infinity * 1px)"];

        // Top border radius utilities (top-left + top-right)
        yield return ["rounded-t-none", "border-top-left-radius: 0"];
        yield return ["rounded-t-sm", "border-top-left-radius:"];     // Uses CSS variable
        yield return ["rounded-t-md", "border-top-right-radius:"];    // Uses CSS variable (test both properties are set)
        yield return ["rounded-t-lg", "border-top-left-radius:"];     // Uses CSS variable
        yield return ["rounded-t-xl", "border-top-right-radius:"];    // Uses CSS variable
        yield return ["rounded-t-2xl", "border-top-left-radius:"];    // Uses CSS variable
        yield return ["rounded-t-full", "border-top-left-radius: calc(infinity * 1px)"];

        // Right border radius utilities (top-right + bottom-right)
        yield return ["rounded-r-none", "border-top-right-radius: 0"];
        yield return ["rounded-r-sm", "border-top-right-radius:"];    // Uses CSS variable
        yield return ["rounded-r-md", "border-bottom-right-radius:"];  // Uses CSS variable (test both properties are set)
        yield return ["rounded-r-lg", "border-top-right-radius:"];    // Uses CSS variable
        yield return ["rounded-r-xl", "border-bottom-right-radius:"];  // Uses CSS variable
        yield return ["rounded-r-2xl", "border-top-right-radius:"];   // Uses CSS variable
        yield return ["rounded-r-full", "border-top-right-radius: calc(infinity * 1px)"];

        // Bottom border radius utilities (bottom-right + bottom-left)
        yield return ["rounded-b-none", "border-bottom-right-radius: 0"];
        yield return ["rounded-b-sm", "border-bottom-right-radius:"];  // Uses CSS variable
        yield return ["rounded-b-md", "border-bottom-left-radius:"];   // Uses CSS variable (test both properties are set)
        yield return ["rounded-b-lg", "border-bottom-right-radius:"];  // Uses CSS variable
        yield return ["rounded-b-xl", "border-bottom-left-radius:"];   // Uses CSS variable
        yield return ["rounded-b-2xl", "border-bottom-right-radius:"]; // Uses CSS variable
        yield return ["rounded-b-full", "border-bottom-right-radius: calc(infinity * 1px)"];

        // Left border radius utilities (top-left + bottom-left)
        yield return ["rounded-l-none", "border-top-left-radius: 0"];
        yield return ["rounded-l-sm", "border-top-left-radius:"];     // Uses CSS variable
        yield return ["rounded-l-md", "border-bottom-left-radius:"];  // Uses CSS variable (test both properties are set)
        yield return ["rounded-l-lg", "border-top-left-radius:"];     // Uses CSS variable
        yield return ["rounded-l-xl", "border-bottom-left-radius:"];  // Uses CSS variable
        yield return ["rounded-l-2xl", "border-top-left-radius:"];    // Uses CSS variable
        yield return ["rounded-l-full", "border-top-left-radius: calc(infinity * 1px)"];

        // Individual corner border radius utilities
        yield return ["rounded-tl-none", "border-top-left-radius: 0"];
        yield return ["rounded-tl-sm", "border-top-left-radius:"];    // Uses CSS variable
        yield return ["rounded-tl-md", "border-top-left-radius:"];    // Uses CSS variable
        yield return ["rounded-tl-lg", "border-top-left-radius:"];    // Uses CSS variable
        yield return ["rounded-tl-xl", "border-top-left-radius:"];    // Uses CSS variable
        yield return ["rounded-tl-2xl", "border-top-left-radius:"];   // Uses CSS variable
        yield return ["rounded-tl-3xl", "border-top-left-radius:"];   // Uses CSS variable
        yield return ["rounded-tl-full", "border-top-left-radius: calc(infinity * 1px)"];

        yield return ["rounded-tr-none", "border-top-right-radius: 0"];
        yield return ["rounded-tr-sm", "border-top-right-radius:"];   // Uses CSS variable
        yield return ["rounded-tr-md", "border-top-right-radius:"];   // Uses CSS variable
        yield return ["rounded-tr-lg", "border-top-right-radius:"];   // Uses CSS variable
        yield return ["rounded-tr-xl", "border-top-right-radius:"];   // Uses CSS variable
        yield return ["rounded-tr-2xl", "border-top-right-radius:"];  // Uses CSS variable
        yield return ["rounded-tr-3xl", "border-top-right-radius:"];  // Uses CSS variable
        yield return ["rounded-tr-full", "border-top-right-radius: calc(infinity * 1px)"];

        yield return ["rounded-bl-none", "border-bottom-left-radius: 0"];
        yield return ["rounded-bl-sm", "border-bottom-left-radius:"];   // Uses CSS variable
        yield return ["rounded-bl-md", "border-bottom-left-radius:"];   // Uses CSS variable
        yield return ["rounded-bl-lg", "border-bottom-left-radius:"];   // Uses CSS variable
        yield return ["rounded-bl-xl", "border-bottom-left-radius:"];   // Uses CSS variable
        yield return ["rounded-bl-2xl", "border-bottom-left-radius:"];  // Uses CSS variable
        yield return ["rounded-bl-3xl", "border-bottom-left-radius:"];  // Uses CSS variable
        yield return ["rounded-bl-full", "border-bottom-left-radius: calc(infinity * 1px)"];

        yield return ["rounded-br-none", "border-bottom-right-radius: 0"];
        yield return ["rounded-br-sm", "border-bottom-right-radius:"];  // Uses CSS variable
        yield return ["rounded-br-md", "border-bottom-right-radius:"];  // Uses CSS variable
        yield return ["rounded-br-lg", "border-bottom-right-radius:"];  // Uses CSS variable
        yield return ["rounded-br-xl", "border-bottom-right-radius:"];  // Uses CSS variable
        yield return ["rounded-br-2xl", "border-bottom-right-radius:"]; // Uses CSS variable
        yield return ["rounded-br-3xl", "border-bottom-right-radius:"]; // Uses CSS variable
        yield return ["rounded-br-full", "border-bottom-right-radius: calc(infinity * 1px)"];

        // Logical corner border radius utilities
        yield return ["rounded-ss-none", "border-start-start-radius: 0"];
        yield return ["rounded-ss-sm", "border-start-start-radius:"];   // Uses CSS variable
        yield return ["rounded-ss-md", "border-start-start-radius:"];   // Uses CSS variable
        yield return ["rounded-ss-lg", "border-start-start-radius:"];   // Uses CSS variable
        yield return ["rounded-ss-xl", "border-start-start-radius:"];   // Uses CSS variable
        yield return ["rounded-ss-2xl", "border-start-start-radius:"];  // Uses CSS variable
        yield return ["rounded-ss-full", "border-start-start-radius: calc(infinity * 1px)"];

        yield return ["rounded-se-none", "border-start-end-radius: 0"];
        yield return ["rounded-se-sm", "border-start-end-radius:"];     // Uses CSS variable
        yield return ["rounded-se-md", "border-start-end-radius:"];     // Uses CSS variable
        yield return ["rounded-se-lg", "border-start-end-radius:"];     // Uses CSS variable
        yield return ["rounded-se-xl", "border-start-end-radius:"];     // Uses CSS variable
        yield return ["rounded-se-2xl", "border-start-end-radius:"];    // Uses CSS variable
        yield return ["rounded-se-full", "border-start-end-radius: calc(infinity * 1px)"];

        yield return ["rounded-ee-none", "border-end-end-radius: 0"];
        yield return ["rounded-ee-sm", "border-end-end-radius:"];       // Uses CSS variable
        yield return ["rounded-ee-md", "border-end-end-radius:"];       // Uses CSS variable
        yield return ["rounded-ee-lg", "border-end-end-radius:"];       // Uses CSS variable
        yield return ["rounded-ee-xl", "border-end-end-radius:"];       // Uses CSS variable
        yield return ["rounded-ee-2xl", "border-end-end-radius:"];      // Uses CSS variable
        yield return ["rounded-ee-full", "border-end-end-radius: calc(infinity * 1px)"];

        yield return ["rounded-es-none", "border-end-start-radius: 0"];
        yield return ["rounded-es-sm", "border-end-start-radius:"];     // Uses CSS variable
        yield return ["rounded-es-md", "border-end-start-radius:"];     // Uses CSS variable
        yield return ["rounded-es-lg", "border-end-start-radius:"];     // Uses CSS variable
        yield return ["rounded-es-xl", "border-end-start-radius:"];     // Uses CSS variable
        yield return ["rounded-es-2xl", "border-end-start-radius:"];    // Uses CSS variable
        yield return ["rounded-es-full", "border-end-start-radius: calc(infinity * 1px)"];

        // Arbitrary value border radius utilities
        yield return ["rounded-[10px]", "border-radius: 10px"];
        yield return ["rounded-[0.5rem]", "border-radius: 0.5rem"];
        yield return ["rounded-t-[5px]", "border-top-left-radius: 5px"];
        yield return ["rounded-t-[5px]", "border-top-right-radius: 5px"];  // Test both corners are set
        yield return ["rounded-tl-[8px]", "border-top-left-radius: 8px"];
        yield return ["rounded-ss-[12px]", "border-start-start-radius: 12px"];

        // =============================================================================
        // 37. Gradient Stop Utilities
        // =============================================================================

        // Gradient From utilities - test for CSS variable assignment
        yield return ["from-transparent", "--tw-gradient-from: transparent"];
        yield return ["from-current", "--tw-gradient-from:"];      // Uses CSS variable
        yield return ["from-inherit", "--tw-gradient-from: inherit"];
        yield return ["from-red-500", "--tw-gradient-from:"];      // Uses CSS variable
        yield return ["from-blue-600", "--tw-gradient-from:"];     // Uses CSS variable
        yield return ["from-green-400", "--tw-gradient-from:"];    // Uses CSS variable

        // Test gradient stops system for from utilities
        yield return ["from-red-500", "--tw-gradient-stops: var(--tw-gradient-via-stops, var(--tw-gradient-position), var(--tw-gradient-from) var(--tw-gradient-from-position), var(--tw-gradient-to) var(--tw-gradient-to-position))"];

        // Gradient Via utilities - test for CSS variable assignment
        yield return ["via-transparent", "--tw-gradient-via: transparent"];
        yield return ["via-current", "--tw-gradient-via:"];        // Uses CSS variable
        yield return ["via-inherit", "--tw-gradient-via: inherit"];
        yield return ["via-red-500", "--tw-gradient-via:"];        // Uses CSS variable
        yield return ["via-blue-600", "--tw-gradient-via:"];       // Uses CSS variable
        yield return ["via-green-400", "--tw-gradient-via:"];      // Uses CSS variable

        // Test complex gradient via stops system
        yield return ["via-blue-500", "--tw-gradient-via-stops: var(--tw-gradient-position), var(--tw-gradient-from) var(--tw-gradient-from-position), var(--tw-gradient-via) var(--tw-gradient-via-position), var(--tw-gradient-to) var(--tw-gradient-to-position)"];
        yield return ["via-blue-500", "--tw-gradient-stops: var(--tw-gradient-via-stops)"];

        // Gradient To utilities - test for CSS variable assignment
        yield return ["to-transparent", "--tw-gradient-to: transparent"];
        yield return ["to-current", "--tw-gradient-to:"];          // Uses CSS variable
        yield return ["to-inherit", "--tw-gradient-to: inherit"];
        yield return ["to-red-500", "--tw-gradient-to:"];          // Uses CSS variable
        yield return ["to-blue-600", "--tw-gradient-to:"];         // Uses CSS variable
        yield return ["to-green-400", "--tw-gradient-to:"];        // Uses CSS variable

        // Test gradient stops system for to utilities
        yield return ["to-green-500", "--tw-gradient-stops: var(--tw-gradient-via-stops, var(--tw-gradient-position), var(--tw-gradient-from) var(--tw-gradient-from-position), var(--tw-gradient-to) var(--tw-gradient-to-position))"];

        // Gradient Stop utilities with opacity modifiers - test for color-mix usage
        yield return ["from-red-500/50", "--tw-gradient-from: color-mix(in oklab,"];  // Test color-mix is used
        yield return ["via-blue-500/25", "--tw-gradient-via: color-mix(in oklab,"];   // Test color-mix is used
        yield return ["to-green-500/75", "--tw-gradient-to: color-mix(in oklab,"];    // Test color-mix is used

        // Test specific gradient colors mentioned in the user's request
        yield return ["from-red-600", "--tw-gradient-from: var(--color-red-600)"];
        yield return ["to-red-600", "--tw-gradient-to: var(--color-red-600)"];
        yield return ["to-sky-400", "--tw-gradient-to: var(--color-sky-400)"];

        // =============================================================================
        // 38. Transform Utilities
        // =============================================================================

        // Scale utilities
        yield return ["scale-0", "--tw-scale-x: 0%"];
        yield return ["scale-50", "--tw-scale-y: 50%"];
        yield return ["scale-75", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-90", "--tw-scale-x: 90%"];
        yield return ["scale-95", "--tw-scale-y: 95%"];
        yield return ["scale-100", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-105", "--tw-scale-x: 105%"];
        yield return ["scale-110", "--tw-scale-y: 110%"];
        yield return ["scale-125", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-150", "--tw-scale-x: 150%"];
        yield return ["-scale-50", "--tw-scale-x: calc(50% * -1)"];

        // Scale X utilities
        yield return ["scale-x-0", "--tw-scale-x: 0%"];
        yield return ["scale-x-50", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-x-75", "--tw-scale-x: 75%"];
        yield return ["scale-x-90", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-x-95", "--tw-scale-x: 95%"];
        yield return ["scale-x-100", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-x-105", "--tw-scale-x: 105%"];
        yield return ["scale-x-110", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-x-125", "--tw-scale-x: 125%"];
        yield return ["scale-x-150", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["-scale-x-50", "--tw-scale-x: calc(50% * -1)"];

        // Scale Y utilities
        yield return ["scale-y-0", "--tw-scale-y: 0%"];
        yield return ["scale-y-50", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-y-75", "--tw-scale-y: 75%"];
        yield return ["scale-y-90", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-y-95", "--tw-scale-y: 95%"];
        yield return ["scale-y-100", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-y-105", "--tw-scale-y: 105%"];
        yield return ["scale-y-110", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["scale-y-125", "--tw-scale-y: 125%"];
        yield return ["scale-y-150", "scale: var(--tw-scale-x) var(--tw-scale-y)"];
        yield return ["-scale-y-50", "--tw-scale-y: calc(50% * -1)"];

        // Rotate utilities
        yield return ["rotate-0", "rotate: 0deg"];
        yield return ["rotate-1", "rotate: 1deg"];
        yield return ["rotate-2", "rotate: 2deg"];
        yield return ["rotate-3", "rotate: 3deg"];
        yield return ["rotate-6", "rotate: 6deg"];
        yield return ["rotate-12", "rotate: 12deg"];
        yield return ["rotate-45", "rotate: 45deg"];
        yield return ["rotate-90", "rotate: 90deg"];
        yield return ["rotate-180", "rotate: 180deg"];
        yield return ["-rotate-1", "rotate: calc(1deg * -1)"];
        yield return ["-rotate-45", "rotate: calc(45deg * -1)"];
        yield return ["-rotate-90", "rotate: calc(90deg * -1)"];

        // Rotate X utilities (3D)
        yield return ["rotate-x-0", "--tw-rotate-x: rotateX(0deg)"];
        yield return ["rotate-x-1", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-x-2", "--tw-rotate-x: rotateX(2deg)"];
        yield return ["rotate-x-3", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-x-6", "--tw-rotate-x: rotateX(6deg)"];
        yield return ["rotate-x-12", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-x-45", "--tw-rotate-x: rotateX(45deg)"];
        yield return ["rotate-x-90", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-x-180", "--tw-rotate-x: rotateX(180deg)"];
        yield return ["-rotate-x-45", "--tw-rotate-x: rotateX(calc(45deg * -1))"];

        // Rotate Y utilities (3D)
        yield return ["rotate-y-0", "--tw-rotate-y: rotateY(0deg)"];
        yield return ["rotate-y-1", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-y-2", "--tw-rotate-y: rotateY(2deg)"];
        yield return ["rotate-y-3", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-y-6", "--tw-rotate-y: rotateY(6deg)"];
        yield return ["rotate-y-12", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-y-45", "--tw-rotate-y: rotateY(45deg)"];
        yield return ["rotate-y-90", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["rotate-y-180", "--tw-rotate-y: rotateY(180deg)"];
        yield return ["-rotate-y-45", "--tw-rotate-y: rotateY(calc(45deg * -1))"];

        // Translate X utilities
        yield return ["translate-x-0", "--tw-translate-x: calc(var(--spacing) * 0)"];
        yield return ["translate-x-1", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-x-2", "--tw-translate-x: calc(var(--spacing) * 2)"];
        yield return ["translate-x-4", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-x-8", "--tw-translate-x: calc(var(--spacing) * 8)"];
        yield return ["translate-x-12", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-x-16", "--tw-translate-x: calc(var(--spacing) * 16)"];
        yield return ["translate-x-full", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-x-1/2", "--tw-translate-x: calc(1/2 * 100%)"];
        yield return ["translate-x-1/3", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-x-2/3", "--tw-translate-x: calc(2/3 * 100%)"];
        yield return ["translate-x-1/4", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["-translate-x-1", "--tw-translate-x: calc(var(--spacing) * -1)"];
        yield return ["-translate-x-4", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["-translate-x-1/2", "--tw-translate-x: calc(calc(1/2 * 100%) * -1)"];

        // Translate Y utilities
        yield return ["translate-y-0", "--tw-translate-y: calc(var(--spacing) * 0)"];
        yield return ["translate-y-1", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-y-2", "--tw-translate-y: calc(var(--spacing) * 2)"];
        yield return ["translate-y-4", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-y-8", "--tw-translate-y: calc(var(--spacing) * 8)"];
        yield return ["translate-y-12", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-y-16", "--tw-translate-y: calc(var(--spacing) * 16)"];
        yield return ["translate-y-full", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-y-1/2", "--tw-translate-y: calc(1/2 * 100%)"];
        yield return ["translate-y-1/3", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["translate-y-2/3", "--tw-translate-y: calc(2/3 * 100%)"];
        yield return ["translate-y-1/4", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["-translate-y-1", "--tw-translate-y: calc(var(--spacing) * -1)"];
        yield return ["-translate-y-4", "translate: var(--tw-translate-x) var(--tw-translate-y)"];
        yield return ["-translate-y-1/2", "--tw-translate-y: calc(calc(1/2 * 100%) * -1)"];

        // Skew X utilities
        yield return ["skew-x-0", "--tw-skew-x: skewX(0deg)"];
        yield return ["skew-x-1", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["skew-x-2", "--tw-skew-x: skewX(2deg)"];
        yield return ["skew-x-3", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["skew-x-6", "--tw-skew-x: skewX(6deg)"];
        yield return ["skew-x-12", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["-skew-x-1", "--tw-skew-x: skewX(calc(1deg * -1))"];
        yield return ["-skew-x-3", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["-skew-x-12", "--tw-skew-x: skewX(calc(12deg * -1))"];

        // Skew Y utilities
        yield return ["skew-y-0", "--tw-skew-y: skewY(0deg)"];
        yield return ["skew-y-1", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["skew-y-2", "--tw-skew-y: skewY(2deg)"];
        yield return ["skew-y-3", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["skew-y-6", "--tw-skew-y: skewY(6deg)"];
        yield return ["skew-y-12", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["-skew-y-1", "--tw-skew-y: skewY(calc(1deg * -1))"];
        yield return ["-skew-y-3", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["-skew-y-12", "--tw-skew-y: skewY(calc(12deg * -1))"];

        // Transform Origin utilities
        yield return ["origin-center", "transform-origin: center"];
        yield return ["origin-top", "transform-origin: top"];
        yield return ["origin-top-right", "transform-origin: top right"];
        yield return ["origin-right", "transform-origin: right"];
        yield return ["origin-bottom-right", "transform-origin: bottom right"];
        yield return ["origin-bottom", "transform-origin: bottom"];
        yield return ["origin-bottom-left", "transform-origin: bottom left"];
        yield return ["origin-left", "transform-origin: left"];
        yield return ["origin-top-left", "transform-origin: top left"];

        // Transform Style utilities
        yield return ["transform-flat", "transform-style: flat"];
        yield return ["transform-3d", "transform-style: preserve-3d"];

        // Transform Box utilities
        yield return ["transform-content", "transform-box: content-box"];
        yield return ["transform-border", "transform-box: border-box"];
        yield return ["transform-fill", "transform-box: fill-box"];
        yield return ["transform-stroke", "transform-box: stroke-box"];
        yield return ["transform-view", "transform-box: view-box"];

        // =============================================================================
        // 39. Animation Utilities
        // =============================================================================

        // Animation utilities
        yield return ["animate-none", "animation: none"];
        yield return ["animate-spin", "animation: var(--animate-spin)"];
        yield return ["animate-ping", "animation: var(--animate-ping)"];
        yield return ["animate-pulse", "animation: var(--animate-pulse)"];
        yield return ["animate-bounce", "animation: var(--animate-bounce)"];

        // =============================================================================
        // 39. Transition Utilities
        // =============================================================================

        // Transition utilities - test for transition-property since they set multiple properties
        yield return ["transition", "transition-property:"];           // Default transition
        yield return ["transition-none", "transition-property: none"];
        yield return ["transition-all", "transition-property: all"];
        yield return ["transition-colors", "transition-property: color, background-color"];  // Check color properties are included
        yield return ["transition-opacity", "transition-property: opacity"];
        yield return ["transition-shadow", "transition-property: box-shadow"];
        yield return ["transition-transform", "transition-property: transform"];

        // Test that transitions include timing function and duration (except none)
        yield return ["transition", "transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1)"];
        yield return ["transition", "transition-duration: 150ms"];
        yield return ["transition-all", "transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1)"];
        yield return ["transition-all", "transition-duration: 150ms"];

        // =============================================================================
        // 40. Transition Duration Utilities
        // =============================================================================

        // Duration utilities - test for both CSS variable and property
        yield return ["duration-75", "--tw-duration: 75ms"];
        yield return ["duration-75", "transition-duration: 75ms"];
        yield return ["duration-100", "--tw-duration: 100ms"];
        yield return ["duration-100", "transition-duration: 100ms"];
        yield return ["duration-150", "--tw-duration: 150ms"];
        yield return ["duration-150", "transition-duration: 150ms"];
        yield return ["duration-200", "--tw-duration: 200ms"];
        yield return ["duration-200", "transition-duration: 200ms"];
        yield return ["duration-300", "--tw-duration: 300ms"];
        yield return ["duration-300", "transition-duration: 300ms"];
        yield return ["duration-500", "--tw-duration: 500ms"];
        yield return ["duration-500", "transition-duration: 500ms"];
        yield return ["duration-700", "--tw-duration: 700ms"];
        yield return ["duration-700", "transition-duration: 700ms"];
        yield return ["duration-1000", "--tw-duration: 1000ms"];
        yield return ["duration-1000", "transition-duration: 1000ms"];

        // =============================================================================
        // 41. Transition Timing Function Utilities
        // =============================================================================

        // Timing function utilities - test for both CSS variable and property
        yield return ["ease-linear", "--tw-ease: linear"];
        yield return ["ease-linear", "transition-timing-function: linear"];
        yield return ["ease-in", "--tw-ease: var(--ease-in)"];
        yield return ["ease-in", "transition-timing-function: cubic-bezier(0.4, 0, 1, 1)"];
        yield return ["ease-out", "--tw-ease: var(--ease-out)"];
        yield return ["ease-out", "transition-timing-function: cubic-bezier(0, 0, 0.2, 1)"];
        yield return ["ease-in-out", "--tw-ease: var(--ease-in-out)"];
        yield return ["ease-in-out", "transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1)"];

        // =============================================================================
        // 42. Transition Delay Utilities
        // =============================================================================

        // Delay utilities
        yield return ["delay-75", "transition-delay: 75ms"];
        yield return ["delay-100", "transition-delay: 100ms"];
        yield return ["delay-150", "transition-delay: 150ms"];
        yield return ["delay-200", "transition-delay: 200ms"];
        yield return ["delay-300", "transition-delay: 300ms"];
        yield return ["delay-500", "transition-delay: 500ms"];
        yield return ["delay-700", "transition-delay: 700ms"];
        yield return ["delay-1000", "transition-delay: 1000ms"];

        // =============================================================================
        // 43. Transition Behavior Utilities
        // =============================================================================

        // Transition behavior utilities
        yield return ["transition-normal", "transition-behavior: normal"];
        yield return ["transition-discrete", "transition-behavior: allow-discrete"];

        // =============================================================================
        // 44. Will Change Utilities
        // =============================================================================

        // Will Change utilities
        yield return ["will-change-auto", "will-change: auto"];
        yield return ["will-change-scroll", "will-change: scroll-position"];
        yield return ["will-change-contents", "will-change: contents"];
        yield return ["will-change-transform", "will-change: transform"];

        // =============================================================================
        // 44. Cursor Utilities
        // =============================================================================

        // Basic cursor utilities
        yield return ["cursor-auto", "cursor: auto"];
        yield return ["cursor-default", "cursor: default"];
        yield return ["cursor-pointer", "cursor: pointer"];
        yield return ["cursor-wait", "cursor: wait"];
        yield return ["cursor-text", "cursor: text"];
        yield return ["cursor-move", "cursor: move"];
        yield return ["cursor-help", "cursor: help"];
        yield return ["cursor-not-allowed", "cursor: not-allowed"];
        yield return ["cursor-none", "cursor: none"];

        // Interactive cursor utilities
        yield return ["cursor-context-menu", "cursor: context-menu"];
        yield return ["cursor-progress", "cursor: progress"];
        yield return ["cursor-cell", "cursor: cell"];
        yield return ["cursor-crosshair", "cursor: crosshair"];
        yield return ["cursor-vertical-text", "cursor: vertical-text"];
        yield return ["cursor-alias", "cursor: alias"];
        yield return ["cursor-copy", "cursor: copy"];
        yield return ["cursor-no-drop", "cursor: no-drop"];
        yield return ["cursor-grab", "cursor: grab"];
        yield return ["cursor-grabbing", "cursor: grabbing"];
        yield return ["cursor-all-scroll", "cursor: all-scroll"];

        // Resize cursor utilities
        yield return ["cursor-col-resize", "cursor: col-resize"];
        yield return ["cursor-row-resize", "cursor: row-resize"];
        yield return ["cursor-n-resize", "cursor: n-resize"];
        yield return ["cursor-e-resize", "cursor: e-resize"];
        yield return ["cursor-s-resize", "cursor: s-resize"];
        yield return ["cursor-w-resize", "cursor: w-resize"];
        yield return ["cursor-ne-resize", "cursor: ne-resize"];
        yield return ["cursor-nw-resize", "cursor: nw-resize"];
        yield return ["cursor-se-resize", "cursor: se-resize"];
        yield return ["cursor-sw-resize", "cursor: sw-resize"];
        yield return ["cursor-ew-resize", "cursor: ew-resize"];
        yield return ["cursor-ns-resize", "cursor: ns-resize"];
        yield return ["cursor-nesw-resize", "cursor: nesw-resize"];
        yield return ["cursor-nwse-resize", "cursor: nwse-resize"];

        // Zoom cursor utilities
        yield return ["cursor-zoom-in", "cursor: zoom-in"];
        yield return ["cursor-zoom-out", "cursor: zoom-out"];

        // Arbitrary cursor utilities - test basic functionality
        yield return ["cursor-[pointer]", "cursor: pointer"];
        yield return ["cursor-[url(hand.cur)]", "cursor: url(hand.cur)"];
        yield return ["cursor-[url(hand.cur),_pointer]", "cursor: url(hand.cur), pointer"];

        // =============================================================================
        // 45. Pointer Events Utilities
        // =============================================================================

        // Pointer Events utilities
        yield return ["pointer-events-auto", "pointer-events: auto"];
        yield return ["pointer-events-none", "pointer-events: none"];

        // =============================================================================
        // 46. Resize Utilities
        // =============================================================================

        // Resize utilities
        yield return ["resize-none", "resize: none"];
        yield return ["resize", "resize: both"];
        yield return ["resize-x", "resize: horizontal"];
        yield return ["resize-y", "resize: vertical"];

        // =============================================================================
        // 47. User Select Utilities
        // =============================================================================

        // User Select utilities - test for webkit prefix included
        yield return ["select-none", "-webkit-user-select: none"];
        yield return ["select-none", "user-select: none"];
        yield return ["select-text", "-webkit-user-select: text"];
        yield return ["select-text", "user-select: text"];
        yield return ["select-all", "-webkit-user-select: all"];
        yield return ["select-all", "user-select: all"];
        yield return ["select-auto", "-webkit-user-select: auto"];
        yield return ["select-auto", "user-select: auto"];

        // =============================================================================
        // 48. Appearance Utilities
        // =============================================================================

        // Appearance utilities
        yield return ["appearance-none", "appearance: none"];
        yield return ["appearance-auto", "appearance: auto"];

        // =============================================================================
        // 49. Filter Utilities
        // =============================================================================

        // Blur utilities - test for both CSS variable and filter property
        yield return ["blur-none", "--tw-blur: "];
        yield return ["blur-none", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["blur-sm", "--tw-blur: blur(4px)"];
        yield return ["blur-sm", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["blur", "--tw-blur: blur(4px)"];
        yield return ["blur", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["blur-md", "--tw-blur: blur(12px)"];
        yield return ["blur-md", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["blur-lg", "--tw-blur: blur(16px)"];
        yield return ["blur-lg", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["blur-xl", "--tw-blur: blur(24px)"];
        yield return ["blur-xl", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["blur-2xl", "--tw-blur: blur(40px)"];
        yield return ["blur-2xl", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["blur-3xl", "--tw-blur: blur(64px)"];
        yield return ["blur-3xl", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary blur values
        yield return ["blur-[8px]", "--tw-blur: blur(8px)"];
        yield return ["blur-[8px]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Brightness utilities - test for both CSS variable and filter property
        yield return ["brightness-0", "--tw-brightness: brightness(0)"];
        yield return ["brightness-0", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["brightness-50", "--tw-brightness: brightness(0.5)"];
        yield return ["brightness-50", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["brightness-75", "--tw-brightness: brightness(0.75)"];
        yield return ["brightness-75", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["brightness-100", "--tw-brightness: brightness(1)"];
        yield return ["brightness-100", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["brightness-125", "--tw-brightness: brightness(1.25)"];
        yield return ["brightness-125", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["brightness-150", "--tw-brightness: brightness(1.5)"];
        yield return ["brightness-150", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["brightness-200", "--tw-brightness: brightness(2)"];
        yield return ["brightness-200", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary brightness values
        yield return ["brightness-[1.2]", "--tw-brightness: brightness(1.2)"];
        yield return ["brightness-[1.2]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Contrast utilities - test for both CSS variable and filter property
        yield return ["contrast-0", "--tw-contrast: contrast(0)"];
        yield return ["contrast-0", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["contrast-50", "--tw-contrast: contrast(0.5)"];
        yield return ["contrast-50", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["contrast-75", "--tw-contrast: contrast(0.75)"];
        yield return ["contrast-75", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["contrast-100", "--tw-contrast: contrast(1)"];
        yield return ["contrast-100", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["contrast-125", "--tw-contrast: contrast(1.25)"];
        yield return ["contrast-125", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["contrast-150", "--tw-contrast: contrast(1.5)"];
        yield return ["contrast-150", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["contrast-200", "--tw-contrast: contrast(2)"];
        yield return ["contrast-200", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary contrast values
        yield return ["contrast-[1.25]", "--tw-contrast: contrast(1.25)"];
        yield return ["contrast-[1.25]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Grayscale utilities - test for both CSS variable and filter property
        yield return ["grayscale", "--tw-grayscale: grayscale(1)"];
        yield return ["grayscale", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["grayscale-0", "--tw-grayscale: grayscale(0)"];
        yield return ["grayscale-0", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary grayscale values
        yield return ["grayscale-[0.5]", "--tw-grayscale: grayscale(0.5)"];
        yield return ["grayscale-[0.5]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Hue Rotate utilities - test for both CSS variable and filter property (including negative values)
        yield return ["hue-rotate-0", "--tw-hue-rotate: hue-rotate(0deg)"];
        yield return ["hue-rotate-0", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["hue-rotate-15", "--tw-hue-rotate: hue-rotate(15deg)"];
        yield return ["hue-rotate-15", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["hue-rotate-30", "--tw-hue-rotate: hue-rotate(30deg)"];
        yield return ["hue-rotate-30", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["hue-rotate-60", "--tw-hue-rotate: hue-rotate(60deg)"];
        yield return ["hue-rotate-60", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["hue-rotate-90", "--tw-hue-rotate: hue-rotate(90deg)"];
        yield return ["hue-rotate-90", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["hue-rotate-180", "--tw-hue-rotate: hue-rotate(180deg)"];
        yield return ["hue-rotate-180", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Negative hue rotate utilities
        yield return ["-hue-rotate-15", "--tw-hue-rotate: hue-rotate(calc(15deg * -1))"];
        yield return ["-hue-rotate-15", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["-hue-rotate-90", "--tw-hue-rotate: hue-rotate(calc(90deg * -1))"];
        yield return ["-hue-rotate-90", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary hue rotate values
        yield return ["hue-rotate-[45deg]", "--tw-hue-rotate: hue-rotate(45deg)"];
        yield return ["hue-rotate-[45deg]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Invert utilities - test for both CSS variable and filter property
        yield return ["invert", "--tw-invert: invert(1)"];
        yield return ["invert", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["invert-0", "--tw-invert: invert(0)"];
        yield return ["invert-0", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary invert values
        yield return ["invert-[0.5]", "--tw-invert: invert(0.5)"];
        yield return ["invert-[0.5]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Saturate utilities - test for both CSS variable and filter property
        yield return ["saturate-0", "--tw-saturate: saturate(0)"];
        yield return ["saturate-0", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["saturate-50", "--tw-saturate: saturate(0.5)"];
        yield return ["saturate-50", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["saturate-100", "--tw-saturate: saturate(1)"];
        yield return ["saturate-100", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["saturate-150", "--tw-saturate: saturate(1.5)"];
        yield return ["saturate-150", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["saturate-200", "--tw-saturate: saturate(2)"];
        yield return ["saturate-200", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary saturate values
        yield return ["saturate-[1.25]", "--tw-saturate: saturate(1.25)"];
        yield return ["saturate-[1.25]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Sepia utilities - test for both CSS variable and filter property
        yield return ["sepia", "--tw-sepia: sepia(1)"];
        yield return ["sepia", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        yield return ["sepia-0", "--tw-sepia: sepia(0)"];
        yield return ["sepia-0", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // Arbitrary sepia values
        yield return ["sepia-[0.5]", "--tw-sepia: sepia(0.5)"];
        yield return ["sepia-[0.5]", "filter: var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)"];

        // =============================================================================
        // 50. Backdrop Filter Utilities
        // =============================================================================

        // Backdrop blur utilities - test for both CSS variable and backdrop-filter property
        yield return ["backdrop-blur-none", "--tw-backdrop-blur: "];
        yield return ["backdrop-blur-none", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-blur-sm", "--tw-backdrop-blur: blur(8px)"];
        yield return ["backdrop-blur-sm", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-blur", "--tw-backdrop-blur: blur(8px)"];
        yield return ["backdrop-blur", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-blur-md", "--tw-backdrop-blur: blur(12px)"];
        yield return ["backdrop-blur-md", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Backdrop brightness utilities
        yield return ["backdrop-brightness-50", "--tw-backdrop-brightness: brightness(50%)"];
        yield return ["backdrop-brightness-50", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-brightness-100", "--tw-backdrop-brightness: brightness(100%)"];
        yield return ["backdrop-brightness-100", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Backdrop contrast utilities
        yield return ["backdrop-contrast-75", "--tw-backdrop-contrast: contrast(75%)"];
        yield return ["backdrop-contrast-75", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-contrast-100", "--tw-backdrop-contrast: contrast(100%)"];
        yield return ["backdrop-contrast-100", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Backdrop grayscale utilities
        yield return ["backdrop-grayscale", "--tw-backdrop-grayscale: grayscale(100%)"];
        yield return ["backdrop-grayscale", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-grayscale-0", "--tw-backdrop-grayscale: grayscale(0%)"];
        yield return ["backdrop-grayscale-0", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Backdrop hue-rotate utilities
        yield return ["backdrop-hue-rotate-15", "--tw-backdrop-hue-rotate: hue-rotate(15deg)"];
        yield return ["backdrop-hue-rotate-15", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["-backdrop-hue-rotate-15", "--tw-backdrop-hue-rotate: hue-rotate(-15deg)"];
        yield return ["-backdrop-hue-rotate-15", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Backdrop invert utilities
        yield return ["backdrop-invert", "--tw-backdrop-invert: invert(100%)"];
        yield return ["backdrop-invert", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-invert-0", "--tw-backdrop-invert: invert(0%)"];
        yield return ["backdrop-invert-0", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Backdrop saturate utilities
        yield return ["backdrop-saturate-50", "--tw-backdrop-saturate: saturate(50%)"];
        yield return ["backdrop-saturate-50", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-saturate-100", "--tw-backdrop-saturate: saturate(100%)"];
        yield return ["backdrop-saturate-100", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Backdrop sepia utilities
        yield return ["backdrop-sepia", "--tw-backdrop-sepia: sepia(100%)"];
        yield return ["backdrop-sepia", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        yield return ["backdrop-sepia-0", "--tw-backdrop-sepia: sepia(0%)"];
        yield return ["backdrop-sepia-0", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // Arbitrary backdrop filter values
        yield return ["backdrop-blur-[5px]", "--tw-backdrop-blur: blur(5px)"];
        yield return ["backdrop-blur-[5px]", "backdrop-filter: var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)"];

        // =============================================================================
        // 14. Table Utilities
        // =============================================================================

        // Table layout utilities
        yield return ["table-auto", "table-layout: auto"];
        yield return ["table-fixed", "table-layout: fixed"];

        // Caption side utilities
        yield return ["caption-top", "caption-side: top"];
        yield return ["caption-bottom", "caption-side: bottom"];

        // =============================================================================
        // 15. List Utilities
        // =============================================================================

        // List style type utilities
        yield return ["list-none", "list-style-type: none"];
        yield return ["list-disc", "list-style-type: disc"];
        yield return ["list-decimal", "list-style-type: decimal"];

        // List style position utilities
        yield return ["list-inside", "list-style-position: inside"];
        yield return ["list-outside", "list-style-position: outside"];

        // =============================================================================
        // 12. Scroll Behavior Utilities
        // =============================================================================

        // Scroll behavior utilities
        yield return ["scroll-auto", "scroll-behavior: auto"];
        yield return ["scroll-smooth", "scroll-behavior: smooth"];

        // =============================================================================
        // 17. Other Utilities - Columns & Object Fit & Position
        // =============================================================================

        // Column utilities
        yield return ["columns-auto", "columns: auto"];
        yield return ["columns-1", "columns: 1"];
        yield return ["columns-2", "columns: 2"];
        yield return ["columns-3", "columns: 3"];
        yield return ["columns-4", "columns: 4"];
        yield return ["columns-5", "columns: 5"];
        yield return ["columns-6", "columns: 6"];
        yield return ["columns-7", "columns: 7"];
        yield return ["columns-8", "columns: 8"];
        yield return ["columns-9", "columns: 9"];
        yield return ["columns-10", "columns: 10"];
        yield return ["columns-11", "columns: 11"];
        yield return ["columns-12", "columns: 12"];
        yield return ["columns-3xs", "columns: var(--container-3xs)"];
        yield return ["columns-2xs", "columns: var(--container-2xs)"];
        yield return ["columns-xs", "columns: var(--container-xs)"];
        yield return ["columns-sm", "columns: var(--container-sm)"];
        yield return ["columns-md", "columns: var(--container-md)"];
        yield return ["columns-lg", "columns: var(--container-lg)"];
        yield return ["columns-xl", "columns: var(--container-xl)"];
        yield return ["columns-2xl", "columns: var(--container-2xl)"];
        yield return ["columns-3xl", "columns: var(--container-3xl)"];
        yield return ["columns-4xl", "columns: var(--container-4xl)"];
        yield return ["columns-5xl", "columns: var(--container-5xl)"];
        yield return ["columns-6xl", "columns: var(--container-6xl)"];
        yield return ["columns-7xl", "columns: var(--container-7xl)"];

        // Object fit utilities
        yield return ["object-contain", "object-fit: contain"];
        yield return ["object-cover", "object-fit: cover"];
        yield return ["object-fill", "object-fit: fill"];
        yield return ["object-none", "object-fit: none"];
        yield return ["object-scale-down", "object-fit: scale-down"];

        // Object position utilities
        yield return ["object-bottom", "object-position: bottom"];
        yield return ["object-center", "object-position: center"];
        yield return ["object-left", "object-position: left"];
        yield return ["object-left-bottom", "object-position: left bottom"];
        yield return ["object-left-top", "object-position: left top"];
        yield return ["object-right", "object-position: right"];
        yield return ["object-right-bottom", "object-position: right bottom"];
        yield return ["object-right-top", "object-position: right top"];
        yield return ["object-top", "object-position: top"];

        // Object View Box utilities
        yield return ["object-view-box-[inset(10%)]", "object-view-box: inset(10%)"];

        // =============================================================================
        // 13. SVG Utilities
        // =============================================================================

        // SVG fill utilities
        yield return ["fill-none", "fill: none"];
        yield return ["fill-current", "fill: currentcolor"];
        yield return ["fill-red-500", "fill: var(--color-red-500)"];

        // SVG stroke utilities
        yield return ["stroke-none", "stroke: none"];
        yield return ["stroke-current", "stroke: currentcolor"];
        yield return ["stroke-red-500", "stroke: var(--color-red-500)"];
        yield return ["stroke-0", "stroke-width: 0"];
        yield return ["stroke-1", "stroke-width: 1"];
        yield return ["stroke-2", "stroke-width: 2"];

        // =============================================================================
        // 12. Interactivity Utilities - NEW
        // =============================================================================

        // Caret color utilities
        yield return ["caret-inherit", "caret-color: inherit"];
        yield return ["caret-current", "caret-color: currentcolor"];
        yield return ["caret-transparent", "caret-color: transparent"];
        yield return ["caret-red-500", "caret-color: var(--color-red-500)"];

        // Accent color utilities
        yield return ["accent-auto", "accent-color: auto"];
        yield return ["accent-inherit", "accent-color: inherit"];
        yield return ["accent-current", "accent-color: currentcolor"];
        yield return ["accent-transparent", "accent-color: transparent"];
        yield return ["accent-red-500", "accent-color: var(--color-red-500)"];

        // Scroll margin utilities
        yield return ["scroll-m-0", "scroll-margin: calc(var(--spacing) * 0)"];
        yield return ["scroll-m-1", "scroll-margin: calc(var(--spacing) * 1)"];
        yield return ["scroll-m-4", "scroll-margin: calc(var(--spacing) * 4)"];
        yield return ["scroll-mx-2", "scroll-margin-inline: calc(var(--spacing) * 2)"];
        yield return ["scroll-my-3", "scroll-margin-block: calc(var(--spacing) * 3)"];
        yield return ["scroll-mt-1", "scroll-margin-top: calc(var(--spacing) * 1)"];
        yield return ["scroll-mr-2", "scroll-margin-right: calc(var(--spacing) * 2)"];
        yield return ["scroll-mb-3", "scroll-margin-bottom: calc(var(--spacing) * 3)"];
        yield return ["scroll-ml-4", "scroll-margin-left: calc(var(--spacing) * 4)"];
        yield return ["scroll-ms-2", "scroll-margin-inline-start: calc(var(--spacing) * 2)"];
        yield return ["scroll-me-3", "scroll-margin-inline-end: calc(var(--spacing) * 3)"];
        yield return ["-scroll-m-4", "scroll-margin: calc(var(--spacing) * -4)"];

        // Scroll padding utilities
        yield return ["scroll-p-0", "scroll-padding: calc(var(--spacing) * 0)"];
        yield return ["scroll-p-1", "scroll-padding: calc(var(--spacing) * 1)"];
        yield return ["scroll-p-4", "scroll-padding: calc(var(--spacing) * 4)"];
        yield return ["scroll-px-2", "scroll-padding-inline: calc(var(--spacing) * 2)"];
        yield return ["scroll-py-3", "scroll-padding-block: calc(var(--spacing) * 3)"];
        yield return ["scroll-pt-1", "scroll-padding-top: calc(var(--spacing) * 1)"];
        yield return ["scroll-pr-2", "scroll-padding-right: calc(var(--spacing) * 2)"];
        yield return ["scroll-pb-3", "scroll-padding-bottom: calc(var(--spacing) * 3)"];
        yield return ["scroll-pl-4", "scroll-padding-left: calc(var(--spacing) * 4)"];
        yield return ["scroll-ps-2", "scroll-padding-inline-start: calc(var(--spacing) * 2)"];
        yield return ["scroll-pe-3", "scroll-padding-inline-end: calc(var(--spacing) * 3)"];

        // =============================================================================
        // 16. Accessibility Utilities
        // =============================================================================

        // Screen reader utilities
        yield return ["sr-only", "position: absolute"];
        yield return ["not-sr-only", "position: static"];

        // =============================================================================
        // 17. Scroll Snap Utilities
        // =============================================================================

        // Scroll snap type utilities
        yield return ["snap-none", "scroll-snap-type: none"];
        yield return ["snap-x", "scroll-snap-type: x var(--tw-scroll-snap-strictness)"];
        yield return ["snap-y", "scroll-snap-type: y var(--tw-scroll-snap-strictness)"];
        yield return ["snap-both", "scroll-snap-type: both var(--tw-scroll-snap-strictness)"];

        // Scroll snap strictness utilities
        yield return ["snap-mandatory", "--tw-scroll-snap-strictness: mandatory"];
        yield return ["snap-proximity", "--tw-scroll-snap-strictness: proximity"];

        // Scroll snap align utilities
        yield return ["snap-start", "scroll-snap-align: start"];
        yield return ["snap-end", "scroll-snap-align: end"];
        yield return ["snap-center", "scroll-snap-align: center"];
        yield return ["snap-align-none", "scroll-snap-align: none"];

        // Scroll snap stop utilities
        yield return ["snap-normal", "scroll-snap-stop: normal"];
        yield return ["snap-always", "scroll-snap-stop: always"];

        // =============================================================================
        // 18. Touch Action Utilities
        // =============================================================================

        // Touch action utilities
        yield return ["touch-auto", "touch-action: auto"];
        yield return ["touch-none", "touch-action: none"];
        yield return ["touch-manipulation", "touch-action: manipulation"];
        yield return ["touch-pan-x", "--tw-pan-x: pan-x"];
        yield return ["touch-pan-x", "touch-action: var(--tw-pan-x,) var(--tw-pan-y,) var(--tw-pinch-zoom,)"];
        yield return ["touch-pan-y", "--tw-pan-y: pan-y"];
        yield return ["touch-pan-y", "touch-action: var(--tw-pan-x,) var(--tw-pan-y,) var(--tw-pinch-zoom,)"];
        yield return ["touch-pan-left", "--tw-pan-x: pan-left"];
        yield return ["touch-pan-right", "--tw-pan-x: pan-right"];
        yield return ["touch-pan-up", "--tw-pan-y: pan-up"];
        yield return ["touch-pan-down", "--tw-pan-y: pan-down"];
        yield return ["touch-pinch-zoom", "--tw-pinch-zoom: pinch-zoom"];

        // =============================================================================
        // 19. Field Sizing Utilities
        // =============================================================================

        // Field sizing utilities
        yield return ["field-sizing-content", "field-sizing: content"];
        yield return ["field-sizing-fixed", "field-sizing: fixed"];

        // Color scheme utilities
        yield return ["scheme-normal", "color-scheme: normal"];
        yield return ["scheme-dark", "color-scheme: dark"];
        yield return ["scheme-light", "color-scheme: light"];
        yield return ["scheme-light-dark", "color-scheme: light dark"];
        yield return ["scheme-only-dark", "color-scheme: only dark"];
        yield return ["scheme-only-light", "color-scheme: only light"];

        // =============================================================================
        // 19. Transform Perspective Utilities
        // =============================================================================

        // Perspective utilities
        yield return ["perspective-none", "perspective: none"];
        yield return ["perspective-[500px]", "perspective: 500px"];

        // Perspective origin utilities
        yield return ["perspective-origin-center", "perspective-origin: center"];
        yield return ["perspective-origin-top", "perspective-origin: top"];
        yield return ["perspective-origin-top-right", "perspective-origin: top right"];
        yield return ["perspective-origin-right", "perspective-origin: right"];
        yield return ["perspective-origin-bottom-right", "perspective-origin: bottom right"];
        yield return ["perspective-origin-bottom", "perspective-origin: bottom"];
        yield return ["perspective-origin-bottom-left", "perspective-origin: bottom left"];
        yield return ["perspective-origin-left", "perspective-origin: left"];
        yield return ["perspective-origin-top-left", "perspective-origin: top left"];

        // Backface visibility utilities
        yield return ["backface-visible", "backface-visibility: visible"];
        yield return ["backface-hidden", "backface-visibility: hidden"];

        // =============================================================================
        // 20. Transform Enabler Utilities
        // =============================================================================

        // Transform enabler utilities
        yield return ["transform", "transform: var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)"];
        yield return ["transform-none", "transform: none"];

        // =============================================================================
        // 21. Forced Color Adjust Utilities
        // =============================================================================

        // Forced color adjust utilities
        yield return ["forced-color-adjust-auto", "forced-color-adjust: auto"];
        yield return ["forced-color-adjust-none", "forced-color-adjust: none"];

        // =============================================================================
        // 21. Box Decoration Break Utilities
        // =============================================================================

        // Box decoration break utilities
        yield return ["box-decoration-slice", "-webkit-box-decoration-break: slice"];
        yield return ["box-decoration-slice", "box-decoration-break: slice"];
        yield return ["box-decoration-clone", "-webkit-box-decoration-break: clone"];
        yield return ["box-decoration-clone", "box-decoration-break: clone"];

        // =============================================================================
        // 22. Break Utilities
        // =============================================================================

        // Break before utilities
        yield return ["break-before-auto", "break-before: auto"];
        yield return ["break-before-avoid", "break-before: avoid"];
        yield return ["break-before-page", "break-before: page"];

        // Break after utilities
        yield return ["break-after-auto", "break-after: auto"];
        yield return ["break-after-avoid", "break-after: avoid"];
        yield return ["break-after-page", "break-after: page"];

        // Break inside utilities
        yield return ["break-inside-auto", "break-inside: auto"];
        yield return ["break-inside-avoid", "break-inside: avoid"];
        yield return ["break-inside-avoid-page", "break-inside: avoid-page"];

        // =============================================================================
        // 23. List Style Image Utilities
        // =============================================================================

        // List style image utilities
        yield return ["list-image-none", "list-style-image: none"];

        // =============================================================================
        // 24. Content Utilities
        // =============================================================================

        // Content utilities
        yield return ["content-none", "content: none"];
        yield return ["content-['hello']", "--tw-content: 'hello'"];
        yield return ["content-['hello']", "content: var(--tw-content)"];
        yield return ["content-['']", "--tw-content: ''"];
        yield return ["content-['']", "content: var(--tw-content)"];
        yield return ["content-[url('/image.png')]", "--tw-content: url('/image.png')"];
        yield return ["content-[url('/image.png')]", "content: var(--tw-content)"];
        yield return ["content-[attr(data-content)]", "--tw-content: attr(data-content)"];
        yield return ["content-[attr(data-content)]", "content: var(--tw-content)"];
        yield return ["content-[counter(item)]", "--tw-content: counter(item)"];
        yield return ["content-[counter(item)]", "content: var(--tw-content)"];
        yield return ["content-[open-quote]", "--tw-content: open-quote"];
        yield return ["content-[open-quote]", "content: var(--tw-content)"];

        // =============================================================================
        // 25. Arbitrary Property Utilities (Phase 8) - NEW
        // =============================================================================

        // Basic arbitrary properties
        yield return ["[display:flex]", "display: flex"];
        yield return ["[font-family:Inter]", "font-family: Inter"];
        yield return ["[background-color:red]", "background-color: red"];
        yield return ["[position:absolute]", "position: absolute"];
        yield return ["[z-index:999]", "z-index: 999"];
        yield return ["[opacity:0.5]", "opacity: 0.5"];
        yield return ["[transition:all_300ms_ease]", "transition: all 300ms ease"];

        // Multi-value properties with underscores converted to spaces
        yield return ["[grid-template-columns:1fr_2fr_1fr]", "grid-template-columns: 1fr 2fr 1fr"];
        yield return ["[border:1px_solid_black]", "border: 1px solid black"];
        yield return ["[margin:10px_20px_30px_40px]", "margin: 10px 20px 30px 40px"];
        yield return ["[padding:1rem_2rem]", "padding: 1rem 2rem"];
        yield return ["[box-shadow:0_4px_6px_rgba(0,0,0,0.1)]", "box-shadow: 0 4px 6px rgba(0,0,0,0.1)"];

        // CSS custom properties
        yield return ["[--custom-property:value]", "--custom-property: value"];
        yield return ["[--primary-color:#3b82f6]", "--primary-color: #3b82f6"];
        yield return ["[--spacing-unit:1rem]", "--spacing-unit: 1rem"];

        // CSS functions
        yield return ["[color:var(--primary)]", "color: var(--primary)"];
        yield return ["[width:calc(100%-2rem)]", "width: calc(100% - 2rem)"];
        yield return ["[background:linear-gradient(to_right,red,blue)]", "background: linear-gradient(to right,red,blue)"];
        yield return ["[transform:rotate(45deg)]", "transform: rotate(45deg)"];
        yield return ["[clip-path:polygon(0_0,100%_0,100%_100%)]", "clip-path: polygon(0 0,100% 0,100% 100%)"];

        // CSS functions with theme variables (should also output theme variable definitions)
        yield return ["[font-family:var(--font-mono)]", "font-family: var(--font-mono)"];
        yield return ["[color:var(--color-red-500)]", "color: var(--color-red-500)"];

        // Complex CSS values
        yield return ["[font:16px/1.5_'Helvetica_Neue',sans-serif]", "font: 16px/1.5 'Helvetica Neue',sans-serif"];
        yield return ["[background-image:url('image.jpg')]", "background-image: url('image.jpg')"];
        yield return ["[animation:spin_1s_linear_infinite]", "animation: spin 1s linear infinite"];
        yield return ["[text-shadow:1px_1px_2px_rgba(0,0,0,0.5)]", "text-shadow: 1px 1px 2px rgba(0,0,0,0.5)"];

        // Modern CSS properties
        yield return ["[container-type:inline-size]", "container-type: inline-size"];
        yield return ["[view-transition-name:header]", "view-transition-name: header"];
        yield return ["[anchor-name:--my-anchor]", "anchor-name: --my-anchor"];
        yield return ["[position-anchor:--my-anchor]", "position-anchor: --my-anchor"];

        // =============================================================================
        // 18. Typography Utilities
        // =============================================================================

        // Prose base tests
        yield return ["prose", "font-size: 1rem"];
        yield return ["prose", "line-height: 1.75"];
        yield return ["prose", "max-width: 65ch"];  // Base prose should have max-width
        yield return ["prose", "color: var(--tw-prose-body)"];
        yield return ["prose", ".prose {"];  // Verify correct selector

        // Prose-base modifier test - size variant only, no colors or max-width
        yield return ["prose-base", ".prose-base {"];  // Verify correct selector
        yield return ["prose-base", "font-size: 1rem"];
        yield return ["prose-base", "line-height: 1.75"];
        // Note: prose-base should NOT have color or max-width - it's a size-only variant

        // Prose size modifier tests - verify both correct selector and values
        yield return ["prose-sm", ".prose-sm {"];  // Verify correct selector
        yield return ["prose-sm", "font-size: 0.875rem"];
        yield return ["prose-sm", "line-height: 1.7142857"];
        yield return ["prose-lg", ".prose-lg {"];  // Verify correct selector
        yield return ["prose-lg", "font-size: 1.125rem"];
        yield return ["prose-lg", "line-height: 1.7777778"];
        yield return ["prose-xl", ".prose-xl {"];  // Verify correct selector
        yield return ["prose-xl", "font-size: 1.25rem"];
        yield return ["prose-xl", "line-height: 1.8"];
        yield return ["prose-2xl", ".prose-2xl {"];  // Verify correct selector
        yield return ["prose-2xl", "font-size: 1.5rem"];
        yield return ["prose-2xl", "line-height: 1.6666667"];

        // Verify child element selectors use correct parent
        yield return ["prose-sm", ".prose-sm :where(p):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))"];  // Child elements should use .prose-sm
        yield return ["prose-lg", ".prose-lg :where(h1):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))"];  // Child elements should use .prose-lg

        // Prose color theme tests (full color themes)
        yield return ["prose-slate", "--tw-prose-body: oklch(37.2% 0.044 257.287)"];
        yield return ["prose-gray", "--tw-prose-body: oklch(37.3% 0.034 259.733)"];
        yield return ["prose-zinc", "--tw-prose-body: oklch(37% 0.013 285.805)"];
        yield return ["prose-neutral", "--tw-prose-body: oklch(37.1% 0 0)"];
        yield return ["prose-stone", "--tw-prose-body: oklch(37.4% 0.01 67.558)"];

        // Prose color theme tests (link-only colors)
        yield return ["prose-red", "--tw-prose-links: oklch(57.7% 0.245 27.325)"];
        yield return ["prose-orange", "--tw-prose-links: oklch(64.6% 0.222 41.116)"];
        yield return ["prose-amber", "--tw-prose-links: oklch(66.6% 0.179 58.318)"];
        yield return ["prose-yellow", "--tw-prose-links: oklch(68.1% 0.162 75.834)"];
        yield return ["prose-lime", "--tw-prose-links: oklch(64.8% 0.2 131.684)"];
        yield return ["prose-green", "--tw-prose-links: oklch(62.7% 0.194 149.214)"];
        yield return ["prose-emerald", "--tw-prose-links: oklch(59.6% 0.145 163.225)"];
        yield return ["prose-teal", "--tw-prose-links: oklch(60% 0.118 184.704)"];
        yield return ["prose-cyan", "--tw-prose-links: oklch(60.9% 0.126 221.723)"];
        yield return ["prose-sky", "--tw-prose-links: oklch(58.8% 0.158 241.966)"];
        yield return ["prose-blue", "--tw-prose-links: oklch(54.6% 0.245 262.881)"];
        yield return ["prose-indigo", "--tw-prose-links: oklch(51.1% 0.262 276.966)"];
        yield return ["prose-violet", "--tw-prose-links: oklch(54.1% 0.281 293.009)"];
        yield return ["prose-purple", "--tw-prose-links: oklch(55.8% 0.288 302.321)"];
        yield return ["prose-fuchsia", "--tw-prose-links: oklch(59.1% 0.293 322.896)"];
        yield return ["prose-pink", "--tw-prose-links: oklch(59.2% 0.249 0.584)"];
        yield return ["prose-rose", "--tw-prose-links: oklch(58.6% 0.253 17.585)"];

        // Prose invert tests (should remap CSS variables)
        yield return ["prose-invert", "--tw-prose-body: var(--tw-prose-invert-body)"];
        yield return ["prose-invert", "--tw-prose-headings: var(--tw-prose-invert-headings)"];
        yield return ["prose-invert", "--tw-prose-links: var(--tw-prose-invert-links)"];

        // Prose with standard variants (testing that variants work correctly with ComponentRule)
        yield return ["sm:prose", "@media (min-width: 640px)"];
        yield return ["sm:prose", ".sm\\:prose {"];
        yield return ["hover:prose", ".hover\\:prose:hover {"];
        yield return ["dark:prose", ".dark\\:prose:where(.dark, .dark *)"];
        yield return ["sm:hover:prose", "@media (min-width: 640px)"];
        yield return ["sm:hover:prose", ".sm\\:hover\\:prose:hover {"];

        // Verify variants work with prose size modifiers
        yield return ["sm:prose-lg", "@media (min-width: 640px)"];
        yield return ["sm:prose-lg", ".sm\\:prose-lg {"];
        yield return ["hover:prose-sm", ".hover\\:prose-sm:hover {"];

        // Verify variants work with child elements
        yield return ["sm:prose", ".sm\\:prose :where(p):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))"];
        yield return ["hover:prose", ".hover\\:prose:hover :where(p):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))"];

        // Prose element variant tests (now working as variants, not utilities)
        // These test that prose element variants properly modify other utilities
        yield return ["prose-headings:text-red-500", ".prose-headings\\:text-red-500 :where(h1, h2, h3, h4, h5, h6, th)"];
        yield return ["prose-headings:text-red-500", "color: var(--color-red-500)"];
        yield return ["prose-a:underline", ".prose-a\\:underline :where(a)"];
        yield return ["prose-a:underline", "text-decoration-line: underline"];
        yield return ["prose-h1:text-4xl", ".prose-h1\\:text-4xl :where(h1)"];
        yield return ["prose-h1:text-4xl", "font-size: 2.25rem"];
        yield return ["prose-code:bg-gray-100", ".prose-code\\:bg-gray-100 :where(code)"];
        yield return ["prose-code:bg-gray-100", "background-color: var(--color-gray-100)"];

        // Test prose element variants with other variants (dark mode, hover, etc.)
        yield return ["dark:prose-headings:text-blue-500", ".dark\\:prose-headings\\:text-blue-500:where(.dark, .dark *)"];
        yield return ["dark:prose-headings:text-blue-500", ":where(h1, h2, h3, h4, h5, h6, th)"];
        yield return ["dark:prose-headings:text-blue-500", "color: var(--color-blue-500)"];
        yield return ["hover:prose-a:text-blue-700", ".hover\\:prose-a\\:text-blue-700:hover :where(a)"];
        yield return ["hover:prose-a:text-blue-700", "color: var(--color-blue-700)"];

        // =============================================================================
        // 30. Custom Utilities (Scrollbar utilities as test case)
        // =============================================================================

        // Note: These custom utilities need to be registered first via framework.AddUtilities()
        // The tests below assume the scrollbar utilities from the custom-utilities.md spec are loaded
        // They demonstrate that custom utilities work seamlessly with the existing infrastructure

        // Static scrollbar utilities
        // yield return ["scrollbar-none", "scrollbar-width: none"];
        // yield return ["scrollbar-none", "::-webkit-scrollbar"];
        // yield return ["scrollbar-none", "display: none"];
        // yield return ["scrollbar-thin", "scrollbar-width: thin"];
        // yield return ["scrollbar-width-auto", "scrollbar-width: auto"];
        // yield return ["scrollbar-gutter-auto", "scrollbar-gutter: auto"];

        // CSS variable based utilities
        // yield return ["scrollbar-both-edges", "--tw-scrollbar-gutter-modifier: both-edges"];
        // yield return ["scrollbar-both-edges", "scrollbar-gutter: stable var(--tw-scrollbar-gutter-modifier)"];
        // yield return ["scrollbar-stable", "scrollbar-gutter: stable"];
        // yield return ["scrollbar-color", "scrollbar-color: var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)"];

        // Dynamic pattern-based utilities (with theme color resolution)
        // yield return ["scrollbar-thumb-red-500", "--tw-scrollbar-thumb-color: #ef4444"];
        // yield return ["scrollbar-track-gray-200", "--tw-scrollbar-track-color:"];
        // yield return ["scrollbar-thumb-[#123456]", "--tw-scrollbar-thumb-color: #123456"];

        // Custom utilities with variants
        // yield return ["hover:scrollbar-thin", ":hover"];
        // yield return ["hover:scrollbar-thin", "scrollbar-width: thin"];
        // yield return ["sm:scrollbar-none", "@media (min-width: 640px)"];
        // yield return ["sm:scrollbar-none", "scrollbar-width: none"];
        // yield return ["dark:scrollbar-thumb-gray-600", "@media (prefers-color-scheme: dark)"];
        // yield return ["!scrollbar-thin", "scrollbar-width: thin !important"];
    }
}