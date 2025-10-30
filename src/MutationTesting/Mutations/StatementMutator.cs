using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates return statements and removes statements.
/// </summary>
public class StatementMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "STATEMENT";

    public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
    {
        if (node.Expression != null)
        {
            // Mutation 1: Return default value
            if (node.Expression is LiteralExpressionSyntax literal)
            {
                if (literal.IsKind(SyntaxKind.NumericLiteralExpression) && literal.Token.Value is int intVal)
                {
                    if (intVal != 0)
                    {
                        var mutated = node.WithExpression(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(0)
                            )
                        );
                        AddMutation(node, mutated, $"Replace return {intVal} with return 0");
                    }

                    if (intVal >= 0)
                    {
                        var mutated = node.WithExpression(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(intVal + 1)
                            )
                        );
                        AddMutation(node, mutated, $"Replace return {intVal} with return {intVal + 1}");
                    }
                }
            }
            // For other expressions, we could try to mutate the expression itself
            else if (node.Expression is BinaryExpressionSyntax)
            {
                // These will be caught by other mutators visiting the expression
            }
        }

        return base.VisitReturnStatement(node);
    }

    public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        // Remove standalone method calls (be careful - this can break code)
        if (node.Expression is InvocationExpressionSyntax invocation)
        {
            // Only remove if it's a void method call (heuristic: no assignment)
            var parent = node.Parent;
            if (parent is BlockSyntax)
            {
                // Create a comment instead of removing entirely
                var comment = SyntaxFactory.Comment($"/* MUTATED: {node.ToString().Trim()} */");
                var emptyStatement = SyntaxFactory.EmptyStatement()
                    .WithLeadingTrivia(comment);

                AddMutation(node, emptyStatement, $"Remove method call: {invocation.Expression}");
            }
        }

        return base.VisitExpressionStatement(node);
    }

    public override SyntaxNode? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        // Mutate ++ and --
        if (node.IsKind(SyntaxKind.PreIncrementExpression))
        {
            var mutated = SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.PreDecrementExpression,
                node.Operand
            );
            AddMutation(node, mutated, "Replace ++ with --");
        }
        else if (node.IsKind(SyntaxKind.PreDecrementExpression))
        {
            var mutated = SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.PreIncrementExpression,
                node.Operand
            );
            AddMutation(node, mutated, "Replace -- with ++");
        }

        return base.VisitPrefixUnaryExpression(node);
    }

    public override SyntaxNode? VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
        // Mutate ++ and --
        if (node.IsKind(SyntaxKind.PostIncrementExpression))
        {
            var mutated = SyntaxFactory.PostfixUnaryExpression(
                SyntaxKind.PostDecrementExpression,
                node.Operand
            );
            AddMutation(node, mutated, "Replace ++ with --");
        }
        else if (node.IsKind(SyntaxKind.PostDecrementExpression))
        {
            var mutated = SyntaxFactory.PostfixUnaryExpression(
                SyntaxKind.PostIncrementExpression,
                node.Operand
            );
            AddMutation(node, mutated, "Replace -- with ++");
        }

        return base.VisitPostfixUnaryExpression(node);
    }
}
