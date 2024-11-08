﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mini.Engine.ECS.Generators.Shared;

namespace Mini.Engine.ECS.Generators
{
    internal sealed class ProcessAttributeReceiver : ISyntaxReceiver
    {
        private sealed class ClassEqualityComparer : IEqualityComparer<ClassDeclarationSyntax>
        {
            public bool Equals(ClassDeclarationSyntax x, ClassDeclarationSyntax y)
            {
                return x.IsIncrementallyIdenticalTo(y);
            }

            public int GetHashCode(ClassDeclarationSyntax syntax)
            {
                return syntax.Identifier.ValueText.GetHashCode();
            }
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
                            if (method.Parent is ClassDeclarationSyntax declaration)
                            {
                                this.ClassList.Add(declaration);
                            }
                        }
                    }
                }
            }
        }

        public ISet<ClassDeclarationSyntax> Classes => this.ClassList;
    }
}
