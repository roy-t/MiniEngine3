using Serilog.Events;
using Vortice.DXGI.Debug;

namespace Mini.Engine.DirectX.Debugging;

internal sealed class DxgiDebugMessageProvider : IDebugMessageProvider
{
    private readonly IDXGIInfoQueue infoQueue;
    private readonly Guid producer;

    public DxgiDebugMessageProvider(IDXGIInfoQueue infoQueue, Guid producer)
    {
        this.infoQueue = infoQueue;
        this.producer = producer;
    }

    public void GetAllMessages(IList<Message> store)
    {
        var count = this.infoQueue.GetNumStoredMessages(this.producer);
        for (var i = 0ul; i < count; i++)
        {
            var message = this.infoQueue.GetMessage(this.producer, i);
            var description = $"[{message.Id}:{message.Category}] {DebugMessageProvider.UnterminateString(message.Description)}";
            var severity = SeverityToLevel(message.Severity);
            store.Add(new Message(description, severity));
        }

        this.infoQueue.ClearStoredMessages(this.producer);
    }

    private static LogEventLevel SeverityToLevel(InfoQueueMessageSeverity severity)
    {
        return severity switch
        {
            InfoQueueMessageSeverity.Corruption => LogEventLevel.Fatal,
            InfoQueueMessageSeverity.Error => LogEventLevel.Error,
            InfoQueueMessageSeverity.Warning => LogEventLevel.Warning,
            _ => LogEventLevel.Information,
        };
    }
}
