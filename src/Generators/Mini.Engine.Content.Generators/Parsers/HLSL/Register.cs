using System;
using System.Globalization;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public static class Register
    {
        private static readonly IFormatProvider IntFormat = CultureInfo.InvariantCulture.NumberFormat;

        public static bool TryGetSlot(SyntaxNodeBase startingNode, out int slot)
        {
            var location = startingNode.DescendantNodesAndSelf()
                .Where(node => node.IsKind(SyntaxKind.RegisterLocation))
                .Cast<RegisterLocation>()
                .FirstOrDefault();

            if (location == null)
            {
                slot = 0;
                return false;
            }

            slot = GetSlot(location);
            return true;
        }

        public static int GetSlot(RegisterLocation location)
        {
            var digits = location.Register.ValueText.SkipWhile(c => !char.IsDigit(c));
            var text = new string(digits.ToArray());
            return int.Parse(text, NumberStyles.Integer, IntFormat);
        }
    }
}
