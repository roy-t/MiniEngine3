using System.Collections.Generic;
using System.Linq;

namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class SourceFile : ISource
    {
        public SourceFile(string fileName)
        {
            this.Name = fileName;
            this.Usings = new List<Using>();
            this.Namespaces = new List<Namespace>();
        }

        public string Name { get; }
        public List<Using> Usings { get; }
        public List<Namespace> Namespaces { get; }

        public void Generate(SourceWriter writer)
        {
            var usings = this.Usings
                .Distinct()
                .OrderBy(x => x.Namespace);

            foreach (var @using in usings)
            {
                @using.Generate(writer);
            }

            writer.ConditionalEmptyLine(usings.Any());

            foreach (var @namespace in this.Namespaces)
            {
                @namespace.Generate(writer);
            }
        }

        public static SourceFileBuilder<SourceFile> Build(string fileName)
        {
            var sourceFile = new SourceFile(fileName);
            return new SourceFileBuilder<SourceFile>(sourceFile, sourceFile);
        }
    }

    public sealed class SourceFileBuilder<TPrevious> : Builder<TPrevious, SourceFile>
    {
        internal SourceFileBuilder(TPrevious previous, SourceFile current)
            : base(previous, current) { }

        public SourceFileBuilder(TPrevious previous, string fileName)
            : base(previous, new SourceFile(fileName)) { }

        public SourceFileBuilder<TPrevious> Using(string @using)
        {
            this.Output.Usings.Add(new Using(@using));
            return this;
        }

        public SourceFileBuilder<TPrevious> Usings(IEnumerable<string> usings)
        {
            this.Output.Usings.AddRange(usings.Select(u => new Using(u)));
            return this;
        }

        public NamespaceBuilder<SourceFileBuilder<TPrevious>> Namespace(string name)
        {
            var builder = new NamespaceBuilder<SourceFileBuilder<TPrevious>>(this, name);
            this.Output.Namespaces.Add(builder.Output);

            return builder;
        }
    }
}
