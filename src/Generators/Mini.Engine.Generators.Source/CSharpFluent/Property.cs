using Microsoft.CodeAnalysis;

namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class Property : ISource
    {
        public Property(string type, string name, bool isReadOnly, params string[] modifiers)
        {
            this.Type = type;
            this.Name = name;
            this.IsReadOnly = isReadOnly;
            this.Modifiers = modifiers;
        }

        public string Type { get; }
        public string Name { get; }
        public bool IsReadOnly { get; }
        public string[] Modifiers { get; }

        public Optional<string[]> GetModifiers { get; set; }
        public Optional<string[]> SetModifiers { get; set; }

        public Optional<Body> GetBody { get; set; }
        public Optional<Body> SetBody { get; set; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteModifiers(this.Modifiers);
            writer.Write($"{this.Type} {this.Name}");
            if (this.IsAutoProperty())
            {
                writer.Write(" { ");
                if (this.GetModifiers.HasValue)
                {
                    writer.WriteModifiers(this.GetModifiers.Value);

                }
                writer.Write("get;");

                if (!this.IsReadOnly)
                {
                    if (this.SetModifiers.HasValue)
                    {
                        writer.WriteModifiers(this.SetModifiers.Value);
                    }
                    writer.Write(" set;");
                }

                writer.Write(" } ");
            }
            else
            {
                writer.WriteLine();
                writer.StartScope();

                if (this.GetBody.HasValue)
                {
                    if (this.GetModifiers.HasValue)
                    {
                        writer.WriteModifiers(this.GetModifiers.Value);
                    }
                    writer.WriteLine("get");
                    writer.StartScope();
                    this.GetBody.Value.Generate(writer);
                    writer.EndScope();
                }

                if (this.SetBody.HasValue)
                {
                    if (this.SetModifiers.HasValue)
                    {
                        writer.WriteModifiers(this.SetModifiers.Value);
                    }
                    writer.WriteLine("set");
                    writer.StartScope();
                    this.SetBody.Value.Generate(writer);
                    writer.EndScope();
                }

                writer.EndScope();
            }
        }

        public bool IsAutoProperty() => !(this.GetBody.HasValue || this.SetBody.HasValue);

        public static PropertyBuilder<Property> Builder(string type, string name, bool isReadOnly, params string[] modifiers)
        {
            var property = new Property(type, name, isReadOnly, modifiers);
            return new PropertyBuilder<Property>(property, property);
        }
    }

    public sealed class PropertyBuilder<TPrevious> : Builder<TPrevious, Property>
    {
        internal PropertyBuilder(TPrevious previous, Property current)
            : base(previous, current) { }

        public PropertyBuilder(TPrevious previous, string type, string name, bool isReadOnly, params string[] modifiers)
            : base(previous, new Property(type, name, isReadOnly, modifiers)) { }

        public BodyBuilder<PropertyBuilder<TPrevious>> Getter()
        {
            var builder = new BodyBuilder<PropertyBuilder<TPrevious>>(this);
            this.Output.GetBody = builder.Output;

            return builder;
        }

        public BodyBuilder<PropertyBuilder<TPrevious>> Setter()
        {
            var builder = new BodyBuilder<PropertyBuilder<TPrevious>>(this);
            this.Output.SetBody = builder.Output;

            return builder;
        }
    }
}
