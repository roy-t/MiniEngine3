using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Content.Generators.Parsers.HLSL;
using Mini.Engine.Generators.Source;
using Mini.Engine.Generators.Source.CSharp;


namespace Mini.Engine.Content.Generators
{
    /// <summary>
    /// "Use View -> Other Windows -> Shader Syntax Visualizer to help create the right code"
    /// </summary>
    [Generator]
    public class ShaderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var generatedFiles = context.AdditionalFiles
                .Where(f => Path.GetExtension(f.Path).Equals(".hlsl", StringComparison.InvariantCultureIgnoreCase))
                .Select(f => new Shader(f))
                .Where(shader => shader.Functions.Any(f => f.IsProgram()))
                .SelectMany(shader =>
                {
                    var structuresFile = SourceFile.Build($"{shader.Name}Structures.cs")
                        .Using("Mini.Engine.DirectX")
                        .Using("System.Runtime.InteropServices")
                        .Namespace($"Mini.Engine.Content.Shaders.{shader.Name}")
                            .Types(shader.Structures.Select(structure =>
                            {
                                return Struct.Build(Naming.ToPascalCase(structure.Name), "public")
                                    .Attribute("StructLayout", "LayoutKind.Sequential")
                                    .Properties(structure.Variables.Select(v => new Property(TypeTranslator.GetDotNetType(v), Naming.ToPascalCase(v.Name), false, "public")))
                                    .Output;
                            }))
                            .Types(shader.CBuffers.Select(cBuffer =>
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
                        .Complete();

                    var classFiles = shader.Functions
                        .Where(f => f.IsProgram())
                        .Select(function =>
                        {
                            return SourceFile.Build($"{shader.Name}{function.Name}.cs")
                                .Using("Mini.Engine.DirectX")
                                .Using("System.Runtime.InteropServices")
                                .Namespace("Mini.Engine.Content.Shaders")
                                    .Class(Naming.ToPascalCase($"{shader.Name}{function.Name}"), "public", "sealed")
                                        .Inherits(BaseTypeTranslator.GetBaseType(function))
                                        .Constructor("public")
                                            .Parameter("Device", "device")
                                            .BaseConstructorCall("device",
                                                SourceUtilities.ToLiteral(shader.FilePath),
                                                SourceUtilities.ToLiteral(function.Name),
                                                SourceUtilities.ToLiteral(function.GetProfile()))
                                            .Complete()
                                        .Complete()
                                    .Complete()
                                .Complete();
                        });

                    return classFiles.Append(structuresFile);
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
