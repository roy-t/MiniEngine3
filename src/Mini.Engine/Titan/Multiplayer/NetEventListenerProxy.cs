using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using Serilog;

namespace Mini.Engine.Titan.Multiplayer;
public sealed class NetEventListenerProxy : INetEventListener
{
    private readonly ILogger Logger;

    public NetEventListenerProxy(ILogger logger, INetEventListener initialListener)
    {
        this.Logger = logger.ForContext<NetEventListenerProxy>();
        this.Current = initialListener;
    }

    public INetEventListener Current { get; set; }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        this.Logger.Debug("OnConnectionRequest source: {@endPoint}", request.RemoteEndPoint);
        this.Current.OnConnectionRequest(request);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        this.Logger.Debug("OnNetworkError source: {@endPoint}, error: {@error}", endPoint, socketError);
        this.Current.OnNetworkError(endPoint, socketError);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        this.Logger.Debug("OnNetworkLatencyUpdate source: {@endPoint}, latency: {@latency}", peer.Address, latency);
        this.Current.OnNetworkLatencyUpdate(peer, latency);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        this.Logger.Debug("OnNetworkReceive source: {@endPoint}, bytes: {@length}, channel: {@channel}, method: {@method}", peer.Address, reader.AvailableBytes, channelNumber, deliveryMethod);
        this.Current.OnNetworkReceive(peer, reader, channelNumber, deliveryMethod);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        this.Logger.Debug("OnNetworkReceiveUnconnected source: {@endPoint}, bytes: {@length}, type: {@method}", remoteEndPoint, reader.AvailableBytes, messageType);
        this.Current.OnNetworkReceiveUnconnected(remoteEndPoint, reader, messageType);
    }

    public void OnPeerConnected(NetPeer peer)
    {
        this.Logger.Debug("OnPeerConnected source: {@endPoint}", peer.Address);
        this.Current.OnPeerConnected(peer);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        this.Logger.Debug("OnPeerDisconnected source: {@endPoint}, reason: {@reason}, code: {@code}", peer.Address, disconnectInfo.Reason, disconnectInfo.SocketErrorCode);
        this.Current.OnPeerDisconnected(peer, disconnectInfo);
    }
}
