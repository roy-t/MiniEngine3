using System;

namespace Mini.Engine.Generators.Source.CSharpFluent
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
        {
            return this.PreviousBuilder == null
                ? throw new InvalidOperationException()
                : this.PreviousBuilder;
        }
    }
}
