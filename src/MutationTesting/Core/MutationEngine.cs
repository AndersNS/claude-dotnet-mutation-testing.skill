using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MutationTesting.Models;
using MutationTesting.Mutations;

namespace MutationTesting.Core;

/// <summary>
/// Core engine for discovering and applying mutations using Roslyn.
/// </summary>
public class MutationEngine
{
    /// <summary>
    /// Discovers all possible mutations in the given source code.
    /// </summary>
    public static List<MutationResult> DiscoverMutations(string sourceCode, string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, path: filePath);
        var mutations = new List<MutationResult>();
        var mutationId = 0;

        // Instantiate mutators for this syntax tree
        var mutators = new List<IMutator>
        {
            new ArithmeticMutator(mutationId),
            new ComparisonMutator(mutationId),
            new LogicalMutator(mutationId),
            new BooleanLiteralMutator(mutationId),
            new LiteralMutator(mutationId),
            new StatementMutator(mutationId),
            new MethodCallMutator(mutationId),
            new AssignmentMutator(mutationId),
        };

        // Collect mutations from all mutators
        foreach (var mutatorResults in mutators.Select(mutator => mutator.FindMutations(syntaxTree, mutationId).ToList()))
        {
            mutations.AddRange(mutatorResults);
            mutationId += mutatorResults.Count;
        }

        return mutations;
    }

    /// <summary>
    /// Applies a specific mutation to the source code.
    /// </summary>
    public string ApplyMutation(string sourceCode, MutationResult mutation, string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, path: filePath);
        var root = syntaxTree.GetRoot();

        // Find the node to replace by its span
        // Get the innermost node that matches the span
        var nodeToReplace = root.FindNode(mutation.OriginalSpan, findInsideTrivia: false, getInnermostNodeForTie: true);

        // If we didn't find the exact type we expected, try to find it within the node
        if (nodeToReplace.GetType() != mutation.MutatedNode.GetType())
        {
            // The FindNode might have returned a parent node (like ArgumentSyntax)
            // Try to find the actual literal node within it
            var literalNode = nodeToReplace.DescendantNodesAndSelf()
                .FirstOrDefault(n => n.Span == mutation.OriginalSpan && n.GetType() == mutation.MutatedNode.GetType());

            if (literalNode != null)
            {
                nodeToReplace = literalNode;
            }
        }

        // Replace with mutated node
        var newRoot = root.ReplaceNode(nodeToReplace, mutation.MutatedNode);

        return newRoot.ToFullString();
    }

    /// <summary>
    /// Validates that the mutated code compiles.
    /// </summary>
    public (bool success, string? errorMessage) ValidateMutation(string mutatedCode, string filePath)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(mutatedCode, path: filePath);

            // Check for syntax errors
            var diagnostics = syntaxTree.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

            if (errors.Any())
            {
                var errorMessages = string.Join("; ", errors.Select(e => e.GetMessage()));
                return (false, errorMessages);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
