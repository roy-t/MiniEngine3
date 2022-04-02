using Microsoft.CodeAnalysis;


namespace Mini.Engine.Content.Generators;

// Based on: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
[Generator]
public sealed class AltShaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var shaderFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".hlsl", StringComparison.InvariantCultureIgnoreCase));
        var provider = shaderFiles.Select((text, cancellationToken)
            => (path: Utilities.FindPathFromMarkerFile(text.Path, ".contentroot"), source: text.GetText(cancellationToken)));

        context.RegisterSourceOutput(provider, (outputContext, nameAndText) =>
        {
            var name = Path.GetFileNameWithoutExtension(nameAndText.path);
            outputContext.AddSource($"Generated.{name}",
$@"
public static partial class ShaderNames
{{
    public const string {name} = ""{name}"";
}}
");
        });
    }


}

