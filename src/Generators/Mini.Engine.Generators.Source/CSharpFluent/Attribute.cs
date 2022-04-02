namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class Attribute : ISource
    {
        public Attribute(string name)
            : this(name, new ArgumentList()) { }

        public Attribute(string name, ArgumentList arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
        }

        public string Name { get; }
        public ArgumentList Arguments { get; }

        public void Generate(SourceWriter writer)
        {
            writer.Write($"[{this.Name}");
            if (this.Arguments.Arguments.Count > 0)
            {
                this.Arguments.Generate(writer);
            }
            writer.WriteLine("]");
        }

        public static AttributeBuilder<Attribute> Builder(string name)
        {
            var attribute = new Attribute(name);
            return new AttributeBuilder<Attribute>(attribute, attribute);
        }
    }

    public sealed class AttributeBuilder<TPrevious> : Builder<TPrevious, Attribute>
    {
        internal AttributeBuilder(TPrevious previous, Attribute current)
            : base(previous, current) { }

        public AttributeBuilder(TPrevious previous, string name)
            : base(previous, new Attribute(name)) { }

        public AttributeBuilder<TPrevious> Argument(string argument)
        {
            this.Output.Arguments.Arguments.Add(argument);
            return this;
        }
    }
}
