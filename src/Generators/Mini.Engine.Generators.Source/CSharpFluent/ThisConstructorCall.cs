namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class ThisConstructorCall : IConstructorChainCall
    {
        public ThisConstructorCall(params string[] arguments)
        {
            this.Arguments = new ArgumentList();
            foreach (var argument in arguments)
            {
                this.Arguments.Arguments.Add(argument);
            }
        }

        public ArgumentList Arguments { get; }

        public void Generate(SourceWriter writer)
        {
            writer.Write(" : this");
            this.Arguments.Generate(writer);
        }

        public static ThisConstructorCallBuilder<ThisConstructorCall> Builder()
        {
            var call = new ThisConstructorCall();
            return new ThisConstructorCallBuilder<ThisConstructorCall>(call, call);
        }
    }

    public sealed class ThisConstructorCallBuilder<TPrevious> : Builder<TPrevious, ThisConstructorCall>
    {
        internal ThisConstructorCallBuilder(TPrevious previous, ThisConstructorCall call)
            : base(previous, call) { }

        public ThisConstructorCallBuilder(TPrevious previous)
            : base(previous, new ThisConstructorCall()) { }


        public ThisConstructorCallBuilder<TPrevious> Argument(string argument)
        {
            this.Output.Arguments.Arguments.Add(argument);
            return this;
        }
    }
}
