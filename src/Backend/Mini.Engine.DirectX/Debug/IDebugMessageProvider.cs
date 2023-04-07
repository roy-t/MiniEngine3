namespace Mini.Engine.DirectX.Debugging;

internal interface IDebugMessageProvider
{
    void GetAllMessages(IList<Message> store);
}
