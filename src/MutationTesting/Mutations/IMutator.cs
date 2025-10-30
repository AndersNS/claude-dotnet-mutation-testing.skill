using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MutationTesting.Models;

namespace MutationTesting.Mutations;

/// <summary>
/// Interface for all mutation operators.
/// </summary>
public interface IMutator
{
    /// <summary>
    /// Gets the name of this mutator type (e.g., "ARITHMETIC", "COMPARISON").
    /// </summary>
    string MutationType { get; }

    /// <summary>
    /// Finds all possible mutations in the given syntax tree.
    /// </summary>
    IEnumerable<MutationResult> FindMutations(SyntaxTree syntaxTree, int startingId);
}

/// <summary>
/// Base class for syntax rewriters that generate mutations.
/// </summary>
public abstract class MutatorBase : CSharpSyntaxRewriter, IMutator
{
    private readonly List<MutationResult> _mutations = [];
    private int _currentId;
    private SyntaxTree? _currentSyntaxTree;

    protected MutatorBase(int startingId)
    {
        _currentId = startingId;
    }

    public abstract string MutationType { get; }

    public IEnumerable<MutationResult> FindMutations(SyntaxTree syntaxTree, int startingId)
    {
        _mutations.Clear();
        _currentId = startingId;
        _currentSyntaxTree = syntaxTree;
        Visit(syntaxTree.GetRoot());
        return _mutations;
    }

    protected void AddMutation(
        SyntaxNode originalNode,
        SyntaxNode mutatedNode,
        string description)
    {
        var location = _currentSyntaxTree!.GetLineSpan(originalNode.Span);
        var originalCode = originalNode.ToString();
        var mutatedCode = mutatedNode.ToString();

        _mutations.Add(new MutationResult
        {
            Id = _currentId++,
            MutationType = MutationType,
            Description = description,
            Location = location,
            OriginalSpan = originalNode.Span,
            OriginalCode = originalCode,
            MutatedCode = mutatedCode,
            MutatedNode = mutatedNode
        });
    }
}