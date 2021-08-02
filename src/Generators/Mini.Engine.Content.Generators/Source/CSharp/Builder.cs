namespace Mini.Engine.Content.Generators.Source.CSharp
{
    public abstract class Builder<TPreviousBuilder, TOutput>
    {
        protected readonly TPreviousBuilder PreviousBuilder;

        protected Builder(TPreviousBuilder previous, TOutput current)
        {
            this.PreviousBuilder = previous;
            this.Output = current;
        }

        public TOutput Output { get; }

        public TPreviousBuilder Complete()
            => this.PreviousBuilder;
    }
}
