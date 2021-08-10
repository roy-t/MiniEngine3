using Microsoft.CodeAnalysis;

namespace Mini.Engine.Generators.Source.CSharp
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
    }

    public sealed class ConstructorBuilder<TPrevious> : Builder<TPrevious, Constructor>
    {
        public ConstructorBuilder(TPrevious previous, string name, params string[] modifiers)
            : base(previous, new Constructor(name, modifiers)) { }


        public ConstructorBuilder<TPrevious> BaseConstructorCall(params string[] arguments)
        {
            var @base = new BaseConstructorCall(arguments);
            this.Output.Chain = new Optional<IConstructorChainCall>(@base);
            return this;
        }

        public ConstructorBuilder<TPrevious> Parameter(string type, string name)
        {
            this.Output.Parameters.Add(type, name);
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
