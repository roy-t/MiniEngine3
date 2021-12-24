using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            this.FilePath = FindRelativePath(shader.Path);
            this.Name = Path.GetFileNameWithoutExtension(shader.Path);

            var options = new HlslParseOptions();
            options.AdditionalIncludeDirectories.Add(Path.GetDirectoryName(shader.Path));

            var contents = shader.GetText();
            var fileSystem = new ContentFileSystem();
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), options, fileSystem);

            this.Structures = Structure.FindAll(syntaxTree.Root);
            this.CBuffers = CBuffer.FindAll(syntaxTree.Root);
            this.Functions = Function.FindAll(syntaxTree.Root);
            this.Variables = Variable.FindAll(syntaxTree.Root);
        }

        public string Name { get; }
        public string FilePath { get; }

        public IReadOnlyList<Structure> Structures { get; }
        public IReadOnlyList<CBuffer> CBuffers { get; }
        public IReadOnlyList<Function> Functions { get; }
        public IReadOnlyList<Variable> Variables { get; }


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
}
