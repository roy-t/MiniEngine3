using System.Collections.Generic;

namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class ArgumentList : ISource
    {
        public ArgumentList(params string[] arguments)
            => this.Arguments = new List<string>(arguments);

        public List<string> Arguments { get; }

        public void Generate(SourceWriter writer)
        {
            var arguments = string.Join(", ", this.Arguments);
            writer.Write($"({arguments})");
        }

        public static ArgumentListBuilder<ArgumentList> Builder()
        {
            var argumentList = new ArgumentList();
            return new ArgumentListBuilder<ArgumentList>(argumentList, argumentList);
        }
    }

    public sealed class ArgumentListBuilder<TPrevious> : Builder<TPrevious, ArgumentList>
    {
        internal ArgumentListBuilder(TPrevious previous, ArgumentList current)
            : base(previous, current) { }

        public ArgumentListBuilder(TPrevious previous)
            : base(previous, new ArgumentList()) { }

        public ArgumentListBuilder<TPrevious> Argument(string argument)
        {
            this.Output.Arguments.Add(argument);
            return this;
        }
    }
}
