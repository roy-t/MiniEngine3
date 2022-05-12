using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.HLSL.Parsers;

public static class FunctionAttribute
{
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> FindAll(SyntaxNodeBase startingNode)
    {
        return startingNode.DescendantNodesAndSelf()
            .Where(node => node.IsKind(SyntaxKind.AttributeDeclaration))
            .Cast<AttributeDeclarationSyntax>()
            .ToDictionary(a => FindName(a), a => FindArguments(a));
    }

    private static string FindName(AttributeDeclarationSyntax syntax)
    {
        var attribute = syntax.Attribute;
        return (attribute.Name as IdentifierNameSyntax)?.Name.ValueText ?? string.Empty;
    }

    private static IReadOnlyList<string> FindArguments(AttributeDeclarationSyntax syntax)
    {
        var attribute = syntax.Attribute;
        if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > 0)
        {
            var arguments = attribute.ArgumentList?.Arguments.Cast<LiteralExpressionSyntax>().ToList();
            return arguments.Select(x => x.ToFullString()).ToList();
        }

        return Array.Empty<string>();
    }
}
