using System;
using Mini.Engine.Content.Generators.Source;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public static class TypeTranslator
    {
        public static string GetDotNetType(Variable variable)
        {
            if (variable.IsCustomType)
            {
                return Naming.ToPascalCase(variable.Type);
            }

            switch (variable.Type)
            {
                case "bool":
                    return Dimension("bool", variable.Dimensions);

                case "int":
                    return Dimension("int", variable.Dimensions);

                case "uint":
                case "dword":
                    return Dimension("uint", variable.Dimensions);

                case "float":
                    return Dimension("float", variable.Dimensions);

                case "double":
                    return Dimension("double", variable.Dimensions);

                case "float2":
                    return Dimension(Numerics("Vector2"), variable.Dimensions);

                case "float3":
                    return Dimension(Numerics("Vector3"), variable.Dimensions);

                case "float4":
                    return Dimension(Numerics("Vector4"), variable.Dimensions);

                case "float4x4":
                    return Dimension(Numerics("Matrix4x4"), variable.Dimensions);

                default:
                    throw new NotSupportedException($"Cannot translate HLSL type {variable.Type} to .NET");
            }
        }

        private static string Dimension(string type, int dimensions)
        {
            var arr = dimensions > 0 ? $"[{new string(',', dimensions - 1)}]" : string.Empty;
            return $"{type}{arr}";
        }

        private static string Numerics(string type)
            => $"System.Numerics.{type}";
    }
}
