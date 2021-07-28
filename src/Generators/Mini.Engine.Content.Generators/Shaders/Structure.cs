using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.Shaders
{
    public sealed class Structure
    {
        public Structure(StructTypeSyntax syntax)
        {
            this.Name = syntax.Name.ValueText;
            this.Variables = Variable.FindAll(syntax);
        }

        public string Name { get; }
        public IReadOnlyList<Variable> Variables { get; }

        public static IReadOnlyList<Structure> FindAll(SyntaxNodeBase startingNode)
        {
            return startingNode.DescendantNodesAndSelf()
                .Where(node => node.IsKind(SyntaxKind.StructType))
                .Cast<StructTypeSyntax>()
                .Select(syntax => new Structure(syntax))
                .ToList();
        }
    }
}

