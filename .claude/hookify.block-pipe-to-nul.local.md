---
name: block-pipe-to-nul
enabled: true
event: bash
pattern: >\s*nul|2>\s*nul|>&\s*nul|2>&1\s*>\s*nul
action: block
---

**Piping to NUL is not allowed**

You're attempting to redirect output to `nul` (Windows null device), which discards the output.

**Why this is blocked:**
- Output should be visible for debugging and verification
- Silently discarding errors can hide problems
- The user wants to see command results

**What to do instead:**
- Let the output display normally
- If output is too verbose, summarize the important parts in your response
- If you need to check for errors, capture and report them
