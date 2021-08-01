using System.Collections.Generic;
using System.IO;
using ShaderTools.CodeAnalysis.Hlsl;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace Mini.Engine.Content.Generators.Parsers.HLSL
{
    public sealed class Shader
    {
        public Shader(Microsoft.CodeAnalysis.AdditionalText shader)
        {
            this.FilePath = shader.Path;
            this.Name = Path.GetFileNameWithoutExtension(shader.Path);

            var options = new HlslParseOptions();
            options.AdditionalIncludeDirectories.Add(Path.GetDirectoryName(shader.Path));

            var contents = shader.GetText();
            var fileSystem = new ContentFileSystem();
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), options, fileSystem);

            this.Structures = Structure.FindAll(syntaxTree.Root);
            this.CBuffers = CBuffer.FindAll(syntaxTree.Root);
        }

        public string Name { get; }
        public string FilePath { get; }

        public IReadOnlyList<Structure> Structures { get; }
        public IReadOnlyList<CBuffer> CBuffers { get; }
    }
}
