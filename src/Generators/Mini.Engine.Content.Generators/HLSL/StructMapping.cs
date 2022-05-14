using Mini.Engine.Content.Generators.HLSL.Parsers;
using Mini.Engine.Generators.Source;

namespace Mini.Engine.Content.Generators.HLSL;

internal sealed class NameAndType
{
    public NameAndType(string name, string type, bool isCustomType = false)
    {
        this.Name = name;
        this.Type = type;
        this.IsCustomType = isCustomType;
    }

    public string Name { get; }
    public string Type { get; }
    public bool IsCustomType { get; }

    public override string ToString()
    {
        return $"{{{this.Name} : {this.Type}}}";
    }
}

internal sealed class FieldMapping
{
    public FieldMapping(string name, string type, int sizeInBytes, IReadOnlyList<NameAndType> path)
    {
        this.Name = name;
        this.Type = type;
        this.SizeInBytes = sizeInBytes;
        this.Path = path;
    }

    public string Name { get; }
    public string Type { get; }
    public int SizeInBytes { get; }
    public IReadOnlyList<NameAndType> Path { get; }

    public override string ToString()
    {
        return $"{this.Name} : {this.Type} {this.SizeInBytes} bytes ~/{string.Join("/", this.Path.Append(new NameAndType(this.Name, this.Type)))}";
    }
}

internal sealed class StructMapping
{
    private StructMapping(string name, IReadOnlyList<FieldMapping> fields)
    {
        this.Name = name;
        this.Fields = fields;
    }

    public string Name { get; }
    public IReadOnlyList<FieldMapping> Fields { get; }

    public string GetFieldForFlattenedStruct(FieldMapping field)
    {
        return string.Join(string.Empty, field.Path
            .Select(f => f.Name)
            .Append(field.Name)
            .Select(p => Naming.ToUpperCamelCase(p)));
    }

    public string GetAssignmentForFlattenedStruct(FieldMapping field)
    {
        if (field.Path.Count > 0)
        {
            var parameterName = Naming.ToLowerCamelCase(field.Path[0].Name);
            return parameterName + "." + string.Join(".", field.Path.Skip(1).Select(f => f.Name).Append(field.Name));
        }

        return Naming.ToLowerCamelCase(field.Name);
    }

    public IReadOnlyList<NameAndType> GetParametersForStruct()
    {
        var parameters = new List<NameAndType>();

        var lastSeenStruct = string.Empty;

        foreach (var field in this.Fields)
        {
            if (field.Path.Count == 0)
            {
                parameters.Add(new NameAndType(Naming.ToLowerCamelCase(field.Name), field.Type, false));
            }

            if (field.Path.Count > 0 && !field.Path[0].Type.Equals(lastSeenStruct, StringComparison.OrdinalIgnoreCase))
            {
                lastSeenStruct = field.Path[0].Type;
                parameters.Add(new NameAndType(Naming.ToLowerCamelCase(field.Path[0].Name), field.Path[0].Type, true));
            }
        }

        return parameters;
    }

    public override string ToString()
    {
        return $"{this.Name} ({this.Fields.Count})";
    }

    public static StructMapping Create(Structure structure, IReadOnlyList<Structure> knownStructures)
    {
        return Create(structure.Name, structure.Variables, knownStructures);
    }

    public static StructMapping Create(CBuffer cbuffer, IReadOnlyList<Structure> knownStructures)
    {
        return Create(cbuffer.Name, cbuffer.Variables, knownStructures);
    }

    private static StructMapping Create(string name, IReadOnlyList<Variable> variables, IReadOnlyList<Structure> knownStructures)
    {
        var fields = new List<FieldMapping>();
        foreach (var field in variables)
        {
            if (field.IsPredefinedType)
            {
                fields.Add(new FieldMapping(field.Name, field.Type, PrimitiveTypeTranslator.GetSizeInBytes(field), new List<NameAndType>(0)));
            }
            else
            {
                var structure = GetStructureFromField(knownStructures, field);
                fields.AddRange(Create(field.Name, structure, new List<NameAndType>(0), knownStructures));
            }
        }

        return new StructMapping(name, fields);
    }

    private static IReadOnlyList<FieldMapping> Create(string fieldName, Structure structure, IReadOnlyList<NameAndType> pathSoFar, IReadOnlyList<Structure> knownStructures)
    {
        var fields = new List<FieldMapping>();

        var path = new List<NameAndType>(pathSoFar)
        {
            new NameAndType(fieldName, structure.Name)
        };

        foreach (var field in structure.Variables)
        {
            if (field.IsPredefinedType)
            {
                fields.Add(new FieldMapping(field.Name, field.Type, PrimitiveTypeTranslator.GetSizeInBytes(field), path));
            }
            else
            {
                var nextStructure = GetStructureFromField(knownStructures, field);
                fields.AddRange(Create(field.Name, nextStructure, path, knownStructures));
            }
        }

        return fields;
    }

    private static Structure GetStructureFromField(IReadOnlyList<Structure> knownStructures, Variable field)
    {
        return knownStructures.First(ks => ks.Name.Equals(field.Type, StringComparison.OrdinalIgnoreCase));
    }
}