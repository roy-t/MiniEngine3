using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public enum ProgramDirectives
    {
        None,
        VertexShader,
        PixelShader,
    }

    public sealed class Function
    {
        public Function(FunctionDefinitionSyntax syntax)
        {
            this.Name = syntax.Name.GetUnqualifiedName().Name.ValueText;
            this.Directives = FindLeadingTriviaOfKind(syntax, SyntaxKind.PragmaDirectiveTrivia)
                .Cast<PragmaDirectiveTriviaSyntax>().ToList()
                .Select(s => string.Join(" ", s.TokenString.Select(t => t.ValueText)))
                .ToList();
        }

        public string Name { get; }

        public IReadOnlyList<string> Directives { get; }

        public bool IsProgram() => this.GetProgramDirective() != ProgramDirectives.None;

        public ProgramDirectives GetProgramDirective()
        {
            foreach (var directive in this.Directives)
            {
                if (Enum.TryParse<ProgramDirectives>(directive, out var programDirective))
                {
                    return programDirective;
                }
            }

            return ProgramDirectives.None;
        }

        public string GetProfile()
        {
            var type = this.GetProgramDirective();
            switch (type)
            {
                case ProgramDirectives.VertexShader:
                    return "vs_5_0";
                case ProgramDirectives.PixelShader:
                    return "ps_5_0";
                default:
                    throw new InvalidOperationException($"Cannot get profile for program directive: {type}");
            }
        }

        private static IEnumerable<SyntaxNode> FindLeadingTriviaOfKind(FunctionDefinitionSyntax syntax, SyntaxKind kind)
        {
            SyntaxNodeBase node = syntax;
            while (node.ChildNodes.Count > 0)
            {
                node = node.ChildNodes.OfType<SyntaxToken>().FirstOrDefault() ?? node.ChildNodes[0];
            }

            if (node is SyntaxToken token)
            {
                return token.LeadingTrivia.Where(t => t.IsKind(kind));
            }

            return Enumerable.Empty<SyntaxNode>();
        }

        public static IReadOnlyList<Function> FindAll(SyntaxNodeBase startingNode)
        {
            return startingNode.DescendantNodesAndSelf()
                .Where(node => node.IsKind(SyntaxKind.FunctionDefinition))
                .Cast<FunctionDefinitionSyntax>()
                .Select(syntax => new Function(syntax))
                .ToList();
        }

    }
}
