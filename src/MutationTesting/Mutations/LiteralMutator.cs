using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates numeric and string literals.
/// </summary>
public class LiteralMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "LITERAL";

    public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        // Numeric literals
        if (node.IsKind(SyntaxKind.NumericLiteralExpression))
        {
            if (node.Token.Value is int intValue)
            {
                MutateIntLiteral(node, intValue);
            }
            else if (node.Token.Value is long longValue)
            {
                MutateLongLiteral(node, longValue);
            }
            else if (node.Token.Value is double doubleValue)
            {
                MutateDoubleLiteral(node, doubleValue);
            }
        }
        // String literals
        else if (node.IsKind(SyntaxKind.StringLiteralExpression))
        {
            MutateStringLiteral(node);
        }
        // Null literal
        else if (node.IsKind(SyntaxKind.NullLiteralExpression))
        {
            // For null, we could mutate to empty string or default, but this is tricky
            // Skip for now as it often causes compilation errors
        }

        return base.VisitLiteralExpression(node);
    }

    private void MutateIntLiteral(LiteralExpressionSyntax node, int value)
    {
        // 0 -> 1, other values -> 0
        if (value == 0)
        {
            var mutated = node.WithToken(SyntaxFactory.Literal(1));
            AddMutation(node, mutated, "Replace 0 with 1");
        }
        else if (value == 1)
        {
            var mutated = node.WithToken(SyntaxFactory.Literal(0));
            AddMutation(node, mutated, "Replace 1 with 0");
        }
        else if (value > 0)
        {
            // Positive -> increment and decrement
            var incremented = node.WithToken(SyntaxFactory.Literal(value + 1));
            AddMutation(node, incremented, $"Replace {value} with {value + 1}");

            if (value > 1)
            {
                var decremented = node.WithToken(SyntaxFactory.Literal(value - 1));
                AddMutation(node, decremented, $"Replace {value} with {value - 1}");
            }
        }
        else // negative
        {
            var incremented = node.WithToken(SyntaxFactory.Literal(value + 1));
            AddMutation(node, incremented, $"Replace {value} with {value + 1}");
        }
    }

    private void MutateLongLiteral(LiteralExpressionSyntax node, long value)
    {
        if (value == 0L)
        {
            var mutated = node.WithToken(SyntaxFactory.Literal(1L));
            AddMutation(node, mutated, "Replace 0L with 1L");
        }
    }

    private void MutateDoubleLiteral(LiteralExpressionSyntax node, double value)
    {
        if (value == 0.0)
        {
            var mutated = node.WithToken(SyntaxFactory.Literal(1.0));
            AddMutation(node, mutated, "Replace 0.0 with 1.0");
        }
    }

    private void MutateStringLiteral(LiteralExpressionSyntax node)
    {
        var value = node.Token.ValueText;

        // Empty string -> non-empty
        if (string.IsNullOrEmpty(value))
        {
            var mutated = node.WithToken(SyntaxFactory.Literal("mutated"));
            AddMutation(node, mutated, "Replace empty string with \"mutated\"");
        }
        // Non-empty -> empty string
        else
        {
            var mutated = node.WithToken(SyntaxFactory.Literal(""));
            AddMutation(node, mutated, $"Replace \"{value}\" with empty string");
        }
    }
}
