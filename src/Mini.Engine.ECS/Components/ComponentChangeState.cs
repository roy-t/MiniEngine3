namespace Mini.Engine.ECS.Components
{
    public struct ComponentChangeState
    {
        public ComponentChangeState()
        {
            this.CurrentState = LifetimeState.Created;
            this.NextState = LifetimeState.New;
        }

        internal LifetimeState CurrentState { get; private set; }
        internal LifetimeState NextState { get; set; }

        public void Change()
        {
            this.NextState = LifetimeState.Changed;
        }

        public void Remove()
        {
            this.NextState = LifetimeState.Removed;
        }

        public void Next()
        {
            this.CurrentState = this.NextState;
            this.NextState = LifetimeState.Unchanged;
        }

        public override string ToString()
        {
            return $"{this.CurrentState} -> {this.NextState}";
        }
    }

    internal enum LifetimeState : byte
    {
        Created,
        New,
        Changed,
        Unchanged,
        Removed
    }
}
