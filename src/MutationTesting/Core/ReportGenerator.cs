using System.Text;
using MutationTesting.Models;

namespace MutationTesting.Core;

/// <summary>
/// Generates mutation testing reports in various formats.
/// </summary>
public class ReportGenerator
{
    public static string GenerateTextReport(MutationReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine("".PadRight(80, '='));
        sb.AppendLine("MUTATION TESTING REPORT");
        sb.AppendLine("".PadRight(80, '='));
        sb.AppendLine();

        sb.AppendLine($"Source File: {report.SourceFilePath}");
        sb.AppendLine($"Test Project: {report.TestProjectPath}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Duration: {report.Duration.TotalSeconds:F1} seconds");
        sb.AppendLine();

        sb.AppendLine("".PadRight(80, '='));
        sb.AppendLine("SUMMARY");
        sb.AppendLine("".PadRight(80, '='));
        sb.AppendLine();

        sb.AppendLine($"Total Mutations:     {report.TotalMutations}");
        sb.AppendLine($"Killed Mutations:    {report.KilledCount} ({(report.TotalMutations > 0 ? (double)report.KilledCount / report.TotalMutations * 100 : 0):F1}%)");
        sb.AppendLine($"Survived Mutations:  {report.SurvivedCount} ({(report.TotalMutations > 0 ? (double)report.SurvivedCount / report.TotalMutations * 100 : 0):F1}%)");
        sb.AppendLine($"Error Mutations:     {report.ErrorCount}");
        sb.AppendLine();
        sb.AppendLine($"MUTATION SCORE: {report.MutationScore:F1}%");
        sb.AppendLine();

        // Survived mutations (most important - test gaps)
        var survived = report.Mutations.Where(m => m.Status == MutationStatus.Survived).ToList();
        if (survived.Any())
        {
            sb.AppendLine("".PadRight(80, '='));
            sb.AppendLine("SURVIVED MUTATIONS (Not caught by tests - potential issues!)");
            sb.AppendLine("".PadRight(80, '='));
            sb.AppendLine();

            foreach (var (mutation, index) in survived.Select((m, i) => (m, i + 1)))
            {
                sb.AppendLine($"{index}. {mutation.Description}");
                sb.AppendLine($"   Location: {Path.GetFileName(report.SourceFilePath)}:{mutation.Location.StartLinePosition.Line + 1}:{mutation.Location.StartLinePosition.Character + 1}");
                sb.AppendLine($"   Original: {mutation.OriginalCode}");
                sb.AppendLine($"   Mutated:  {mutation.MutatedCode}");
                sb.AppendLine($"   Type:     {mutation.MutationType}");
                sb.AppendLine();
            }
        }

        // Killed mutations summary
        var killed = report.Mutations.Where(m => m.Status == MutationStatus.Killed).ToList();
        if (killed.Count != 0)
        {
            sb.AppendLine("".PadRight(80, '='));
            sb.AppendLine($"KILLED MUTATIONS (Caught by tests - {killed.Count} mutations)");
            sb.AppendLine("".PadRight(80, '='));
            sb.AppendLine();

            var byType = killed.GroupBy(m => m.MutationType)
                .OrderByDescending(g => g.Count());

            foreach (var group in byType)
            {
                sb.AppendLine($"{group.Key}: {group.Count()} mutations");
            }
            sb.AppendLine();
        }

        // Errors
        var errors = report.Mutations.Where(m => m.Status == MutationStatus.BuildError || m.Status == MutationStatus.TestError).ToList();
        if (errors.Count != 0)
        {
            sb.AppendLine("".PadRight(80, '='));
            sb.AppendLine("ERROR MUTATIONS (Could not test)");
            sb.AppendLine("".PadRight(80, '='));
            sb.AppendLine();

            foreach (var (mutation, index) in errors.Select((m, i) => (m, i + 1)))
            {
                sb.AppendLine($"{index}. {mutation.Description}");
                sb.AppendLine($"   Location: Line {mutation.Location.StartLinePosition.Line + 1}");
                sb.AppendLine($"   Error: {mutation.ErrorMessage}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("".PadRight(80, '='));

        return sb.ToString();
    }

    public static async Task<string> SaveReportAsync(MutationReport report, string? outputPath = null)
    {
        var reportText = GenerateTextReport(report);

        if (string.IsNullOrEmpty(outputPath))
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            outputPath = $"mutation_report_{timestamp}.txt";
        }

        await File.WriteAllTextAsync(outputPath, reportText);

        return outputPath;
    }
}
