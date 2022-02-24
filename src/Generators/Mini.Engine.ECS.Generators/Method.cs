using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mini.Engine.ECS.Generators.Shared;

namespace Mini.Engine.ECS.Generators
{
    internal sealed class Method
    {
        public Method(MethodDeclarationSyntax method)
        {
            this.Name = method.Identifier.ValueText;            

            this.Components = method.ParameterList.Parameters
                .Select(parameter => GetTypeName(parameter.Type))
                .ToList();

            this.Query = method.AttributeLists
                .SelectMany(list => list.Attributes)
                .Select(attribute => GetPropertyValueOrDefault(attribute, nameof(ProcessAttribute.Query), ProcessQuery.None))
                .FirstOrDefault();
        }

        public ProcessQuery Query { get; }

        public string Name { get; }

        public IReadOnlyList<string> Components { get; }

        private static T GetPropertyValueOrDefault<T>(AttributeSyntax attribute, string propertyName, T @default)
            where T : Enum
        {
            var queryProperty = attribute.ArgumentList?.Arguments.Where(x => x.NameEquals?.Name.Identifier.ValueText == propertyName).FirstOrDefault();
            var queryValue = queryProperty == null ? @default : (T)Enum.Parse(typeof(T), (queryProperty.Expression as MemberAccessExpressionSyntax)?.Name.Identifier.ValueText);

            return queryValue;
        }

        private static string GetTypeName(TypeSyntax type)
        {
            if (type is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.ValueText;
            }

            if (type is PredefinedTypeSyntax predefinedType)
            {
                return predefinedType.Keyword.ValueText;
            }

            if (type is ArrayTypeSyntax arrayType)
            {
                var elementType = GetTypeName(arrayType.ElementType);
                return $"{elementType}[]";                
            }

            throw new Exception($"Unexpected parameter type {type.GetType().FullName}");
        }
    }
}
