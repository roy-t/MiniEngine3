using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mini.Engine.ECS.Generators
{
    public static class AttributeMatcher
    {
        public static bool Matches(AttributeSyntax attribute, Type attributeType)
        {
            var name = attributeType.Name;
            var shortName = attributeType.Name.Split(new[] { "Attribute" }, StringSplitOptions.RemoveEmptyEntries)[0];

            return attribute.Name.ToString() == name || attribute.Name.ToString() == shortName;
        }
    }
}
