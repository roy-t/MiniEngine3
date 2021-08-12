namespace Mini.Engine.UI
{
    internal class MicroBenchmark
    {
        private readonly string Name;
        private readonly TimeSpan UpdateInterval;

        private DateTime lastMicroBenchmark;
        private int frameAccumulator;

        public MicroBenchmark(string name, TimeSpan updateInterval)
        {
            this.lastMicroBenchmark = DateTime.MinValue;
            this.frameAccumulator = 0;
            this.LastResult = 0;
            this.Name = name;
            this.UpdateInterval = updateInterval;
        }

        public double LastResult { get; private set; }

        public void Update()
        {
            var elapsed = (DateTime.Now - this.lastMicroBenchmark);
            if (elapsed > this.UpdateInterval)
            {
                this.lastMicroBenchmark = DateTime.Now;
                this.LastResult = elapsed.TotalMilliseconds / this.frameAccumulator;
                this.frameAccumulator = 0;
            }

            this.frameAccumulator++;
        }

        public override string ToString()
        {
            var elapsed = (DateTime.Now - this.lastMicroBenchmark);
            var progress = elapsed.TotalMilliseconds / this.UpdateInterval.TotalMilliseconds;
            var seconds = (int)(progress * this.UpdateInterval.TotalSeconds);
            var invSeconds = (int)this.UpdateInterval.TotalSeconds - seconds;

            return $"{this.Name} {this.LastResult:F2}ms [{new string('|', seconds)}{new string('.', invSeconds)}]";
        }
    }
}
