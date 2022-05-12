using System.Text;
using Mini.Engine.Content.Generators.HLSL.Parsers;
using Mini.Engine.Generators.Source;

namespace Mini.Engine.Content.Generators.HLSL;

internal static class StructGenerator
{
    public static string Generate(IReadOnlyList<Structure> structures)
    {
        var builder = new StringBuilder();

        foreach (var structure in structures)
        {
            builder.AppendLine(Generate(structure));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public static string Generate(Structure structure)
    {
        var builder = new StringBuilder();

        builder.Append($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]");
        GenerateBody(structure.Name, structure.Variables, builder);

        return builder.ToString();
    }

    public static string Generate(IReadOnlyList<CBuffer> cbuffers, IReadOnlyList<Structure> knownStructures)
    {
        var builder = new StringBuilder();

        foreach (var cbuffer in cbuffers)
        {
            builder.Append(Generate(cbuffer, knownStructures));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public static IReadOnlyList<Variable> Flatten(CBuffer cbuffer, IReadOnlyList<Structure> knownStructures)
    {
        return Flatten(string.Empty, cbuffer.Variables, knownStructures);
    }

    public static IReadOnlyList<Variable> Flatten(string prefix, IReadOnlyList<Variable> variables, IReadOnlyList<Structure> knownStructures, bool dot = false)
    {
        var output = new List<Variable>();
        foreach(var variable in variables)
        {
            if(variable.IsCustomType)
            {
                var foo = dot? prefix + variable.Name + "." : prefix + variable.Name;
                var type = knownStructures.First(ks => ks.Name.Equals(variable.Type, StringComparison.InvariantCultureIgnoreCase));
                output.AddRange(Flatten(foo, type.Variables, knownStructures, dot));
            }
            else
            {
                output.Add(new Variable(variable.Type, variable.IsPredefinedType, $"{prefix}{variable.Name}", variable.Dimensions, variable.Slot));
            }
        }

        return output;
    }

    // CBuffers have very explicit packing and block size rules so we explicitly layout such a structures fields
    // more info: https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-packing-rules
    public static string Generate(CBuffer cbuffer, IReadOnlyList<Structure> knownStructures)
    {
        var builder = new StringBuilder();
        
        var body = Generate(cbuffer.Name, cbuffer.Variables, knownStructures, 4, 16, out var size);
        builder.AppendLine($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = {size})]");
        builder.Append(body);
      
        return builder.ToString();
    }

    private static string Generate(string name, IReadOnlyList<Variable> variables, IReadOnlyList<Structure> knownStructures, int packSize, int blockSize, out int size)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"public struct {name}");
        builder.AppendLine("{");

        var total = 0;
        var block = 0;

        var allVariables = Flatten(string.Empty, variables, knownStructures);

        foreach (var variable in allVariables)
        {
            var fieldOffset = total;

            var variableSize = Math.Max(packSize, GetVariableSizeInBytes(variable, knownStructures));

            if (block != 0 && block + variableSize > blockSize)
            {
                fieldOffset += blockSize - block;
                block = 0;
            }

            builder.AppendLine($"[System.Runtime.InteropServices.FieldOffset({fieldOffset})]");
            builder.AppendLine($"public {PrimitiveTypeTranslator.ToDotNetType(variable)} {variable.Name};");

            total = fieldOffset + variableSize;
            block = (block + variableSize) % blockSize;
        }


        builder.AppendLine("}");

        var remainder = total % blockSize;
        if (remainder > 0)
        {
            total += blockSize - remainder;
        }

        size = total;

        return builder.ToString();
    }

    private static int ComputeStructureSizeInBytes(Structure structure, IReadOnlyList<Structure> knownStructures)
    {
        var size = 0;
        foreach (var variable in structure.Variables)
        {
            size += GetVariableSizeInBytes(variable, knownStructures);
        }

        return size;
    }

    private static int GetVariableSizeInBytes(Variable variable, IReadOnlyList<Structure> knownStructures)
    {
        if (variable.IsCustomType)
        {
            var structure = knownStructures.First(s => s.Name.Equals(variable.Type, StringComparison.InvariantCultureIgnoreCase));
            return ComputeStructureSizeInBytes(structure, knownStructures);
        }

        return PrimitiveTypeTranslator.GetSizeInBytes(variable);
    }

    private static void GenerateBody(string name, IReadOnlyList<Variable> properties, StringBuilder builder)
    {
        builder.Append($@"
        public struct {name}
        {{
        ");
        foreach (var property in properties)
        {
            builder.AppendLine($"public {PrimitiveTypeTranslator.ToDotNetType(property)} {Naming.ToPascalCase(property.Name)};");
        }
        builder.AppendLine("}");
    }

}