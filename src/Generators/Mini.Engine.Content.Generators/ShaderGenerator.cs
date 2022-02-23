using System;
using System.Collections.Generic;
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
            var contentFiles = new List<SourceFile>();

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
                                return Struct.Builder(Naming.ToPascalCase(structure.Name), "public")
                                    .Attribute("StructLayout", "LayoutKind.Sequential")
                                    .Properties(structure.Variables.Select(v => new Property(TypeTranslator.GetDotNetType(v), Naming.ToPascalCase(v.Name), false, "public")))
                                    .Output;
                            }))
                            .Types(shader.CBuffers.Select(cBuffer =>
                            {
                                return Struct.Builder(Naming.ToPascalCase(cBuffer.Name), "public")
                                    .Attribute("StructLayout", "LayoutKind.Sequential", "Pack = 4")
                                    .Field("int", "Slot", "public", "const")
                                        .Value($"{cBuffer.Slot}")
                                        .Complete()
                                    .Properties(cBuffer.Variables.Select(v => new Property(TypeTranslator.GetDotNetType(v), Naming.ToPascalCase(v.Name), false, "public")))
                                    .Output;
                            }))
                            .Complete()
                        .Complete();

                    var shaderFile = SourceFile.Build($"{shader.Name}.cs")
                        .Namespace($"Mini.Engine.Content.Shaders.{shader.Name}")
                            .Class(Naming.ToPascalCase($"{shader.Name}"), "public", "static")
                                .Fields(shader.Variables
                                    .Where(v => v.Slot != null)
                                    .Select(v => Field.Builder("int", Naming.ToPascalCase(v.Name), "public", "const")
                                    .Value(v.Slot?.ToString() ?? "0")
                                    .Complete()))
                                .Complete()
                            .Complete()
                        .Complete();

                    var classFiles = shader.Functions
                        .Where(f => f.IsProgram())
                        .Select(function =>
                        {
                            return SourceFile.Build($"{shader.Name}{function.Name}.cs")
                                .Using("Mini.Engine.Configuration")
                                .Using("Mini.Engine.Content")
                                .Using("Mini.Engine.DirectX")
                                .Using("Mini.Engine.DirectX.Resources")
                                .Using("Mini.Engine.IO")
                                .Using("System.Runtime.InteropServices")
                                .Namespace("Mini.Engine.Content.Shaders")
                                    .Class(Naming.ToPascalCase($"{shader.Name}{function.Name}"), "public", "sealed")
                                        .Attribute("Content")
                                        .Inherits(BaseTypeTranslator.GetBaseType(function))
                                        .Constructor("public")
                                            .Parameter("Device", "device")
                                            .Parameter("IVirtualFileSystem", "fileSystem")
                                            .Parameter("ContentManager", "content")
                                            .BaseConstructorCall("device", "fileSystem", "content",
                                                $"new ContentId({SourceUtilities.ToLiteral(shader.FilePath)},{SourceUtilities.ToLiteral(function.Name)})",
                                                SourceUtilities.ToLiteral(function.GetProfile()))
                                            .Complete()
                                        .Complete()
                                    .Complete()
                                .Complete();
                        });

                    contentFiles.AddRange(classFiles);
                    return classFiles.Append(structuresFile).Append(shaderFile);
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
