using System;

namespace Mini.Engine.Content.Generators.Shaders
{
    public static class TypeTranslator
    {
        public static string TranslateToDotNet(Variable variable)
        {
            if (variable.IsCustomType)
            {
                return Utilities.ToDotNetImportantName(variable.Type);
            }

            switch (variable.Type)
            {
                case "bool":
                    return "bool";

                case "int":
                    return "int";

                case "uint":
                case "dword":
                    return "uint";

                case "float":
                    return "float";

                case "double":
                    return "double";

                case "float2":
                    return Numerics("Vector2");

                case "float3":
                    return Numerics("Vector3");

                case "float4":
                    return Numerics("Vector4");

                case "float4x4":
                    return Numerics("Matrix4x4");

                default:
                    throw new NotSupportedException($"Cannot translate HLSL type {variable.Type} to .NET");
            }
        }

        private static string Numerics(string type)
            => $"System.Numerics.{type}";
    }
}
