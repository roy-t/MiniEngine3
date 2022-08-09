namespace Mini.Engine.Generators.Source.CSharp;
public sealed class WhileLoop : ICodeBlock
{
    public WhileLoop(string condition)
    {
        this.Condition = condition;
        this.Body = new Body();
    }

    public string Condition { get; }

    public Body Body { get; set; }

    public void Generate(SourceWriter writer)
    {
        writer.WriteLine($"while({this.Condition})");
        writer.StartScope();
        this.Body.Generate(writer);
        writer.EndScope();
    }


    public sealed class WhileLoopBuilder<TPrevious> : Builder<TPrevious, WhileLoop>
    {
        public WhileLoopBuilder(TPrevious previous, WhileLoop current)
            : base(previous, current) { }

        public WhileLoopBuilder(TPrevious previous, string condition)
            : base(previous, new WhileLoop(condition)) { }

        public BodyBuilder<WhileLoopBuilder<TPrevious>> Body()
        {
            var builder = new BodyBuilder<WhileLoopBuilder<TPrevious>>(this);
            this.Output.Body = builder.Output;

            return builder;
        }
    }
}

