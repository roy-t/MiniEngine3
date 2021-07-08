namespace Mini.Engine.Content.Generators.Source
{
    public sealed class Field : ISource
    {
        public Field(string type, string name, params string[] modifiers)
        {
            this.Type = type;
            this.Name = name;
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
                writer.Write($"= {this.Value}");
            }

            writer.WriteLine(";");
        }
    }
}
