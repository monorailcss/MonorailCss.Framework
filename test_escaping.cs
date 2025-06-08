using System;
using MonorailCss;

class TestEscaping 
{
    static void Main() 
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        
        // Test with question mark
        Console.WriteLine("=== Testing with ? character ===");
        var result1 = framework.Process(["bg-[color:red?]"]);
        Console.WriteLine(result1);
        
        // Test with equals sign  
        Console.WriteLine("\n=== Testing with = character ===");
        var result2 = framework.Process(["bg-[color=red]"]);
        Console.WriteLine(result2);
        
        // Test with both
        Console.WriteLine("\n=== Testing with both ? and = characters ===");
        var result3 = framework.Process(["bg-[color=red?important]"]);
        Console.WriteLine(result3);
    }
}