using System.Text;
using MonorailCss;
Console.OutputEncoding = Encoding.UTF8;

var framework = new CssFramework(new CssFrameworkSettings { IncludePreflight = false });

// Check if we have command-line arguments
if (args.Length > 0)
{
    // Process the arguments as CSS classes
    var input = string.Join(" ", args);
    ProcessAndDisplay(framework, input);
    return;
}

Console.WriteLine("MonorailCss Interactive Tester");
Console.WriteLine("==============================");
Console.WriteLine("Enter CSS classes to process, or 'exit' to quit.");
Console.WriteLine("Example: bg-red-500 text-white p-4 flex");
Console.WriteLine();

// Interactive mode
while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        break;

    ProcessAndDisplay(framework, input);
    Console.WriteLine();
}

static void ProcessAndDisplay(CssFramework framework, string input)
{
    var result = framework.ProcessWithDetails(input);

    // Display input
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Input: {result.Input}");
    Console.ResetColor();
    Console.WriteLine();

    // Display generated CSS
    if (!string.IsNullOrEmpty(result.GeneratedCss))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Generated CSS:");
        Console.ResetColor();
        Console.WriteLine(result.GeneratedCss);
        Console.WriteLine();
    }

    // Display processed classes details
    if (result.ProcessedClasses.Length > 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Processed Classes:");
        Console.ResetColor();

        foreach (var processed in result.ProcessedClasses)
        {
            Console.Write("  • ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{processed.ClassName}");
            Console.ResetColor();
            Console.Write(" → ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{processed.UtilityName}");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    // Display invalid classes
    if (result.InvalidClasses.Length > 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Unrecognized Classes:");
        Console.ResetColor();
        foreach (var invalid in result.InvalidClasses)
        {
            Console.WriteLine($"  ✗ {invalid}");
        }
        Console.WriteLine();
    }

    // Display summary
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("Summary:");
    Console.ResetColor();
    Console.WriteLine($"  Processed: {result.ProcessedClasses.Length} classes");
    Console.WriteLine($"  Invalid: {result.InvalidClasses.Length} classes");
}