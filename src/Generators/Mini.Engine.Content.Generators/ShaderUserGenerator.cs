using System.Text;
using Mini.Engine.Content.Generators.HLSL;
using Mini.Engine.Content.Generators.HLSL.Parsers;
using Mini.Engine.Generators.Source;

namespace Mini.Engine.Content.Generators;

internal static class ShaderUserGenerator
{
    public static string Generate(Shader shader)
    {
        if (shader.CBuffers.Count == 0)
        {
            return string.Empty;
        }

        var @class = "User";

        var fields = GenerateConstantBufferFields(shader.CBuffers);

        var arguments = "Mini.Engine.DirectX.Device device, string user";

        var assignments = GenerateConstantBufferAssignments(shader.CBuffers, shader.Name);

        var methods = GenerateConstantBufferMethods(shader.CBuffers, shader.Structures);

        var disposes = GenerateDisposeCalls(shader.CBuffers);

        return FormatClassSkeleton(@class, fields, arguments, assignments, methods, disposes);
    }

    private static string GenerateConstantBufferFields(IReadOnlyList<CBuffer> cbuffers)
    {
        var builder = new StringBuilder();
        foreach (var cbuffer in cbuffers)
        {
            var structName = Naming.ToUpperCamelCase(cbuffer.Name);
            builder.AppendLine($"public readonly Mini.Engine.DirectX.Buffers.ConstantBuffer<{structName}> {structName}Buffer;");
        }

        return builder.ToString();
    }

    private static string GenerateConstantBufferAssignments(IReadOnlyList<CBuffer> cbuffers, string name)
    {
        var builder = new StringBuilder();
        foreach (var cbuffer in cbuffers)
        {
            var structName = Naming.ToUpperCamelCase(cbuffer.Name);
            builder.AppendLine($"this.{structName}Buffer = new Mini.Engine.DirectX.Buffers.ConstantBuffer<{structName}>(device, user);");
        }

        return builder.ToString();
    }

    private static string GenerateConstantBufferMethods(IReadOnlyList<CBuffer> cBuffers, IReadOnlyList<Structure> knownStructures)
    {
        var builder = new StringBuilder();

        foreach (var cbuffer in cBuffers)
        {
            var name = Naming.ToUpperCamelCase(cbuffer.Name);
            var mapping = StructMapping.Create(cbuffer, knownStructures);

            var parameters = mapping.GetParametersForStruct();
            var arguments = string.Join(", ", parameters.Select(p => $"{PrimitiveTypeTranslator.ToDotNetType(p.Type, p.IsCustomType, 0)} {Naming.ToLowerCamelCase(p.Name)}"));

            var assignments = GenerateStructAssignments(mapping);
            var fieldName = $"{Naming.ToUpperCamelCase(cbuffer.Name)}Buffer";
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

    private static string GenerateStructAssignments(StructMapping mapping)
    {
        var lines = new List<string>();
        foreach (var field in mapping.Fields)
        {
            var path = mapping.GetAssignmentForFlattenedStruct(field);
            var fieldName = mapping.GetFieldForFlattenedStruct(field);
            lines.Add($"{fieldName} = {path}");
        }

        return string.Join($",{Environment.NewLine}", lines.ToArray());
    }

    private static string GenerateDisposeCalls(IReadOnlyList<CBuffer> cBuffers)
    {
        var builder = new StringBuilder();

        foreach (var cbuffer in cBuffers)
        {
            builder.AppendLine($"this.{Naming.ToUpperCamelCase(cbuffer.Name)}Buffer.Dispose();");
        }

        return builder.ToString();
    }

    private static string FormatClassSkeleton(string @class, string fields, string arguments, string assignments, string methods, string disposes)
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
}