using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MonorailCss;
using MonorailCss.Discovery;

BenchmarkRunner.Run(new[] { typeof(GenerationBenchmark), typeof(ScanningBenchmark) });

/// <summary>
/// Measures the generation hot path — turning a known set of class names into CSS.
/// The framework is built once in <see cref="GlobalSetupAttribute"/> so the per-iteration
/// number reflects parsing + pipeline + emission, not one-time reflection-based discovery.
/// </summary>
[MemoryDiagnoser]
public class GenerationBenchmark
{
    private CssFramework _framework = null!;

    [GlobalSetup]
    public void Setup()
    {
        _framework = new CssFramework();
    }

    [Benchmark]
    public string Process() => _framework.Process(SampleData.Classes);
}

/// <summary>
/// Measures the scanning hot path — discovering class candidates in source files.
/// A fixed on-disk corpus (markup, C#, markdown) is generated once; three benchmarks cover
/// the cold lexer+validation path, the warm mtime-cache replay path, and end-to-end
/// <see cref="MonorailCssGenerator.Generate"/>.
/// </summary>
[MemoryDiagnoser]
public class ScanningBenchmark
{
    private string _corpusDir = null!;
    private string[] _files = null!;
    private CssFramework _framework = null!;
    private SourceFileScanner _warmScanner = null!;

    [GlobalSetup]
    public void Setup()
    {
        _framework = new CssFramework();
        _corpusDir = SampleData.BuildCorpus();
        _files = Directory.GetFiles(_corpusDir, "*", SearchOption.AllDirectories);

        // Prime the warm scanner so ScanWarm exercises the path+mtime cache replay.
        _warmScanner = new SourceFileScanner(new ValidationCache(_framework));
        var sink = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in _files)
        {
            _warmScanner.ScanFile(file, sink);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            Directory.Delete(_corpusDir, recursive: true);
        }
        catch
        {
            // Best-effort temp cleanup.
        }
    }

    /// <summary>Fresh validator + scanner every invocation: the full lex + validate cost.</summary>
    [Benchmark]
    public int ScanCold()
    {
        var scanner = new SourceFileScanner(new ValidationCache(_framework));
        var output = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in _files)
        {
            scanner.ScanFile(file, output);
        }

        return output.Count;
    }

    /// <summary>Reused primed scanner: the mtime-cache replay path.</summary>
    [Benchmark]
    public int ScanWarm()
    {
        var output = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in _files)
        {
            _warmScanner.ScanFile(file, output);
        }

        return output.Count;
    }

    /// <summary>End-to-end: a fresh generator scans the corpus and emits CSS.</summary>
    [Benchmark]
    public int GenerateCold()
    {
        var generator = new MonorailCssGenerator();
        var result = generator.Generate(new MonorailCssGenerationRequest
        {
            BaseFramework = _framework,
            SourceFiles = _files,
        });

        return result.Css.Length;
    }
}

