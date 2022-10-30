namespace Mini.Engine.Content.v2;
public interface IContent : IDisposable
{
    ContentId Id { get; }
    string GeneratorKey { get; set; }
    ISet<string> Dependencies { get; }
}
