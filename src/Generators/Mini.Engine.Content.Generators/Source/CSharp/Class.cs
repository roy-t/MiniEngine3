using System.Collections.Generic;

namespace Mini.Engine.Content.Generators.Source.CSharp
{
    public sealed class Class : Type
    {
        public Class(string name, params string[] modifiers)
            : base(name, modifiers) { }

        public override string TypeKeyword => "class";
    }

    public sealed class ClassBuilder<TPrevious> : Builder<TPrevious, Class>
    {
        public ClassBuilder(TPrevious previous, string name, params string[] modifiers)
            : base(previous, new Class(name, modifiers)) { }

        public ClassBuilder<TPrevious> Inherits(string name)
        {
            this.Output.InheritsFrom.Add(name);
            return this;
        }

        public ConstructorBuilder<ClassBuilder<TPrevious>> Constructor(params string[] modifiers)
        {
            var builder = new ConstructorBuilder<ClassBuilder<TPrevious>>(this, this.Output.Name, modifiers);
            this.Output.Constructors.Add(builder.Output);
            return builder;
        }

        public ClassBuilder<TPrevious> InnerTypes(IEnumerable<Type> types)
        {
            this.Output.InnerTypes.AddRange(types);
            return this;
        }
    }
}
