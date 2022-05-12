using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace Mini.Engine.Content.Generators.HLSL.Parsers;
public sealed class Shader
{
    public Shader(Microsoft.CodeAnalysis.AdditionalText shader)
       : this(shader.Path, shader.GetText())
    { }


    public Shader(string fullPath, SourceText? contents)
    {
        this.FilePath = Utilities.FindPathFromMarkerFile(fullPath, ".contentroot");
        this.Name = Path.GetFileNameWithoutExtension(fullPath);

        var options = new HlslParseOptions();
        options.AdditionalIncludeDirectories.Add(Path.GetDirectoryName(fullPath));            
        var fileSystem = new ContentFileSystem();
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), options, fileSystem);

        this.Structures = Structure.FindAll(syntaxTree.Root);
        this.CBuffers = CBuffer.FindAll(syntaxTree.Root);
        this.Functions = Function.FindAll(syntaxTree.Root);
        this.Variables = Variable.FindAll(syntaxTree.Root);
    }

    public static Shader Parse(string fullPath, SourceText? contents, CancellationToken cancellationToken)
    {
        var filePath = Utilities.FindPathFromMarkerFile(fullPath, ".contentroot");
        var name = Path.GetFileNameWithoutExtension(fullPath);

        var options = new HlslParseOptions();
        options.AdditionalIncludeDirectories.Add(Path.GetDirectoryName(fullPath));
        var fileSystem = new ContentFileSystem();
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), options, fileSystem, cancellationToken);

        var structures = Structure.FindAll(syntaxTree.Root);
        var cBuffers = CBuffer.FindAll(syntaxTree.Root);
        var functions = Function.FindAll(syntaxTree.Root);
        var variables = Variable.FindAll(syntaxTree.Root);

        return new Shader(name, filePath, structures, cBuffers, functions, variables);
    }

   

    private Shader(string name, string filePath, IReadOnlyList<Structure> structures, IReadOnlyList<CBuffer> cBuffers, IReadOnlyList<Function> functions, IReadOnlyList<Variable> variables)
    {
        this.Name = name;
        this.FilePath = filePath;
        this.Structures = structures;
        this.CBuffers = cBuffers;
        this.Functions = functions;
        this.Variables = variables;
    }

    public string Name { get; }
    public string FilePath { get; }

    public IReadOnlyList<Structure> Structures { get; }
    public IReadOnlyList<CBuffer> CBuffers { get; }
    public IReadOnlyList<Function> Functions { get; }
    public IReadOnlyList<Variable> Variables { get; }
}
