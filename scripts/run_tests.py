#!/usr/bin/env python3
import subprocess
import sys
from pathlib import Path


class DotnetTestRunner:
    """Runs .NET tests and captures results"""

    def __init__(self, test_project_path: str):
        self.test_project_path = Path(test_project_path)

        if not self.test_project_path.exists():
            raise FileNotFoundError(f"Test project not found: {test_project_path}")

    def run_tests(self, verbose: bool = False) -> dict:
        """
        Run dotnet test and return results.

        Returns:
            dict with keys: 'passed' (bool), 'total', 'passed_count', 'failed_count', 'output'
        """
        cmd = ["dotnet", "test", str(self.test_project_path), "--nologo"]

        if not verbose:
            cmd.append("--verbosity")
            cmd.append("quiet")

        try:
            # Run the tests
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=60,  # 60 second timeout
            )

            output = result.stdout + result.stderr

            passed = result.returncode == 0

            total, passed_count, failed_count = self._parse_test_counts(output)

            return {
                "passed": passed,
                "total": total,
                "passed_count": passed_count,
                "failed_count": failed_count,
                "output": output,
                "returncode": result.returncode,
            }

        except subprocess.TimeoutExpired:
            return {
                "passed": False,
                "total": 0,
                "passed_count": 0,
                "failed_count": 0,
                "output": "Test execution timed out after 60 seconds",
                "returncode": -1,
            }
        except Exception as e:
            return {
                "passed": False,
                "total": 0,
                "passed_count": 0,
                "failed_count": 0,
                "output": f"Error running tests: {str(e)}",
                "returncode": -1,
            }

    def _parse_test_counts(self, output: str) -> tuple:
        """
        Parse test counts from dotnet test output.

        Returns:
            Tuple of (total, passed, failed)
        """
        import re

        total = 0
        passed = 0
        failed = 0

        # Try pattern 1: "Passed: X, Failed: Y, ... Total: Z"
        total_match = re.search(r"Total:\s*(\d+)", output)
        passed_match = re.search(r"Passed:\s*(\d+)", output)
        failed_match = re.search(r"Failed:\s*(\d+)", output)

        if total_match:
            total = int(total_match.group(1))
        if passed_match:
            passed = int(passed_match.group(1))
        if failed_match:
            failed = int(failed_match.group(1))

        # Try pattern 2: "Total tests: X. Passed: Y. Failed: Z."
        if total == 0:
            total_match2 = re.search(r"Total tests:\s*(\d+)", output)
            passed_match2 = re.search(r"Passed:\s*(\d+)", output)
            failed_match2 = re.search(r"Failed:\s*(\d+)", output)

            if total_match2:
                total = int(total_match2.group(1))
            if passed_match2:
                passed = int(passed_match2.group(1))
            if failed_match2:
                failed = int(failed_match2.group(1))

        return total, passed, failed


def run_tests(test_project_path: str, verbose: bool = False) -> dict:
    """
    Convenience function to run tests.

    Args:
        test_project_path: Path to the test project (.csproj file or directory)
        verbose: Whether to show verbose output

    Returns:
        Dictionary with test results
    """
    runner = DotnetTestRunner(test_project_path)
    return runner.run_tests(verbose=verbose)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python run_tests.py <test_project_path>")
        sys.exit(1)

    test_project = sys.argv[1]
    verbose = "--verbose" in sys.argv

    results = run_tests(test_project, verbose=verbose)

    print("\nTest Results:")
    print(f"  Status: {'PASSED' if results['passed'] else 'FAILED'}")
    print(f"  Total: {results['total']}")
    print(f"  Passed: {results['passed_count']}")
    print(f"  Failed: {results['failed_count']}")

    if verbose or not results["passed"]:
        print(f"\nOutput:\n{results['output']}")
