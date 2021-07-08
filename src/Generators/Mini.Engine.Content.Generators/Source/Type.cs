using System.Collections.Generic;

namespace Mini.Engine.Content.Generators.Source
{
    public abstract class Type : ISource
    {
        public Type(string name, params string[] modifiers)
        {
            this.Name = name;
            this.Modifiers = modifiers;
            this.Fields = new List<Field>();
            this.Constructors = new List<Constructor>();
            this.Properties = new List<Property>();
            this.Methods = new List<Method>();
            this.Attributes = new List<Attribute>();
            this.InheritsFrom = new List<string>();
        }

        public string Name { get; }
        public string[] Modifiers { get; }

        public List<Field> Fields { get; }
        public List<Constructor> Constructors { get; }
        public List<Property> Properties { get; }
        public List<Method> Methods { get; }
        public List<Attribute> Attributes { get; }
        public List<string> InheritsFrom { get; }

        public abstract string TypeKeyword { get; }

        public void Generate(SourceWriter writer)
        {
            foreach (var attribute in this.Attributes)
            {
                attribute.Generate(writer);
            }

            writer.WriteModifiers(this.Modifiers);
            writer.WriteLine($"{this.TypeKeyword} {this.Name}");
            if (this.InheritsFrom.Count > 0)
            {
                writer.StartIndent();
                writer.Write(": ");
                writer.Write(string.Join(", ", this.InheritsFrom));
                writer.EndIndent();
                writer.WriteLine();
            }
            writer.StartScope();

            foreach (var field in this.Fields)
            {
                field.Generate(writer);
            }

            writer.ConditionalEmptyLine(this.Fields.Count > 0);

            foreach (var constructor in this.Constructors)
            {
                constructor.Generate(writer);
                writer.WriteLine();
            }

            foreach (var property in this.Properties)
            {
                property.Generate(writer);
                writer.WriteLine();
            }

            foreach (var method in this.Methods)
            {
                method.Generate(writer);
                writer.WriteLine();
            }

            writer.EndScope();
        }
    }
}
