using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates assignment operators: +=, -=, *=, /=
/// </summary>
public class AssignmentMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "ASSIGNMENT";

    private static readonly Dictionary<SyntaxKind, SyntaxKind[]> _mutations = new()
    {
        { SyntaxKind.AddAssignmentExpression, [SyntaxKind.SubtractAssignmentExpression] },
        { SyntaxKind.SubtractAssignmentExpression, [SyntaxKind.AddAssignmentExpression] },
        { SyntaxKind.MultiplyAssignmentExpression, [SyntaxKind.DivideAssignmentExpression] },
        { SyntaxKind.DivideAssignmentExpression, [SyntaxKind.MultiplyAssignmentExpression] },
    };

    public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        if (_mutations.TryGetValue(node.Kind(), out var replacements))
        {
            foreach (var replacement in replacements)
            {
                var mutatedNode = SyntaxFactory.AssignmentExpression(
                    replacement,
                    node.Left,
                    node.Right
                );

                AddMutation(
                    node,
                    mutatedNode,
                    $"Replace {GetOperatorSymbol(node.Kind())} with {GetOperatorSymbol(replacement)}"
                );
            }
        }

        return base.VisitAssignmentExpression(node);
    }

    private static string GetOperatorSymbol(SyntaxKind kind) => kind switch
    {
        SyntaxKind.AddAssignmentExpression => "+=",
        SyntaxKind.SubtractAssignmentExpression => "-=",
        SyntaxKind.MultiplyAssignmentExpression => "*=",
        SyntaxKind.DivideAssignmentExpression => "/=",
        _ => kind.ToString()
    };
}