/// <summary>
/// Shared benchmark inputs: a representative utility-class list and a generated source-file
/// corpus mixing real class names with natural-language noise.
/// </summary>
internal static class SampleData
{
    public static readonly string[] Classes =
    [
        "block", "inline", "inline-block", "flex", "inline-flex",
        "grid", "inline-grid", "table", "table-cell", "table-row",
        "flow-root", "contents", "list-item", "hidden", "none",
        "table-caption", "table-column", "table-footer-group", "table-header-group", "table-row-group",
        "static", "fixed", "absolute", "relative", "sticky",
        "inset-0", "inset-x-0", "inset-y-0", "top-0", "bottom-0",
        "bg-red-50", "bg-red-100", "bg-red-200", "bg-red-300", "bg-red-400",
        "bg-red-500", "bg-red-600", "bg-red-700", "bg-red-800", "bg-red-900",
        "bg-blue-50", "bg-blue-100", "bg-blue-200", "bg-blue-300", "bg-blue-400",
        "bg-blue-500", "bg-blue-600", "bg-blue-700", "bg-blue-800", "bg-blue-900",
        "bg-green-50", "bg-green-100", "bg-green-200", "bg-green-300", "bg-green-400",
        "bg-green-500", "bg-green-600", "bg-green-700", "bg-green-800", "bg-green-900",
        "text-white", "text-black", "text-transparent", "text-current",
        "text-gray-100", "text-gray-200", "text-gray-300", "text-gray-400", "text-gray-500",
        "text-gray-600", "text-gray-700", "text-gray-800", "text-gray-900",
        "text-yellow-500", "text-purple-500", "text-pink-500", "text-indigo-500",
        "text-teal-500", "text-orange-500", "text-lime-500",
        "p-0", "p-0.5", "p-1", "p-2", "p-3", "p-4", "p-5", "p-6", "p-8", "p-10",
        "px-4", "py-2", "pt-4", "pb-4", "pl-4",
        "m-0", "m-1", "m-2", "m-3", "m-4", "m-5", "m-6", "m-8", "m-10", "m-auto",
        "mx-auto", "my-4", "mt-4", "mb-4", "ml-4",
        "w-0", "w-1", "w-2", "w-4", "w-8", "w-16", "w-32", "w-64",
        "w-full", "w-screen", "w-min", "w-max", "w-fit",
        "w-1/2", "w-1/3",
        "h-0", "h-1", "h-2", "h-4", "h-8", "h-16", "h-32", "h-64",
        "h-full", "h-screen", "h-min", "h-max", "h-fit",
        "h-1/2", "h-1/3",
        "max-w-none", "max-w-xs", "max-w-sm", "max-w-md", "max-w-lg",
        "min-h-0", "min-h-full", "min-h-screen", "max-h-screen", "max-h-full",
        "font-thin", "font-light", "font-normal", "font-medium", "font-semibold",
        "font-bold", "font-extrabold", "font-black",
        "text-xs", "text-sm", "text-base", "text-lg", "text-xl", "text-2xl", "text-3xl",
        "text-left", "text-center", "text-right", "text-justify",
        "underline", "overline", "line-through", "no-underline",
        "uppercase", "lowercase",
        "border", "border-0", "border-2", "border-4", "border-8",
        "border-t", "border-b", "border-l", "border-r",
        "border-solid", "border-dashed", "border-dotted", "border-double",
        "border-black", "border-white",
        "rounded", "rounded-none", "rounded-sm", "rounded-md", "rounded-lg",
        "rounded-xl", "rounded-2xl", "rounded-3xl", "rounded-full",
        "rounded-t-lg",
        "shadow", "shadow-sm", "shadow-md", "shadow-lg", "shadow-xl",
        "shadow-2xl", "shadow-inner", "shadow-none",
        "shadow-black", "shadow-white",
        "opacity-0", "opacity-10", "opacity-20", "opacity-30", "opacity-40",
        "opacity-50", "opacity-60", "opacity-70", "opacity-80", "opacity-100",
        "flex-row", "flex-row-reverse", "flex-col", "flex-col-reverse",
        "flex-wrap", "flex-nowrap", "flex-wrap-reverse",
        "flex-1", "flex-auto", "flex-initial", "flex-none",
        "grow", "grow-0", "shrink", "shrink-0",
        "grid-cols-1", "grid-cols-2", "grid-cols-3", "grid-cols-4", "grid-cols-5",
        "grid-rows-1", "grid-rows-2", "grid-rows-3", "grid-rows-4", "grid-rows-5",
        "justify-start", "justify-end", "justify-center", "justify-between", "justify-around",
        "items-start", "items-end", "items-center", "items-baseline", "items-stretch",
        "scale-50", "scale-75", "scale-90", "scale-100", "scale-110",
        "rotate-0", "rotate-45", "rotate-90", "rotate-180", "-rotate-45",
        "sm:p-4", "sm:m-4", "sm:flex", "sm:grid", "sm:hidden",
        "md:p-6", "md:m-6", "md:block", "md:text-lg", "md:w-1/2",
        "lg:p-8", "lg:m-8", "lg:text-xl", "lg:w-1/3", "lg:grid-cols-3",
        "hover:bg-blue-600", "hover:text-white", "hover:shadow-lg", "hover:scale-105", "hover:opacity-80",
        "focus:outline-none", "focus:ring", "focus:ring-blue-500", "focus:border-blue-500", "focus:shadow-outline",
        "active:bg-blue-700", "active:scale-95", "active:shadow-inner", "active:opacity-90", "active:translate-y-1",
        "dark:bg-gray-800", "dark:text-white", "dark:border-gray-700", "dark:shadow-2xl", "dark:opacity-90",
        "dark:hover:bg-gray-700", "dark:focus:ring-white", "dark:active:bg-gray-900",
        "dark:md:text-xl", "dark:lg:p-8",
        "bg-[#123456]", "text-[#fedcba]", "w-[100px]", "h-[200px]", "p-[25px]",
        "m-[10%]", "rounded-[15px]", "shadow-[0_0_10px_rgba(0,0,0,0.5)]",
        "border-[3px]", "opacity-[0.33]",
        "bg-red-500/50", "text-blue-700/25", "border-green-400/75",
        "shadow-black/30", "bg-purple-600/[0.85]",
        "hover:bg-red-500/60", "dark:bg-gray-900/90",
        "focus:border-blue-500/100", "active:text-white/80", "sm:bg-yellow-400/40",
        "!p-4", "!m-0", "!text-red-500", "hover:!bg-blue-600", "!important",
        "overflow-hidden", "overflow-auto", "overflow-scroll", "overflow-visible",
        "z-0", "z-10", "z-20", "z-30", "z-40", "z-50",
        "cursor-pointer", "cursor-default", "cursor-wait", "cursor-text", "cursor-move",
    ];

