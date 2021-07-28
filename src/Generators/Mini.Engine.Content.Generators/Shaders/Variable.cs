using System;
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
            this.Name = syntax.Identifier.ValueText;
            this.IsPredefinedType = true;
            this.IsArray = syntax.ArrayRankSpecifiers.Any(); // TODO: support both [,] and [][] multi dimensional array syntax

            switch (type)
            {
                case VectorTypeSyntax vector:
                    this.Type = vector.TypeToken.ValueText;
                    break;
                case ScalarTypeSyntax scalar:
                    this.Type = scalar.TypeTokens[0].ValueText;
                    break;
                case MatrixTypeSyntax matrix:
                    this.Type = matrix.TypeToken.ValueText;
                    break;
                case PredefinedObjectTypeSyntax obj:
                    this.Type = obj.ObjectTypeToken.ValueText;
                    break;
                case IdentifierNameSyntax structure:
                    this.Type = structure.Name.ValueText;
                    this.IsPredefinedType = false;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported type {type}");
            };
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
    }
}
