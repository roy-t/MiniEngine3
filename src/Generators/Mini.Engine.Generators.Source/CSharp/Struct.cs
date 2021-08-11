using System.Collections.Generic;

namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class Struct : Type
    {
        public Struct(string name, params string[] modifiers)
          : base(name, modifiers) { }

        public override string TypeKeyword => "struct";

        public static StructBuilder<Struct> Builder(string name, params string[] modifiers)
        {
            var @struct = new Struct(name, modifiers);
            return new StructBuilder<Struct>(@struct, @struct);
        }
    }

    public sealed class StructBuilder<TPrevious> : Builder<TPrevious, Struct>
    {
        internal StructBuilder(TPrevious previous, Struct current)
            : base(previous, current) { }

        public StructBuilder(TPrevious previous, string name, params string[] modifiers)
            : base(previous, new Struct(name, modifiers)) { }

        public StructBuilder<TPrevious> Attribute(string name, params string[] arguments)
        {
            var attribute = new Attribute(name, new ArgumentList(arguments));
            this.Output.Attributes.Add(attribute);

            return this;
        }

        public StructBuilder<TPrevious> Properties(IEnumerable<Property> properties)
        {
            this.Output.Properties.AddRange(properties);
            return this;
        }

        public FieldBuilder<StructBuilder<TPrevious>> Field(string type, string name, params string[] modifiers)
        {
            var builder = new FieldBuilder<StructBuilder<TPrevious>>(this, type, name, modifiers);
            this.Output.Fields.Add(builder.Output);

            return builder;
        }
    }
}
