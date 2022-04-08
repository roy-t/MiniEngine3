using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mini.Engine.Content.Generators.Parsers.HLSL;
using Mini.Engine.Generators.Source;
using Mini.Engine.Generators.Source.CSharp;


namespace Mini.Engine.Content.Generators;

/// <summary>
/// "Use View -> Other Windows -> Shader Syntax Visualizer to help create the right code"
/// </summary>
[Generator]
public class ShaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var shaderFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".hlsl", StringComparison.InvariantCultureIgnoreCase));
        var provider = shaderFiles.Select((text, cancellationToken)
            => (path: text.Path, source: text.GetText(cancellationToken)));

        context.RegisterSourceOutput(provider, (outputContext, nameAndText) =>
        {
            var generated = GenerateFiles(nameAndText.path, nameAndText.source);
            foreach (var file in generated)
            {
                var writer = new SourceWriter();
                file.Generate(writer);
                outputContext.AddSource(file.Name, writer.ToString());
            }
        });
    }

    private static IEnumerable<SourceFile> GenerateFiles(string path, SourceText? contents)
    {
        var shader = new Shader(path, contents);

        var structuresFile = SourceFile.Build($"{shader.Name}Structures.cs")
            .Using("Mini.Engine.DirectX")
            .Using("System.Runtime.InteropServices")
            .Namespace($"Mini.Engine.Content.Shaders.{shader.Name}")
                .Types(shader.Structures.Select(structure =>
                {
                    return Struct.Builder(Naming.ToPascalCase(structure.Name), "public")
                        .Attribute("StructLayout", "LayoutKind.Sequential")
                        .Properties(structure.Variables.Select(v => new Property(Types.ToDotNetType(v), Naming.ToPascalCase(v.Name), false, "public")))
                        .Output;
                }))
                .Types(shader.CBuffers.Select(cBuffer =>
                {
                    return Struct.Builder(Naming.ToPascalCase(cBuffer.Name), "public")
                        .Attribute("StructLayout", "LayoutKind.Sequential", "Pack = 4")
                        .Field("int", "Slot", "public", "const")
                            .Value($"{cBuffer.Slot}")
                            .Complete()
                        .Properties(cBuffer.Variables.Select(v => new Property(Types.ToDotNetType(v), Naming.ToPascalCase(v.Name), false, "public")))
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
            .Where(function => function.GetProgramDirective() != ProgramDirectives.ComputeShader)
            .Select(function => SourceFile.Build($"{shader.Name}{function.Name}.cs")
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
                    .Complete())
            .Union
            (
                shader.Functions
                    .Where(f => f.IsProgram())
                    .Where(function => function.GetProgramDirective() == ProgramDirectives.ComputeShader)
                    .Select(function => SourceFile.Build($"{shader.Name}{function.Name}.cs")
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
                                            SourceUtilities.ToLiteral(function.GetProfile()),
                                            function.Attributes["numthreads"][0],
                                            function.Attributes["numthreads"][1],
                                            function.Attributes["numthreads"][2])
                                        .Complete()
                                    .Complete()
                                .Complete()
                            .Complete())
            );

        return classFiles.Append(structuresFile).Append(shaderFile);
    }
}
