using LiteNetLib;
using Serilog;

namespace Mini.Engine.Titan.Multiplayer;
public abstract class MultiplayerService : IDisposable
{
    private readonly NetEventListenerProxy Listener;
    protected readonly NetManager NetManager;
    protected readonly ILogger Logger;

    public MultiplayerService(ILogger logger, IMultiplayerState initial)
    {
        this.Logger = logger; // Do not use ForContext since this is an abstract class
        this.Listener = new NetEventListenerProxy(logger, initial);
        this.NetManager = new NetManager(this.Listener)
        {
            NatPunchEnabled = true,
        };
    }

    public IReadOnlyList<NetPeer> ConnectedPeers => this.NetManager.ConnectedPeerList;

    public void Update()
    {
        this.NetManager.PollEvents();
    }

    public void SwitchState(IMultiplayerState manager)
    {
        this.Listener.Current = manager;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);
}
