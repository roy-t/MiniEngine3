using Mini.Engine.Configuration;
using Serilog;

namespace Mini.Engine.Titan.Multiplayer;
[Service]
public sealed class MultiplayerHost : MultiplayerService
{
    public MultiplayerHost(ILogger logger)
        : base(logger.ForContext<MultiplayerHost>(), new NullState())
    {
    }


    public void Start(int port)
    {
        this.Logger.Debug("Host started on port {@port}", port);
        this.NetManager.Start(port);
    }

    public void Stop()
    {
        this.NetManager.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Stop();
        }
    }
}
