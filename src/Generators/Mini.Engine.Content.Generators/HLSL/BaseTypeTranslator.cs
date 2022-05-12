using Mini.Engine.Content.Generators.HLSL.Parsers;

namespace Mini.Engine.Content.Generators.HLSL;

public static class BaseTypeTranslator
{
    public static string GetBaseType(Function function)
    {
        var type = function.GetProgramDirective();
        switch (type)
        {
            case ProgramDirectives.VertexShader:
                return "VertexShaderContent";
            case ProgramDirectives.PixelShader:
                return "PixelShaderContent";
            case ProgramDirectives.ComputeShader:
                return "ComputeShaderContent";
            default:
                throw new InvalidOperationException($"Cannot get base type for program directive: {type}");
        }
    }
}
