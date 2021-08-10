using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mini.Engine.ECS.Generators
{
    public static class Utilities
    {
        public static IEnumerable<string> GetUsings(TypeDeclarationSyntax type)
        {
            var usings = SearchUpForNodesOfType<UsingDirectiveSyntax>(type);
            return usings.Select(u => u.Name.ToString());
        }

        public static string GetNamespace(Compilation compilation, TypeDeclarationSyntax type)
        {
            var space = type.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            var model = compilation.GetSemanticModel(space.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(space) as INamespaceSymbol;

            var names = new List<string>();
            do
            {
                names.Insert(0, symbol.Name);
                symbol = symbol.ContainingNamespace;
            } while (symbol.ContainingNamespace != null);
            return string.Join(".", names);
        }

        public static IEnumerable<T> SearchUpForNodesOfType<T>(SyntaxNode node)
        {
            while (node != null)
            {
                var ofType = node.ChildNodes().OfType<T>();
                if (ofType.Any())
                {
                    return ofType.ToList();
                }
                node = node.Parent;
            }

            return Enumerable.Empty<T>();
        }
    }
}
