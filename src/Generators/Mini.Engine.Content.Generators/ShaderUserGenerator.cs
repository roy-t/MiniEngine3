using System.Text;
using Mini.Engine.Content.Generators.HLSL.Parsers;
using Mini.Engine.Content.Generators.HLSL;
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

    private static string GenerateConstantBufferMethods(IReadOnlyList<CBuffer> cBuffers, IReadOnlyList<Structure> knownStructures)
    {
        var builder = new StringBuilder();

        foreach (var cbuffer in cBuffers)
        {
            var name = Naming.ToPascalCase(cbuffer.Name);

            var variables = cbuffer.Variables.Where(v => !v.Name.StartsWith("__")).ToList();

            var arguments = string.Join(", ", variables.Select(v => $"{PrimitiveTypeTranslator.ToDotNetType(v)} {Naming.ToCamelCase(v.Name)}"));            
            var assignments = GenerateStructAssignments(variables, knownStructures);
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

    private static string GenerateStructAssignments(IReadOnlyList<Variable> variables, IReadOnlyList<Structure> knownStructures)
    {
        var lines = new List<string>();
        foreach(var variable in variables)
        {
            if (variable.IsCustomType)
            {
                var type = knownStructures.First(ks => ks.Name.Equals(variable.Type, StringComparison.InvariantCultureIgnoreCase));
                var flatten = StructGenerator.Flatten(Naming.ToCamelCase(variable.Name) + ".", type.Variables, knownStructures, true);
                foreach (var v in flatten)
                {
                    var fieldName = string.Join(string.Empty, v.Name.Split('.').Select(p => Naming.ToPascalCase(p)));
                    lines.Add($"{fieldName} = {v.Name}");
                }
            }
            else
            {
                lines.Add($"{Naming.ToPascalCase(variable.Name)} = {Naming.ToCamelCase(variable.Name)}");
            }
        }

        return string.Join($",{Environment.NewLine}", lines.ToArray());
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