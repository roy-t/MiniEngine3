using System.Collections.Generic;

namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class ParameterList : ISource
    {
        public ParameterList() => this.Parameters = new List<Parameter>();

        public List<Parameter> Parameters { get; }

        public void Generate(SourceWriter writer)
        {
            writer.Write("(");
            for (var i = 0; i < this.Parameters.Count; i++)
            {
                this.Parameters[i].Generate(writer);
                if (i < this.Parameters.Count - 1)
                {
                    writer.Write(", ");
                }
            }

            writer.Write(")");
        }

        public static ParameterListBuilder<ParameterList> Builder()
        {
            var parameterList = new ParameterList();
            return new ParameterListBuilder<ParameterList>(parameterList, parameterList);
        }
    }

    public sealed class ParameterListBuilder<TPrevious> : Builder<TPrevious, ParameterList>
    {
        internal ParameterListBuilder(TPrevious previous, ParameterList current)
            : base(previous, current) { }

        public ParameterListBuilder(TPrevious previous)
            : base(previous, new ParameterList()) { }

        public ParameterListBuilder<TPrevious> Parameter(string type, string name)
        {
            this.Output.Parameters.Add(new Parameter(type, name));
            return this;
        }
    }
}
