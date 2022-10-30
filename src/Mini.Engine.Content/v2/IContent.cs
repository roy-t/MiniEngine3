namespace Mini.Engine.Content.v2;
public interface IContent
{
    ContentId Id { get; }    
    string GeneratorKey { get; set; }
    ISet<string> Dependencies { get; }
    void Dispose();
}
