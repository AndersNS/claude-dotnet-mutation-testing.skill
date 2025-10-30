using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates comparison operators: ==, !=, <, >, <=, >=
/// </summary>
public class ComparisonMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "COMPARISON";

    private static readonly Dictionary<SyntaxKind, SyntaxKind[]> _mutations = new()
    {
        { SyntaxKind.EqualsExpression, [SyntaxKind.NotEqualsExpression] },
        { SyntaxKind.NotEqualsExpression, [SyntaxKind.EqualsExpression] },
        { SyntaxKind.LessThanExpression, [SyntaxKind.GreaterThanExpression] },
        { SyntaxKind.GreaterThanExpression, [SyntaxKind.LessThanExpression] },
        { SyntaxKind.LessThanOrEqualExpression, [SyntaxKind.GreaterThanExpression] },
        { SyntaxKind.GreaterThanOrEqualExpression, [SyntaxKind.LessThanExpression] },
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
        SyntaxKind.EqualsExpression => SyntaxKind.EqualsEqualsToken,
        SyntaxKind.NotEqualsExpression => SyntaxKind.ExclamationEqualsToken,
        SyntaxKind.LessThanExpression => SyntaxKind.LessThanToken,
        SyntaxKind.GreaterThanExpression => SyntaxKind.GreaterThanToken,
        SyntaxKind.LessThanOrEqualExpression => SyntaxKind.LessThanEqualsToken,
        SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.GreaterThanEqualsToken,
        _ => throw new ArgumentException($"Unknown expression kind: {expressionKind}")
    };

    private static string GetOperatorSymbol(SyntaxKind kind) => kind switch
    {
        SyntaxKind.EqualsExpression => "==",
        SyntaxKind.NotEqualsExpression => "!=",
        SyntaxKind.LessThanExpression => "<",
        SyntaxKind.GreaterThanExpression => ">",
        SyntaxKind.LessThanOrEqualExpression => "<=",
        SyntaxKind.GreaterThanOrEqualExpression => ">=",
        _ => kind.ToString()
    };
}
