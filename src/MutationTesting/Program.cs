using MutationTesting.Core;

namespace MutationTesting;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Roslyn-based Mutation Testing Tool for C#");
            Console.WriteLine();
            Console.WriteLine("Usage: MutationTesting <source-file> <test-project> [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  source-file    Path to the C# source file to mutate");
            Console.WriteLine("  test-project   Path to the test project (.csproj file or directory)");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -o, --output <file>   Output file path for the report (default: auto-generated)");
            Console.WriteLine("  -v, --verbose         Enable verbose output");
            Console.WriteLine("  -h, --help            Show this help message");
            return 1;
        }

        var sourceFile = args[0];
        var testProject = args[1];
        string? outputFile = null;
        var verbose = false;

        // Parse options
        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-v":
                case "--verbose":
                    verbose = true;
                    break;
                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        outputFile = args[++i];
                    }
                    break;
                case "-h":
                case "--help":
                    Console.WriteLine("Roslyn-based Mutation Testing Tool for C#");
                    return 0;
            }
        }

        try
        {
            if (verbose)
            {
                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine("MUTATION TESTING - ROSLYN EDITION");
                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine();
            }

            // Run mutation testing
            var orchestrator = new MutationOrchestrator(testProject, verbose);
            var report = await orchestrator.RunMutationTestingAsync(sourceFile);

            // Generate report
            var reportGenerator = new ReportGenerator();
            var reportPath = await ReportGenerator.SaveReportAsync(report, outputFile);

            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine($"Report saved to: {reportPath}");
                Console.WriteLine();

                // Print summary
                Console.WriteLine(ReportGenerator.GenerateTextReport(report));
            }
            else
            {
                Console.WriteLine($"Mutation testing complete!");
                Console.WriteLine($"Total: {report.TotalMutations}, Killed: {report.KilledCount}, Survived: {report.SurvivedCount}");
                Console.WriteLine($"Mutation Score: {report.MutationScore:F1}%");
                Console.WriteLine($"Report: {reportPath}");
            }

            // Exit with non-zero if mutation score is low (< 80%)
            return report.MutationScore >= 80.0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return 1;
        }
    }
}
