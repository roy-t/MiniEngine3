namespace Mini.Engine.Generators.Source.CSharpFluent
{
    public sealed class BaseConstructorCall : IConstructorChainCall
    {
        public BaseConstructorCall(params string[] arguments)
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
            writer.Write(" : base");
            this.Arguments.Generate(writer);
        }

        public static BaseConstructorCallBuilder<BaseConstructorCall> Builder()
        {
            var call = new BaseConstructorCall();
            return new BaseConstructorCallBuilder<BaseConstructorCall>(call, call);
        }
    }


    public sealed class BaseConstructorCallBuilder<TPrevious> : Builder<TPrevious, BaseConstructorCall>
    {
        internal BaseConstructorCallBuilder(TPrevious previous, BaseConstructorCall call)
            : base(previous, call) { }

        public BaseConstructorCallBuilder(TPrevious previous)
            : base(previous, new BaseConstructorCall()) { }


        public BaseConstructorCallBuilder<TPrevious> Argument(string argument)
        {
            this.Output.Arguments.Arguments.Add(argument);
            return this;
        }
    }
}
