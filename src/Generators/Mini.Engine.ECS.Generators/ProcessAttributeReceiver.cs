using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mini.Engine.ECS.Generators.Shared;

namespace Mini.Engine.ECS.Generators
{
    internal sealed class ProcessAttributeReceiver : ISyntaxReceiver
    {
        private sealed class ClassEqualityComparer : IEqualityComparer<ClassDeclarationSyntax>
        {
            public bool Equals(ClassDeclarationSyntax x, ClassDeclarationSyntax y)
                => x.IsIncrementallyIdenticalTo(y);

            public int GetHashCode(ClassDeclarationSyntax syntax)
                => syntax.Identifier.ValueText.GetHashCode();
        }

        private readonly Type TargetAttributeType;
        private readonly HashSet<ClassDeclarationSyntax> ClassList;

        public ProcessAttributeReceiver()
        {
            this.TargetAttributeType = typeof(ProcessAttribute);
            this.ClassList = new HashSet<ClassDeclarationSyntax>(new ClassEqualityComparer());
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax method)
            {
                foreach (var attributeList in method.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (AttributeMatcher.Matches(attribute, this.TargetAttributeType))
                        {
                            this.ClassList.Add(method.Parent as ClassDeclarationSyntax);
                        }
                    }
                }
            }
        }

        public ISet<ClassDeclarationSyntax> Classes => this.ClassList;
    }
}
