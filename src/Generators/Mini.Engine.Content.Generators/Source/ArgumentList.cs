using System.Collections.Generic;

namespace Mini.Engine.Content.Generators.Source
{
    public sealed class ArgumentList : ISource
    {
        public ArgumentList()
        {
            this.Arguments = new List<string>();
        }

        public List<string> Arguments { get; }

        public void Generate(SourceWriter writer)
        {
            var arguments = string.Join(", ", this.Arguments);
            writer.Write($"({arguments})");
        }
    }
}
