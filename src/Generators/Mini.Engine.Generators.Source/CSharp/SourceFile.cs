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

        public static SourceFileBuilder Build(string fileName)
            => new SourceFileBuilder(fileName);
    }

    public sealed class SourceFileBuilder
    {
        private readonly SourceFile Current;

        public SourceFileBuilder(string fileName)
        {
            this.Current = new SourceFile(fileName);
        }

        public SourceFile Complete()
            => this.Current;

        public SourceFileBuilder Using(string @using)
        {
            this.Current.Usings.Add(new Using(@using));
            return this;
        }

        public SourceFileBuilder Usings(IEnumerable<string> usings)
        {
            this.Current.Usings.AddRange(usings.Select(u => new Using(u)));
            return this;
        }

        public NamespaceBuilder<SourceFileBuilder> Namespace(string name)
        {
            var builder = new NamespaceBuilder<SourceFileBuilder>(this, name);
            this.Current.Namespaces.Add(builder.Output);

            return builder;
        }
    }
}
