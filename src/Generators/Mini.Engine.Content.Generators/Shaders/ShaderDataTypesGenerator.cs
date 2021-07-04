using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Content.Generators.Source;
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
            foreach (var effectFile in context.AdditionalFiles
                .Where(f => System.IO.Path.GetExtension(f.Path).Equals(".fx", StringComparison.InvariantCultureIgnoreCase)))
            {
                var contents = effectFile.GetText();
                var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), null, new ContentFileSystem());

                var name = System.IO.Path.GetFileNameWithoutExtension(effectFile.Path);

                var file = new File($"{name}.cs");
                file.Usings.Add(new Using("Mini.Engine.DirectX"));

                var @namespace = new Namespace("Mini.Engine.Content");
                file.Namespaces.Add(@namespace);

                var @class = new Class(name, "public", "sealed");
                @namespace.Types.Add(@class);

                @class.InheritsFrom.Add("Shader");


                // TODO: first find all custom types

                var cbuffers = CBuffer.FindAll(syntaxTree.Root);
                foreach (var cbuffer in cbuffers)
                {
                    // TODO: add structlayout attribute
                    var @struct = new Struct($"{name}CBuffer{cbuffer.Slot}", "public");
                    @namespace.Types.Add(@struct);

                    foreach (var variable in cbuffer.Variables)
                    {
                        // TODO translate HLSL type to C# type
                        @struct.Properties.Add(new Property(variable.Type, variable.Name, false, "public"));
                    }
                }


                var writer = new SourceWriter();
                file.Generate(writer);
                context.AddSource($"{name}.cs", writer.ToString());
            }
        }
    }
}
