namespace Mini.Engine.Content.Generators.Source.CSharp
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
            this.Arguments.Generate(writer);
            writer.WriteLine("]");
        }
    }
}
