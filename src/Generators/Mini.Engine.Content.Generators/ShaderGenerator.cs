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
public sealed class ShaderGenerator : IIncrementalGenerator
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

        var innerClass = ShaderUserGenerator.Generate(shader);

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