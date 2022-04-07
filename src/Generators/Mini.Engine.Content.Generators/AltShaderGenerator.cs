using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mini.Engine.Content.Generators.Parsers.HLSL;
using Mini.Engine.Generators.Source;
using Mini.Engine.Generators.Source.CSharp;

namespace Mini.Engine.Content.Generators;

// Based on: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
[Generator]
public sealed class AltShaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var shaderFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".hlsl", StringComparison.InvariantCultureIgnoreCase));
        var provider = shaderFiles.Select((text, cancellationToken)
            => (path: text.Path, text: text.GetText(cancellationToken)));

        context.RegisterSourceOutput(provider, static (outputContext, file) =>
        {            
            var name = Path.GetFileNameWithoutExtension(file.path);
            var source = GenerateShaderFile(file.path, file.text, outputContext.CancellationToken);
            if (source != null)
            {
                outputContext.AddSource(name, source);
            }
        });
    }

    private static SourceText? GenerateShaderFile(string path, SourceText? text, CancellationToken cancellationToken)
    {
        var shader = Shader.Parse(path, text, cancellationToken);
        if (shader.Functions.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder();
        builder.Append($@"
namespace Mini.Engine.Content.Shaders.Buffers
{{
    public sealed class {shader.Name}
    {{
");
        WriteConstructorAndProperties(shader.Name, builder);
        WriteStructures(shader.Structures, builder);
        WriteConstantBufferStructures(shader.CBuffers, builder);
        WriteResourceBindings(shader.Variables, builder);
        WriteShaders(shader.FilePath, shader.Functions, builder);
        builder.Append($@"
    }}
}}");

        var formatted = CodeFormatter.Format(builder.ToString(), FormatOptions.Default);
        return SourceText.From(formatted.ToString(), Encoding.UTF8);
    }

    private static void WriteConstructorAndProperties(string name, StringBuilder builder)
    {
        builder.Append($@"
private readonly Mini.Engine.DirectX.Device Device;
private readonly Mini.Engine.IO.IVirtualFileSystem FileSystem;
private readonly Mini.Engine.Content.ContentManager Content;

public {name}(Mini.Engine.DirectX.Device device, Mini.Engine.IO.IVirtualFileSystem fileSystem, Mini.Engine.Content.ContentManager content)
{{
    this.Device = device;
    this.FileSystem = fileSystem;
    this.Content = content;
}}

");
    }

    private static void WriteStructures(IReadOnlyList<Structure> structures, StringBuilder builder)
    {
        foreach (var structure in structures)
        {
            builder.Append($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]");
            WriteStructure(structure.Name, structure.Variables, builder);
        }
    }

    private static void WriteConstantBufferStructures(IReadOnlyList<CBuffer> cbuffers, StringBuilder builder)
    {
        foreach (var cbuffer in cbuffers)
        {
            builder.Append($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]");
            WriteStructure(cbuffer.Name, cbuffer.Variables, builder);
        }
    }

    private static void WriteResourceBindings(IReadOnlyList<Variable> variables, StringBuilder builder)
    {
        foreach(var variable in variables)
        {
            if (variable.Slot != null)
            {
                builder.AppendLine($"public static int {Naming.ToPascalCase(variable.Name)} = {variable.Slot}");
            }
        }
    }

    private static void WriteShaders(string filePath, IReadOnlyList<Function> functions, StringBuilder builder)
    {
        foreach(var function in functions)
        {
            var id = $"new ContentId({SourceUtilities.ToLiteral(filePath)},{SourceUtilities.ToLiteral(function.Name)})";
            var interfaceType = string.Empty;
            var concreteType = string.Empty;
            switch (function.GetProgramDirective())
            {
                case ProgramDirectives.VertexShader:
                    interfaceType = "Mini.Engine.DirectX.Resources.IVertexShader";
                    concreteType = $"new Mini.Engine.Content.Shaders.VertexShaderContent(this.Device, this.FileSystem, this.Content, {id}, \"{function.GetProfile()}\");";
                    break;
                case ProgramDirectives.PixelShader:
                    interfaceType = "Mini.Engine.DirectX.Resources.IPixelShader";
                    concreteType = $"new Mini.Engine.Content.Shaders.PixelShaderContent(this.Device, this.FileSystem, this.Content, {id}, \"{function.GetProfile()}\");";
                    break;
                case ProgramDirectives.ComputeShader:
                    interfaceType = "Mini.Engine.DirectX.Resources.IComputeShader";
                    var x = function.Attributes["numthreads"][0];
                    var y = function.Attributes["numthreads"][1];
                    var z  =function.Attributes["numthreads"][2];
                    concreteType = $"new Mini.Engine.Content.Shaders.ComputeShaderContent(this.Device, this.FileSystem, this.Content, {id}, \"{function.GetProfile()}\", {x}, {y}, {z});";
                    break;
            }

            if (!string.IsNullOrEmpty(interfaceType))
            {
                builder.AppendLine($"public readonly {interfaceType} {Naming.ToPascalCase(function.Name)} = {concreteType}");
            }
        }
    }

    private static void WriteStructure(string name, IReadOnlyList<Variable> properties, StringBuilder builder)
    {
        builder.Append($@"
public struct {name}
{{
");
        foreach (var property in properties)
        {
            builder.AppendLine($"public {Types.ToDotNetType(property)} {Naming.ToPascalCase(property.Name)} {{ get; set; }}");
        }
        builder.AppendLine("}");
        builder.AppendLine();
    }
}

// What I would like to generate
/*
namespace Mini.Engine.Content.Shaders
{
    // File generated for a .hlsl file
    public sealed class SunLight
    {
        // Struct defined in file itself
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public struct PS_INPUT
        {
            System.Numerics.Vector4 pos;
            System.Numerics.Vector2 tex;
        }

        // Struct defined in an included file
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public struct ShadowProperties
        {
            // ..
        };

        // Struct and resource binding defined for a cbuffer declaration
        public static int ConstantsBuffer = 0;
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Constants
        {
            System.Numerics.Vector4 Color;
            System.Numerics.Vector3 SurfaceToLight;
            // ..
        }

        // Resource bindings
        public static int TextureSampler = 0;
        public static int Albedo = 0;
        public static int Material = 1;
        public static int Depth = 2;
        public static int Normal = 3;
        public static int ShadowMap = 4;
        public static int ShadowSampler = 1;

        // Shaders for each program
        public Mini.Engine.DirectX.Resources.IPixelShader Ps { get; }
        public Mini.Engine.DirectX.Resources.IVertexShader Vs { get; }
        public Mini.Engine.DirectX.Resources.IComputeShader Cs { get; }

        public Buffers CreateBuffers(Mini.Engine.DirectX.Device device, Mini.Engine.DirectX.DeviceContext context)
        {
            return new Buffers(device, context);
        }

        public sealed class Buffers : System.IDisposable
        {
            private readonly Mini.Engine.DirectX.Device Device;
            private readonly Mini.Engine.DirectX.DeviceContext Context;

            private readonly ConstantBuffer<Constants> ConstantsBuffer;
            
            private Binder(Mini.Engine.DirectX.Device device, Mini.Engine.DirectX.DeviceContext context, string name)
            {
                this.Device = device;
                this.Context = context;

                this.ConstantsBuffer = new ConstantsBuffer<Constants>(device, $"{name}_Constants_CB");
            }

            public void MapConstants(System.Numerics.Vector4 color, System.Numerics.Vector2 surfaceToLight)
            {
                var constants = new Constants()
                {
                    Color = color,
                    SurfaceToLight = surfaceToLight
                };

                this.ConstantsBuffer.MapData(this.Context, constants);
            }

            public void Dispose()
            {
                this.ConstantsBuffer.Dispose();
            }
        }
    }
}
*/