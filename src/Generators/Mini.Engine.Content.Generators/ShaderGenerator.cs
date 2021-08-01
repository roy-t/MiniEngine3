using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Content.Generators.Parsers.HLSL;
using Mini.Engine.Content.Generators.Source;
using Mini.Engine.Content.Generators.Source.CSharp;
using ShaderTools.CodeAnalysis.Hlsl.Text;


namespace Mini.Engine.Content.Generators
{

    [Generator]
    public class ShaderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            // TODO: make start of path disappear and let effect search for it in the CWD and every dir above it?
            // Or give a file that sets the parent directory for release builds?


            var fileSystem = new ContentFileSystem();
            foreach (var shaderFile in context.AdditionalFiles
                .Where(f => System.IO.Path.GetExtension(f.Path).Equals(".fx", StringComparison.InvariantCultureIgnoreCase)))
            {
                var shader = new Shader(shaderFile);

                var file = new File($"{shader.Name}.cs");
                file.Usings.Add(new Using("Mini.Engine.DirectX"));
                file.Usings.Add(new Using("System.Runtime.InteropServices"));

                var @namespace = new Namespace("Mini.Engine.Content");
                file.Namespaces.Add(@namespace);

                var @class = new Class(shader.Name, "public", "sealed");
                @namespace.Types.Add(@class);

                @class.InheritsFrom.Add("Shader");

                var constructor = new Constructor(@class.Name, "public");
                @class.Constructors.Add(constructor);

                constructor.Parameters.Add(new Field("Device", "device"));
                constructor.Chain = new BaseConstructorCall("device", SourceUtilities.ToLiteral(shader.FilePath));

                foreach (var structure in shader.Structures)
                {
                    var @struct = new Struct(Naming.ToPascalCase(structure.Name), "public");
                    @struct.Attributes.Add(new Source.CSharp.Attribute("StructLayout", new ArgumentList("LayoutKind.Sequential")));
                    foreach (var variable in structure.Variables)
                    {
                        var typeName = TypeTranslator.GetDotNetType(variable);
                        @struct.Properties.Add(new Property(typeName, Naming.ToPascalCase(variable.Name), false, "public"));
                    }

                    @class.InnerTypes.Add(@struct);
                }

                foreach (var cbuffer in shader.CBuffers)
                {
                    // Create a custom type for the CBuffer's data
                    var @struct = new Struct($"CBuffer{cbuffer.Slot}", "public");
                    @struct.Attributes.Add(new Source.CSharp.Attribute("StructLayout", new ArgumentList("LayoutKind.Sequential")));
                    @struct.Fields.Add(new Field("int", $"Slot = {cbuffer.Slot}", "public", "const"));

                    foreach (var variable in cbuffer.Variables)
                    {
                        var typeName = TypeTranslator.GetDotNetType(variable);
                        @struct.Properties.Add(new Property(typeName, variable.Name, false, "public"));
                    }

                    @class.InnerTypes.Add(@struct);
                }

                var writer = new SourceWriter();
                file.Generate(writer);
                context.AddSource($"{shader.Name}.cs", writer.ToString());
            }
        }
    }
}
