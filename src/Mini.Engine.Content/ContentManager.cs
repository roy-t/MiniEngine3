using Mini.Engine.Configuration;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content
{
    [Service]
    public sealed partial class ContentManager : IDisposable
    {
        private readonly Device Device;
        private readonly Stack<List<IContent>> ContentStack;

        public ContentManager(Device device)
        {
            this.ContentStack = new Stack<List<IContent>>();
            this.ContentStack.Push(new List<IContent>());
            this.Device = device;
        }

        public void Push()
        {
            this.ContentStack.Push(new List<IContent>());
        }

        public void Pop()
        {
            Dispose(this.ContentStack.Pop());
        }

        public void Dispose()
        {
            while (this.ContentStack.Count > 0)
            {
                Dispose(this.ContentStack.Pop());
            }
        }

        private void Add(IContent content)
        {
            this.ContentStack.Peek().Add(content);
        }

        private static void Dispose(List<IContent> list)
        {
            foreach (var content in list)
            {
                content.Dispose();
            }
        }
    }
}
