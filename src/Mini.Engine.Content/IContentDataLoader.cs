namespace Mini.Engine.Content;

internal interface IContentDataLoader<T>
    where T : IContentData
{
    T Load(ContentId id);
}
