namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class ForeachLoop : ICodeBlock
    {
        public ForeachLoop(string variable, string enumerable)
        {
            this.Variable = variable;
            this.Enumerable = enumerable;
            this.Body = new Body();
        }

        public string Variable { get; }
        public string Enumerable { get; }

        public Body Body { get; set; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteLine($"foreach(var {this.Variable} in {this.Enumerable})");
            writer.StartScope();
            this.Body.Generate(writer);
            writer.EndScope();
        }

        public static ForLoopBuilder<ForLoop> Builder(string variable, string start, string op, string condition)
        {
            var forLoop = new ForLoop(variable, start, op, condition);
            return new ForLoopBuilder<ForLoop>(forLoop, forLoop);
        }
    }

    public sealed class ForeachLoopBuilder<TPrevious> : Builder<TPrevious, ForeachLoop>
    {
        internal ForeachLoopBuilder(TPrevious previous, ForeachLoop current)
            : base(previous, current) { }

        public ForeachLoopBuilder(TPrevious previous, string variable, string enumerable)
            : base(previous, new ForeachLoop(variable, enumerable)) { }


        public BodyBuilder<ForeachLoopBuilder<TPrevious>> Body()
        {
            var builder = new BodyBuilder<ForeachLoopBuilder<TPrevious>>(this);
            this.Output.Body = builder.Output;

            return builder;
        }
    }
}
