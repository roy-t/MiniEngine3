namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class TextCodeBlock : ICodeBlock
    {
        public TextCodeBlock()
            : this(string.Empty) { }

        public TextCodeBlock(string text)
        {
            this.Text = new SourceWriter();
            this.Text.Write(text);
        }

        public SourceWriter Text { get; }

        public void Generate(SourceWriter writer)
        {
            var text = this.Text.ToString();
            writer.WriteMultiLine(text);
        }
    }
}
