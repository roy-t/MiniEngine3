using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mini.Engine.Content.Generators.Parsers.HLSL;
using Mini.Engine.Generators.Source;

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
namespace Mini.Engine.Content.Shaders
{{
    public sealed class {shader.Name}
    {{
");
        WriteStructures(shader.Structures, builder);

        builder.Append($@"
    }}
}}
");

        return SourceText.From(builder.ToString(), Encoding.UTF8);
    }

    private static void WriteStructures(IReadOnlyList<Structure> structures, StringBuilder builder)
    {
        foreach (var structure in structures)
        {
            builder.Append($@"
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public struct {structure.Name}
        {{");
            foreach (var field in structure.Variables)
            {
                builder.Append($@"
            public {Types.ToDotNetType(field)} {Naming.ToPascalCase(field.Name)} {{get; set; }}
");
            }


            builder.Append($@"
        }}
");
        }
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