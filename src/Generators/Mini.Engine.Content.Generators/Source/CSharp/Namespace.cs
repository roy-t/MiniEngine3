using System.Collections.Generic;

namespace Mini.Engine.Content.Generators.Source.CSharp
{
    public sealed class Namespace : ISource
    {
        public Namespace(string name)
        {
            this.Name = name;
            this.Types = new List<Type>();
        }

        public string Name { get; }

        public List<Type> Types { get; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteLine($"namespace {this.Name}");
            writer.StartScope();

            foreach (var @class in this.Types)
            {
                @class.Generate(writer);
            }

            writer.EndScope();
        }
    }

    public sealed class NamespaceBuilder<TPrevious> : Builder<TPrevious, Namespace>
    {
        public NamespaceBuilder(TPrevious previous, string name)
            : base(previous, new Namespace(name)) { }

        public ClassBuilder<NamespaceBuilder<TPrevious>> Class(string name, params string[] modifiers)
        {
            var builder = new ClassBuilder<NamespaceBuilder<TPrevious>>(this, name, modifiers);
            this.Output.Types.Add(builder.Output);

            return builder;
        }
    }
}
