using System.Text;

namespace Mini.Engine.Content.Generators.Source
{
    public static class Naming
    {
        public static string ToCamelCase(string name)
             => LowerCaseFirstLetter(ToPascalCase(name));

        public static string ToPascalCase(string name)
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

        public static string UpperCaseFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            if (s.Length == 1)
                return s.ToUpper();
            return s.Remove(1).ToUpper() + s.Substring(1);
        }


        public static string LowerCaseFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            if (s.Length == 1)
                return s.ToLower();
            return s.Remove(1).ToLower() + s.Substring(1);
        }
    }
}
