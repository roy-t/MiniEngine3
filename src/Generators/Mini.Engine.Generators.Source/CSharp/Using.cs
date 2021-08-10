namespace Mini.Engine.Generators.Source.CSharp
{
    public sealed class Using : ISource
    {
        public Using(string @namespace)
        {
            this.Namespace = @namespace;
        }

        public string Namespace { get; }

        public void Generate(SourceWriter writer)
            => writer.WriteLine($"using {this.Namespace};");
    }
}
