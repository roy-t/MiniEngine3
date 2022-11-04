namespace Mini.Engine.Content.v2;
public interface IContent : IDisposable
{
    ContentId Id { get; }
    ISet<string> Dependencies { get; }
}
