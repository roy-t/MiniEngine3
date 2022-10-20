namespace Mini.Engine.Content.v2;

internal sealed record ContentFrame(string Name, List<IContent> Content)
{
    public ContentFrame(string name) : this(name, new List<IContent>()) { }
}
