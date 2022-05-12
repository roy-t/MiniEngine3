using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mini.Engine.Content.Generators.HLSL;
using Mini.Engine.Content.Generators.HLSL.Parsers;
using Mini.Engine.Generators.Source;
using Mini.Engine.Generators.Source.CSharp;

namespace Mini.Engine.Content.Generators;

// Based on: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
// Use View -> Other Windows -> Shader Syntax Visualizer to help create the right code
[Generator]
public sealed class AltShaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var shaderFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".hlsl", StringComparison.InvariantCultureIgnoreCase));
        var provider = shaderFiles.Select(static (text, cancellationToken) => (path: text.Path, text: text.GetText(cancellationToken)));

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
        if (!HasRelevantFunction(shader))
        {
            return null;
        }

        var @namespace = "Mini.Engine.Content.Shaders.Generated";
        var @class = Naming.ToPascalCase(shader.Name);

        var constants = GenerateResourceSlotConstants(shader.Variables, shader.CBuffers);

        var fields = @"private readonly Mini.Engine.DirectX.Device Device;
                       private readonly Mini.Engine.IO.IVirtualFileSystem FileSystem;
                       private readonly Mini.Engine.Content.ContentManager Content;";


        var arguments = "Mini.Engine.DirectX.Device device, Mini.Engine.IO.IVirtualFileSystem fileSystem, Mini.Engine.Content.ContentManager content";

        var assignments = GenerateFieldAssignments() + GenerateShaderPropertyAssignments(shader.FilePath, shader.Functions);

        var properties = GenerateShaderProperties(shader.Functions);

        var structures = StructGenerator.Generate(shader.Structures) + StructGenerator.Generate(shader.CBuffers, shader.Structures);

        var methods = string.Empty;
        if (shader.CBuffers.Count > 0)
        {
            methods = $"public {@class}.User CreateUserFor<T>() {{ return new {@class}.User(this.Device, typeof(T).Name); }}";
        }

        var innerClass = GenerateShaderUser(shader);

        var code = FormatFileSkeleton(@namespace, @class, constants, fields, arguments, assignments, properties, structures, methods, innerClass);

        var formatted = CodeFormatter.Format(code, FormatOptions.Default);
        return SourceText.From(formatted, Encoding.UTF8);
    }

    private static bool HasRelevantFunction(Shader shader)
    {
        return shader.Functions.Any(f => f.GetProgramDirective() != ProgramDirectives.None);
    }

    private static string GenerateResourceSlotConstants(IReadOnlyList<Variable> variables, IReadOnlyList<CBuffer> cbuffers)
    {
        var builder = new StringBuilder();

        foreach (var variable in variables)
        {
            if (variable.Slot != null)
            {
                builder.AppendLine($"public const int {Naming.ToPascalCase(variable.Name)} = {variable.Slot};");
            }
        }

        foreach (var cbuffer in cbuffers)
        {
            builder.AppendLine($"public const int {Naming.ToPascalCase(cbuffer.Name)}Slot = {cbuffer.Slot};");
        }

        return builder.ToString();
    }

    private static string GenerateFieldAssignments()
    {
        return @"this.Device = device;
                 this.FileSystem = fileSystem;
                 this.Content = content;";
    }

    private static string GenerateShaderPropertyAssignments(string filePath, IReadOnlyList<Function> functions)
    {
        var builder = new StringBuilder();
        foreach (var function in functions)
        {
            var id = $"new Mini.Engine.Content.ContentId({SourceUtilities.ToLiteral(filePath)},{SourceUtilities.ToLiteral(function.Name)})";
            var instantation = string.Empty;
            switch (function.GetProgramDirective())
            {
                case ProgramDirectives.VertexShader:
                    instantation = $"new Mini.Engine.Content.Shaders.VertexShaderContent(this.Device, this.FileSystem, this.Content, {id}, \"{function.GetProfile()}\")";
                    break;
                case ProgramDirectives.PixelShader:
                    instantation = $"new Mini.Engine.Content.Shaders.PixelShaderContent(this.Device, this.FileSystem, this.Content, {id}, \"{function.GetProfile()}\")";
                    break;
                case ProgramDirectives.ComputeShader:
                    var x = function.Attributes["numthreads"][0];
                    var y = function.Attributes["numthreads"][1];
                    var z = function.Attributes["numthreads"][2];
                    instantation = $"new Mini.Engine.Content.Shaders.ComputeShaderContent(this.Device, this.FileSystem, this.Content, {id}, \"{function.GetProfile()}\", {x}, {y}, {z})";
                    break;
            }

            if (!string.IsNullOrEmpty(instantation))
            {
                builder.AppendLine($"this.{Naming.ToPascalCase(function.Name)} = {instantation};");
            }
        }

        return builder.ToString();
    }

    private static string GenerateShaderProperties(IReadOnlyList<Function> functions)
    {
        var builder = new StringBuilder();

        foreach (var function in functions)
        {
            var interfaceType = string.Empty;
            switch (function.GetProgramDirective())
            {
                case ProgramDirectives.VertexShader:
                    interfaceType = "Mini.Engine.DirectX.Resources.IVertexShader";
                    break;
                case ProgramDirectives.PixelShader:
                    interfaceType = "Mini.Engine.DirectX.Resources.IPixelShader";
                    break;
                case ProgramDirectives.ComputeShader:
                    interfaceType = "Mini.Engine.DirectX.Resources.IComputeShader";
                    break;
            }

            if (!string.IsNullOrEmpty(interfaceType))
            {
                builder.AppendLine($"public {interfaceType} {Naming.ToPascalCase(function.Name)} {{ get; }}");
            }
        }

        return builder.ToString();
    }

    private static string GenerateShaderUser(Shader shader)
    {
        if (shader.CBuffers.Count == 0)
        {
            return string.Empty;
        }

        var @class = "User";

        var fields = GenerateConstantBufferFields(shader.CBuffers);

        var arguments = "Mini.Engine.DirectX.Device device, string user";

        var assignments = GenerateConstantBufferAssignments(shader.CBuffers, shader.Name);

        var methods = GenerateConstantBufferMethods(shader.CBuffers);

        var disposes = GenerateDisposeCalls(shader.CBuffers);

        return FormatInnerClassSkeleton(@class, fields, arguments, assignments, methods, disposes);
    }

    private static string GenerateConstantBufferFields(IReadOnlyList<CBuffer> cbuffers)
    {
        var builder = new StringBuilder();
        foreach (var cbuffer in cbuffers)
        {
            var structName = Naming.ToPascalCase(cbuffer.Name);
            builder.AppendLine($"public readonly Mini.Engine.DirectX.Buffers.ConstantBuffer<{structName}> {structName}Buffer;");
        }

        return builder.ToString();
    }

    private static string GenerateConstantBufferAssignments(IReadOnlyList<CBuffer> cbuffers, string name)
    {
        var builder = new StringBuilder();
        foreach (var cbuffer in cbuffers)
        {
            var structName = Naming.ToPascalCase(cbuffer.Name);            
            builder.AppendLine($"this.{structName}Buffer = new Mini.Engine.DirectX.Buffers.ConstantBuffer<{structName}>(device, user);");
        }

        return builder.ToString();
    }

    private static string GenerateConstantBufferMethods(IReadOnlyList<CBuffer> cBuffers)
    {
        var builder = new StringBuilder();

        foreach (var cbuffer in cBuffers)
        {
            var name = Naming.ToPascalCase(cbuffer.Name);

            var variables = cbuffer.Variables.Where(v => !v.Name.StartsWith("__"));

            var arguments = string.Join(", ", variables.Select(v => $"{PrimitiveTypeTranslator.ToDotNetType(v)} {Naming.ToCamelCase(v.Name)}"));
            var assignments = string.Join($",{Environment.NewLine}", variables.Select(v => $"{Naming.ToPascalCase(v.Name)} = {Naming.ToCamelCase(v.Name)}"));
            var fieldName = $"{Naming.ToPascalCase(cbuffer.Name)}Buffer";
            var method = $@"            
            public void Map{name}(Mini.Engine.DirectX.Contexts.DeviceContext context, {arguments})
            {{
                var constants = new {name}()
                {{
                    {assignments}
                }};

                this.{fieldName}.MapData(context, constants);
            }}";

            builder.AppendLine(method);
        }

        return builder.ToString();
    }

    private static string GenerateDisposeCalls(IReadOnlyList<CBuffer> cBuffers)
    {
        var builder = new StringBuilder();

        foreach (var cbuffer in cBuffers)
        {
            builder.AppendLine($"this.{Naming.ToPascalCase(cbuffer.Name)}Buffer.Dispose();");
        }

        return builder.ToString();
    }

    private static string FormatInnerClassSkeleton(string @class, string fields, string arguments, string assignments, string methods, string disposes)
    {
        return $@"            
            public sealed class {@class} : System.IDisposable
            {{
                {fields}

                public {@class}({arguments})
                {{
                    {assignments}
                }}

                {methods}

                public void Dispose()
                {{
                    {disposes}
                }}
            }}";
    }

    private static string FormatFileSkeleton(string @namespace, string @class, string constants, string fields, string arguments, string assignments, string properties, string structures, string methods, string innerClass)
    {
        return $@"
            namespace {@namespace}
            {{
                [Mini.Engine.Configuration.Content]
                public sealed class {@class}
                {{
                    {constants}

                    {fields}

                    public {@class}({arguments})
                    {{
                        {assignments}
                    }}

                    {properties}

                    {structures}

                    {methods}

                    {innerClass}
                }}
            }}";
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