---
name: block-csx-files
enabled: true
event: all
conditions:
  - field: file_path
    operator: regex_match
    pattern: \.csx$
action: block
---

🚫 **C# Script (.csx) files are not supported**

You're trying to work with a `.csx` file, but these are not a standard feature.

**What you probably want instead:**

Use **single-file .NET applications** with a `.cs` extension:

```csharp
// MyScript.cs
Console.WriteLine("Hello World!");
```

**Run with:**
```bash
dotnet run MyScript.cs
```

**Key differences:**
- ✅ `.cs` files - Single-file .NET applications (supported)
- ❌ `.csx` files - C# Script format (not a thing)

Please use `.cs` extension instead.
