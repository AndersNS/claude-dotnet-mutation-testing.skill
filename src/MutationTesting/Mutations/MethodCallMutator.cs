using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MutationTesting.Mutations;

/// <summary>
/// Mutates method calls, particularly LINQ operations.
/// </summary>
public class MethodCallMutator(int startingId) : MutatorBase(startingId)
{
    public override string MutationType => "METHOD_CALL";

    private static readonly Dictionary<string, string[]> _linqMutations = new()
    {
        { "First", ["Last"] },
        { "Last", ["First"] },
        { "Any", ["All"] },
        { "All", ["Any"] },
        { "FirstOrDefault", ["LastOrDefault"] },
        { "LastOrDefault", ["FirstOrDefault"] },
        { "Skip", ["Take"] },
        { "Take", ["Skip"] },
        { "SkipWhile", ["TakeWhile"] },
        { "TakeWhile", ["SkipWhile"] },
        { "Min", ["Max"] },
        { "Max", ["Min"] },
    };

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Handle LINQ method mutations
        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.Text;

            if (_linqMutations.TryGetValue(methodName, out var replacements))
            {
                foreach (var replacement in replacements)
                {
                    var mutatedName = SyntaxFactory.IdentifierName(replacement);
                    var mutatedMemberAccess = memberAccess.WithName(mutatedName);
                    var mutatedNode = node.WithExpression(mutatedMemberAccess);

                    AddMutation(
                        node,
                        mutatedNode,
                        $"Replace {methodName}() with {replacement}()"
                    );
                }
            }
        }

        return base.VisitInvocationExpression(node);
    }
}
