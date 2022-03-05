using System;

namespace Mini.Engine.Generators.Source.CSharp
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

        public Builder<TPreviousBuilder, TOutput> If(bool condition, Action<Builder<TPreviousBuilder, TOutput>> action)
        {
            if(condition)
            {
                action(this);
            }
            return this;
        }
    }
}
