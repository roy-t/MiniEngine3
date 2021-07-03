using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace Mini.Engine.Content.Generators.Shaders
{
    public static class Utilities
    {
        public static int RegisterToSlot(RegisterLocation location)
        {
            var digits = location.Register.ValueText.SkipWhile(c => !char.IsDigit(c));
            var text = new string(digits.ToArray());
            return int.Parse(text);
        }
    }
}
