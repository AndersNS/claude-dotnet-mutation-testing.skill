# Dotnet Mutation Testing Skill for Claude Code

A mutation testing skill for Claude Code built with Roslyn that uses syntax tree analysis to generate precise, context-aware mutations for .NET C# projects.

## Claude Code Skill

This is a **Claude Code Skill** - an extension that gives Claude specialized capabilities. This skill enables Claude to perform mutation testing on your .NET projects.

### Installing This Skill

Skills are stored in `~/.claude/skills/` directory. To install:

```bash
git clone git@github.com:AndersNS/claude-dotnet-mutation-testing.skill.git ~/.claude/skills/dotnet-mutation-testing
```

Claude should now pick up the skill automatically. To verify you can ask claude what skills claude has access to. It should then list dotnet mutation testing among them.

### Using This Skill

Once installed, simply ask Claude Code to run mutation testing:

**Example requests:**

- "Run mutation testing on `Calculator.cs` using the tests in `CalculatorTests.csproj`"
- "Check my test coverage with mutations on `MyService.cs`"
- "Mutate this code and run tests: `src/Utils/Parser.cs`"

Claude will automatically:

1. Build the mutation testing tool (if needed)
2. Run mutation testing with Roslyn-based analysis
3. Generate a detailed report
4. Explain the results and suggest test improvements

## Overview

Mutation testing is a technique for evaluating the coverage and quality of your tests by
introducing bugs ("mutations") into your implementation and checking whether your tests catch
them.

How it works:

1. Analyzes C# source code using Roslyn syntax trees
2. Generates various types of mutations (operator changes, literal modifications, etc.)
3. Runs your tests against each mutation
4. Each mutation falls into one of three categories:
   - Killed ✓ (Good): Tests failed, meaning your tests detected the bug
   - Survived ✗ (Bad): Tests passed, meaning your tests missed the bug
   - Error: The mutated code didn't compile or tests couldn't run

If a mutation survives it indicates a gap in your tests. It can either mean that specific case isn't covered by a test, or that your assertions aren't good enough.

### Mutations

We use the Roslyn API to identify and apply mutations. Below are the currently supported mutations:

**Basic Operators:**

- Arithmetic: `+` ↔ `-`, `*` ↔ `/`, `%` → `*`
- Comparison: `==` ↔ `!=`, `<` ↔ `>`, `<=` → `>`, `>=` → `<`
- Logical: `&&` ↔ `||`
- Boolean: `true` ↔ `false`
- Unary: `++` ↔ `--`

**Advanced Mutations:**

- **Literals**:
  - Integers: `0` ↔ `1`, positive n > 1 → `n+1` and `n-1`, negative n → `n+1`
  - Longs: `0L` → `1L`
  - Doubles: `0.0` → `1.0`
  - Strings: `""` ↔ `"mutated"`, non-empty ↔ empty string
- **LINQ Methods**:
  - `First()` ↔ `Last()`
  * `FirstOrDefault()` ↔ `LastOrDefault()`
  * `Any()` ↔ `All()`
  - `Min()` ↔ `Max()`
  - `Skip()` ↔ `Take()`
  - `SkipWhile()` ↔ `TakeWhile()`
- **Assignments**:
  - `+=` ↔ `-=`
  - `*=` ↔ `/=`
- **Statements**: Return values (non-zero integers → `0`, integers → `n+1`), remove method calls

## Installation

### Prerequisites

- .NET 8 SDK or later
- `dotnet` CLI in your PATH

### Build

```bash
cd src/MutationTesting
dotnet build
```

## Usage

### Basic Command

```bash
cd src/MutationTesting
dotnet run -- <source-file> <test-project> [options]
```

### Arguments

- `source-file`: Path to the C# file to mutate
- `test-project`: Path to test project (`.csproj` file or directory)

### Options

- `-v, --verbose`: Show detailed progress during mutation testing
- `-o, --output <file>`: Custom output path for the report
- `-h, --help`: Show help message

### Examples

```bash
# Basic usage
dotnet run -- ~/MyApp/Calculator.cs ~/MyApp.Tests/MyApp.Tests.csproj

# With verbose output
dotnet run -- ~/MyApp/Calculator.cs ~/MyApp.Tests/MyApp.Tests.csproj -v

# Custom report location
dotnet run -- ~/MyApp/Calculator.cs ~/MyApp.Tests/MyApp.Tests.csproj -o my_report.txt
```

## How It Works

1. **Discovery**: Parses source file into Roslyn syntax tree and identifies mutation opportunities
2. **Baseline**: Runs tests with original code to ensure they pass
3. **Mutation Loop**: For each mutation:
   - Apply mutation to source file
   - Run tests
   - Record if mutation was "killed" (tests failed) or "survived" (tests passed)
   - Restore original code
4. **Report**: Generate detailed report highlighting test gaps (survived mutations)

## Example Report

```
================================================================================
MUTATION TESTING REPORT
================================================================================

Source File: Calculator.cs
Generated: 2025-10-29 14:32:15
Duration: 12.3 seconds

SUMMARY
Total Mutations:     15
Killed Mutations:    12 (80.0%)
Survived Mutations:  3 (20.0%)

MUTATION SCORE: 80.0%

SURVIVED MUTATIONS (Test Gaps)
1. Replace + with -
   Location: Calculator.cs:15:20
   Original: a + b
   Mutated:  a - b

...
```

## Exit Codes

- `0`: Mutation score ≥ 80%
- `1`: Mutation score < 80% or error occurred

## Troubleshooting

### "Baseline tests failed"

- Ensure your tests pass before running mutation testing
- Check that test project path is correct

### "File not found"

- Use absolute paths or paths relative to current directory
- Verify file exists: `ls <source-file>`

### "dotnet: command not found"

- Install .NET SDK: https://dotnet.microsoft.com/download
- Ensure `dotnet` is in your PATH

## Contributing

The architecture is extensible - new mutators can be added by implementing `IMutator` or extending `MutatorBase`.

## License

This is a skill for Claude Code. Use as part of the Claude Code Skills system.
