using System.Collections;

namespace Mini.Engine.Content.v2;
public sealed class ContentStack : IEnumerable<IContent>
{
    private readonly Stack<ContentFrame> Stack;
    private readonly Dictionary<string, IContentCache> Caches;    

    public ContentStack(Dictionary<string, IContentCache> caches)
    {        
        this.Caches = caches;
        this.Stack = new Stack<ContentFrame>();
        this.Stack.Push(new ContentFrame("Root"));        
    }    

    public void Add(IContent content)
    {
        this.Stack.Peek().Content.Add(content);
    }

    public void Push(string frameName)
    {
        this.Stack.Push(new ContentFrame(frameName));
    }

    public void Pop()
    {
        var frame = this.Stack.Pop();
        foreach (var content in frame.Content)
        {
            var cache = this.Caches[content.GeneratorKey];
            cache.Unload(content.Id);
        }
    }    

    public void Clear()
    {
        while (this.Stack.Count > 0)
        {
            this.Pop();
        }
    }

    public IEnumerator<IContent> GetEnumerator()
    {
        return this.Stack.SelectMany(x => x.Content).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.Stack.SelectMany(x => x.Content).GetEnumerator();
    }
}
