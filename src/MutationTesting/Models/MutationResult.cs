using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MutationTesting.Models;

/// <summary>
/// Represents a single mutation that can be applied to source code.
/// </summary>
public class MutationResult
{
    public required int Id { get; init; }
    public required string MutationType { get; init; }
    public required string Description { get; init; }
    public required FileLinePositionSpan Location { get; init; }
    public required TextSpan OriginalSpan { get; init; }
    public required string OriginalCode { get; init; }
    public required string MutatedCode { get; init; }
    public required SyntaxNode MutatedNode { get; init; }

    public MutationStatus Status { get; set; } = MutationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public TimeSpan? ExecutionTime { get; set; }
}

/// <summary>
/// Status of a mutation test.
/// </summary>
public enum MutationStatus
{
    Pending,
    Running,
    Killed,      // Tests failed - mutation was detected (GOOD)
    Survived,    // Tests passed - mutation was NOT detected (BAD)
    BuildError,  // Mutated code didn't compile. This shouldn't happen since we use Roslyn.
    TestError    // Test execution failed for other reasons
}

/// <summary>
/// Summary report of mutation testing run.
/// </summary>
public class MutationReport
{
    public required string SourceFilePath { get; init; }
    public required string TestProjectPath { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime? EndTime { get; set; }
    public List<MutationResult> Mutations { get; init; } = new();

    public int TotalMutations => Mutations.Count;
    public int KilledCount => Mutations.Count(m => m.Status == MutationStatus.Killed);
    public int SurvivedCount => Mutations.Count(m => m.Status == MutationStatus.Survived);
    public int ErrorCount => Mutations.Count(m => m.Status is MutationStatus.BuildError or MutationStatus.TestError);

    public double MutationScore => TotalMutations > 0
        ? (double)KilledCount / (TotalMutations - ErrorCount) * 100.0
        : 0.0;

    public TimeSpan Duration => (EndTime ?? DateTime.Now) - StartTime;
}
