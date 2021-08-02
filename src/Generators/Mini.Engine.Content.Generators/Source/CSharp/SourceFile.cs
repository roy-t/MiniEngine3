using System.Collections.Generic;

namespace Mini.Engine.Content.Generators.Source.CSharp
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
            foreach (var @using in this.Usings)
            {
                @using.Generate(writer);
            }

            writer.ConditionalEmptyLine(this.Usings.Count > 0);

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

        public NamespaceBuilder<SourceFileBuilder> Namespace(string name)
        {
            var builder = new NamespaceBuilder<SourceFileBuilder>(this, name);
            this.Current.Namespaces.Add(builder.Output);

            return builder;
        }
    }
}
