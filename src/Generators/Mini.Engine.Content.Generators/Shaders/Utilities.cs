using System.Linq;
using System.Text;
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

        public static string ToDotNetImportantName(string name)
        {
            var builder = new StringBuilder(name.Length);
            var upperCase = false;
            for (var i = 0; i < name.Length; i++)
            {
                var current = name[i];

                // Start with capital letter
                upperCase |= i == 0;

                // Default lowercase
                upperCase |= i > 0 && char.IsLower(name[i - 1]) && char.IsUpper(current);

                // Camel case
                if (upperCase)
                {
                    current = char.ToUpper(current);
                    upperCase = false;
                }
                else
                {
                    current = char.ToLower(current);
                }

                // snake case to camel case
                if (current == '_')
                {
                    upperCase = true;
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }
    }
}
