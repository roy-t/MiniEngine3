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
            builder.AppendLine(Generate(structure, structures));
            builder.AppendLine();
        }

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

    public static string Generate(CBuffer cbuffer, IReadOnlyList<Structure> knownStructures)
    {
        var builder = new StringBuilder();

        var size = ComputeCBufferSizeInBytes(cbuffer, knownStructures);
        builder.Append($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4, Size = {size})]");
        GenerateStructureBody(cbuffer.Name, cbuffer.Variables, builder);        

        return builder.ToString();
    }

    public static string Generate(Structure structure, IReadOnlyList<Structure> knownStructures)
    {
        var builder = new StringBuilder();

        builder.Append($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]");
        GenerateStructureBody(structure.Name, structure.Variables, builder);

        return builder.ToString();
    }

    // TODO: we should not only compute the CBuffer size, we should also make sure that no structure crosses a 16 byte boundary
    // see Sunlight.hlsl for an example. What makes this extra tricky is that any structure used in a CBuffer should
    // also play by these rules. For now users will need to add a few paddings if they end on an uneven structure before a big structure
    private static int ComputeCBufferSizeInBytes(CBuffer cbuffer, IReadOnlyList<Structure> knownStructures)
    {
        var size = 0;
        foreach (var variable in cbuffer.Variables)
        {
            size += GetVariableSizeInBytes(variable, knownStructures);
        }

        // Make sure that the CBuffer structure size is a multiple of 16 bytes
        var remainder = size % 16;
        if (remainder > 0)
        {
            size = size - remainder + 16;
        }

        return size;
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

    private static void GenerateStructureBody(string name, IReadOnlyList<Variable> properties, StringBuilder builder)
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