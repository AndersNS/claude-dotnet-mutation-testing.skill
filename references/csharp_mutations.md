# C# Mutation Patterns

This document describes all mutation patterns supported by the mutation testing skill.

## Arithmetic Operators

| Original | Mutated | Description |
|----------|---------|-------------|
| `+` | `-` | Addition to subtraction |
| `-` | `+` | Subtraction to addition |
| `*` | `/` | Multiplication to division |
| `/` | `*` | Division to multiplication |
| `%` | `*` | Modulo to multiplication |

**Example:**
```csharp
// Original
int result = a + b;

// Mutated
int result = a - b;
```

## Comparison Operators

| Original | Mutated | Description |
|----------|---------|-------------|
| `==` | `!=` | Equality to inequality |
| `!=` | `==` | Inequality to equality |
| `<` | `>` | Less than to greater than |
| `>` | `<` | Greater than to less than |
| `<=` | `>` | Less than or equal to greater than |
| `>=` | `<` | Greater than or equal to less than |

**Example:**
```csharp
// Original
if (x == y) { }

// Mutated
if (x != y) { }
```

## Logical Operators

| Original | Mutated | Description |
|----------|---------|-------------|
| `&&` | `\|\|` | Logical AND to logical OR |
| `\|\|` | `&&` | Logical OR to logical AND |

**Example:**
```csharp
// Original
if (isValid && isActive) { }

// Mutated
if (isValid || isActive) { }
```

## Boolean Literals

| Original | Mutated | Description |
|----------|---------|-------------|
| `true` | `false` | True to false |
| `false` | `true` | False to true |

**Example:**
```csharp
// Original
bool flag = true;

// Mutated
bool flag = false;
```

## Unary Operators

| Original | Mutated | Description |
|----------|---------|-------------|
| `++` | `--` | Increment to decrement |
| `--` | `++` | Decrement to increment |

**Example:**
```csharp
// Original
counter++;

// Mutated
counter--;
```

## Mutation Testing Concepts

### Killed Mutation
A mutation is "killed" when it causes at least one test to fail. This indicates that your tests can detect this type of defect.

### Survived Mutation
A mutation "survives" when all tests still pass despite the code being changed. This indicates a gap in test coverage or a redundant code path.

### Mutation Score
The mutation score is calculated as:
```
Mutation Score = (Killed Mutations / Total Mutations) × 100%
```

A higher mutation score indicates more effective tests. Generally:
- 80-100%: Excellent test coverage
- 60-80%: Good test coverage
- 40-60%: Adequate test coverage
- Below 40%: Poor test coverage
