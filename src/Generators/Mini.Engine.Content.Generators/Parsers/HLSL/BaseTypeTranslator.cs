using System;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public static class BaseTypeTranslator
    {
        public static string GetBaseType(Function function)
        {
            var type = function.GetProgramDirective();
            switch (type)
            {
                case ProgramDirectives.VertexShader:
                    return "VertexShader";
                case ProgramDirectives.PixelShader:
                    return "PixelShader";
                default:
                    throw new InvalidOperationException($"Cannot get base type for program directive: {type}");
            }
        }
    }
}
