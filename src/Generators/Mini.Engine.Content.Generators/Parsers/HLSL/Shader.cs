using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace Mini.Engine.Content.Generators.Parsers.HLSL;

public sealed class Shader
{
    public Shader(string fullPath, string name, SourceText? contents)
    {
        this.FilePath = Utilities.FindPathFromMarkerFile(fullPath, ".contentroot");
        this.Name = name;

        var options = new HlslParseOptions();
        options.AdditionalIncludeDirectories.Add(Path.GetDirectoryName(fullPath));            
        var fileSystem = new ContentFileSystem();
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), options, fileSystem);

        this.Structures = Structure.FindAll(syntaxTree.Root);
        this.CBuffers = CBuffer.FindAll(syntaxTree.Root);
        this.Functions = Function.FindAll(syntaxTree.Root);
        this.Variables = Variable.FindAll(syntaxTree.Root);
    }

    public Shader(Microsoft.CodeAnalysis.AdditionalText shader)
        : this(shader.Path, Path.GetFileNameWithoutExtension(shader.Path), shader.GetText())
    { }

    public string Name { get; }
    public string FilePath { get; }

    public IReadOnlyList<Structure> Structures { get; }
    public IReadOnlyList<CBuffer> CBuffers { get; }
    public IReadOnlyList<Function> Functions { get; }
    public IReadOnlyList<Variable> Variables { get; }
}
