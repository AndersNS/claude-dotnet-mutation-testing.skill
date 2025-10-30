using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates boolean literals: true <-> false
/// </summary>
public class BooleanLiteralMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "BOOLEAN";

    public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        if (node.IsKind(SyntaxKind.TrueLiteralExpression))
        {
            var mutatedNode = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
            AddMutation(node, mutatedNode, "Replace true with false");
        }
        else if (node.IsKind(SyntaxKind.FalseLiteralExpression))
        {
            var mutatedNode = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
            AddMutation(node, mutatedNode, "Replace false with true");
        }

        return base.VisitLiteralExpression(node);
    }
}
