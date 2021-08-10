using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mini.Engine.ECS.Generators.Shared;

namespace Mini.Engine.ECS.Generators
{
    internal sealed class SystemClass
    {
        private Dictionary<ProcessQuery, MethodDeclarationSyntax> ProcessorDictionary;

        public SystemClass(ClassDeclarationSyntax @class)
        {
            this.Class = @class;
            this.ProcessorDictionary = new Dictionary<ProcessQuery, MethodDeclarationSyntax>();
        }

        public ClassDeclarationSyntax Class { get; }
        public IReadOnlyDictionary<ProcessQuery, MethodDeclarationSyntax> Processors => this.ProcessorDictionary;


        public void Add(ProcessQuery query, MethodDeclarationSyntax method) => this.ProcessorDictionary.Add(query, method);
    }
}
