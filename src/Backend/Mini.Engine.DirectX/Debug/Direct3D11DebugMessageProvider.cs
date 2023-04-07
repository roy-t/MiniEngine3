using Serilog.Events;
using Vortice.Direct3D11.Debug;

namespace Mini.Engine.DirectX.Debugging;

internal sealed class Direct3D11DebugMessageProvider : IDebugMessageProvider
{
    private readonly ID3D11InfoQueue infoQueue;

    public Direct3D11DebugMessageProvider(ID3D11InfoQueue infoQueue)
    {
        this.infoQueue = infoQueue;
    }

    public void GetAllMessages(IList<Message> store)
    {
        var count = this.infoQueue.NumStoredMessages;
        for (var i = 0ul; i < count; i++)
        {
            var message = this.infoQueue.GetMessage(i);
            var description = $"[{message.Id}:{message.Category}] {DebugMessageProvider.UnterminateString(message.Description)}";
            var severity = SeverityToLevel(message.Severity);
            store.Add(new Message(description, severity));
        }

        this.infoQueue.ClearStoredMessages();
    }

    private static LogEventLevel SeverityToLevel(MessageSeverity severity)
    {
        return severity switch
        {
            MessageSeverity.Corruption => LogEventLevel.Fatal,
            MessageSeverity.Error => LogEventLevel.Error,
            MessageSeverity.Warning => LogEventLevel.Warning,
            _ => LogEventLevel.Information,
        };
    }
}
