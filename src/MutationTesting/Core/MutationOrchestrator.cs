using MutationTesting.Models;
using MutationTesting.TestRunner;

namespace MutationTesting.Core;

/// <summary>
/// Orchestrates the mutation testing workflow.
/// </summary>
public class MutationOrchestrator(string testProjectPath, bool verbose = false)
{
    private readonly MutationEngine _engine = new();
    private readonly DotnetTestRunner _testRunner = new(testProjectPath);

    /// <summary>
    /// Runs mutation testing on the specified source file.
    /// </summary>
    public async Task<MutationReport> RunMutationTestingAsync(string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
        }

        var originalCode = await File.ReadAllTextAsync(sourceFilePath);

        var report = new MutationReport
        {
            SourceFilePath = sourceFilePath,
            TestProjectPath = _testRunner.ToString() ?? "",
            StartTime = DateTime.Now
        };

        // Step 1: Discover mutations
        if (verbose)
        {
            Console.WriteLine($"Discovering mutations in {Path.GetFileName(sourceFilePath)}...");
        }

        var mutations = MutationEngine.DiscoverMutations(originalCode, sourceFilePath);
        report.Mutations.AddRange(mutations);

        if (verbose)
        {
            Console.WriteLine($"Found {mutations.Count} possible mutations\n");
        }

        if (mutations.Count == 0)
        {
            if (verbose)
                Console.WriteLine("No mutations found. Exiting.");
            report.EndTime = DateTime.Now;
            return report;
        }

        // Step 2: Run baseline tests
        if (verbose)
        {
            Console.WriteLine("Running baseline tests with original code...");
        }

        var (baselinePassed, baselineOutput, baselineResults) = _testRunner.RunTests(verbose);

        if (!baselinePassed)
        {
            throw new Exception($"Baseline tests failed! Cannot proceed with mutation testing.\n{baselineOutput}");
        }

        if (verbose)
        {
            Console.WriteLine($"✓ Baseline tests passed ({baselineResults.Passed} tests)\n");
        }

        // Step 3: Test each mutation
        for (var i = 0; i < mutations.Count; i++)
        {
            var mutation = mutations[i];

            if (verbose)
            {
                Console.Write($"[{i + 1}/{mutations.Count}] Testing: {mutation.Description} ");
                Console.Write($"({mutation.Location.StartLinePosition.Line + 1}:{mutation.Location.StartLinePosition.Character + 1})");
            }

            try
            {
                mutation.Status = MutationStatus.Running;

                // Apply mutation
                var mutatedCode = _engine.ApplyMutation(originalCode, mutation, sourceFilePath);

                // Validate it compiles
                var (valid, errorMessage) = _engine.ValidateMutation(mutatedCode, sourceFilePath);
                if (!valid)
                {
                    mutation.Status = MutationStatus.BuildError;
                    mutation.ErrorMessage = errorMessage;

                    if (verbose)
                        Console.WriteLine($" - BUILD ERROR");

                    continue;
                }

                // Write mutated code to file
                await File.WriteAllTextAsync(sourceFilePath, mutatedCode);

                // Run tests
                var startTime = DateTime.Now;
                var (testsPassed, _, _) = _testRunner.RunTests(false);
                mutation.ExecutionTime = DateTime.Now - startTime;

                // Determine if mutation was killed
                mutation.Status = testsPassed ? MutationStatus.Survived : MutationStatus.Killed;

                if (verbose)
                {
                    var status = mutation.Status == MutationStatus.Killed ? "KILLED ✓" : "SURVIVED ✗";
                    Console.WriteLine($" - {status}");
                }
            }
            catch (Exception ex)
            {
                mutation.Status = MutationStatus.TestError;
                mutation.ErrorMessage = ex.Message;

                if (verbose)
                    Console.WriteLine($" - ERROR: {ex.Message}");
            }
            finally
            {
                // Always restore original code
                await File.WriteAllTextAsync(sourceFilePath, originalCode);
            }
        }

        report.EndTime = DateTime.Now;

        return report;
    }
}
