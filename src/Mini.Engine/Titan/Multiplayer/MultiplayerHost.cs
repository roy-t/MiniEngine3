using LiteNetLib;
using Mini.Engine.Configuration;
using Serilog;

namespace Mini.Engine.Titan.Multiplayer;
[Service]
public sealed class MultiplayerHost : MultiplayerService
{
    private readonly List<NetPeer> Clients;

    public MultiplayerHost(ILogger logger)
        : base(logger.ForContext<MultiplayerHost>(), new NullState())
    {
        this.Clients = new List<NetPeer>();
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
