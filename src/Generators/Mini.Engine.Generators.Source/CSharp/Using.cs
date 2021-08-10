using System;

namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class Using : ISource, IEquatable<Using>
    {
        public Using(string @namespace)
        {
            this.Namespace = @namespace;
        }

        public string Namespace { get; }

        public void Generate(SourceWriter writer)
            => writer.WriteLine($"using {this.Namespace};");

        public bool Equals(Using other) => this.Namespace.Equals(other.Namespace);
        public override bool Equals(object obj) => this.Equals(obj as Using);
        public override int GetHashCode() => this.Namespace.GetHashCode();
    }
}
