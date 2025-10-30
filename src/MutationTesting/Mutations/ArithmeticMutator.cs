using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates arithmetic operators: +, -, *, /, %
/// </summary>
public class ArithmeticMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "ARITHMETIC";

    private static readonly Dictionary<SyntaxKind, SyntaxKind[]> Mutations = new()
    {
        { SyntaxKind.AddExpression, [SyntaxKind.SubtractExpression] },
        { SyntaxKind.SubtractExpression, [SyntaxKind.AddExpression] },
        { SyntaxKind.MultiplyExpression, [SyntaxKind.DivideExpression] },
        { SyntaxKind.DivideExpression, [SyntaxKind.MultiplyExpression] },
        { SyntaxKind.ModuloExpression, [SyntaxKind.MultiplyExpression] },
    };

    public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        if (Mutations.TryGetValue(node.Kind(), out var replacements))
        {
            foreach (var replacement in replacements)
            {
                var mutatedNode = node.WithOperatorToken(
                    SyntaxFactory.Token(GetTokenKind(replacement))
                );

                var opSymbol = GetOperatorSymbol(replacement);
                AddMutation(
                    node,
                    mutatedNode,
                    $"Replace {GetOperatorSymbol(node.Kind())} with {opSymbol}"
                );
            }
        }

        return base.VisitBinaryExpression(node);
    }

    private static SyntaxKind GetTokenKind(SyntaxKind expressionKind) => expressionKind switch
    {
        SyntaxKind.AddExpression => SyntaxKind.PlusToken,
        SyntaxKind.SubtractExpression => SyntaxKind.MinusToken,
        SyntaxKind.MultiplyExpression => SyntaxKind.AsteriskToken,
        SyntaxKind.DivideExpression => SyntaxKind.SlashToken,
        SyntaxKind.ModuloExpression => SyntaxKind.PercentToken,
        _ => throw new ArgumentException($"Unknown expression kind: {expressionKind}")
    };

    private static string GetOperatorSymbol(SyntaxKind kind) => kind switch
    {
        SyntaxKind.AddExpression => "+",
        SyntaxKind.SubtractExpression => "-",
        SyntaxKind.MultiplyExpression => "*",
        SyntaxKind.DivideExpression => "/",
        SyntaxKind.ModuloExpression => "%",
        _ => kind.ToString()
    };
}
