using System.Collections;
using Mini.Engine.Content.v2.Textures;

namespace Mini.Engine.Content.v2;
public sealed class ContentStack : IEnumerable<IContent>
{
    private readonly Stack<ContentFrame> Stack;
    private readonly ContentCache<TextureContent> TextureCache;

    public ContentStack(ContentCache<TextureContent> textureCache)
    {
        this.TextureCache = textureCache; // TODO: instead register callbacks for certain types that are unloaded?

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
            switch (content)
            {
                case TextureContent texture:
                    this.TextureCache.Unload(texture.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected content type: {content.GetType().FullName}");
            }
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
