using System.Collections.Generic;
using System.Linq;

namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class Body : ICodeBlock
    {
        public Body(ICodeBlock block)
            :this()
        {
            this.Code.Add(block);
        }

        public Body()
            => this.Code = new List<ICodeBlock>();

        public List<ICodeBlock> Code { get; }

        public void Generate(SourceWriter writer)
        {
            foreach (var codeBlock in this.Code)
            {
                codeBlock.Generate(writer);
            }
        }

        public static BodyBuilder<Body> Builder()
        {
            var body = new Body();
            return new BodyBuilder<Body>(body, body);
        }
    }

    public sealed class BodyBuilder<TPrevious> : Builder<TPrevious, Body>
    {
        internal BodyBuilder(TPrevious previous, Body current)
            : base(previous, current) { }

        public BodyBuilder(TPrevious previous)
            : base(previous, new Body()) { }

        public BodyBuilder<TPrevious> TextCodeBlock(string text)
        {
            this.Output.Code.Add(new TextCodeBlock(text));
            return this;
        }

        public BodyBuilder<TPrevious> TextCodeBlocks(IEnumerable<string> texts)
        {
            this.Output.Code.AddRange(texts.Select(t => new TextCodeBlock(t)));
            return this;
        }

        public BodyBuilder<TPrevious> CodeBlocks(IEnumerable<ICodeBlock> blocks)
        {
            this.Output.Code.AddRange(blocks);
            return this;
        }

        public ForLoopBuilder<BodyBuilder<TPrevious>> ForLoop(string variable, string start, string op, string condition)
        {
            var builder = new ForLoopBuilder<BodyBuilder<TPrevious>>(this, variable, start, op, condition);
            this.Output.Code.Add(builder.Output);
            return builder;
        }
    }
}
