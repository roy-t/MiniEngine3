namespace Mini.Engine.Content;

internal interface IContentLoader<T>
{

    T Load(string name);
}
