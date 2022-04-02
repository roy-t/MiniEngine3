namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class Field : ISource
    {
        public Field(string type, string name, params string[] modifiers)
        {
            this.Type = type;
            this.Name = name;
            this.Value = string.Empty;
            this.Modifiers = modifiers;
        }

        public string[] Modifiers { get; }
        public string Type { get; }
        public string Name { get; }
        public string Value { get; set; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteModifiers(this.Modifiers);
            writer.Write($"{this.Type} {this.Name}");

            if (!string.IsNullOrEmpty(this.Value))
            {
                writer.Write($" = {this.Value}");
            }

            writer.WriteLine(";");
        }

        public static FieldBuilder<Field> Builder(string type, string name, params string[] modifiers)
        {
            var field = new Field(type, name, modifiers);
            return new FieldBuilder<Field>(field, field);
        }
    }

    public sealed class FieldBuilder<TPrevious> : Builder<TPrevious, Field>
    {
        internal FieldBuilder(TPrevious previous, Field current)
            : base(previous, current) { }

        public FieldBuilder(TPrevious previous, string type, string name, params string[] modifiers)
            : base(previous, new Field(type, name, modifiers)) { }

        public FieldBuilder<TPrevious> Value(string value)
        {
            this.Output.Value = value;
            return this;
        }
    }
}
