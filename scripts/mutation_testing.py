#!/usr/bin/env python3
import sys
import os
from datetime import datetime

# Import our custom modules
sys.path.insert(0, os.path.dirname(__file__))
from mutate_csharp import CSharpMutator, Mutation, mutate_file
from run_tests import DotnetTestRunner


class MutationTestReport:
    """Generates mutation testing reports."""

    def __init__(self, source_file: str, test_project: str):
        self.source_file = source_file
        self.test_project = test_project
        self.survived_mutations = []
        self.killed_mutations = []
        self.error_mutations = []
        self.total_mutations = 0
        self.start_time: datetime | None = None
        self.end_time: datetime | None = None

    def add_result(self, mutation: Mutation, killed: bool, error: str | None = None):
        """Add a mutation test result."""
        if error:
            self.error_mutations.append((mutation, error))
        elif killed:
            self.killed_mutations.append(mutation)
        else:
            self.survived_mutations.append(mutation)

    def generate_report(self, output_file: str | None = None):
        """Generate a text report of mutation testing results."""
        if output_file is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            output_file = f"mutation_report_{timestamp}.txt"

        mutation_score = 0
        if self.total_mutations > 0:
            mutation_score = (len(self.killed_mutations) / self.total_mutations) * 100

        duration = ""
        if self.start_time and self.end_time:
            elapsed = (self.end_time - self.start_time).total_seconds()
            duration = f"{elapsed:.1f} seconds"

        report_lines = [
            "=" * 80,
            "MUTATION TESTING REPORT",
            "=" * 80,
            "",
            f"Source File: {self.source_file}",
            f"Test Project: {self.test_project}",
            f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}",
            f"Duration: {duration}",
            "",
            "=" * 80,
            "SUMMARY",
            "=" * 80,
            "",
            f"Total Mutations:     {self.total_mutations}",
            f"Killed Mutations:    {len(self.killed_mutations)} ({len(self.killed_mutations) / self.total_mutations * 100:.1f}%)"
            if self.total_mutations > 0
            else "Killed Mutations:    0",
            f"Survived Mutations:  {len(self.survived_mutations)} ({len(self.survived_mutations) / self.total_mutations * 100:.1f}%)"
            if self.total_mutations > 0
            else "Survived Mutations:  0",
            f"Error Mutations:     {len(self.error_mutations)}",
            "",
            f"MUTATION SCORE: {mutation_score:.1f}%",
            "",
        ]

        # Survived mutations (these are the important ones - weak spots in tests)
        if self.survived_mutations:
            report_lines.extend(
                [
                    "=" * 80,
                    "SURVIVED MUTATIONS (Not caught by tests - potential issues!)",
                    "=" * 80,
                    "",
                ]
            )

            for i, mut in enumerate(self.survived_mutations, 1):
                report_lines.extend(
                    [
                        f"{i}. Line {mut.line_number}: {mut.description}",
                        f"   Original: {mut.original}",
                        f"   Mutated:  {mut.mutated}",
                        f"   Type:     {mut.mutation_type}",
                        "",
                    ]
                )

        # Killed mutations summary (less detail needed)
        if self.killed_mutations:
            report_lines.extend(
                [
                    "=" * 80,
                    f"KILLED MUTATIONS (Caught by tests - {len(self.killed_mutations)} mutations)",
                    "=" * 80,
                    "",
                ]
            )

            # Group by type
            by_type = {}
            for mut in self.killed_mutations:
                by_type.setdefault(mut.mutation_type, []).append(mut)

            for mut_type, muts in by_type.items():
                report_lines.append(f"{mut_type}: {len(muts)} mutations")
            report_lines.append("")

        # Errors
        if self.error_mutations:
            report_lines.extend(
                [
                    "=" * 80,
                    "ERROR MUTATIONS (Could not test)",
                    "=" * 80,
                    "",
                ]
            )

            for i, (mut, error) in enumerate(self.error_mutations, 1):
                report_lines.extend(
                    [
                        f"{i}. Line {mut.line_number}: {mut.description}",
                        f"   Error: {error}",
                        "",
                    ]
                )

        report_lines.append("=" * 80)

        report_text = "\n".join(report_lines)

        with open(output_file, "w") as f:
            f.write(report_text)

        return output_file, report_text


def run_mutation_testing(
    source_file: str,
    test_project: str,
    output_file: str | None = None,
    verbose: bool = False,
) -> str:
    """
    Run mutation testing on a source file.

    Args:
        source_file: Path to C# source file to mutate
        test_project: Path to test project (.csproj or directory)
        output_file: Optional output file path for report
        verbose: Print progress messages

    Returns:
        Path to generated report file
    """

    # Read source file
    with open(source_file, "r") as f:
        original_code = f.read()

    # Initialize components
    CSharpMutator()
    test_runner = DotnetTestRunner(test_project)
    report = MutationTestReport(source_file, test_project)

    # Generate all mutations
    _, mutations = mutate_file(source_file)
    report.total_mutations = len(mutations)

    if verbose:
        print(f"Found {len(mutations)} possible mutations")
        print(f"Source file: {source_file}")
        print(f"Test project: {test_project}")
        print()

    # First, verify tests pass with original code
    if verbose:
        print("Running baseline tests with original code...")

    baseline_passed, baseline_output, baseline_results = test_runner.run_tests()

    if not baseline_passed:
        error_msg = f"Baseline tests failed! Cannot proceed with mutation testing.\n{baseline_output}"
        print(error_msg)
        raise Exception(error_msg)

    if verbose:
        print(f"✓ Baseline tests passed ({baseline_results.get('passed', 0)} tests)")
        print()

    report.start_time = datetime.now()

    # Test each mutation
    for i, mutation in enumerate(mutations):
        if verbose:
            print(
                f"Testing mutation {i + 1}/{len(mutations)}: {mutation.description} (line {mutation.line_number})",
                end="",
            )

        try:
            # Apply mutation
            mutated_code, _ = mutate_file(source_file, mutation_index=i)

            with open(source_file, "w") as f:
                f.write(mutated_code)

            # Run tests
            tests_passed, output, results = test_runner.run_tests()

            # Record result
            killed = not tests_passed
            report.add_result(mutation, killed)

            if verbose:
                status = " - KILLED ✓" if killed else " - SURVIVED ✗"
                print(status)

        except Exception as e:
            report.add_result(mutation, False, str(e))
            if verbose:
                print(f" - ERROR: {str(e)}")

        finally:
            # Restore original code
            with open(source_file, "w") as f:
                f.write(original_code)

    report.end_time = datetime.now()

    # Generate report
    report_file, report_text = report.generate_report(output_file)

    if verbose:
        print()
        print(f"Report generated: {report_file}")
        print()
        print(report_text)

    return report_file


def main():
    """Command-line interface."""
    if len(sys.argv) < 3:
        print(
            "Usage: python mutation_testing.py <source_file> <test_project> [output_file]"
        )
        print()
        print("Arguments:")
        print("  source_file:   Path to C# source file to mutate")
        print("  test_project:  Path to test project (.csproj file or directory)")
        print(
            "  output_file:   Optional output file for report (default: auto-generated)"
        )
        sys.exit(1)

    source_file = sys.argv[1]
    test_project = sys.argv[2]
    output_file = sys.argv[3] if len(sys.argv) > 3 else None

    try:
        report_file = run_mutation_testing(
            source_file, test_project, output_file, verbose=True
        )
        print(f"\n✓ Mutation testing complete! Report: {report_file}")
    except Exception as e:
        print(f"\n✗ Error: {str(e)}")
        sys.exit(1)


if __name__ == "__main__":
    main()
