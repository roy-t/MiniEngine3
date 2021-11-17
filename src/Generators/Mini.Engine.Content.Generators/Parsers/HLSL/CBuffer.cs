using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public sealed class CBuffer
    {
        private static readonly IFormatProvider IntFormat = CultureInfo.InvariantCulture.NumberFormat;

        public CBuffer(ConstantBufferSyntax syntax)
        {
            this.Slot = GetRegisterSlot(syntax.Register);
            this.Variables = Variable.FindAll(syntax);
        }

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

        private static int GetRegisterSlot(RegisterLocation location)
        {
            var digits = location.Register.ValueText.SkipWhile(c => !char.IsDigit(c));
            var text = new string(digits.ToArray());
            return int.Parse(text, NumberStyles.Integer, IntFormat);
        }
    }
}
