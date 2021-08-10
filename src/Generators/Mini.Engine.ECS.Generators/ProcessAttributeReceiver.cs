using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mini.Engine.ECS.Generators.Shared;

namespace Mini.Engine.ECS.Generators
{
    internal sealed class ProcessAttributeReceiver : ISyntaxReceiver
    {
        private readonly Type TargetAttributeType = typeof(ProcessAttribute);

        public Dictionary<ClassDeclarationSyntax, SystemClass> Targets { get; } = new Dictionary<ClassDeclarationSyntax, SystemClass>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax method)
            {
                foreach (var attributeList in method.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (MatchesAttribute(attribute, this.TargetAttributeType))
                        {
                            var value = GetPropertyValueOrDefault(attribute, nameof(ProcessAttribute.Query), ProcessQuery.None);
                            GetTargetClass(method).Add(value, method);
                        }
                    }
                }
            }
        }

        private T GetPropertyValueOrDefault<T>(AttributeSyntax attribute, string propertyName, T @default)
            where T : Enum
        {
            var queryProperty = attribute.ArgumentList?.Arguments.Where(x => x.NameEquals?.Name.Identifier.ValueText == propertyName).FirstOrDefault();
            var queryValue = queryProperty == null ? @default : (T)Enum.Parse(typeof(T), (queryProperty.Expression as MemberAccessExpressionSyntax)?.Name.Identifier.ValueText);

            return queryValue;
        }

        private SystemClass GetTargetClass(MethodDeclarationSyntax syntaxNode)
        {
            var parent = syntaxNode.Parent as ClassDeclarationSyntax;
            if (!this.Targets.TryGetValue(parent, out var target))
            {
                target = new SystemClass(parent);
                this.Targets.Add(parent, target);
            }

            return target;
        }

        private static bool MatchesAttribute(AttributeSyntax attribute, Type attributeType)
        {
            var name = attributeType.Name;
            var shortName = attributeType.Name.Split(new[] { "Attribute" }, StringSplitOptions.RemoveEmptyEntries)[0];

            return attribute.Name.ToString() == name || attribute.Name.ToString() == shortName;
        }
    }
}
