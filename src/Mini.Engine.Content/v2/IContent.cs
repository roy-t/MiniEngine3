namespace Mini.Engine.Content.v2;
internal interface IContent
{
    ContentId Id { get; }
    IReadOnlyList<string> Dependencies { get; }
    void Dispose();
}
