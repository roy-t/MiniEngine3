using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mini.Engine.ECS.Generators.Shared;

namespace Mini.Engine.ECS.Generators
{
    internal sealed class Class
    {
        public Class(Compilation compilation, ClassDeclarationSyntax @class)
        {
            this.Name = @class.Identifier.ValueText;
            this.Namespace = GetNamespace(compilation, @class);
            this.Usings = GetUsings(@class);

            this.Methods = @class.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(m => new Method(m))
                .Where(m => m.Query != ProcessQuery.Invalid)
                .ToList();
        }

        public string Name { get; }
        public string Namespace { get; }
        public IReadOnlyList<Method> Methods { get; }
        public IReadOnlyList<string> Usings { get; }

        public IEnumerable<string> GetUniqueComponents()
            => this.Methods.SelectMany(m => m.Components).Distinct();

        private static string GetNamespace(Compilation compilation, TypeDeclarationSyntax type)
        {
            var space = type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
            var model = compilation.GetSemanticModel(space.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(space) as INamespaceSymbol;

            var names = new List<string>();
            do
            {
                names.Insert(0, symbol?.Name ?? string.Empty);
                symbol = symbol?.ContainingNamespace;
            } while (symbol?.ContainingNamespace != null);
            return string.Join(".", names);
        }

        private static IReadOnlyList<string> GetUsings(TypeDeclarationSyntax type)
        {
            var usings = SearchUpForNodesOfType<UsingDirectiveSyntax>(type);
            return usings.Select(u => u.Name.ToString()).ToList();
        }

        private static IEnumerable<T> SearchUpForNodesOfType<T>(SyntaxNode? node)
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
