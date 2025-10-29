#!/usr/bin/env python3
import re
from dataclasses import dataclass
from typing import List, Tuple
from pathlib import Path


@dataclass
class Mutation:
    """Represents a single mutation"""

    line_number: int
    original: str
    mutated: str
    mutation_type: str
    description: str


class CSharpMutator:
    """Applies mutations to C# source code"""

    def __init__(self):
        # (pattern, replacement, type, description)
        self.mutations = [
            # Arithmetic operators
            (r"\+", "-", "ARITHMETIC", "Replace + with -"),
            (r"-", "+", "ARITHMETIC", "Replace - with +"),
            (r"\*", "/", "ARITHMETIC", "Replace * with /"),
            (r"/", "*", "ARITHMETIC", "Replace / with *"),
            (r"%", "*", "ARITHMETIC", "Replace % with *"),
            # Comparison operators
            (r"==", "!=", "COMPARISON", "Replace == with !="),
            (r"!=", "==", "COMPARISON", "Replace != with =="),
            (r"<", ">", "COMPARISON", "Replace < with >"),
            (r">", "<", "COMPARISON", "Replace > with <"),
            (r"<=", ">", "COMPARISON", "Replace <= with >"),
            (r">=", "<", "COMPARISON", "Replace >= with <"),
            # Logical operators
            (r"&&", r"||", "LOGICAL", "Replace && with ||"),
            (r"\|\|", "&&", "LOGICAL", "Replace || with &&"),
            # Boolean literals
            (r"\btrue\b", "false", "BOOLEAN", "Replace true with false"),
            (r"\bfalse\b", "true", "BOOLEAN", "Replace false with true"),
            # Unary operators
            (r"\+\+", "--", "UNARY", "Replace ++ with --"),
            (r"--", "++", "UNARY", "Replace -- with ++"),
        ]

    def find_mutations(self, source_code: str) -> List[Mutation]:
        """Find all possible mutations in the source code"""
        mutations = []
        lines = source_code.split("\n")

        for line_num, line in enumerate(lines, start=1):
            # Skip comments and empty lines
            if (
                line.strip().startswith("//")
                or line.strip().startswith("/*")
                or not line.strip()
            ):
                continue

            for pattern, replacement, mut_type, description in self.mutations:
                # Find all matches in the line
                matches = list(re.finditer(pattern, line))

                for match in matches:
                    mutations.append(
                        Mutation(
                            line_number=line_num,
                            original=match.group(0),
                            mutated=replacement,
                            mutation_type=mut_type,
                            description=f"{description} on line {line_num}",
                        )
                    )

        return mutations

    def apply_mutation(self, source_code: str, mutation: Mutation) -> str:
        """Apply a single mutation to the source code"""
        lines = source_code.split("\n")

        if mutation.line_number > len(lines):
            raise ValueError(f"Line number {mutation.line_number} exceeds file length")

        # Get the target line (line_number is 1-indexed)
        target_line = lines[mutation.line_number - 1]

        # Apply the mutation (replace first occurrence only to be precise)
        mutated_line = target_line.replace(mutation.original, mutation.mutated, 1)

        # Replace the line
        lines[mutation.line_number - 1] = mutated_line

        return "\n".join(lines)


def mutate_file(
    file_path: str, mutation_index: int | None = None
) -> Tuple[str, List[Mutation]]:
    """
    Read a C# file and either list all mutations or apply a specific one.

    Args:
        file_path: Path to the C# source file
        mutation_index: If provided, apply this mutation (0-indexed)

    Returns:
        Tuple of (mutated_code or original_code, list_of_mutations)
    """
    path = Path(file_path)

    if not path.exists():
        raise FileNotFoundError(f"File not found: {file_path}")

    with open(path, "r", encoding="utf-8") as f:
        original_code = f.read()

    mutator = CSharpMutator()
    mutations = mutator.find_mutations(original_code)

    if mutation_index is not None:
        if mutation_index < 0 or mutation_index >= len(mutations):
            raise ValueError(f"Invalid mutation index: {mutation_index}")

        mutated_code = mutator.apply_mutation(original_code, mutations[mutation_index])
        return mutated_code, mutations

    return original_code, mutations


if __name__ == "__main__":
    import sys

    if len(sys.argv) < 2:
        print("Usage: python mutate_csharp.py <file.cs> [mutation_index]")
        sys.exit(1)

    file_path = sys.argv[1]
    mutation_idx: int | None = int(sys.argv[2]) if len(sys.argv) > 2 else None

    code, mutations = mutate_file(file_path, mutation_idx)

    if mutation_idx is None:
        print(f"Found {len(mutations)} possible mutations:")
        for i, mut in enumerate(mutations):
            print(f"  [{i}] Line {mut.line_number}: {mut.description}")
    else:
        print(code)
