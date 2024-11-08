﻿namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class Method : ISource
    {
        public Method(string type, string name, params string[] modifiers)
        {
            this.Type = type;
            this.Name = name;
            this.Modifiers = modifiers;
            this.Parameters = new ParameterList();
            this.Body = new Body();
        }

        public string Type { get; }
        public string Name { get; }
        public string[] Modifiers { get; }
        public ParameterList Parameters { get; }
        public Body Body { get; set; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteModifiers(this.Modifiers);
            writer.Write($"{this.Type} {this.Name}");
            this.Parameters.Generate(writer);
            writer.WriteLine();

            writer.StartScope();
            this.Body.Generate(writer);
            writer.EndScope();
        }

        public static MethodBuilder<Method> Builder(string type, string name, params string[] modifiers)
        {
            var method = new Method(type, name, modifiers);
            return new MethodBuilder<Method>(method, method);
        }
    }

    public sealed class MethodBuilder<TPrevious> : Builder<TPrevious, Method>
    {
        internal MethodBuilder(TPrevious previous, Method current)
            : base(previous, current) { }

        public MethodBuilder(TPrevious previous, string type, string name, params string[] modifiers)
            : base(previous, new Method(type, name, modifiers)) { }

        public MethodBuilder<TPrevious> Parameter(string type, string name)
        {
            this.Output.Parameters.Parameters.Add(new Parameter(type, name));
            return this;
        }

        public BodyBuilder<MethodBuilder<TPrevious>> Body()
        {
            var builder = new BodyBuilder<MethodBuilder<TPrevious>>(this);
            this.Output.Body = builder.Output;

            return builder;
        }
    }
}
