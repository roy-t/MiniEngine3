using Mini.Engine.DirectX;

namespace Mini.Engine.Content.v2;

public class ContentLoadTask<T>
    where T : IDeviceResource
{
    private IResource<T>? content;

    public bool IsDone { get; private set; }





    internal void Complete(IResource<T> content)
    {
        this.content = content;
        this.IsDone = true;
    }

    public IResource<T> GetContent()
    {
        if (this.IsDone)
        {
            return this.content!;
        }

        throw new NullReferenceException("Content has not finished loading yet");
    }
}
