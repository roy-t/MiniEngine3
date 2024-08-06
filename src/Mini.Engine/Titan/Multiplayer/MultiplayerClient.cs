using System.Net;
using LiteNetLib;
using Mini.Engine.Configuration;
using Serilog;

namespace Mini.Engine.Titan.Multiplayer;
[Service]
public sealed class MultiplayerClient : MultiplayerService
{
    private NetPeer? host;

    public MultiplayerClient(ILogger logger)
        : base(logger.ForContext<MultiplayerClient>(), new NullState())
    {

    }

    public void Connect(IPEndPoint endPoint, string key)
    {
        this.NetManager.Start();
        this.host = this.NetManager.Connect(endPoint, key);
    }

    public void Disconnect()
    {
        this.host?.Disconnect();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Disconnect();
        }
    }
}
