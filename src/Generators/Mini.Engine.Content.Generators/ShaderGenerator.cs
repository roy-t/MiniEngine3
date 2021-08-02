using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Content.Generators.Parsers.HLSL;
using Mini.Engine.Content.Generators.Source;
using Mini.Engine.Content.Generators.Source.CSharp;


namespace Mini.Engine.Content.Generators
{

    [Generator]
    public class ShaderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var generatedFiles = context.AdditionalFiles
                .Where(f => Path.GetExtension(f.Path).Equals(".fx", StringComparison.InvariantCultureIgnoreCase))
                .Select(f => new Shader(f))
                .Select(shader =>
                {
                    return SourceFile.Build($"{shader.Name}.cs")
                        .Using("Mini.Engine.DirectX")
                        .Using("System.Runtime.InteropServices")
                        .Namespace("Mini.Engine.Content")
                            .Class(Naming.ToPascalCase(shader.Name), "public", "sealed")
                                .Inherits("Shader")
                                .Constructor("public")
                                    .Parameter("Device", "device")
                                    .BaseConstructorCall("device", SourceUtilities.ToLiteral(shader.FilePath))
                                    .Complete()
                                .InnerTypes(shader.Structures.Select(structure =>
                                {
                                    return Struct.Build(Naming.ToPascalCase(structure.Name), "public")
                                        .Attribute("StructLayout", "LayoutKind.Sequential")
                                        .Properties(structure.Variables.Select(v => new Property(TypeTranslator.GetDotNetType(v), Naming.ToPascalCase(v.Name), false, "public")))
                                        .Output;
                                }))
                                .InnerTypes(shader.CBuffers.Select(cBuffer =>
                                {
                                    return Struct.Build($"CBuffer{cBuffer.Slot}", "public")
                                        .Attribute("StructLayout", "LayoutKind.Sequential")
                                        .Field("int", "Slot", "public", "const")
                                            .Value($"{cBuffer.Slot}")
                                            .Complete()
                                        .Properties(cBuffer.Variables.Select(v => new Property(TypeTranslator.GetDotNetType(v), Naming.ToPascalCase(v.Name), false, "public")))
                                        .Output;
                                }))
                                .Complete()
                            .Complete()
                        .Complete();
                });

            foreach (var file in generatedFiles)
            {
                var writer = new SourceWriter();
                file.Generate(writer);
                context.AddSource(file.Name, writer.ToString());
            }
        }
    }
}
