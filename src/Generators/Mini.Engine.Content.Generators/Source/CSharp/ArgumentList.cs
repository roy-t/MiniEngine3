using System.Collections.Generic;

namespace Mini.Engine.Content.Generators.Source.CSharp
{
    public sealed class ArgumentList : ISource
    {
        public ArgumentList(params string[] arguments)
        {
            this.Arguments = new List<string>(arguments);
        }

        public List<string> Arguments { get; }

        public void Generate(SourceWriter writer)
        {
            var arguments = string.Join(", ", this.Arguments);
            writer.Write($"({arguments})");
        }
    }
}
