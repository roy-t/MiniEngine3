namespace Mini.Engine.Content;
public interface IContent : IDisposable
{
    ContentId Id { get; }
    ISet<string> Dependencies { get; }
}

public interface IContent<TContent,TSettings>
     : IContent
{
    TSettings Settings { get; }
    void Reload(TContent content);
}
