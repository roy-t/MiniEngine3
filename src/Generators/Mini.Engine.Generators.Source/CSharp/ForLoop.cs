namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class ForLoop : ICodeBlock
    {
        public ForLoop(string variable, string start, string op, string condition)
        {

            Variable = variable;
            Start = start;
            Op = op;
            Condition = condition;
            this.Body = new Body();
        }

        public string Variable { get; }
        public string Start { get; }
        public string Op { get; }
        public string Condition { get; }

        public Body Body { get; set; }

        public void Generate(SourceWriter writer)
        {
            writer.WriteLine($"for(var {this.Variable} = {this.Start}; {this.Variable} {this.Op} {this.Condition}; {this.Variable}++)");
            writer.StartScope();
            this.Body.Generate(writer);
            writer.EndScope();
        }
    }

    public sealed class ForLoopBuilder<TPrevious> : Builder<TPrevious, ForLoop>
    {
        public ForLoopBuilder(TPrevious previous, string variable, string start, string op, string condition)
            : base(previous, new ForLoop(variable, start, op, condition)) { }


        public BodyBuilder<ForLoopBuilder<TPrevious>> Body()
        {
            var builder = new BodyBuilder<ForLoopBuilder<TPrevious>>(this);
            this.Output.Body = builder.Output;

            return builder;
        }
    }
}
