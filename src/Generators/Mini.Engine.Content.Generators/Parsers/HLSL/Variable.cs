using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public sealed class Variable
    {
        public Variable(TypeSyntax type, VariableDeclaratorSyntax syntax)
        {
            this.Name = syntax.Identifier.ValueText;
            this.IsPredefinedType = true;
            this.Dimensions = syntax.DescendantNodes().Count(x =>
                        x.IsKind(SyntaxKind.NumericLiteralExpression) || x.IsKind(SyntaxKind.IdentifierName));

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
        public int Dimensions { get; }

        public static IReadOnlyList<Variable> FindAll(SyntaxNodeBase startingNode)
        {
            return startingNode.DescendantNodesAndSelf()
                .Where(node => node.IsKind(SyntaxKind.VariableDeclarationStatement))
                .Cast<VariableDeclarationStatementSyntax>()
                .SelectMany(syntax => FindAll(syntax))
                .ToList();
        }

        public override string ToString()
        {
            var arr = this.Dimensions > 0 ? $"[{new string(',', this.Dimensions - 1)}]" : string.Empty;
            return $"{this.Type} {this.Name}{arr}";
        }

        public static IReadOnlyList<Variable> FindAll(VariableDeclarationStatementSyntax syntax)
            => syntax.Declaration.Variables.Select(node => new Variable(syntax.Declaration.Type, node)).ToList();
    }
}
