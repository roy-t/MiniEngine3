using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.Shaders
{
    public sealed class Variable
    {
        public Variable(TypeSyntax type, VariableDeclaratorSyntax syntax)
        {
            this.Type = type.ToString();
            this.IsPredefinedType = PredefinedType(type.Kind);

            this.Name = syntax.Identifier.ValueText;
            this.IsArray = syntax.ArrayRankSpecifiers.Any(); // TODO: support both [,] and [][] multi dimensional array syntax
        }

        public string Type { get; }
        public bool IsPredefinedType { get; }
        public bool IsCustomType => !this.IsPredefinedType;

        public string Name { get; }
        public bool IsArray { get; }

        public static IReadOnlyList<Variable> FindAll(SyntaxNodeBase startingNode)
        {
            return startingNode.DescendantNodesAndSelf()
                .Where(node => node.IsKind(SyntaxKind.VariableDeclarationStatement))
                .Cast<VariableDeclarationStatementSyntax>()
                .SelectMany(syntax => FindAll(syntax))
                .ToList();
        }

        public override string ToString()
            => $"{this.Type} {this.Name}{(this.IsArray ? "[]" : string.Empty)}";

        public static IReadOnlyList<Variable> FindAll(VariableDeclarationStatementSyntax syntax)
            => syntax.Declaration.Variables.Select(node => new Variable(syntax.Declaration.Type, node)).ToList();

        private static bool PredefinedType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PredefinedScalarType:
                    return true;
                case SyntaxKind.PredefinedVectorType:
                    return true;
                case SyntaxKind.PredefinedGenericVectorType:
                    return true;
                case SyntaxKind.PredefinedMatrixType:
                    return true;
                case SyntaxKind.PredefinedGenericMatrixType:
                    return true;
                case SyntaxKind.PredefinedObjectType:
                    return true;
                default:
                    return false;
            }
        }
    }
}
