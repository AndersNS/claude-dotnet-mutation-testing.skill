---
name: dotnet-mutation-testing
description: Perform mutation testing on .NET/C# projects using Roslyn-based syntax tree analysis. Use when the user wants to test the quality of their test suite by applying code mutations and checking if tests catch them. Triggered by requests like "run mutation testing on this file", "check my test coverage with mutations", "mutate this code and run tests", or when discussing test effectiveness.
allowed-tools: Read, Write, Edit, Bash, Glob, Grep
---

# .NET Mutation Testing

Mutation testing evaluates test suite quality by introducing small code changes (mutations) and checking if tests catch them. This skill uses a Roslyn-based C# tool for precise, syntax-aware mutations.

## Quick Start

**Basic usage:**

1. User provides a C# source file path and test project path
2. Build and run the mutation testing tool: `dotnet run --project src/MutationTesting -- <source-file> <test-project> [-v]`
3. Tool discovers mutations using Roslyn syntax trees
4. Tool runs tests after each mutation
5. Tool generates a detailed report showing which mutations survived

**Example user request:**

> "Run mutation testing on `Calculator.cs` using tests in `CalculatorTests/CalculatorTests.csproj`"

## How It Works

The tool is built entirely in C# using Roslyn (the C# compiler platform) for accurate, syntax-aware mutation generation.

### Running Mutation Testing

```bash
dotnet run --project src/MutationTesting -- <source-file> <test-project> [options]

# Example:
dotnet run --project src/MutationTesting -- ~/MyProject/Calculator.cs ~/MyProject/Tests/CalculatorTests.csproj -v

# Options:
#   -v, --verbose        Show detailed progress and output
#   -o, --output <file>  Custom report output path
#   -h, --help           Show help message
```

Note: `dotnet run` will automatically build the project if needed, so no explicit build step is required.

### Architecture

**Core Components:**

1. **MutationEngine** (`Core/MutationEngine.cs`)
   - Discovers all mutations using Roslyn syntax trees
   - Applies mutations to code
   - Validates mutated code compiles

2. **Mutators** (`Mutations/` directory)
   - Each mutator implements specific mutation logic
   - Uses `CSharpSyntaxRewriter` to visit and transform syntax nodes
   - **Basic Mutators:**
     - `ArithmeticMutator`: +, -, \*, /, %
     - `ComparisonMutator`: ==, !=, <, >, <=, >=
     - `LogicalMutator`: &&, ||
     - `BooleanLiteralMutator`: true <-> false
   - **Advanced Mutators:**
     - `LiteralMutator`: Numeric constants (0→1, n→n±1), strings
     - `StatementMutator`: Return values, ++/--, remove method calls
     - `MethodCallMutator`: LINQ methods (First/Last, Any/All, Min/Max, etc.)
     - `AssignmentMutator`: +=, -=, \*=, /=

3. **MutationOrchestrator** (`Core/MutationOrchestrator.cs`)
   - Coordinates the workflow
   - Runs baseline tests
   - Applies each mutation and tests
   - Restores original code after each test

4. **DotNetTestRunner** (`TestRunner/DotNetTestRunner.cs`)
   - Executes `dotnet test` via process
   - Parses test results
   - Handles timeouts

5. **ReportGenerator** (`Core/ReportGenerator.cs`)
   - Generates detailed text reports
   - Highlights survived mutations (test gaps)
   - Shows mutation score

### Workflow Detail

1. **Discovery Phase**
   - Parse C# source file into Roslyn SyntaxTree
   - Each mutator visits the tree and identifies mutation opportunities
   - Generate `MutationResult` objects with location, original/mutated code

2. **Baseline Validation**
   - Run tests with original code
   - Fail fast if tests don't pass initially

3. **Mutation Testing Loop**
   - For each mutation:
     - Apply mutation to syntax tree
     - Write mutated code to source file
     - Run tests via `dotnet test`
     - Record: KILLED (tests failed) or SURVIVED (tests passed)
     - Restore original code

4. **Report Generation**
   - Calculate mutation score: `killed / (total - errors) * 100`
   - List survived mutations (test gaps) with full detail
   - Summarize killed mutations by type

## Example Report Output

```
================================================================================
MUTATION TESTING REPORT
================================================================================

Source File: Calculator.cs
Test Project: CalculatorTests.csproj
Generated: 2025-10-29 14:32:15
Duration: 12.3 seconds

================================================================================
SUMMARY
================================================================================

Total Mutations:     15
Killed Mutations:    12 (80.0%)
Survived Mutations:  3 (20.0%)
Error Mutations:     0

MUTATION SCORE: 80.0%

================================================================================
SURVIVED MUTATIONS (Not caught by tests - potential issues!)
================================================================================

1. Replace + with -
   Location: Calculator.cs:15:20
   Original: a + b
   Mutated:  a - b
   Type:     ARITHMETIC

2. Replace 0 with 1
   Location: Calculator.cs:28:24
   Original: 0
   Mutated:  1
   Type:     LITERAL

3. Replace First() with Last()
   Location: Calculator.cs:42:15
   Original: First()
   Mutated:  Last()
   Type:     METHOD_CALL

================================================================================
KILLED MUTATIONS (Caught by tests - 12 mutations)
================================================================================

ARITHMETIC: 5 mutations
COMPARISON: 4 mutations
LOGICAL: 2 mutations
BOOLEAN: 1 mutations

================================================================================
```

## Key Features

**Syntax-Aware Mutations:**

- Won't mutate operators inside strings or comments
- Understands C# semantics via Roslyn
- Type-safe transformations

**Rich Mutation Types:**

- Numeric literals (0→1, boundary values)
- String literals (""→"mutated")
- LINQ method calls (First/Last, Any/All, etc.)
- Assignment operators (+=, -=, etc.)
- Return statement values

**Performance:**

- In-memory syntax tree operations
- Validates mutations compile before testing
- Fast mutation discovery and application

## Usage from Claude Code

When user requests mutation testing:

1. **Validate inputs:**

   ```bash
   # Check file exists
   ls <source-file>
   # Check test project exists
   ls <test-project>
   ```

2. **Run mutation testing:**

   ```bash
   dotnet run --project src/MutationTesting -- <source-file> <test-project> -v
   ```

   Note: `dotnet run` automatically builds if needed, so no separate build step is required.

3. **Read and display report:**

   ```bash
   cat mutation_report_*.txt
   ```

4. **Interpret results for user:**
   - Explain what survived mutations mean
   - Suggest test improvements
   - Highlight critical test gaps

## Supported Mutations

**Basic Operators:**

- Arithmetic: +, -, \*, /, %
- Comparison: ==, !=, <, >, <=, >=
- Logical: &&, ||
- Boolean: true ↔ false
- Unary: ++, --

**Advanced:**

- Numeric literals: 0→1, n→n±1
- String literals: ""→"mutated"
- LINQ methods: First/Last, Any/All, Min/Max, Skip/Take, etc.
- Assignment: +=, -=, \*=, /=
- Return values: increment/decrement returned numbers

## Requirements

- .NET 8 SDK or later
- `dotnet` CLI available in PATH
- Source file and test project must be in valid .NET project structure

## Implementation Notes

This skill uses a Roslyn-based C# solution for syntax-aware mutation testing with accurate code analysis and advanced mutation types.
