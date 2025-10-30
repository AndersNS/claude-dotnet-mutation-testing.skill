using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MutationTesting.TestRunner;

/// <summary>
/// Runs dotnet test and parses results.
/// </summary>
public class DotnetTestRunner
{
    private readonly string _testProjectPath;
    private readonly int _timeoutSeconds;

    public DotnetTestRunner(string testProjectPath, int timeoutSeconds = 60)
    {
        _testProjectPath = testProjectPath;
        _timeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// Runs tests and returns whether they passed.
    /// </summary>
    public (bool passed, string output, TestResults results) RunTests(bool verbose = false)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"test \"{_testProjectPath}\" --nologo" + (verbose ? " -v n" : " -v q"),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var output = new System.Text.StringBuilder();
        var errorOutput = new System.Text.StringBuilder();

        using var process = new Process();
        process.StartInfo = startInfo;

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                errorOutput.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var completed = process.WaitForExit(_timeoutSeconds * 1000);

        if (!completed)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // ignored
            }

            return (false, "Test execution timed out", new TestResults
            {
                Total = 0,
                Passed = 0,
                Failed = 0,
                Skipped = 0,
                TimedOut = true
            });
        }

        var exitCode = process.ExitCode;
        var fullOutput = output + errorOutput.ToString();

        var results = ParseTestResults(fullOutput);

        return (exitCode == 0, fullOutput, results);
    }

    private static TestResults ParseTestResults(string output)
    {
        var results = new TestResults();

        // Parse patterns like:
        // "Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5"
        // "Failed!  - Failed:     2, Passed:     3, Skipped:     0, Total:     5"

        var passedMatch = Regex.Match(output, @"Passed:\s+(\d+)", RegexOptions.IgnoreCase);
        var failedMatch = Regex.Match(output, @"Failed:\s+(\d+)", RegexOptions.IgnoreCase);
        var skippedMatch = Regex.Match(output, @"Skipped:\s+(\d+)", RegexOptions.IgnoreCase);
        var totalMatch = Regex.Match(output, @"Total:\s+(\d+)", RegexOptions.IgnoreCase);

        if (passedMatch.Success)
            results.Passed = int.Parse(passedMatch.Groups[1].Value);

        if (failedMatch.Success)
            results.Failed = int.Parse(failedMatch.Groups[1].Value);

        if (skippedMatch.Success)
            results.Skipped = int.Parse(skippedMatch.Groups[1].Value);

        if (totalMatch.Success)
            results.Total = int.Parse(totalMatch.Groups[1].Value);

        return results;
    }
}

/// <summary>
/// Results from running tests.
/// </summary>
public class TestResults
{
    public int Total { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public bool TimedOut { get; set; }
}