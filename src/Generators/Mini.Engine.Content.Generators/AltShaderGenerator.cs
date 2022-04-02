using Microsoft.CodeAnalysis;


namespace Mini.Engine.Content.Generators;

// TODO: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
[Generator]
public sealed class AltShaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var shaderFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".hlsl", StringComparison.InvariantCultureIgnoreCase));
        var provider = shaderFiles.Select((text, cancellationToken) => (path: FindRelativePath(text.Path), source: text.GetText(cancellationToken)));

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

    private static string FindRelativePath(string path)
    {
        var relativePath = Path.GetFileName(path);
        var directory = new DirectoryInfo(Path.GetDirectoryName(path));
        while (directory != null && !directory.EnumerateFiles(".contentroot").Any())
        {
            relativePath = Path.Combine(directory.Name, relativePath);
            directory = directory.Parent;

        }

        if (directory != null)
        {
            return relativePath;
        }
        else
        {
            throw new Exception($"Could not find .contentroot file in the directory {Path.GetDirectoryName(path)} or any of its parent directories");
        }
    }
}

