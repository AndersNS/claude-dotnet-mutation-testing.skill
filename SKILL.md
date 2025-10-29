---
name: dotnet-mutation-testing
description: Perform mutation testing on .NET/C# projects. Use when the user wants to test the quality of their test suite by applying code mutations and checking if tests catch them. Triggered by requests like "run mutation testing on this file", "check my test coverage with mutations", "mutate this code and run tests", or when discussing test effectiveness.
allowed-tools: Read, Write, Edit, Bash, Glob, Grep
---

# .NET Mutation Testing

Mutation testing evaluates test suite quality by introducing small code changes (mutations) and checking if tests catch them. This skill automates mutation testing for .NET C# projects.

## Quick Start

**Basic usage:**
1. User provides a C# source file path and test project path
2. Claude applies mutations systematically
3. Claude runs tests after each mutation
4. Claude generates a report showing which mutations survived

**Example user request:**
> "Run mutation testing on `Calculator.cs` using tests in `CalculatorTests.csproj`"

## Workflow

### Step 1: Identify Mutations

Use `scripts/mutate_csharp.py` to analyze the source file and find all possible mutations:

```python
from scripts.mutate_csharp import mutate_file

# Find all mutations
original_code, mutations = mutate_file("path/to/file.cs")
print(f"Found {len(mutations)} possible mutations")
```

Each mutation includes:
- Line number
- Original code
- Mutated code
- Mutation type (ARITHMETIC, COMPARISON, LOGICAL, BOOLEAN, UNARY)
- Description

### Step 2: Apply Each Mutation and Test

For each mutation:

1. **Backup original file** - Keep original content in memory
2. **Apply mutation** - Use `mutate_file()` with mutation index
3. **Write mutated code** - Overwrite source file with mutated version
4. **Run tests** - Execute tests using `scripts/run_tests.py`
5. **Restore original** - Write original content back to file
6. **Record results** - Track whether mutation was killed or survived

```python
from scripts.mutate_csharp import mutate_file
from scripts.run_tests import run_tests

# Read original
with open(source_file, 'r') as f:
    original_code = f.read()

# Apply mutation
mutated_code, mutations = mutate_file(source_file, mutation_index=i)

# Write mutated version
with open(source_file, 'w') as f:
    f.write(mutated_code)

# Run tests
results = run_tests(test_project_path)

# Restore original
with open(source_file, 'w') as f:
    f.write(original_code)

# Check if mutation was killed
mutation_killed = not results['passed']
```

### Step 3: Generate Report

Create a text report file containing:

1. **Summary Section**
   - Total mutations tested
   - Mutations killed (tests caught the change)
   - Mutations survived (tests still passed)
   - Mutation score percentage

2. **Survived Mutations** (most important)
   - Line number and description
   - Original vs mutated code
   - Why this indicates a test gap

3. **Killed Mutations** (optional detail)
   - Brief list for completeness

**Report format example:**

```
MUTATION TESTING REPORT
=======================
File: Calculator.cs
Test Project: CalculatorTests.csproj
Date: 2025-10-29

SUMMARY
-------
Total Mutations: 10
Killed: 8 (80.0%)
Survived: 2 (20.0%)

Mutation Score: 80.0%

SURVIVED MUTATIONS (Test Gaps)
-------------------------------
[1] Line 15: Replace - with + on line 15
    Original: return a - b;
    Mutated:  return a + b;
    → Tests did not catch this change in the Subtract method

[2] Line 28: Replace == with != on line 28
    Original: return value == 0;
    Mutated:  return value != 0;
    → Tests did not verify the equality check

KILLED MUTATIONS
----------------
✓ Line 9: Replace + with - (caught by tests)
✓ Line 19: Replace > with < (caught by tests)
...
```

## Implementation Notes

### Error Handling

- Verify source file exists before starting
- Verify test project exists and is valid
- Ensure dotnet CLI is available
- Handle test timeout (60 seconds per run)
- Always restore original file, even if errors occur

### Performance Considerations

- Mutation testing can be slow (N mutations × test execution time)
- Inform user of progress regularly
- Consider showing progress: "Testing mutation 3 of 15..."

### File Safety

Always use this pattern to ensure file restoration:
```python
try:
    with open(source_file, 'r') as f:
        original_code = f.read()
    
    # Apply mutation and test
    # ...
    
finally:
    # Always restore
    with open(source_file, 'w') as f:
        f.write(original_code)
```

## Supported Mutations

See [references/csharp_mutations.md](references/csharp_mutations.md) for complete list of mutation patterns including:

- Arithmetic operators (+, -, *, /, %)
- Comparison operators (==, !=, <, >, <=, >=)
- Logical operators (&&, ||)
- Boolean literals (true, false)
- Unary operators (++, --)

## Scripts

### mutate_csharp.py

Python module for finding and applying mutations to C# files.

**Key functions:**
- `mutate_file(file_path, mutation_index=None)` - Find mutations or apply a specific one
- Returns tuple: (code, mutations_list)

### run_tests.py

Python module for executing dotnet tests and capturing results.

**Key functions:**
- `run_tests(test_project_path, verbose=False)` - Run tests and return results
- Returns dict with: `passed`, `total`, `passed_count`, `failed_count`, `output`

## Limitations

- Only supports C# syntax
- Requires dotnet CLI to be installed
- Does not mutate comments or whitespace
- Does not mutate method calls or return statements (yet)
- Test timeout set to 60 seconds per mutation
