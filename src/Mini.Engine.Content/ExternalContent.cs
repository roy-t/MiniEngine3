using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

/// <summary>
/// Wrapper that wraps any object that the content manager should control the lifetime of
/// for example a generated texture
/// </summary>
internal sealed class ExternalContent : IContent
{
    public ExternalContent(object content, string id)
    {
        this.Content = content;
        this.Id = new ContentId("<external>", id);
    }

    public ContentId Id { get; }
    public object Content { get; }

    public void Reload(Device device) { }
}
