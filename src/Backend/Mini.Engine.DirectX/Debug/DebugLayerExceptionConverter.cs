using Serilog.Events;
using System.Runtime.ExceptionServices;
using Vortice.DXGI.Debug;

namespace Mini.Engine.DirectX.Debugging;

internal sealed class DebugLayerExceptionConverter
{
    private readonly List<IDebugMessageProvider> Providers;
    private readonly LogEventLevel LogEventLevel;

    public DebugLayerExceptionConverter(LogEventLevel minLogEventLevel = LogEventLevel.Warning)
    {
        this.Providers = new List<IDebugMessageProvider>();
        this.LogEventLevel = minLogEventLevel;

        AppDomain.CurrentDomain.FirstChanceException += this.CheckExceptions;
    }

    public void Register(IDXGIInfoQueue infoQueue, Guid producer)
    {
        this.Providers.Add(new DxgiDebugMessageProvider(infoQueue, producer));
    }    

    public void CheckExceptions()
    {
        this.CheckExceptions(null, null);
    }

    private void CheckExceptions(object? _, FirstChanceExceptionEventArgs? e)
    {
        var exceptions = new List<Exception>();
        var buffer = new List<Message>();

        for (var i = 0; i < this.Providers.Count; i++)
        {
            buffer.Clear();
            var provider = this.Providers[i];
            provider.GetAllMessages(buffer);

            for (var j = 0; j < buffer.Count; j++)
            {
                var message = buffer[j];
                if (message.Level >= this.LogEventLevel)
                {
                    exceptions.Add(new Exception($"{message.Level}: {message.Description}", e?.Exception));
                }

            }
        }

        if (exceptions.Any())
        {
            File.WriteAllLines("DebugLayerLog.txt", buffer.Select(m => m.ToString()));
        }

        if (exceptions.Count == 1)
        {
            throw exceptions[0];
        }

        if (exceptions.Count > 1)
        {
            // TODO: somehow the primitive system sometimes keeps a context or shader resource view or something alive, not sure why its not disposed
            throw new AggregateException(exceptions);
        }
    }
}