    private static readonly string[] Prose =
    [
        "the", "and", "of", "to", "in", "is", "that", "it", "for", "with",
        "as", "was", "on", "are", "this", "be", "at", "by", "from", "or",
        "an", "will", "can", "has", "have", "but", "not", "all", "one", "when",
        "what", "your", "more", "about", "which", "their", "them", "then", "some", "time",
        "into", "over", "also", "after", "most", "such", "only", "other", "than", "like",
        "content", "page", "layout", "section", "header", "footer", "sidebar", "panel", "component", "render",
    ];

    /// <summary>
    /// Generates a deterministic temp-directory corpus of <c>.razor</c>, <c>.cs</c>,
    /// <c>.html</c>, and <c>.md</c> files, each interleaving real utility classes with
    /// natural-language noise so the scanner has both signal and false positives to chew on.
    /// </summary>
    /// <returns>The absolute path of the generated corpus directory.</returns>
    public static string BuildCorpus()
    {
        var dir = Path.Combine(Path.GetTempPath(), "monorail-bench-corpus-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        for (var i = 0; i < 25; i++)
        {
            File.WriteAllText(Path.Combine(dir, $"Component{i}.razor"), Markup(i, blocks: 30));
        }

        for (var i = 0; i < 8; i++)
        {
            File.WriteAllText(Path.Combine(dir, $"Page{i}.html"), Markup(i + 100, blocks: 30));
        }

        for (var i = 0; i < 12; i++)
        {
            File.WriteAllText(Path.Combine(dir, $"Source{i}.cs"), CSharp(i + 200, fields: 26));
        }

        for (var i = 0; i < 15; i++)
        {
            File.WriteAllText(Path.Combine(dir, $"Doc{i}.md"), Markdown(i + 300, paragraphs: 20));
        }

        return dir;
    }

    private static string Markup(int seed, int blocks)
    {
        var rng = new Random(seed);
        var sb = new StringBuilder();
        for (var b = 0; b < blocks; b++)
        {
            sb.Append("<div class=\"").Append(PickClasses(rng, 3, 6)).Append("\">\n  ");
            sb.Append(PickProse(rng, 20, 40)).Append("\n</div>\n");
        }

        return sb.ToString();
    }

    private static string CSharp(int seed, int fields)
    {
        var rng = new Random(seed);
        var sb = new StringBuilder();
        sb.Append("namespace Bench;\n\npublic class Source").Append(seed).Append("\n{\n");
        for (var f = 0; f < fields; f++)
        {
            var value = f % 2 == 0 ? PickClasses(rng, 2, 5) : PickProse(rng, 8, 16);
            sb.Append("    public string Field").Append(f).Append(" = \"").Append(value).Append("\";\n");
        }

        sb.Append("\n    public void Run()\n    {\n");
        for (var m = 0; m < 6; m++)
        {
            sb.Append("        System.Console.WriteLine(\"").Append(PickProse(rng, 6, 12)).Append("\");\n");
        }

        sb.Append("    }\n}\n");
        return sb.ToString();
    }

    private static string Markdown(int seed, int paragraphs)
    {
        var rng = new Random(seed);
        var sb = new StringBuilder();
        sb.Append("# Document ").Append(seed).Append("\n\n");
        for (var p = 0; p < paragraphs; p++)
        {
            sb.Append(PickProse(rng, 30, 60));
            sb.Append(" `").Append(PickClasses(rng, 1, 2)).Append("` ");
            sb.Append(PickProse(rng, 10, 20)).Append("\n\n");
        }

        return sb.ToString();
    }

    private static string PickClasses(Random rng, int min, int max)
    {
        var count = rng.Next(min, max + 1);
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                sb.Append(' ');
            }

            sb.Append(Classes[rng.Next(Classes.Length)]);
        }

        return sb.ToString();
    }

    private static string PickProse(Random rng, int min, int max)
    {
        var count = rng.Next(min, max + 1);
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                sb.Append(' ');
            }

            sb.Append(Prose[rng.Next(Prose.Length)]);
        }

        return sb.ToString();
    }
}
