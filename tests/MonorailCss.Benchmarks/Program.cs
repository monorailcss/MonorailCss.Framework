using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MonorailCss;
BenchmarkRunner.Run<Benchmark>();

[MemoryDiagnoser]
public class Benchmark
{

    private List<string> _classes = [];

    [GlobalSetup]
    public void Setup()
    {
        _classes = [
            "block", "inline", "inline-block", "flex", "inline-flex",
            "grid", "inline-grid", "table", "table-cell", "table-row",
            "flow-root", "contents", "list-item", "hidden", "none",
            "table-caption", "table-column", "table-footer-group", "table-header-group", "table-row-group",

            // Position utilities (10)
            "static", "fixed", "absolute", "relative", "sticky",
            "inset-0", "inset-x-0", "inset-y-0", "top-0", "bottom-0",

            // Background colors with various scales (30)
            "bg-red-50", "bg-red-100", "bg-red-200", "bg-red-300", "bg-red-400",
            "bg-red-500", "bg-red-600", "bg-red-700", "bg-red-800", "bg-red-900",
            "bg-blue-50", "bg-blue-100", "bg-blue-200", "bg-blue-300", "bg-blue-400",
            "bg-blue-500", "bg-blue-600", "bg-blue-700", "bg-blue-800", "bg-blue-900",
            "bg-green-50", "bg-green-100", "bg-green-200", "bg-green-300", "bg-green-400",
            "bg-green-500", "bg-green-600", "bg-green-700", "bg-green-800", "bg-green-900",

            // Text colors (20)
            "text-white", "text-black", "text-transparent", "text-current",
            "text-gray-100", "text-gray-200", "text-gray-300", "text-gray-400", "text-gray-500",
            "text-gray-600", "text-gray-700", "text-gray-800", "text-gray-900",
            "text-yellow-500", "text-purple-500", "text-pink-500", "text-indigo-500",
            "text-teal-500", "text-orange-500", "text-lime-500",

            // Spacing utilities - padding (15)
            "p-0", "p-0.5", "p-1", "p-2", "p-3", "p-4", "p-5", "p-6", "p-8", "p-10",
            "px-4", "py-2", "pt-4", "pb-4", "pl-4",

            // Spacing utilities - margin (15)
            "m-0", "m-1", "m-2", "m-3", "m-4", "m-5", "m-6", "m-8", "m-10", "m-auto",
            "mx-auto", "my-4", "mt-4", "mb-4", "ml-4",

            // Width utilities (15)
            "w-0", "w-1", "w-2", "w-4", "w-8", "w-16", "w-32", "w-64",
            "w-full", "w-screen", "w-min", "w-max", "w-fit",
            "w-1/2", "w-1/3",

            // Height utilities (15)
            "h-0", "h-1", "h-2", "h-4", "h-8", "h-16", "h-32", "h-64",
            "h-full", "h-screen", "h-min", "h-max", "h-fit",
            "h-1/2", "h-1/3",

            // Min/Max width and height (10)
            "max-w-none", "max-w-xs", "max-w-sm", "max-w-md", "max-w-lg",
            "min-h-0", "min-h-full", "min-h-screen", "max-h-screen", "max-h-full",

            // Typography utilities (15)
            "font-thin", "font-light", "font-normal", "font-medium", "font-semibold",
            "font-bold", "font-extrabold", "font-black",
            "text-xs", "text-sm", "text-base", "text-lg", "text-xl", "text-2xl", "text-3xl",

            // Text alignment and decoration (10)
            "text-left", "text-center", "text-right", "text-justify",
            "underline", "overline", "line-through", "no-underline",
            "uppercase", "lowercase",

            // Border utilities (15)
            "border", "border-0", "border-2", "border-4", "border-8",
            "border-t", "border-b", "border-l", "border-r",
            "border-solid", "border-dashed", "border-dotted", "border-double",
            "border-black", "border-white",

            // Border radius (10)
            "rounded", "rounded-none", "rounded-sm", "rounded-md", "rounded-lg",
            "rounded-xl", "rounded-2xl", "rounded-3xl", "rounded-full",
            "rounded-t-lg",

            // Shadow utilities (10)
            "shadow", "shadow-sm", "shadow-md", "shadow-lg", "shadow-xl",
            "shadow-2xl", "shadow-inner", "shadow-none",
            "shadow-black", "shadow-white",

            // Opacity utilities (10)
            "opacity-0", "opacity-10", "opacity-20", "opacity-30", "opacity-40",
            "opacity-50", "opacity-60", "opacity-70", "opacity-80", "opacity-100",

            // Flexbox utilities (15)
            "flex-row", "flex-row-reverse", "flex-col", "flex-col-reverse",
            "flex-wrap", "flex-nowrap", "flex-wrap-reverse",
            "flex-1", "flex-auto", "flex-initial", "flex-none",
            "grow", "grow-0", "shrink", "shrink-0",

            // Grid utilities (10)
            "grid-cols-1", "grid-cols-2", "grid-cols-3", "grid-cols-4", "grid-cols-5",
            "grid-rows-1", "grid-rows-2", "grid-rows-3", "grid-rows-4", "grid-rows-5",

            // Justify and align utilities (10)
            "justify-start", "justify-end", "justify-center", "justify-between", "justify-around",
            "items-start", "items-end", "items-center", "items-baseline", "items-stretch",

            // Transform utilities (10)
            "scale-50", "scale-75", "scale-90", "scale-100", "scale-110",
            "rotate-0", "rotate-45", "rotate-90", "rotate-180", "-rotate-45",

            // Responsive variants (15)
            "sm:p-4", "sm:m-4", "sm:flex", "sm:grid", "sm:hidden",
            "md:p-6", "md:m-6", "md:block", "md:text-lg", "md:w-1/2",
            "lg:p-8", "lg:m-8", "lg:text-xl", "lg:w-1/3", "lg:grid-cols-3",

            // State variants (15)
            "hover:bg-blue-600", "hover:text-white", "hover:shadow-lg", "hover:scale-105", "hover:opacity-80",
            "focus:outline-none", "focus:ring", "focus:ring-blue-500", "focus:border-blue-500", "focus:shadow-outline",
            "active:bg-blue-700", "active:scale-95", "active:shadow-inner", "active:opacity-90", "active:translate-y-1",

            // Dark mode variants (10)
            "dark:bg-gray-800", "dark:text-white", "dark:border-gray-700", "dark:shadow-2xl", "dark:opacity-90",
            "dark:hover:bg-gray-700", "dark:focus:ring-white", "dark:active:bg-gray-900",
            "dark:md:text-xl", "dark:lg:p-8",

            // Arbitrary value utilities (10)
            "bg-[#123456]", "text-[#fedcba]", "w-[100px]", "h-[200px]", "p-[25px]",
            "m-[10%]", "rounded-[15px]", "shadow-[0_0_10px_rgba(0,0,0,0.5)]",
            "border-[3px]", "opacity-[0.33]",

            // Color with opacity modifiers (10)
            "bg-red-500/50", "text-blue-700/25", "border-green-400/75",
            "shadow-black/30", "bg-purple-600/[0.85]",
            "hover:bg-red-500/60", "dark:bg-gray-900/90",
            "focus:border-blue-500/100", "active:text-white/80", "sm:bg-yellow-400/40",

            // Important modifiers (5)
            "!p-4", "!m-0", "!text-red-500", "hover:!bg-blue-600", "!important",

            // Additional utilities to reach 200+ (15)
            "overflow-hidden", "overflow-auto", "overflow-scroll", "overflow-visible",
            "z-0", "z-10", "z-20", "z-30", "z-40", "z-50",
            "cursor-pointer", "cursor-default", "cursor-wait", "cursor-text", "cursor-move",
        ];
    }

    [Benchmark]
    public string ProcessAll()
    {
        var framework = new CssFramework();
        return framework.Process(_classes);
    }
}