using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public sealed class CBuffer
    {
        public CBuffer(ConstantBufferSyntax syntax)
        {
            this.Name = syntax.Name.ValueText;
            this.Slot = Register.GetSlot(syntax.Register);
            this.Variables = Variable.FindAll(syntax);
        }

        public string Name { get; }

        public int Slot { get; }

        public IReadOnlyList<Variable> Variables { get; }

        public static IReadOnlyList<CBuffer> FindAll(SyntaxNodeBase startingNode)
        {
            return startingNode.DescendantNodesAndSelf()
                .Where(node => node.IsKind(SyntaxKind.ConstantBufferDeclaration))
                .Cast<ConstantBufferSyntax>()
                .Select(syntax => new CBuffer(syntax))
                .ToList();
        }
    }
}
