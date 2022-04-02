using Microsoft.CodeAnalysis;

namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class Constructor : ISource
    {
        public Constructor(string @class, params string[] modifiers)
        {
            this.Class = @class;
            this.Modifiers = modifiers;
            this.Parameters = new ParameterList();
            this.Body = new Body();
        }

        public string Class { get; }
        public string[] Modifiers { get; }
        public ParameterList Parameters { get; }
        public Body Body { get; set; }

        public Optional<IConstructorChainCall> Chain { get; set; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteModifiers(this.Modifiers);
            writer.Write($"{this.Class}");
            this.Parameters.Generate(writer);
            if (this.Chain.HasValue)
            {
                writer.StartIndent();
                writer.WriteLine();
                this.Chain.Value.Generate(writer);
                writer.EndIndent();
            }
            writer.WriteLine();
            writer.StartScope();
            this.Body.Generate(writer);
            writer.EndScope();
        }

        public static ConstructorBuilder<Constructor> Builder(string @class, params string[] modifiers)
        {
            var constructor = new Constructor(@class, modifiers);
            return new ConstructorBuilder<Constructor>(constructor, constructor);
        }
    }

    public sealed class ConstructorBuilder<TPrevious> : Builder<TPrevious, Constructor>
    {
        internal ConstructorBuilder(TPrevious previous, Constructor current)
            : base(previous, current) { }

        public ConstructorBuilder(TPrevious previous, string @class, params string[] modifiers)
            : base(previous, new Constructor(@class, modifiers)) { }


        public ConstructorBuilder<TPrevious> BaseConstructorCall(params string[] arguments)
        {
            var @base = new BaseConstructorCall(arguments);
            this.Output.Chain = new Optional<IConstructorChainCall>(@base);
            return this;
        }

        public ConstructorBuilder<TPrevious> Parameter(string type, string name)
        {
            this.Output.Parameters.Parameters.Add(new Parameter(type, name));
            return this;
        }

        public BodyBuilder<ConstructorBuilder<TPrevious>> Body()
        {
            var builder = new BodyBuilder<ConstructorBuilder<TPrevious>>(this);
            this.Output.Body = builder.Output;

            return builder;
        }
    }
}
