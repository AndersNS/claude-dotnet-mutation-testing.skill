using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates logical operators: &&, ||, !
/// </summary>
public class LogicalMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "LOGICAL";

    private static readonly Dictionary<SyntaxKind, SyntaxKind[]> _mutations = new()
    {
        { SyntaxKind.LogicalAndExpression, [SyntaxKind.LogicalOrExpression] },
        { SyntaxKind.LogicalOrExpression, [SyntaxKind.LogicalAndExpression] },
    };

    public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        if (_mutations.TryGetValue(node.Kind(), out var replacements))
        {
            foreach (var replacement in replacements)
            {
                var mutatedNode = node.WithOperatorToken(
                    SyntaxFactory.Token(GetTokenKind(replacement))
                );

                AddMutation(
                    node,
                    mutatedNode,
                    $"Replace {GetOperatorSymbol(node.Kind())} with {GetOperatorSymbol(replacement)}"
                );
            }
        }

        return base.VisitBinaryExpression(node);
    }

    private static SyntaxKind GetTokenKind(SyntaxKind expressionKind) => expressionKind switch
    {
        SyntaxKind.LogicalAndExpression => SyntaxKind.AmpersandAmpersandToken,
        SyntaxKind.LogicalOrExpression => SyntaxKind.BarBarToken,
        _ => throw new ArgumentException($"Unknown expression kind: {expressionKind}")
    };

    private static string GetOperatorSymbol(SyntaxKind kind) => kind switch
    {
        SyntaxKind.LogicalAndExpression => "&&",
        SyntaxKind.LogicalOrExpression => "||",
        _ => kind.ToString()
    };
}
