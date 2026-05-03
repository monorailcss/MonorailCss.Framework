---
name: prefer-builtin-tools
enabled: true
event: bash
pattern: \b(find|rg|dir|ls|sed|awk)\b
action: block
---

**Use Built-in Tools Instead of Bash**

You're attempting to use a Bash command that has a dedicated built-in tool:

| Bash Command | Use Instead |
|--------------|-------------|
| `find`, `ls`, `dir` | **Glob** tool for file pattern matching |
| `grep`, `rg` | **Grep** tool for content searching |
| `cat` | **Read** tool for reading files |
| `sed`, `awk` | **Edit** tool for file modifications |
| `echo` | Direct text output in your response |

**Why this matters:**
- Built-in tools are optimized for the codebase
- They provide better formatted output
- They handle permissions and access correctly
- Results are easier to parse and use

**Action:** Use the appropriate built-in tool instead.
