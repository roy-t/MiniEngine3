using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;


namespace Mini.Engine.Content.Generators.Shaders
{
    [Generator]
    public class ShaderDataTypesGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var file in context.AdditionalFiles
                .Where(f => Path.GetExtension(f.Path).Equals(".fx", StringComparison.InvariantCultureIgnoreCase)))
            {
                var contents = file.GetText();
                var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), null, new ContentFileSystem());

                // TODO: first find all custom types
                var cbuffers = CBuffer.FindAll(syntaxTree.Root);


                var text = "";
                var name = Path.GetFileName(file.Path);
                context.AddSource($"{name}.cs", text);
            }
        }
    }
}
