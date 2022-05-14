using Mini.Engine.Content.Generators.HLSL.Parsers;
using Mini.Engine.Generators.Source;

namespace Mini.Engine.Content.Generators.HLSL;

public static class PrimitiveTypeTranslator
{
    public static string ToDotNetType(Variable variable)
    {
        return ToDotNetType(variable.Type, variable.IsCustomType, variable.Dimensions);
    }
    
    public static string ToDotNetType(string type, bool isCustomType, int dimensions)
    {
        if (isCustomType)
        {
            return Naming.ToUpperCamelCase(type);
        }

        switch (type)
        {
            case "bool":
                return Dimension("bool", dimensions);

            case "int":
                return Dimension("int", dimensions);

            case "uint":
            case "dword":
                return Dimension("uint", dimensions);

            case "float":
                return Dimension("float", dimensions);

            case "double":
                return Dimension("double", dimensions);

            case "float2":
                return Dimension(Numerics("Vector2"), dimensions);

            case "float3":
                return Dimension(Numerics("Vector3"), dimensions);

            case "float4":
                return Dimension(Numerics("Vector4"), dimensions);

            case "float4x4":
                return Dimension(Numerics("Matrix4x4"), dimensions);
            default:
                throw new NotSupportedException($"Cannot translate HLSL type {type} to .NET");
        }
    }

    public static int GetSizeInBytes(Variable variable)
    {
        if (variable.IsCustomType)
        {
            throw new NotSupportedException($"Cannot compute size of custom type: {variable.Type}");
        }

        if (variable.Dimensions > 0)
        {
            throw new NotSupportedException($"Cannot compute size of array type: {variable.Type}");
        }

        switch (variable.Type)
        {
            case "bool":
            case "int":
            case "uint":
            case "dword":
                return 4;

            case "float":
                return 4;

            case "double":
                return 8;

            case "float2":
                return 4 * 2;

            case "float3":
                return 4 * 3;

            case "float4":
                return 4 * 4;

            case "float4x4":
                return 4 * 4 * 4;
            default:
                throw new NotSupportedException($"Cannot compute size of {variable.Type}");
        }
    }

    private static string Dimension(string type, int dimensions)
    {
        var arr = dimensions > 0 ? $"[{new string(',', dimensions - 1)}]" : string.Empty;
        return $"{type}{arr}";
    }

    private static string Numerics(string type)
    {
        return $"System.Numerics.{type}";
    }

    
}
