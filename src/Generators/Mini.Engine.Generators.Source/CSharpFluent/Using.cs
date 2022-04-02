using System;

namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class Using : ISource, IEquatable<Using>
    {
        public Using(string @namespace) => this.Namespace = @namespace;

        public string Namespace { get; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteLine($"using {this.Namespace};");
        }

        public bool Equals(Using other)
        {
            return this.Namespace.Equals(other.Namespace);
        }

        public override bool Equals(object obj)
        {
            if(obj is Using other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Namespace.GetHashCode();
        }
    }
}
